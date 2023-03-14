using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Events.Request;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Core.Models.Ethereum.Types;
using WalletConnectSharp.Core.Network;
using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.Core
{
    public class WalletConnectSession : WalletConnectProtocol
    {
        private string _handshakeTopic;

        private long _handshakeId;

        public event EventHandler<WalletConnectSession> OnSessionConnect;
        public event EventHandler<WalletConnectSession> OnSessionCreated;
        public event EventHandler<WalletConnectSession> OnSessionResumed;
        public event EventHandler OnSessionDisconnect;
        public event EventHandler<WalletConnectSession> OnSend;
        public event EventHandler<WCSessionData> SessionUpdate;

        public int NetworkId { get; private set; }

        public string[] Accounts { get; private set; }

        public bool ReadyForUserPrompt { get; private set; }

        public bool SessionUsed { get; private set; }

        public int ChainId { get; private set; }

        private string clientId = "";

        public string URI
        {
            get
            {
                var topicEncode = WebUtility.UrlEncode(_handshakeTopic);
                var versionEncode = WebUtility.UrlEncode(Version);
                var bridgeUrlEncode = WebUtility.UrlEncode(_bridgeUrl);
                var keyEncoded = WebUtility.UrlEncode(_key);

                return "wc:" + topicEncode + "@" + versionEncode + "?bridge=" + bridgeUrlEncode + "&key=" + keyEncoded;
            }
        }

        /// <summary>
        /// Create a WalletConnect Session from a SavedSession
        /// </summary>
        public WalletConnectSession(SavedSession savedSession, ITransport transport = null, ICipher cipher = null, EventDelegator eventDelegator = null)
            : base(savedSession, transport, cipher, eventDelegator)
        {
            this.DappMetadata = savedSession.DappMeta;
            this.WalletMetadata = savedSession.WalletMeta;
            this.ChainId = savedSession.ChainID;

            clientId = savedSession.ClientID;

            this.Accounts = savedSession.Accounts;

            this.NetworkId = savedSession.NetworkID;

            this._handshakeId = savedSession.HandshakeID;

            this.SessionConnected = true;
        }

        /// <summary>
        /// Create a new WalletConnect Session
        /// </summary>
        public WalletConnectSession(ClientMeta clientMeta, string bridgeUrl = null, ITransport transport = null, ICipher cipher = null, int chainId = 1, EventDelegator eventDelegator = null)
            : base(transport, cipher, eventDelegator)
        {
            if (clientMeta == null)
            {
                throw new ArgumentException("clientMeta cannot be null!");
            }

            if (string.IsNullOrWhiteSpace(clientMeta.Description))
            {
                throw new ArgumentException("clientMeta must include a valid Description");
            }

            if (string.IsNullOrWhiteSpace(clientMeta.Name))
            {
                throw new ArgumentException("clientMeta must include a valid Name");
            }

            if (string.IsNullOrWhiteSpace(clientMeta.URL))
            {
                throw new ArgumentException("clientMeta must include a valid URL");
            }

            if (clientMeta.Icons == null || clientMeta.Icons.Length == 0)
            {
                throw new ArgumentException("clientMeta must include an array of Icons the Wallet app can use. These Icons must be URLs to images. You must include at least one image URL to use");
            }

            if (bridgeUrl == null)
            {
                bridgeUrl = DefaultBridge.ChooseRandomBridge();
            }

            bridgeUrl = DefaultBridge.GetBridgeUrl(bridgeUrl);

            this.DappMetadata = clientMeta;
            this.ChainId = chainId;
            this._bridgeUrl = bridgeUrl;

            this.SessionConnected = false;

            CreateNewSession();
        }

        protected void CreateNewSession()
        {
            if (SessionConnected)
            {
                throw new IOException("You cannot create a new session after connecting the session. Create a new WalletConnectSession object to create a new session");
            }

            this._bridgeUrl = DefaultBridge.GetBridgeUrl(this._bridgeUrl);

            var topicGuid = Guid.NewGuid();

            _handshakeTopic = topicGuid.ToString();

            clientId = Guid.NewGuid().ToString();

            GenerateKey();

            if (Transport != null)
            {
                Transport.ClearSubscriptions();
            }

            SessionUsed = false;
            ReadyForUserPrompt = false;
        }

        protected void EnsureNotDisconnected()
        {
            if (Disconnected)
            {
                throw new IOException("Session stale! The session has been disconnected. This session cannot be reused.");
            }
        }

        protected void GenerateKey()
        {
            //Generate a random secret
            byte[] secret = new byte[32];
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(secret);

            this._keyRaw = secret;

            //Convert hex
            this._key = this._keyRaw.ToHex().ToLower();
        }

        public virtual async Task<WCSessionData> ConnectSession()
        {
            EnsureNotDisconnected();

            Connecting = true;
            try
            {
                if (SessionConnected)
                {
                    //Listen for the _handshakeId response
                    //The response will be of type WCSessionRequestResponse
                    //We do this now before subscribing
                    //This is in case we need to respond to a session disconnect and this is a
                    //resume session
                    Events.ListenForResponse<WCSessionRequestResponse>(this._handshakeId, async (sender, @event) => await HandleSessionResponse(sender, @event));
                }

                if (!base.TransportConnected)
                {
                    await base.SetupTransport();
                }

                ReadyForUserPrompt = false;
                await SubscribeAndListenToTopic(this.clientId);

                ListenToTopic(this._handshakeTopic);

                WCSessionData result;
                if (!SessionConnected)
                {
                    //result = await CreateSession();

                    //throw timeout exception after two minutes
                    int timeout = 120000;
                    var task = CreateSession();
                    if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                    {
                        // task completed within timeout
                        result = task.Result;
                    }
                    else
                    {
                        // timeout logic
                        throw new TimeoutException();
                    }
                    //Reset this back after we have established a session
                    ReadyForUserPrompt = false;
                    Connecting = false;

                    if (OnSessionCreated != null)
                        OnSessionCreated(this, this);
                }
                else
                {
                    result = new WCSessionData()
                    {
                        accounts = Accounts,
                        approved = true,
                        chainId = ChainId,
                        networkId = NetworkId,
                        peerId = PeerId,
                        peerMeta = WalletMetadata
                    };
                    Connecting = false;

                    if (OnSessionResumed != null)
                        OnSessionResumed(this, this);
                }

                if (OnSessionConnect != null)
                    OnSessionConnect(this, this);

                return result;
            }
            catch (IOException e)
            {
                //If the transport is connected, then disconnect that
                //we tried our best, they can try again
                if (TransportConnected)
                {
                    await DisconnectTransport();
                }
                else
                {
                    throw new IOException("Transport Connection failed", e);
                }

                throw new IOException("Session Connection failed", e);
            }
            finally
            {
                //The session has been made, we are no longer ready for another user prompt
                ReadyForUserPrompt = false;
                Connecting = false;
            }
        }

        public override async Task Connect()
        {
            EnsureNotDisconnected();

            await base.Connect();

            await ConnectSession();
        }

        public virtual async Task DisconnectSession(string disconnectMessage = "Session Disconnected", bool createNewSession = true)
        {
            EnsureNotDisconnected();

            var request = new WCSessionUpdate(
                new WCSessionData()
                {
                    approved = false,
                    chainId = 0,
                    accounts = null,
                    networkId = 0
                }
            );

            await SendRequest(request);

            await base.Disconnect();

            await HandleSessionDisconnect(disconnectMessage, "disconnect", createNewSession);
        }

        public override async Task Disconnect()
        {
            EnsureNotDisconnected();

            await DisconnectSession();
        }

        public virtual async Task<string> WalletAddEthChain(EthChainData chainData)
        {
            EnsureNotDisconnected();

            var request = new WalletAddEthChain(chainData);

            var response = await Send<WalletAddEthChain, EthResponse>(request);

            return response.Result;
        }

        public virtual async Task<string> WalletSwitchEthChain(EthChainData chainData)
        {
            EnsureNotDisconnected();

            var request = new WalletSwitchEthChain(chainData);

            var response = await Send<WalletSwitchEthChain, EthResponse>(request);

            return response.Result;
        }

        public virtual async Task<string> EthSign(string address, string message)
        {
            EnsureNotDisconnected();

            if (!message.IsHex())
            {
                var rawMessage = Encoding.UTF8.GetBytes(message);

                var byteList = new List<byte>();
                var bytePrefix = "0x19".HexToByteArray();
                var textBytePrefix = Encoding.UTF8.GetBytes("Ethereum Signed Message:\n" + rawMessage.Length);

                byteList.AddRange(bytePrefix);
                byteList.AddRange(textBytePrefix);
                byteList.AddRange(rawMessage);

                var hash = new Sha3Keccack().CalculateHash(byteList.ToArray());

                message = "0x" + hash.ToHex();

                //message = "0x" + Encoding.UTF8.GetBytes(message).ToHex();
            }

            var request = new EthSign(address, message);

            var response = await Send<EthSign, EthResponse>(request);

            return response.Result;
        }

        public virtual async Task<string> EthPersonalSign(string address, string message)
        {
            EnsureNotDisconnected();

            if (!message.IsHex())
            {
                /*var rawMessage = Encoding.UTF8.GetBytes(message);
    
                var byteList = new List<byte>();
                var bytePrefix = "0x19".HexToByteArray();
                var textBytePrefix = Encoding.UTF8.GetBytes("Ethereum Signed Message:\n" + rawMessage.Length);
    
                byteList.AddRange(bytePrefix);
                byteList.AddRange(textBytePrefix);
                byteList.AddRange(rawMessage);
    
                var hash = new Sha3Keccack().CalculateHash(byteList.ToArray());*/

                message = "0x" + Encoding.UTF8.GetBytes(message).ToHex();
            }

            var request = new EthPersonalSign(address, message);

            var response = await Send<EthPersonalSign, EthResponse>(request);

            return response.Result;
        }

        public virtual async Task<string> EthSignTypedData<T>(string address, T data, EIP712Domain eip712Domain)
        {
            EnsureNotDisconnected();

            var request = new EthSignTypedData<T>(address, data, eip712Domain);

            var response = await Send<EthSignTypedData<T>, EthResponse>(request);

            return response.Result;
        }

        public virtual async Task<string> EthSendTransaction(params TransactionData[] transaction)
        {
            EnsureNotDisconnected();

            var request = new EthSendTransaction(transaction);

            var response = await Send<EthSendTransaction, EthResponse>(request);

            return response.Result;
        }

        public virtual async Task<string> EthSignTransaction(params TransactionData[] transaction)
        {
            EnsureNotDisconnected();

            var request = new EthSignTransaction(transaction);

            var response = await Send<EthSignTransaction, EthResponse>(request);

            return response.Result;
        }

        public virtual async Task<string> EthSendRawTransaction(string data, Encoding messageEncoding = null)
        {
            EnsureNotDisconnected();

            if (!data.IsHex())
            {
                var encoding = messageEncoding;
                if (encoding == null)
                {
                    encoding = Encoding.UTF8;
                }

                data = "0x" + encoding.GetBytes(data).ToHex();
            }

            var request = new EthSendRawTransaction(data);

            var response = await Send<EthSendRawTransaction, EthResponse>(request);

            return response.Result;
        }

        public override async Task SendRequest<T>(T requestObject, string sendingTopic = null, bool? forcePushNotification = null)
        {
            await base.SendRequest(requestObject, sendingTopic, forcePushNotification);

            if (OnSend != null)
            {
                OnSend(this, this);
            }
        }

        public override Task<R> Send<T, R>(T data)
        {
            EnsureNotDisconnected();
            return base.Send<T, R>(data);
        }

        /// <summary>
        /// Create a new WalletConnect session with a Wallet.
        /// </summary>
        /// <returns></returns>
        protected async Task<WCSessionData> CreateSession()
        {
            EnsureNotDisconnected();

            var data = new WcSessionRequest(DappMetadata, clientId, ChainId);

            this._handshakeId = data.ID;

            //Debug.Log("[WalletConnect] Sending Session to topic " + _handshakeTopic);

            await SendRequest(data, this._handshakeTopic);

            SessionUsed = true;

            TaskCompletionSource<WCSessionData> eventCompleted = new TaskCompletionSource<WCSessionData>(TaskCreationOptions.None);

            //Listen for the _handshakeId response
            //The response will be of type WCSessionRequestResponse
            Events.ListenForResponse<WCSessionRequestResponse>(this._handshakeId, async (sender, @event) => await HandleSessionResponse(sender, @event));

            //Listen for wc_sessionUpdate requests
            Events.ListenFor(WalletConnectStates.SessionUpdate, async (object sender, GenericEvent<WCSessionUpdate> @event) => await HandleSessionUpdate(@event.Response.parameters[0]));

            //Listen for the "connect" event triggered by 'HandleSessionResponse' above
            //This will have the type WCSessionData
            Events.ListenFor<WCSessionData>(
                WalletConnectStates.Connect,
                (sender, @event) =>
                {
                    eventCompleted.TrySetResult(@event.Response);
                }
            );

            //Listen for the "session_failed" event triggered by 'HandleSessionResponse' above
            //This will have the type failure reason
            Events.ListenFor(
                WalletConnectStates.SessionFailed,
                delegate(object sender, GenericEvent<ErrorResponse> @event)
                {
                    if (@event.Response.Message is "Not Approved" or "Session Rejected")
                    {
                        eventCompleted.TrySetCanceled();
                    }
                    else
                    {
                        eventCompleted.TrySetException(new IOException("WalletConnect: Session Failed: " + @event.Response.Message));
                    }
                }
            );

            ReadyForUserPrompt = true;

            var response = await eventCompleted.Task;

            ReadyForUserPrompt = false;

            return response;
        }

        protected async Task HandleSessionResponse(object sender, JsonRpcResponseEvent<WCSessionRequestResponse> jsonresponse)
        {
            var response = jsonresponse.Response.result;

            if (response != null && response.approved)
            {
                await HandleSessionUpdate(response);
            }
            else if (jsonresponse.Response.IsError)
            {
                await HandleSessionDisconnect(jsonresponse.Response.Error.Message, WalletConnectStates.SessionFailed);
            }
            else
            {
                await HandleSessionDisconnect("Not Approved", WalletConnectStates.SessionFailed);
            }
        }

        protected async Task HandleSessionUpdate(WCSessionData data)
        {
            if (data == null)
                return;

            bool wasConnected = SessionConnected;

            //We are connected if we are approved
            SessionConnected = data.approved;

            if (data.chainId != null)
                ChainId = (int)data.chainId;

            if (data.networkId != null)
                NetworkId = (int)data.networkId;

            Accounts = data.accounts;

            if (!wasConnected)
            {
                PeerId = data.peerId;

                WalletMetadata = data.peerMeta;

                Events.Trigger(WalletConnectStates.Connect, data);
            }
            else if (wasConnected && !SessionConnected)
            {
                await HandleSessionDisconnect("Wallet Disconnected");
            }

            if (SessionUpdate != null)
                SessionUpdate(this, data);
        }

        protected async Task HandleSessionDisconnect(string msg, string topic = "", bool createNewSession = true)
        {
            var broadcastTopic = string.IsNullOrWhiteSpace(topic) ? WalletConnectStates.Disconnect : topic;

            SessionConnected = false;
            Disconnected = true;

            Events.Trigger(broadcastTopic, new ErrorResponse(msg));

            if (TransportConnected)
            {
                await DisconnectTransport();
            }

            _activeTopics.Clear();

            Events.Clear();

            if (OnSessionDisconnect != null)
                OnSessionDisconnect(this, EventArgs.Empty);
        }

        /// <summary>
        /// Creates and returns a serializable class that holds all session data required to resume later
        /// </summary>
        /// <returns></returns>
        public virtual SavedSession SaveSession()
        {
            if (!SessionConnected || Disconnected)
            {
                return null;
            }

            return new SavedSession(clientId, _handshakeId, _bridgeUrl, _key, _keyRaw, PeerId, NetworkId, Accounts, ChainId, DappMetadata, WalletMetadata);
        }

        /// <summary>
        /// Save the current session to a Stream. This function will write a GZIP Compressed JSON blob
        /// of the contents of SaveSession()
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <param name="leaveStreamOpen">Whether to leave the stream open</param>
        /// <exception cref="IOException">If there is currently no session active, or if writing to the stream fails</exception>
        public void SaveSession(Stream stream, bool leaveStreamOpen = true)
        {
            //We'll save the current session as a GZIP compressed JSON blob
            var data = SaveSession();

            if (data == null)
            {
                throw new IOException("No session is active to save");
            }

            var json = JsonConvert.SerializeObject(data);

            byte[] encodedJson = Encoding.UTF8.GetBytes(json);

            using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress, leaveStreamOpen))
            {
                byte[] sizeEncoded = BitConverter.GetBytes(encodedJson.Length);

                gZipStream.Write(sizeEncoded, 0, sizeEncoded.Length);
                gZipStream.Write(encodedJson, 0, encodedJson.Length);
            }
        }

        /// <summary>
        /// Reads a GZIP Compressed JSON blob of a SavedSession object from a given Stream. This is the reverse of
        /// SaveSession(Stream)
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <param name="leaveStreamOpen">Whether to leave the stream open</param>
        /// <exception cref="IOException">If reading from the stream fails</exception>
        /// <returns>A SavedSession object</returns>
        public static SavedSession ReadSession(Stream stream, bool leaveStreamOpen = true)
        {
            string json;
            using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress, leaveStreamOpen))
            {
                byte[] sizeEncoded = new byte[4];

                gZipStream.Read(sizeEncoded, 0, 4);

                int size = BitConverter.ToInt32(sizeEncoded, 0);

                byte[] jsonEncoded = new byte[size];

                gZipStream.Read(jsonEncoded, 0, size);

                json = Encoding.UTF8.GetString(jsonEncoded);
            }

            return JsonConvert.DeserializeObject<SavedSession>(json);
        }

        protected override void DisposeManaged()
        {
            Events.Clear();
            SessionConnected = false;
            Disconnected = true;

            if (TransportConnected)
            {
                DisconnectTransport().Wait();
            }

            base.DisposeManaged();
        }
    }
}
