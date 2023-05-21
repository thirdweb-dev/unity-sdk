using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using MetaMask.Logging;
using MetaMask.Models;
using MetaMask.Sockets;
using MetaMask.Transports;

using Newtonsoft.Json;

namespace MetaMask
{

    /// <summary>
    /// The main interface to interact with the MetaMask wallet.
    /// </summary>
    public class MetaMaskWallet : IDisposable
    {

        #region Events

        /// <summary>Raised when the wallet is ready.</summary>
        public event EventHandler WalletReady;
        /// <summary>Raised when the wallet is paused.</summary>
        public event EventHandler WalletPaused;
        /// <summary>Occurs when a wallet is connected.</summary>
        public event EventHandler WalletConnected;
        /// <summary>Occurs when a wallet is disconnected.</summary>
        public event EventHandler WalletDisconnected;
        /// <summary>Occurs when the chain ID is changed.</summary>
        public event EventHandler ChainIdChanged;
        /// <summary>Occurs when the account is changed.</summary>
        public event EventHandler AccountChanged;
        /// <summary>Occurs when the wallet connection is authorized by the user.</summary>
        public event EventHandler WalletAuthorized;
        /// <summary>Occurs when the wallet connection is unauthorized by the user.</summary>
        public event EventHandler WalletUnauthorized;
        /// <summary>Occurs when the Ethereum request's response received.</summary>
        public event EventHandler<MetaMaskEthereumRequestResultEventArgs> EthereumRequestResultReceived;
        /// <summary>Occurs when the Ethereum request has failed.</summary>
        public event EventHandler<MetaMaskEthereumRequestFailedEventArgs> EthereumRequestFailed;

        #endregion

        #region Constants

        /// <summary>The URL of the MetaMask app.</summary>
        public const string MetaMaskAppLinkUrl = "https://metamask.app.link";

        /// <summary>The URL of the socket.io server.</summary>
        public const string SocketUrl = "https://socket.codefi.network";

        /// <summary>The name of the event that is fired when a message is received.</summary>
        protected const string MessageEventName = "message";
        /// <summary>The name of the event that is raised when a user joins a channel.</summary>       
        protected const string JoinChannelEventName = "join_channel";
        /// <summary>The name of the event that is fired when the user leaves a channel.</summary>       
        protected const string LeaveChannelEventName = "leave_channel";
        /// <summary>The name of the event that is fired when a client connects.</summary>
        protected const string ClientsConnectedEventName = "clients_connected";
        /// <summary>The name of the event that is raised when clients are disconnected.</summary>
        protected const string ClientsDisconnectedEventName = "clients_disconnected";
        /// <summary>The name of the event that is raised when clients are waiting to join.</summary>
        protected const string ClientsWaitingToJoinEventName = "clients_waiting_to_join";

        protected const string TrackingEventRequest = "sdk_connect_request_started";
        protected const string TrackingEventConnected = "sdk_connection_established";
        protected const string TrackingEventDisconnected = "sdk_disconnected";

        #endregion

        #region Fields
        /// <summary>List of methods that should be redirected.</summary>
        protected static List<string> MethodsToRedirect = new List<string>() {
            "eth_sendTransaction",
            "eth_signTransaction",
            "eth_sign",
            "personal_sign",
            "eth_signTypedData",
            "eth_signTypedData_v3",
            "eth_signTypedData_v4",
            "wallet_watchAsset",
            "wallet_addEthereumChain",
            "wallet_switchEthereumChain"
        };
        /// <summary>The users wallet session.</summary>
        protected MetaMaskSession session;
        /// <summary>The transport used in the wallet session.</summary>
        protected IMetaMaskTransport transport;
        /// <summary>The socket connection used in the user wallet session.</summary>
        protected IMetaMaskSocketWrapper socket;
        /// <summary>The socket connection url.</summary>
        protected string socketUrl;

        /// <summary>Indicates whether the keys have been exchanged.</summary>
        /// <returns>True if the keys have been exchanged; otherwise, false.</returns>
        protected bool keysExchanged = false;
        /// <summary>The public key of the wallet.</summary>
        protected string walletPublicKey = string.Empty;
        /// <summary>Gets or sets the selected address.</summary>
        protected string selectedAddress = string.Empty;
        /// <summary>The ID of the chain that is currently selected.</summary>
        protected string selectedChainId = string.Empty;

