using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;
using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.Core
{
    public class WalletConnectProtocol : DisposableBase
    {
        public readonly EventDelegator Events;

        protected string Version = "1";
        protected string _bridgeUrl;
        protected string _key;
        protected byte[] _keyRaw;
        protected List<string> _activeTopics = new List<string>();

        public event EventHandler<WalletConnectProtocol> OnTransportConnect;
        public event EventHandler<WalletConnectProtocol> OnTransportDisconnect;

        public bool SessionConnected { get; protected set; }

        public bool Disconnected { get; protected set; }

        public bool Connected
        {
            get { return SessionConnected && TransportConnected; }
        }

        public bool Connecting { get; protected set; }

        public bool TransportConnected
        {
            get { return Transport != null && Transport.Connected && Transport.URL.Replace("https://", "").Replace("wss://", "") == _bridgeUrl.Replace("https://", "").Replace("wss://", ""); }
        }

        public ITransport Transport { get; protected set; }

        public ICipher Cipher { get; protected set; }

        public ClientMeta DappMetadata { get; set; }

        public ClientMeta WalletMetadata { get; set; }

        public ReadOnlyCollection<string> ActiveTopics
        {
            get { return _activeTopics.AsReadOnly(); }
        }

        public string PeerId { get; protected set; }

        /// <summary>
        /// Create a new WalletConnectProtocol object using a SavedSession as the session data. This will effectively resume
        /// the session, as long as the session data is valid
        /// </summary>
        /// <param name="savedSession">The SavedSession data to use. Cannot be null</param>
        /// <param name="transport">The transport interface to use for sending/receiving messages, null will result in the default transport being used</param>
        /// <param name="cipher">The cipher to use for encrypting and decrypting payload data, null will result in AESCipher being used</param>
        /// <param name="eventDelegator">The EventDelegator class to use, null will result in the default being used</param>
        /// <exception cref="ArgumentException">If a null SavedSession object was given</exception>
        public WalletConnectProtocol(SavedSession savedSession, ITransport transport = null, ICipher cipher = null, EventDelegator eventDelegator = null)
        {
            if (savedSession == null)
                throw new ArgumentException("savedSession cannot be null");

            if (eventDelegator == null)
                eventDelegator = new EventDelegator();

            this.Events = eventDelegator;

            //TODO Do we need this for resuming?
            //_handshakeTopic = topicGuid.ToString();

            if (transport == null)
                transport = TransportFactory.Instance.BuildDefaultTransport(eventDelegator);

            this._bridgeUrl = savedSession.BridgeURL;
            this.Transport = transport;

            if (cipher == null)
                cipher = new AESCipher();

            this.Cipher = cipher;

            this._keyRaw = savedSession.KeyRaw;

            //Convert hex
            this._key = savedSession.Key;

            this.PeerId = savedSession.PeerID;

            /*Transport.Open(this._bridgeUrl).ContinueWith(delegate(Task task)
            {
                Transport.Subscribe(savedSession.ClientID);
            });
    
            this.Connected = true;*/
        }

        /// <summary>
        /// Create a new WalletConnectProtocol object and create a new dApp session.
        /// </summary>
        /// <param name="clientMeta">The metadata to send to wallets</param>
        /// <param name="transport">The transport interface to use for sending/receiving messages, null will result in the default transport being used</param>
        /// <param name="cipher">The cipher to use for encrypting and decrypting payload data, null will result in AESCipher being used</param>
        /// <param name="chainId">The chainId this dApp is using</param>
        /// <param name="bridgeUrl">The bridgeURL to use to communicate with the wallet</param>
        /// <param name="eventDelegator">The EventDelegator class to use, null will result in the default being used</param>
        /// <exception cref="ArgumentException">If an invalid ClientMeta object was given</exception>
        public WalletConnectProtocol(ITransport transport = null, ICipher cipher = null, EventDelegator eventDelegator = null)
        {
            if (eventDelegator == null)
                eventDelegator = new EventDelegator();

            this.Events = eventDelegator;

            if (transport == null)
                transport = TransportFactory.Instance.BuildDefaultTransport(eventDelegator);

            this.Transport = transport;

            if (cipher == null)
                cipher = new AESCipher();

            this.Cipher = cipher;
        }

        protected async Task SetupTransport()
        {
            Transport.MessageReceived += TransportOnMessageReceived;

            await Transport.Open(this._bridgeUrl);

            //Debug.Log("[WalletConnect] Transport Opened");

            TriggerOnTransportConnect();
        }

        protected async Task DisconnectTransport()
        {
            await Transport.Close();

            Transport.MessageReceived -= TransportOnMessageReceived;

            if (OnTransportDisconnect != null)
                OnTransportDisconnect(this, this);
        }

        protected virtual void TriggerOnTransportConnect()
        {
            if (OnTransportConnect != null)
                OnTransportConnect(this, this);
        }

        public virtual async Task Connect()
        {
            await SetupTransport();
        }

        public virtual async Task SubscribeAndListenToTopic(string topic)
        {
            await Transport.Subscribe(topic);

            ListenToTopic(topic);
        }

        public virtual void ListenToTopic(string topic)
        {
            if (!_activeTopics.Contains(topic))
            {
                _activeTopics.Add(topic);
            }
        }

        protected async void TransportOnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var networkMessage = e.Message;

            if (!_activeTopics.Contains(networkMessage.Topic))
                return;

            var encryptedPayload = JsonConvert.DeserializeObject<EncryptedPayload>(networkMessage.Payload);

            var json = await Cipher.DecryptWithKey(_keyRaw, encryptedPayload);

            var payload = JsonConvert.DeserializeObject<JsonRpcPayload>(json);

            if (payload != null)
            {
                Events.Trigger(payload.Event, json);
            }
        }

        public virtual async Task<TR> SendRequestAwaitResponse<T, TR>(T requestObject, object requestId, string sendingTopic = null, bool? forcePushNotification = null)
        {
            TaskCompletionSource<TR> response = new TaskCompletionSource<TR>(TaskCreationOptions.None);

            Events.ListenForGenericResponse<TR>(
                requestId,
                (sender, args) =>
                {
                    response.SetResult(args.Response);
                }
            );

            await SendRequest(requestObject, sendingTopic, forcePushNotification);

            return await response.Task;
        }

        public virtual async Task<R> Send<T, R>(T data)
            where T : JsonRpcRequest
            where R : JsonRpcResponse
        {
            TaskCompletionSource<R> eventCompleted = new TaskCompletionSource<R>(TaskCreationOptions.None);

            Events.ListenForResponse<R>(
                data.ID,
                (sender, @event) =>
                {
                    var response = @event.Response;
                    if (response.IsError)
                    {
                        eventCompleted.SetException(new WalletException(response.Error));
                    }
                    else
                    {
                        eventCompleted.SetResult(@event.Response);
                    }
                }
            );

            await SendRequest(data);

            return await eventCompleted.Task;
        }

        public virtual async Task SendRequest<T>(T requestObject, string sendingTopic = null, bool? forcePushNotification = null)
        {
            var silent = isRequestSilent(forcePushNotification, requestObject);
            var json = JsonConvert.SerializeObject(requestObject);

            var encrypted = await Cipher.EncryptWithKey(_keyRaw, json);

            if (sendingTopic == null)
                sendingTopic = PeerId;

            var message = new NetworkMessage()
            {
                Payload = JsonConvert.SerializeObject(encrypted),
                Silent = silent,
                Topic = sendingTopic,
                Type = "pub"
            };

            await this.Transport.SendMessage(message);
        }

        public virtual async Task Disconnect()
        {
            await DisconnectTransport();
        }

        protected override void DisposeManaged()
        {
            if (Transport != null)
            {
                Transport.Dispose();
                Transport = null;
            }
        }

        private bool isRequestSilent<T>(bool? forcePushNotification, T requestObject)
        {
            if (forcePushNotification.HasValue)
            {
                return !forcePushNotification.Value;
            }

            return requestObject is JsonRpcRequest request && (JsonRpcRequest.IsWalletConnectMethod(request.Method) || !JsonRpcRequest.IsSigningMethod(request.Method));
        }
    }
}
