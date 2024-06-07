using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EventEmitter.NET;
using evm.net;
using evm.net.Models;
using MetaMask.Contracts;
using MetaMask.Cryptography;
using MetaMask.IO;
using MetaMask.Editor.NaughtyAttributes;
using MetaMask.Logging;
using MetaMask.Models;
using MetaMask.Providers;
using MetaMask.Scripts.Utilities;
using MetaMask.SocketIOClient;
using MetaMask.Sockets;
using MetaMask.Transports;
using MetaMask.Transports.Unity;
using MetaMask.Transports.Unity.UI;
using MetaMask.Unity.Models;
using MetaMask.Unity.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace MetaMask.Unity
{
    [RequireComponent(typeof(MetaMaskUnityEventHandler))]
    [RequireComponent(typeof(MetaMaskHttpService))]
    public class MetaMaskSDK : BindableMonoBehavior, IMetaMaskSDK, IAppConfig, IMetaMaskTransport
    {
        
        #region SDK Static API (Singleton + IsMobile + Log)

        public static IMetaMaskSDK Instance => FindSDK(true);

        public static MetaMaskSDK SDKInstance => FindSDK(false) as MetaMaskSDK;

        private static IMetaMaskSDK FindSDK(bool checkLegacy)
        {
            var sdk = FindObjectOfType<MetaMaskSDK>();

            if (sdk != null) return sdk;
                
            if (checkLegacy)
            {
                // Try MetaMaskUnity
                var sdk2 = FindObjectOfType<MetaMaskUnity>();
                if (sdk2 != null) return sdk2;
            }

            MetaMaskDebug.LogError("No MetaMaskSDK instance found! Please setup a MetaMaskSDK prefab in your scene.");
            return null;
        }
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void OpenMetaMaskDeeplink(string url);
        
        [DllImport("__Internal")]
        public static extern bool WebGLIsMobile();
#endif
        
        public static void OpenDeeplinkURL(string url)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OpenMetaMaskDeeplink(url);
#else
            Application.OpenURL(url);
#endif
        }

        public static bool IsMobile
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                    return WebGLIsMobile();
#else
                return Application.isMobilePlatform;
#endif
            }
        }

        private static bool _enableLogging;

        public static bool EnableLogging
        {
            get
            {
                return _enableLogging;
            }
            set
            {
                _enableLogging = value;
                MetaMaskUnityLogger.Instance.UpdateLogFilter();
            }
        }
        
        #endregion
        
        #region SDK Inspector Options
        [Header("Dapp Metadata")]
        [HorizontalLine]
        [InfoBox("The following settings are required to properly identify your game inside MetaMask." +
                 "Icons provided must be 32x32 or smaller.")]
        [Tooltip("Dapp Name - The name of the dapp the user is connecting to.")]
        [Required]
        public string dappName;

        [Tooltip("Dapp URL - The url of the dapp the user is connecting to. For example, mygame.io")]
        [Required]
        public string dappUrl;

        [Tooltip("Dapp Icon URL - The url of the dapp's icon. For example, mygame.io/icon.png. Supported formats are:" +
                 "png, svg")]
        public string dappIconUrl;

        [Tooltip("Dapp Icon - A icon texture for dapp. The max size is 32x32")]
        public Texture2D dappIcon;

        [Header("UI Settings")]
        [HorizontalLine]
        [InfoBox("The following settings control what UI canvas is shown when prompting the user to connect.")]
        [Tooltip(
            "Whether the provided canvas needs to be created at connection time, or if it'll already exist in the scene")]
        public bool spawnCanvas = true;

        [Tooltip("The canvas to use for prompting the user to connect")]
        [EnableIf(nameof(spawnCanvas))]
        public GameObject connectionCanvas;
        
        [InfoBox("Whether to use the legacy MetaMaskUnityUITransport ScriptableObject API for UI event handling")]
        public bool useLegacyUITransport = false;
        
        [EnableIf(nameof(useLegacyUITransport))]
        public MetaMaskUnityScriptableObjectTransport uiTransport;


        [Header("Session Settings")]
        [HorizontalLine]
        [InfoBox("The following settings are related to the active session after the user has connected their wallet.")]
        
        public bool useConnectAndSign;

        [InfoBox("The connectAndSign method simplifies mobile dapp integrations with MetaMask by combining " +
                 "connection and message signing into one efficient step, enhancing user experience by minimising app " +
                 "switching on mobile to mobile connections.")]
        [EnableIf(nameof(useConnectAndSign))]
        public string connectAndSignMessage;

        [Serializable]
        public class ConnectAndSignUnityEvent : UnityEvent<string>
        {
        }

        [EnableIf(nameof(useConnectAndSign))]
        public ConnectAndSignUnityEvent onConnectAndSignCompleted;

        [InfoBox("You can use the Infura API from your dapp with MetaMask SDK installed to make direct, " +
                 "read-only JSON-RPC requests that do not require user wallet interaction. Your dapp can " +
                 "directly call most JSON-RPC API methods, bypassing user wallet authentication for read-only " +
                 "operations.")]
        public bool useInfura = true;

        [InfoBox("An Infura projectId is required to use Infura. You can get a projectId from Infura.io")]
        [EnableIf(nameof(useInfura)), Required] public string infuraProjectId;

        /// <summary>
        /// The RPC URL to use for web3 query requests when the MetaMask wallet is paused
        /// </summary>
        [SerializeField]
        internal List<MetaMaskUnityRpcUrlConfig> rpcUrl;

        public bool useDefaultChain;

        [EnableIf(nameof(useDefaultChain))]
        public ChainId defaultChain = ChainId.Other;

        [SerializeField, FormerlySerializedAs("otherDefaultChainData")]
        [EnableIf(nameof(ShouldUseOtherChainData)), Required("Please fill out all required information for the default chain")]
        private UnityChainInfo _otherDefaultChainData;
        
        protected ChainInfo OtherDefaultChainData => _otherDefaultChainData;


        [Header("Persistent Data")]
        [HorizontalLine]
        [InfoBox("When enabled, the session is persisted and the user will not need to reconnect their wallet." +
                 "When disabled, the session is always ended when you disconnect the SDK, requiring the user to reconnect their" +
                 " wallet.")]
        public bool saveSessionState = true;

        [SerializeField] [EnableIf(nameof(saveSessionState))] [Tooltip("Whether to encrypt the persistent data.")]
        internal bool encrypt = true;

        /// <summary>The password used to encrypt the persistent data.</summary>
        [SerializeField]
        [EnableIf(EConditionOperator.And, nameof(saveSessionState), nameof(encrypt))]
        [Tooltip("The password to use when encrypting session data locally.")]
        internal string encryptionPassword = MetaMaskDataManager.RandomString(12);

        [Header("Advanced")]
        [HorizontalLine]
        [InfoBox("The following settings are advanced settings, and usually don't need to be changed.")]
        [Tooltip("Whether or not this gameObject should persist between game scenes")]
        public bool dontDestroyOnLoad = false;
        
        [Tooltip("Whether to turn off the debug logs.")]
        public bool debugLogging = true;

        [SerializeField] [Tooltip("The socket URL to use when connecting via socket-io.")]
        internal string socketUrl = MetaMaskWallet.SocketUrl;

        [Tooltip("User Agent - The user agent to send in SDK requests")]
        public string userAgent = "UnityUGUITransport/1.0.0";

        public bool useUniversalLinks;
        #endregion

        #region SDK Properties
        public ChainInfo DefaultChainInfo
        {
            get
            {
                ChainInfo chainData;
                if (!useInfura)
                {
                    chainData = OtherDefaultChainData ?? throw new ArgumentException(
                        "Please fill out all default chain information or select an Infura preset");
                }
                else
                {
                    switch (defaultChain)
                    {
                        case ChainId.Other:
                            chainData = OtherDefaultChainData;
                            break;
                        default:
                            var effectiveInfuraProjectId = useInfura ? infuraProjectId : null;
                            chainData = ChainInfo.FromEnum(defaultChain, effectiveInfuraProjectId);
                            break;
                    }
                }

                return chainData;
            }
            set
            {
                _otherDefaultChainData = new UnityChainInfo(value);
            }
        }
        public TransportMode ConnectionMode { get; set; }
        public string AppName => this.dappName;
        public string AppUrl => this.dappUrl;

        private string _textureCache;
        private string _textureCacheId;

        public string AppIcon
        {
            get
            {
                if (dappIcon == null)
                    return null;
                
                var currentId = dappIcon.GetInstanceID().ToString();
                
                if (!string.IsNullOrWhiteSpace(_textureCache) && currentId == _textureCacheId)
                    return _textureCache;

                _textureCache = TextureBase64.TextureToBase64(dappIcon);
                _textureCacheId = currentId;

                return _textureCache;
            }
        }
        public string AppIconUrl => this.dappIconUrl;

        public string InfuraProjectId => infuraProjectId;
        public IAppConfig Config => this;
        public string SDKVersion => MetaMaskWallet.Version;

        List<MetaMaskUnityRpcUrlConfig> IMetaMaskSDK.RpcUrl => rpcUrl;

        public event EventHandler MetaMaskUnityBeforeInitialized;
        public event EventHandler MetaMaskUnityInitialized;
        
        public event EventHandler<MetaMaskUnityRequestEventArgs> Requesting;

        public MetaMaskWallet Wallet => _wallet;

        public List<MetaMaskUnityRpcUrlConfig> RpcUrl
        {
            get { return rpcUrl; }
        }

        public IMetaMaskEventsHandler Events => _eventHandler;
        #endregion

        #region SDK Internal State

        protected MetaMaskWallet _wallet;

        protected bool _didInit;
        
        protected Thread unityThread;

        protected MetaMaskDataManager dataManager;

        [Inject]
        protected MetaMaskUnityEventHandler _eventHandler;

        #endregion
        
        #region SDK Transport State
        
        protected GameObject metaMaskCanvasInstance;
        protected MetaMaskUnityUIHandler uiHandler;
        protected string connectionDeepLinkUrl;
        protected string connectionUniversalLinkUrl;

        #endregion
        
        #region Inspector Actions and Config Validation

        protected override void Awake()
        {
            base.Awake();

            if (!dontDestroyOnLoad) return;
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }

        private void OnValidate()
        {
            try
            {
                EnableLogging = debugLogging;
                
                if (dappIcon == null)
                    ValidateStringProp(dappIconUrl, "No Dapp Icon, Dapp Icon URL");

                ValidateStringProp(userAgent, "User Agent");

#pragma warning disable CS0612 // Type or member is obsolete
                // use startsWith to catch any trailing / 
                if (socketUrl.StartsWith(MetaMaskWallet.DeprecatedSocketUrl))
                {
                    MetaMaskDebug.LogWarning($"Upgrading socket url to new socket server: {MetaMaskWallet.SocketUrl}");
                    socketUrl = MetaMaskWallet.SocketUrl;
                }
#pragma warning restore CS0612 // Type or member is obsolete
                
                if (useDefaultChain)
                    ValidateDefaultChain();

#if UNITY_IOS || UNITY_ANDROID
                if (useDefaultChain && useConnectAndSign)
                {
                    MetaMaskDebug.LogWarning("When using Connect & Sign with a default chain on mobile, you may need to ask the user to redirect back to MetaMask to complete the signature. Read more here: https://docs.metamask.io/wallet/how-to/use-sdk/gaming/unity/");
                }
#endif
            }
            catch (Exception e)
            {
                throw new MetaMaskUnityException($"[MetaMask SDK] Validation failed on game object {gameObject.name}",
                    e);
            }
        }

        private void ValidateDefaultChain()
        {
            var chainInfo = OtherDefaultChainData;
            var defaultChainLong = (long)defaultChain;

            var defaultChainString = ChainInfo.ChainIdToHex(defaultChainLong);
            if (defaultChain != ChainId.Other && !string.Equals(chainInfo.ChainId, defaultChainString, StringComparison.CurrentCultureIgnoreCase))
            {
                chainInfo.ChainId = defaultChainString;
                
                // clear the other stuff so we auto-set it
                chainInfo.RpcUrls = null;
                chainInfo.BlockExplorerUrls = null;
                chainInfo.NativeCurrency = null;
                // These will be set below
            }

            if (defaultChain != ChainId.Other)
            {
                var defaultName = Enum.GetName(typeof(ChainId), defaultChain);
                if (!string.IsNullOrWhiteSpace(defaultName) && !string.Equals(chainInfo.ChainName, defaultName, StringComparison.CurrentCultureIgnoreCase))
                {
                    chainInfo.ChainName = defaultName;
                }
            }
            
            if (chainInfo.RpcUrls == null || chainInfo.RpcUrls.Length == 0)
            {
                var chainId = ChainInfo.ChainToId(chainInfo.ChainId);

                Func<bool, Func<MetaMaskUnityRpcUrlConfig, bool>> chainIdChecker = (shouldInvert) => (c) =>
                    shouldInvert != ChainInfo.ChainIdMatch(c.ChainId, chainInfo.ChainId);

                if (useInfura && !string.IsNullOrWhiteSpace(infuraProjectId) && Infura.IsSupported((ChainId)chainId))
                {
                    chainInfo.RpcUrls = new[] { Infura.Url(infuraProjectId, chainId) };
                }
                else
                {
                    if (rpcUrl.Any(chainIdChecker(false)))
                    {
                        chainInfo.RpcUrls = new[] { rpcUrl.FirstOrDefault(chainIdChecker(false))?.RpcUrl };
                    }
                    else if (Blockchains.RpcUrls.TryGetValue(chainId, out var url1))
                    {
                        chainInfo.RpcUrls = url1;
                    }
                    else if (rpcUrl.All(chainIdChecker(true)))
                    {
                        chainInfo.RpcUrls = Array.Empty<string>();
                    }
                }
            }

            if ((chainInfo.BlockExplorerUrls == null || chainInfo.BlockExplorerUrls.Length == 0) && Blockchains.BlockExplorerUrls.TryGetValue(defaultChainLong, out var url))
            {
                chainInfo.BlockExplorerUrls = url;
            }

            if ((chainInfo.NativeCurrency == null || string.IsNullOrWhiteSpace(chainInfo.NativeCurrency.Name)) && Blockchains.NativeCurrencies.TryGetValue(defaultChainLong, out var nativeCurrency))
            {
                chainInfo.NativeCurrency = nativeCurrency;
            }
        }

        private void ValidateStringProp(string prop, string propName)
        {
            if (string.IsNullOrWhiteSpace(prop))
                throw new ArgumentException($"{propName} must be provided");
        }

        private bool ShouldUseOtherChainData()
        {
            return useDefaultChain && defaultChain == ChainId.Other;
        }
        
        [Button]
        public void ClearSessionData()
        {
            if (this._wallet != null)
                // We are inside editor code, we are safe to clear session here.
#pragma warning disable CS0618
                this._wallet.ClearSession();
#pragma warning restore CS0618
            else
            {
                if (this.dataManager == null)
                    SetupDataManager();
                    
                if (this.dataManager != null)
                    this.dataManager.Delete(EncryptedProvider.SessionId);
            }
        }
        #endregion

        #region Public SDK API
        public async void Connect()
        {
            ValidateInitialized();

            await _DefaultConnect();
        }

        public async Task<string> ConnectAndSign(string message)
        {
            ValidateInitialized();
            var response = await _wallet.ConnectAndSign(message);
            
            if (onConnectAndSignCompleted != null)
                onConnectAndSignCompleted.Invoke(response);

            return response;
        }
        
        public Task<TR> ConnectWith<TR>(string method, params object[] @params)
        {
            ValidateInitialized();
            return _wallet.ConnectWith<TR>(method, @params);
        }

        public Task ConnectAndBatch(BatchRequester requests)
        {
            ValidateInitialized();
            return _wallet.ConnectAndBatch(requests);
        }

        public void Disconnect(bool endSession = false)
        {
            ValidateInitialized();
            
            if (endSession)
            {
                MetaMaskDebug.Log("Disconnecting session with an EndSession");
                EndSession();
            }
            
            if (_wallet.IsConnected)
                _wallet.Disconnect();
        }

        public void EndSession()
        {
            ValidateInitialized();
            _wallet.EndSession();
        }

        public bool IsInUnityThread()
        {
            return Application.isEditor || (unityThread != null && Thread.CurrentThread.ManagedThreadId == unityThread.ManagedThreadId);
        }

        public object Request(string method, object[] parameters = null)
        {
            ValidateInitialized();
            return this._wallet.Request(method, parameters);
        }

        public Task<TR> Request<TR>(string method, object[] parameters = null)
        {
            ValidateInitialized();
            return this._wallet.Request<TR>(method, parameters);
        }

        public BatchRequester BatchRequests()
        {
            ValidateInitialized();
            return this._wallet.BatchRequests();
        }

        public void SaveSession()
        {
            ValidateInitialized();
            this._wallet.SaveSession();
        }

        public Task<object> Request(MetaMaskEthereumRequest request)
        {
            ValidateInitialized();
            return this._wallet.Request(request);
        }

        public bool IsWebGL()
        {
            // pass the buck to the compiler :^)
            #if UNITY_WEBGL && !UNITY_EDITOR
            return true;
            #else
            return false;
            #endif
        }
        #endregion

        #region SDK Initialize

        protected void ValidateInitialized()
        {
            if (!_didInit)
                Initialize();

            if (_wallet == null)
                throw new Exception("MetaMaskWallet not initialized");
        }

        public void Initialize()
        {
            if (_didInit)
                throw new Exception("MetaMaskSDK already initialized");
            
            // keep track of what thread the unity thread is
            this.unityThread = Thread.CurrentThread;

            EnableLogging = debugLogging;
            
            UnityBinder.Inject(this);
            
            if (this.MetaMaskUnityBeforeInitialized != null)
                this.MetaMaskUnityBeforeInitialized(this, EventArgs.Empty);
            
            CheckWebsocketDispatcher();
            SetupDataManager();
            SetupWallet();
            SetupRPCUrls();
            SetupTransport();
            SetupUEvents();

            if (this.MetaMaskUnityInitialized != null)
                this.MetaMaskUnityInitialized(this, EventArgs.Empty);

            _didInit = true;
        }

        protected void CheckWebsocketDispatcher()
        {
            // Check if we need to create a WebsocketDispatcher
            var dispatcher = FindObjectOfType<WebSocketDispatcher>();
            if (dispatcher == null)
            {
                MetaMaskDebug.Log("No WebSocketDispatcher found in scene, creating one on " + gameObject.name);
                gameObject.AddComponent<WebSocketDispatcher>();
            }
        }

        protected void SetupDataManager()
        {
            // Configure persistent data manager
            this.dataManager = new MetaMaskDataManager(MetaMaskUnityStorage.Instance, this.encrypt, this.encryptionPassword);

            if (!saveSessionState)
            {
                // if we have this disabled, always delete any
                // session data, just in case ;)
                this.dataManager.Delete(EncryptedProvider.SessionId);
            }
        }

        protected void SetupConnectionMode()
        {
            // if we are on mobile, set the connection mode to deeplink
            ConnectionMode = IsMobile ? TransportMode.Deeplink : TransportMode.QRCode;
        }

        protected void SetupWallet()
        {
            // What transport do we use?
            IMetaMaskTransport transportToUse;
            if (useLegacyUITransport && uiTransport != null)
                transportToUse = uiTransport;
            else
                transportToUse = this;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
            // Setup the JS SDK wallet
            var providerEngine = new MetaMask.Unity.Providers.JsSDKProvider(this);
            this._wallet = new MetaMaskWallet(this.dataManager, transportToUse, providerEngine);
            #else
            // Setup socket
            var socket = new MetaMaskUnitySocketIO();
            
            // Setup the wallet
            this._wallet = new MetaMaskWallet(this.dataManager, this, 
                UnityEciesProvider.Singleton, 
                transportToUse, socket, socketUrl);
            #endif
            
            this._wallet.ProviderEngine.AnalyticsPlatform = $"unity_{Application.unityVersion}";
            this._wallet.ProviderEngine.DappId = Application.identifier;
        }

        protected void SetupRPCUrls()
        {
            if (useInfura && !string.IsNullOrWhiteSpace(infuraProjectId))
            {
                rpcUrl ??= new List<MetaMaskUnityRpcUrlConfig>();

                foreach (var chainId in Infura.ChainIdToName.Keys)
                {
                    var chainName = Infura.ChainIdToName[chainId];

                    if (rpcUrl.Any(r => ChainInfo.ChainIdMatch(r.ChainId, chainId)))
                        continue;

                    rpcUrl.Add(new MetaMaskUnityRpcUrlConfig()
                    {
                        ChainId = ChainInfo.ChainIdToHex(chainId),
                        RpcUrl = Infura.Url(infuraProjectId, chainName)
                    });
                }
            }
                
            // Setup the fallback provider, if set
            if (rpcUrl != null && rpcUrl.Count > 0)
            {
                var rpcUrlMap = rpcUrl.ToDictionary(
                    c => ChainInfo.ChainToId(c.ChainId),
                    c => c.RpcUrl
                );
                
                this._wallet.FallbackProvider = new HttpProvider(rpcUrlMap, this._wallet);
            }
        }

        protected void SetupTransport()
        {
            this._wallet.Transport?.Initialize();
        }

        protected void SetupUEvents()
        {
            if (_eventHandler != null)
                _eventHandler.SetupEvents();
        }

        #endregion

        #region SDK Transport Wrapping

        protected virtual async Task _DefaultConnect()
        {
            if (!useDefaultChain && useConnectAndSign && !string.IsNullOrWhiteSpace(connectAndSignMessage))
            {
                // re-reoute to ConnectAndSign
                // we don't need to await the call, because this version of Connect() behaves
                // this way
                await ConnectAndSign(connectAndSignMessage);
            }
            else if (!useDefaultChain && !useConnectAndSign)
            {
                await _wallet.Connect();
            }
            else if (useDefaultChain && (!useConnectAndSign || (useConnectAndSign && string.IsNullOrWhiteSpace(connectAndSignMessage))))
            {
                await ConnectWithDefaultChain();
            }
            else if (useDefaultChain && useConnectAndSign && !string.IsNullOrWhiteSpace(connectAndSignMessage))
            {
                await ConnectWithDefaultChain();

                await _wallet.WaitForAccount();
                
                var hexEncoded = Encoding.UTF8.GetBytes(connectAndSignMessage).ToHex(true);
                var response = await _wallet.Request<string>(RpcMethods.PersonalSign, new object[] { hexEncoded, _wallet.SelectedAddress });

                if (onConnectAndSignCompleted != null)
                {
                    onConnectAndSignCompleted.Invoke(response);
                }
            }
            else
            {
                await _wallet.Connect();
            }
        }

        protected async Task ConnectWithDefaultChain()
        {
            var chainData = DefaultChainInfo;
            if (Blockchains.MetaMaskDefaults.Contains((long)defaultChain))
            {
                await _wallet.ConnectWith<object>(RpcMethods.WalletSwitchEthereumChain,
                    new object[] { chainData.AsSwitchChainRequest() });
                return;
            }
            
            await _wallet.ConnectWith<object>(RpcMethods.WalletAddEthereumChain,
                new object[] { chainData });
        }
        
        public void UpdateUrls(string universalLink, string deepLink)
        {
            this.connectionDeepLinkUrl = deepLink;
            this.connectionUniversalLinkUrl = universalLink;
        }

        public void OnConnectRequest()
        {
            if (IsMobile)
            {
                OpenConnectionDeepLink();
            }
        }
        
        public void OpenConnectionDeepLink()
        {
            var url = useUniversalLinks ? this.connectionUniversalLinkUrl : this.connectionDeepLinkUrl;
            Debug.Log("Opening Connection URL: " + url);
            OpenDeeplinkURL(url);
        }

        public void OnRequest(string id, MetaMaskEthereumRequest request)
        {
            Requesting?.Invoke(this, new MetaMaskUnityRequestEventArgs(request));
            
            if (IsMobile)
            {
                // Use otp to re-enable host approval
                OpenConnectionDeepLink();
            }
            
            EmitListenerEvent(l => l.OnMetaMaskRequest(id, request));
            AlertBroadcasterIfInUse(mmutb => mmutb.OnMetaMaskRequest(id, request));
        }

        public void OnOTPCode(int code)
        {
            if (this.uiHandler != null)
            {
                this.uiHandler.OnMetaMaskOTP(code);
            }
            
            EmitListenerEvent(l => l.OnMetaMaskOTP(code));
            AlertBroadcasterIfInUse(mmutb => mmutb.OnMetaMaskOTPCode(code));
        }

        public void OnFailure(Exception error)
        {
            Debug.LogError("On Failure: " + error);
            
            EmitListenerEvent(l => l.OnMetaMaskFailure(error));
            AlertBroadcasterIfInUse(mmutb => mmutb.OnMetaMaskFailure(error));
        }

        public void OnSuccess()
        {
            EmitListenerEvent(l => l.OnMetaMaskSuccess());
            AlertBroadcasterIfInUse(mmutb => mmutb.OnMetaMaskSuccess());
        }

        public void OnDisconnect()
        {
            EmitListenerEvent(l => l.OnMetaMaskDisconnected());
        }
        
        void IMetaMaskTransport.Initialize()
        {
            SetupConnectionMode();
            
            if (this.spawnCanvas)
            {
                this.metaMaskCanvasInstance = Instantiate(connectionCanvas);
                this.uiHandler = this.metaMaskCanvasInstance.GetComponent<MetaMaskUnityUIHandler>();
            }
            
            MetaMaskUnity.Instance.Events.StartConnecting += WalletOnStartConnecting;
        }
        
        private void WalletOnStartConnecting(object sender, MetaMaskConnectEventArgs e)
        {
            var universalLink = this.connectionUniversalLinkUrl;
            var deepLink = this.connectionDeepLinkUrl;
            
            if (this.uiHandler != null)
            {
                this.uiHandler.OpenQRCode();
            }
            
            EmitListenerEvent(l => l.OnMetaMaskConnectRequest(universalLink, deepLink));
            AlertBroadcasterIfInUse(mmutb => mmutb.OnMetaMaskConnectRequest(universalLink, deepLink));
        }
        
        private void AlertBroadcasterIfInUse(Action<MetaMaskUnityTransportBroadcaster> action)
        {
            if (MetaMaskUnityTransportBroadcaster.Instance == null)
                return;
            
            action(MetaMaskUnityTransportBroadcaster.Instance);
        }
        
        private void EmitListenerEvent(Action<IMetaMaskUnityTransportListener> callback)
        {
            if (this.metaMaskCanvasInstance)
            {
                UnityThread.executeInUpdate(() =>
                {
                    var listeners =
                        this.metaMaskCanvasInstance.GetComponentsInChildren<IMetaMaskUnityTransportListener>();
                    for (int i = 0; i < listeners.Length; i++)
                    {
                        callback(listeners[i]);
                    }
                });
            }
        }

        #endregion

        #region AOT Code Strip Prevention System

        [Preserve]
        public void AotStopCodeStrip()
        {
            var eventDelegator = new EventDelegator();
            eventDelegator.ListenFor<string>("test", (sender, @event) =>
            {
                Debug.Log(@event.EventData);
            });
            eventDelegator.Trigger("test", "hi");
            
            var obj = new JsonRpcPayload();
            Debug.Log(obj.Id);
            Debug.Log(obj.Method);

            // we only need 1 generic type, in IL2CPP land, all reference types
            // use the same type (smart pointer type) (MetaMaskTypedDataMessage<Ptr>)
            var obj2 = new MetaMaskTypedDataMessage<string>();
            Debug.Log(obj2.Data);
            Debug.Log(obj2.Name);

            var obj3 = new JsonRpcResult<string>();
            Debug.Log(obj3.Result);
            Debug.Log(obj3.Id);

            var obj4 = new GenericError();
            Debug.Log(obj4.Message);
            Debug.Log(obj4.Code);

            var obj5 = new JsonRpcError();
            Debug.Log(obj5.Error);

            var obj6 = new MetaMaskMessage<string>();
            Debug.Log(obj6.Id);
            Debug.Log(obj6.Message);
            
            
            
            // All contract types
            Debug.Log(new ERC20Backing(null, null, null));
            Debug.Log(new ERC721Backing(null, null, null));
            Debug.Log(new ERC1155Backing(null, null, null));
            Debug.Log(new ERC20PresetFixedSupplyBacking(null, null, null));
            Debug.Log(new ERC20PresetMinterPauserBacking(null, null, null));
            Debug.Log(new ERC721PresetMinterPauserAutoIdBacking(null, null, null));
            
            throw new Exception("This method should not be ran at runtime");
        }

        #endregion
        
    }
}