        /// <summary>Indicates whether the application is connected to the Internet.</summary>
        /// <returns>True if the application is connected to the Internet; otherwise, false.</returns>
        protected bool connected = false;
        /// <summary>Gets or sets a value indicating whether the application is paused.</summary>
        /// <value>true if the application is paused; otherwise, false.</value>
        protected bool paused = false;

        protected bool authorized = false;

        protected TaskCompletionSource<JsonElement> connectionTcs;

        /// <summary>The Socket URL</summary>
        protected string connectionUrl;

        /// <summary>Submitted requests dictionary.</summary>
        protected Dictionary<string, MetaMaskSubmittedRequest> submittedRequests = new Dictionary<string, MetaMaskSubmittedRequest>();

        protected string analyticsPlatform = "unknown";

        #endregion

        #region Properties

        /// <summary>The MetaMask session.</summary>
        public MetaMaskSession Session => this.session;

        /// <summary>Gets or sets the transport used to send and receive data.</summary>
        /// <value>The transport used to send and receive data.</value>
        public IMetaMaskTransport Transport
        {
            get => this.transport;
            set => this.transport = value;
        }

        /// <summary>Gets or sets the socket.</summary>
        /// <value>The socket.</value>
        public IMetaMaskSocketWrapper Socket
        {
            get => this.socket;
            set => this.socket = value;
        }

        /// <summary>Gets the currently selected address.</summary>
        /// <returns>The currently selected address.</returns>
        public string SelectedAddress => this.selectedAddress;

        /// <summary>Gets the ID of the currently selected chain.</summary>
        /// <returns>The ID of the currently selected chain.</returns>
        public string SelectedChainId => this.selectedChainId;

        /// <summary>Gets the public key of the wallet.</summary>
        /// <returns>The public key of the wallet.</returns>
        public string WalletPublicKey => this.walletPublicKey;

        /// <summary>Gets a value indicating whether the client is connected to the server.</summary>
        /// <returns>true if the client is connected to the server; otherwise, false.</returns>
        public bool IsConnected => this.connected;

        /// <summary>Gets a value indicating whether the application is paused.</summary>
        /// <returns>true if the application is paused; otherwise, false.</returns>
        public bool IsPaused => this.paused;

        public bool IsAuthorized => this.authorized;

        /// <summary>Gets or sets the analytics platform.</summary>
        public string AnalyticsPlatform
        {
            get
            {
                return this.analyticsPlatform;
            }
            set
            {
                this.analyticsPlatform = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>Creates a new instance of the MetaMaskWallet class.</summary>
        /// <param name="session">The MetaMask session.</param>
        /// <param name="transport">The MetaMask transport.</param>
        /// <param name="socket">The MetaMask socket.</param>
        /// <param name="socketUrl">The MetaMask socket URL.</param>
        public MetaMaskWallet(MetaMaskSession session, IMetaMaskTransport transport, IMetaMaskSocketWrapper socket, string socketUrl = SocketUrl)
        {
            this.session = session;
            this.transport = transport;
            this.socket = socket;
            this.socketUrl = socketUrl;

            this.socket.Connected += OnSocketConnected;
            this.socket.Disconnected += OnSocketDisconnected;
        }

        #endregion

        #region Protected Methods

        /// <summary>Sends a message to the other party.</summary>
        /// <param name="data">The data to send.</param>
        /// <param name="encrypt">Whether to encrypt the data.</param>
        protected void SendMessage(object data, bool encrypt)
        {
            var message = this.session.PrepareMessage(data, encrypt, this.walletPublicKey);
            if (this.paused)
            {
                MetaMaskDebug.Log("Queuing message");
                void SendMessageWhenReady(object sender, EventArgs e)
                {
                    this.socket.Emit(MessageEventName, message);
                    WalletReady -= SendMessageWhenReady;
                }
                WalletReady += SendMessageWhenReady;
            }
            else
            {
                MetaMaskDebug.Log("Sending message");
                this.socket.Emit(MessageEventName, message);
            }
        }

        /// <summary>Sends analytics data to Socket.io server.</summary>
        /// <param name="analyticsInfo">JSON string with parameters</param>
        public async void SendAnalytics(MetaMaskAnalyticsInfo analyticsInfo)
        {
            string jsonString = JsonConvert.SerializeObject(analyticsInfo);
            MetaMaskDebug.Log("Sending Analytics: " + jsonString);

            var response = await this.socket.SendWebRequest(this.socketUrl.EndsWith("/") ? this.socketUrl + "debug" : this.socketUrl + "/debug", jsonString, new Dictionary<string, string> { { "Content-Type", "application/json" } });
            if (response.IsSuccessful)
            {
                MetaMaskDebug.Log("Analytics sent successfully!");
            }
            else
            {
                MetaMaskDebug.LogWarning("Sending analytics has failed:");
                MetaMaskDebug.LogWarning(response.Response);
                MetaMaskDebug.LogWarning(response.Error);
            }
        }

        /// <summary>Sends the originator information to the clipboard.</summary>
        protected void SendOriginatorInfo()
        {
            var originatorInfo = new MetaMaskOriginatorInfo
            {
                Title = this.session.Data.AppName,
                Url = this.session.Data.AppUrl,
                Platform = this.analyticsPlatform
            };
            var requestInfo = new MetaMaskRequestInfo
            {
                Type = "originator_info",
                OriginatorInfo = originatorInfo
            };
            SendMessage(requestInfo, true);

            var analyticsInfo = new MetaMaskAnalyticsInfo
            {
                Id = this.session.Data.ChannelId,
                Event = TrackingEventRequest,
                CommunicationLayerPreference = "socket",
                SdkVersion = "0.2.0",
                OriginatorInfo = originatorInfo
            };
            SendAnalytics(analyticsInfo);
        }

        /// <summary>Called when the wallet is paused.</summary>
        protected void OnWalletPaused()
        {
            MetaMaskDebug.Log("Wallet Paused");
            this.paused = true;

            WalletPaused?.Invoke(this, null);
        }

        /// <summary>Called when the wallet is ready.</summary>
        protected void OnWalletReady()
        {
            MetaMaskDebug.Log("Wallet Ready");
            this.paused = false;

            InitializeState();

            this.connectionTcs = new TaskCompletionSource<JsonElement>();
            var request = new MetaMaskEthereumRequest
            {
                Method = "eth_requestAccounts",
                Parameters = new string[] { }
            };
            string id = Guid.NewGuid().ToString();

            var submittedRequest = new MetaMaskSubmittedRequest
            {
                Method = request.Method,
                Promise = this.connectionTcs
            };

            this.submittedRequests.Add(id, submittedRequest);
            SendEthereumRequest(id, request, false);

            WalletReady?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Called when the wallet is connected.</summary>
        protected void OnWalletConnected()
        {
            MetaMaskDebug.Log("Wallet information retrieved");
            this.connected = true;
            this.paused = false;

            InitializeState();

            this.connectionTcs = new TaskCompletionSource<JsonElement>();
            var request = new MetaMaskEthereumRequest
            {
                Method = "eth_requestAccounts",
                Parameters = new string[] { }
            };
            string id = Guid.NewGuid().ToString();

            var submittedRequest = new MetaMaskSubmittedRequest
            {
                Method = request.Method,
                Promise = this.connectionTcs
            };

            this.submittedRequests.Add(id, submittedRequest);
            SendEthereumRequest(id, request, false);

            WalletConnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Initialize the wallet state.</summary>
        protected void InitializeState()
        {
            var request = new MetaMaskEthereumRequest
            {
                Method = "metamask_getProviderState",
                Parameters = new string[] { }
            };
            Request(request);
        }

        /// <summary>Raised when the socket is connected.</summary>
        protected void OnSocketConnected(object sender, EventArgs e)
        {
            string channelId = this.session.Data.ChannelId;
            MetaMaskDebug.Log("Socket connected");
            MetaMaskDebug.Log("Channel ID: " + channelId);
            MetaMaskDebug.Log($"{MessageEventName}-{channelId}");

            // Listen for messages using the channel
            this.socket.On(MessageEventName, OnMessageReceived);
            this.socket.On($"{MessageEventName}-{channelId}", OnMessageReceived);
            this.socket.On($"{ClientsConnectedEventName}-{channelId}", OnClientsConnected);
            this.socket.On($"{ClientsDisconnectedEventName}-{channelId}", OnClientsDisconnected);
            this.socket.On($"{ClientsWaitingToJoinEventName}-{channelId}", OnClientsWaitingToJoin);

            // Join the channel
            JoinChannel(channelId);
        }

        private void OnSocketDisconnected(object sender, EventArgs e)
        {
        }

        protected void JoinChannel(string channelId)
        {
            MetaMaskDebug.Log("Joining channel");
            this.socket.Emit(JoinChannelEventName, channelId);
        }

        protected void LeaveChannel(string channelId)
        {
            MetaMaskDebug.Log("Leaving channel");
            this.socket.Emit(LeaveChannelEventName, channelId);
        }

        /// <summary>Called when a message is received.</summary>
        /// <param name="response">The response from the background task.</param>
        protected void OnMessageReceived(string response)
        {
            MetaMaskDebug.Log("Message received");
            MetaMaskDebug.Log(response);

            var document = JsonDocument.Parse(response);
            JsonElement value;
            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                value = document.RootElement[0];
            }
            else
            {
                value = document.RootElement;
            }
            var message = value.GetProperty("message");
            string messageType = string.Empty;
            if (message.ValueKind == JsonValueKind.Object && message.TryGetProperty("type", out var messageTypeProperty))
            {
                messageType = messageTypeProperty.ToString();
            }

            // Key exchange & handshake
            if (!this.keysExchanged)
            {
                if (messageType == "key_handshake_SYNACK")
                {
                    MetaMaskDebug.Log("Wallet public key");
                    this.walletPublicKey = message.GetProperty("pubkey").GetString();
                    MetaMaskDebug.Log(this.walletPublicKey);
                    var keyExchangeACK = new MetaMaskKeyExchangeMessage("key_handshake_ACK", this.session.PublicKey);

                    SendMessage(keyExchangeACK, false);

                    this.keysExchanged = true;
                    SendOriginatorInfo();
                }
            }
            else if (this.paused && message.ValueKind != JsonValueKind.String && messageType == "key_handshake_start")
            {
                this.keysExchanged = false;
                this.paused = false;
                this.connected = false;
                var keyExchangeSYN = new MetaMaskKeyExchangeMessage("key_handshake_SYN", this.session.PublicKey);

                SendMessage(keyExchangeSYN, false);
            }
            else
            {
                MetaMaskDebug.Log("Encrypted message received");
                var decryptedJson = this.session.DecryptMessage(message.ToString());
                MetaMaskDebug.Log(decryptedJson);
                var decryptedMessage = JsonDocument.Parse(decryptedJson).RootElement;
                var decryptedMessageType = decryptedMessage.TryGetProperty("type", out var type) ? type.ToString() : string.Empty;

                if (decryptedMessageType == "pause")
                {
                    OnWalletPaused();
                    return;
                }
                else if (decryptedMessageType == "ready")
                {
                    OnWalletReady();
                    return;
                }
                if (!this.connected)
                {
                    if (decryptedMessageType == "wallet_info")
                    {
                        OnWalletConnected();
                        return;
                    }
                }

                if (decryptedMessage.TryGetProperty("data", out var data))
                {
                    if (data.TryGetProperty("id", out var id))
                    {
                        OnEthereumRequestReceived(id.ToString(), data);
                    }
                    else
                    {
                        OnEthereumEventReceived(data);
                    }
                }
                else
                {
                    if (decryptedMessage.TryGetProperty("walletinfo", out var walletinfo))
                    {
                        OnEthereumEventReceived(walletinfo);
                    }
                }
            }
        }

        /// <summary>Called when the clients are waiting to join.</summary>
        /// <param name="response">The response sent by the server.</param>
        protected void OnClientsWaitingToJoin(string response)
        {
            MetaMaskDebug.Log("Clients waiting to join");
            transport.OnConnectRequest(connectionUrl);
        }

        /// <summary>Called when the server sends a response to the client's connection request.</summary>
        /// <param name="response">The response sent by the server.</param>
        protected void OnClientsConnected(string response)
        {
            MetaMaskDebug.Log("Clients connected");

            if (!this.keysExchanged)
            {
                MetaMaskDebug.Log("Exchanging keys");
                var keyExchangeSYN = new MetaMaskKeyExchangeMessage("key_handshake_SYN", this.session.PublicKey);
                SendMessage(keyExchangeSYN, false);
            }
        }

        /// <summary>Called when the server sends a response to the client's disconnection request.</summary>
        /// <param name="response">The response sent by the server.</param>
        protected void OnClientsDisconnected(string response)
        {
            MetaMaskDebug.Log("Clients disconnected");

            if (!this.paused)
            {
                this.connected = false;
                this.keysExchanged = false;

                // TODO: Reset session

                Disconnect();
            }
        }

        protected void OnWalletAuthorized()
        {
            if (!this.authorized)
            {
                this.authorized = true;
                WalletAuthorized?.Invoke(this, EventArgs.Empty);
            }
        }

        protected void OnWalletUnauthorized()
        {
            this.authorized = false;
            WalletUnauthorized?.Invoke(this, EventArgs.Empty);
            Disconnect();
        }

        /// <summary>Raised when an Ethereum request is received.</summary>
        /// <param name="id">The request ID.</param>
        /// <param name="data">The request data.</param>
        protected void OnEthereumRequestReceived(string id, JsonElement data)
        {
            var request = this.submittedRequests[id];

            // The request has failed with an error
            if (data.TryGetProperty("error", out var error))
            {
                switch (request.Method)
                {
                    case "eth_requestAccounts":
                        OnWalletUnauthorized();
                        break;
                }

                var ex = new Exception(error.ToString());
                this.transport.OnFailure(ex);
                request.Promise.SetException(ex);
                EthereumRequestFailed?.Invoke(this, new MetaMaskEthereumRequestFailedEventArgs(request, error));
            }

            // The request has been successful
            else if (data.TryGetProperty("result", out var result))
            {
                switch (request.Method)
                {
                    case "metamask_getProviderState":
                        OnAccountsChanged(result.GetProperty("accounts"));
                        OnChainIdChanged(result.GetProperty("chainId").ToString());
                        break;
                    case "eth_requestAccounts":
                        OnWalletAuthorized();
                        OnAccountsChanged(result);
                        break;
                    case "eth_chainId":
                        OnChainIdChanged(result.ToString());
                        break;
                }
                request.Promise.SetResult(result);
                EthereumRequestResultReceived?.Invoke(this, new MetaMaskEthereumRequestResultEventArgs(request, data));
            }
        }

        /// <summary>Handles the event that is fired when an Ethereum event is received.</summary>
        /// <param name="data">The event data.</param>
        protected void OnEthereumEventReceived(JsonElement data)
        {
            var method = data.GetProperty("method").ToString();
            var @params = data.GetProperty("params");
            switch (method)
            {
                case "metamask_accountsChanged":
                    OnAccountsChanged(@params);
                    break;
                case "metamask_chainChanged":
                    OnChainIdChanged(@params.GetProperty("chainId").ToString());
                    break;
            }
        }

        /// <summary>Handles the event that is fired when an Account changed event is received.</summary>
        /// <param name="data">The event data.</param>
        protected void OnAccountsChanged(JsonElement accounts)
        {
            MetaMaskDebug.Log("Account changed");
            try
            {
                this.selectedAddress = accounts[0].ToString();
                AccountChanged?.Invoke(this, EventArgs.Empty);
                if (this.paused)
                {
                    OnWalletReady();
                }
            }
            catch
            {
                this.selectedAddress = string.Empty;
            }
        }

        /// <summary>Handles the event that is fired when an Chain ID changed event is received.</summary>
        /// <param name="data">The event data.</param>
        protected void OnChainIdChanged(string newChainId)
        {
            MetaMaskDebug.Log("Chain ID changed");
            this.selectedChainId = newChainId;
            ChainIdChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Sends an Ethereum request to the MetaMask server.</summary>
        /// <param name="id">The request ID.</param>
        /// <param name="request">The request to send.</param>
        /// <param name="openTransport">Whether to open the transport if it isn't already open.</param>
        protected void SendEthereumRequest(string id, MetaMaskEthereumRequest request, bool openTransport)
        {
            request.Id = id;
            MetaMaskDebug.Log("Sending a new request");
            MetaMaskDebug.Log(JsonConvert.SerializeObject(request));

            SendMessage(request, true);

            if (openTransport)
            {
                try
                {
                    this.transport.OnRequest(id, request);
                }
                catch
                {
                    // Ignore the exception as some transport may not implement this method
                }
            }
        }

        /// <summary>Determines whether the specified method should open the MetaMask app.</summary>
        /// <param name="method">The method to check.</param>
        /// <returns>true if the method should open the MetaMask app; otherwise, false.</returns>
        protected bool ShouldOpenMM(string method)
        {

            // Only open the wallet for requesting accounts when the address is not already provided.
            if (method == "eth_requestAccounts" && string.IsNullOrEmpty(this.selectedAddress))
            {
                return true;
            }

            return MethodsToRedirect.Contains(method);
        }

        #endregion

        #region Public Methods

        /// <summary>Sends a request to the MetaMask server.</summary>
        /// <param name="request">The request to send.</param>
        /// <returns>The response from the server.</returns>
        public Task<JsonElement> Request(MetaMaskEthereumRequest request)
        {
            if (request.Method == "eth_requestAccounts" && !this.connected)
            {
                if (this.connectionTcs == null || this.connectionTcs.Task.IsCompleted || (this.connectionTcs.Task.IsCompleted && !this.connected))
                {
                    Connect();
                }
                return this.connectionTcs.Task;
            }
            else if (!this.connected)
            {
                throw new Exception("MetaMask Wallet is not connected.");
            }
            else
            {
                var tcs = new TaskCompletionSource<JsonElement>();
                var id = Guid.NewGuid().ToString();
                var submittedRequest = new MetaMaskSubmittedRequest()
                {
                    Method = request.Method,
                    Promise = tcs
                };
                this.submittedRequests.Add(id, submittedRequest);
                SendEthereumRequest(id, request, ShouldOpenMM(request.Method));
                return tcs.Task;
            }
        }

        /// <summary>Connects to the server.</summary>
        public void Connect()
        {
            MetaMaskDebug.Log("Connecting...");
            this.connectionTcs = new TaskCompletionSource<JsonElement>();

            // Initialize the socket
            this.socket.Initialize(this.socketUrl, new MetaMaskSocketOptions()
            {
                ExtraHeaders = new Dictionary<string, string>
                {
                    {"User-Agent", this.transport.UserAgent}
                }
            });
            this.socket.ConnectAsync();

            this.session.Data.ChannelId = Guid.NewGuid().ToString();
            string channelId = this.session.Data.ChannelId;

            // Open the transport for connection
            this.connectionUrl = MetaMaskAppLinkUrl + "/connect?channelId=" + Uri.EscapeDataString(channelId) + "&pubkey=" + Uri.EscapeDataString(this.session.PublicKey);
            try
            {
                this.transport.Connect(connectionUrl);
            }
            catch (Exception exception)
            {
                MetaMaskDebug.LogError("Opening transport for connection has failed");
                MetaMaskDebug.LogException(exception);
            }
        }

        /// <summary>Disconnects the client from the server.</summary>
        public void Disconnect()
        {
            MetaMaskDebug.Log("Disconnected");

            this.connected = false;
            this.connectionTcs = null;

            // Force reauthorization
            this.authorized = false;
            this.paused = false;
            this.keysExchanged = false;

            this.walletPublicKey = string.Empty;
            this.selectedAddress = string.Empty;
            this.selectedChainId = string.Empty;

            this.socket.DisconnectAsync();
            WalletDisconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Disposes and resets the wallet client when a user is disconnected.</summary>
        public void Dispose()
        {
            string channelId = this.session.Data.ChannelId;

            // Leave the channel
            LeaveChannel(channelId);

            Disconnect();
            this.socket.Dispose();
        }

        #endregion

    }

    public class MetaMaskEthereumRequestResultEventArgs : EventArgs
    {
        public readonly MetaMaskSubmittedRequest Request;
        public readonly JsonElement Result;
        public readonly string TransactionHash;

        /// <summary>Initializes a new instance of the <see cref="MetaMaskEthereumRequestResultEventArgs"/> class.</summary>
        /// <param name="request">The initial Ethereum request.</param>
        /// <param name="result">The request's result.</param>
        public MetaMaskEthereumRequestResultEventArgs(MetaMaskSubmittedRequest request, JsonElement result)
        {
            this.Request = request;
            this.Result = result;
            this.TransactionHash = result.ValueKind == JsonValueKind.String ? result.ToString() : string.Empty;
        }
    }

    public class MetaMaskEthereumRequestFailedEventArgs : EventArgs
    {
        public readonly MetaMaskSubmittedRequest Request;
        public readonly JsonElement Error;

        /// <summary>Initializes a new instance of the <see cref="MetaMaskEthereumRequestFailedEventArgs"/> class.</summary>
        /// <param name="request">The initial Ethereum request.</param>
        /// <param name="error">The request's result.</param>
        public MetaMaskEthereumRequestFailedEventArgs(MetaMaskSubmittedRequest request, JsonElement error)
        {
            this.Request = request;
            this.Error = error;

        }
    }
}