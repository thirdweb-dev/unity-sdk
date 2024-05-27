using System;
using System.Collections.Generic;
using System.Linq;
using evm.net;
using System.Threading;
using System.Threading.Tasks;
using EventEmitter.NET;
using evm.net.Models;
using MetaMask.Contracts;
using MetaMask.Cryptography;
using MetaMask.IO;
using MetaMask.Logging;
using MetaMask.Models;
using MetaMask.Providers;
using MetaMask.SocketIOClient;
using MetaMask.Sockets;
using MetaMask.Transports;
using MetaMask.Transports.Unity;
using MetaMask.Transports.Unity.UI;
using MetaMask.Unity.Utils;
using MetaMask.Editor.NaughtyAttributes;
using MetaMask.Scripts.Utilities;
using MetaMask.Unity.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MetaMask.Unity
{
    [RequireComponent(typeof(MetaMaskUnityEventHandler))]
    [RequireComponent(typeof(MetaMaskHttpService))]
    public class MetaMaskUnity : MonoBehaviour, IMetaMaskSDK
    {
        [Button]
        public void SwitchToNewPrefab()
        {
            Debug.Log("Migrating MetaMaskUnity script to MetaMaskSDK prefab..");
            
            #if UNITY_EDITOR
            // use Undo.AddComponent so action can be undone
            var sdk = Undo.AddComponent<MetaMaskSDK>(gameObject);
            #else
            var sdk = gameObject.AddComponent<MetaMaskSDK>();
            #endif
            
            // copy attributes
            sdk.dappName = config.AppName;
            sdk.dappUrl = config.AppUrl;
            if (!string.IsNullOrWhiteSpace(config.AppIcon))
                sdk.dappIcon = TextureBase64.Base64ToTexture(config.AppIcon);

            var mmuuitransport = _transport as MetaMaskUnityUITransport;
            if (mmuuitransport != null)
            {
                sdk.useLegacyUITransport = false;
                sdk.spawnCanvas = mmuuitransport.spawnCanvas;
                sdk.connectionCanvas = mmuuitransport.metaMaskCanvas;
                
                // still set the _transport, in case the dev enables UseLegacyUITransport
                sdk.uiTransport = _transport;
            }
            else if (_transport != null)
            {
                sdk.useLegacyUITransport = true;
                sdk.uiTransport = _transport;
            }


            sdk.useInfura = !string.IsNullOrWhiteSpace(_infuraProjectId);
            sdk.infuraProjectId = _infuraProjectId;

            sdk.rpcUrl = _rpcUrl;
            
            sdk.userAgent = config.UserAgent;
            sdk.encrypt = config.Encrypt;
            sdk.encryptionPassword = config.EncryptionPassword;
            sdk.debugLogging = config.Log;
            sdk.socketUrl = config.SocketUrl;
            
            #if UNITY_EDITOR
            // now that we are setup, destroy this with undo history
            Undo.DestroyObjectImmediate(this);
            #else
            DestroyImmediate(this);
            #endif
            
            Debug.Log("Migration to MetaMaskSDK prefab complete. Please review the settings. You may safely ignore any errors logged during migration.");
        }
        
        public static readonly string Version = MetaMaskWallet.Version;
        
        #region Fields

        protected static IMetaMaskSDK instance;

        /// <summary>The configuration for the MetaMask client.</summary>
        [SerializeField]
        protected MetaMaskConfig config;
        /// <summary>Whether or not to initialize the wallet on awake.</summary>
        /// <remarks>This is useful for testing.</remarks>
        [FormerlySerializedAs("initializeOnStart")] [SerializeField]
        protected bool initializeOnAwake = true;

        [SerializeField]
        protected MetaMaskUnityScriptableObjectTransport _transport;


        /// <summary>Initializes the MetaMask Wallet Plugin.</summary>
        protected bool initialized = false;

        /// <param name="transport">The transport to use for communication with the MetaMask backend.</param>
        protected IMetaMaskTransport transport;
        /// <param name="socket">The socket wrapper to use for communication with the MetaMask backend.</param>
        protected IMetaMaskSocketWrapper socket;
        /// <param name="dataManager">The data manager to use for storing data.</param>
        protected MetaMaskDataManager dataManager;
        /// <param name="session">The session to use for storing data.</param>
        protected MetaMaskSession session;
        /// <param name="sessionData">The session data to use for storing data.</param>
        protected MetaMaskSessionData sessionData;
        /// <param name="wallet">The wallet to use for storing data.</param>
        protected MetaMaskWallet wallet;
        /// <summary>
        /// The Infura Project Id to use for connecting to an RPC endpoint. This can be used instead of
        /// RpcUrl
        /// </summary>
        [FormerlySerializedAs("InfuraProjectId")] [SerializeField]
        protected string _infuraProjectId;

        public string InfuraProjectId
        {
            get
            {
                return _infuraProjectId;
            }
        }
        
        /// <summary>
        /// The RPC URL to use for web3 query requests when the MetaMask wallet is paused
        /// </summary>
        [FormerlySerializedAs("RpcUrl")] [SerializeField]
        protected List<MetaMaskUnityRpcUrlConfig> _rpcUrl;

        public string SDKVersion => Version;

        public List<MetaMaskUnityRpcUrlConfig> RpcUrl
        {
            get
            {
                return _rpcUrl;
            }
        }
        
        internal Thread unityThread;

        #endregion
        
        #region Events

        [Inject]
        private MetaMaskUnityEventHandler _eventHandler;

        public IMetaMaskEventsHandler Events => _eventHandler;

        public event EventHandler MetaMaskUnityBeforeInitialized;
        public event EventHandler MetaMaskUnityInitialized;
        public event EventHandler<MetaMaskUnityRequestEventArgs> Requesting;

        #endregion

        #region Properties

        /// <summary>Gets the singleton instance of the <see cref="MetaMaskUnity"/> class.</summary>
        /// <returns>The singleton instance of the <see cref="MetaMaskUnity"/> class.</returns>
        public static IMetaMaskSDK Instance
        {
            get
            {
                if (instance == null)
                {
                    var instances = FindObjectsOfType<MetaMaskUnity>();
                    if (instances.Length > 1)
                    {
                        Debug.LogError("There are more than 1 instances of " + nameof(MetaMaskUnity) + " inside the scene, there should be only one.");
                        instance = instances[0];
                    }
                    else if (instances.Length == 1)
                    {
                        instance = instances[0];
                    }
                    // Don't automatically create new instances
                    /*
                    else
                    {
                        instance = CreateNewInstance();
                    }*/
                    
                    var instances2 = FindObjectsOfType<MetaMaskSDK>();
                    if (instances2.Length > 1)
                    {
                        Debug.LogError("There are more than 1 instances of " + nameof(MetaMaskSDK) + " inside the scene, there should be only one.");
                        instance = instances2[0];
                    }
                    else if (instances2.Length == 1)
                    {
                        instance = instances2[0];
                    }
                    // Don't automatically create new instances
                    /*
                    else
                    {
                        instance = CreateNewInstance();
                    }*/
                }
                return instance;
            }
        }

        /// <summary>Gets the configuration for the MetaMask client.</summary>
        /// <returns>The configuration for the MetaMask client.</returns>
        public IAppConfig Config
        {
            get
            {
                if (this.config == null)
                {
                    this.config = MetaMaskConfig.DefaultInstance;
                }
                return this.config;
            }
        }

        public MetaMaskConfig MetaMaskConfig => Config as MetaMaskConfig;

        /// <summary>The wallet associated with this instance.</summary>
        public MetaMaskWallet Wallet => this.wallet;

        #endregion

        #region Unity Messages

        /// <summary>Resets the configuration to the default instance.</summary>
        private void Reset()
        {
            this.config = MetaMaskConfig.DefaultInstance;
        }

        /// <summary>Initializes the MetaMask Unity SDK.</summary>
        /// <param name="config">The configuration to use.</param>
        protected void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance as MetaMaskUnity != this)
            {
                Debug.LogError("There are more than 1 instances of " + nameof(MetaMaskUnity) + " inside the scene, there should be only one.");
                Destroy(gameObject);
            }
            if (this.initializeOnAwake)
            {
                Initialize(MetaMaskConfig);
            }
        }


        /// <summary>Saves the current session.</summary>
        protected void OnApplicationQuit()
        {
            MetaMaskDebug.Log("Would've call Dispose on MetaMaskWallet");
            //Release();
        }

        #endregion

        #region Public Methods

        /// <summary>Initializes the MetaMask client.</summary>
        /// <param name="config">The configuration to use.</param>
        /// <param name="transport">The transport to use.</param>
        /// <param name="socket">The socket to use.</param>
        public void Initialize()
        {
            var transport = _transport ? _transport : Resources.Load<MetaMaskUnityUITransport>("MetaMask/Transports/UnityUI");
            var socket = new MetaMaskUnitySocketIO();
            Initialize(MetaMaskConfig, transport, socket);
        }

        /// <summary>Initializes the MetaMask client.</summary>
        /// <param name="config">The configuration to use.</param>
        public void Initialize(MetaMaskConfig config)
        {
            var transport = _transport ? _transport : Resources.Load<MetaMaskUnityUITransport>("MetaMask/Transports/UnityUI");
            var socket = new MetaMaskUnitySocketIO();
            Initialize(config, transport, socket);
        }

        /// <summary>Initializes the MetaMask client.</summary>
        /// <param name="transport">The transport to use.</param>
        /// <param name="socket">The socket to use.</param>
        public void Initialize(IMetaMaskTransport transport, IMetaMaskSocketWrapper socket)
        {
            Initialize(MetaMaskConfig, transport, socket);
        }

        /// <summary>Initializes the MetaMask client.</summary>
        /// <param name="config">The configuration to use.</param>
        /// <param name="transport">The transport to use.</param>
        /// <param name="socket">The socket to use.</param>
        public void Initialize(MetaMaskConfig config, IMetaMaskTransport transport, IMetaMaskSocketWrapper socket)
        {
            if (this.initialized)
            {
                return;
            }

            // Keep a reference to the config
            this.config = config;

            this.transport = transport;
            this.socket = socket;
            
            // update log var
            MetaMaskSDK.EnableLogging = config.Log;
            
            // Inject variables
            UnityBinder.Inject(this);

            // Validate config
            if (Config.AppName == "example" || Config.AppUrl == "example.com")
            {
                if (SceneManager.GetActiveScene().name.ToLower() != "metamask main (sample)")
                    throw new ArgumentException(
                        "Cannot use example App name or App URL, please update app info in Window > MetaMask > Setup Window under Credentials");
            }
            
            try
            {
                // Check if we need to create a WebsocketDispatcher
                var dispatcher = FindObjectOfType<WebSocketDispatcher>();
                if (dispatcher == null)
                {
                    MetaMaskDebug.Log("No WebSocketDispatcher found in scene, creating one on " + gameObject.name);
                    gameObject.AddComponent<WebSocketDispatcher>();
                }
                
                this.unityThread = Thread.CurrentThread;
                
                // Configure persistent data manager
                this.dataManager = new MetaMaskDataManager(MetaMaskUnityStorage.Instance, this.config.Encrypt, this.config.EncryptionPassword);

#pragma warning disable CS0612 // Type or member is obsolete
                // use startsWith to catch any trailing / 
                if (this.config.SocketUrl.StartsWith(MetaMaskWallet.DeprecatedSocketUrl))
                {
                    var newUrl = MetaMaskWallet.SocketUrl;
                    MetaMaskDebug.LogWarning($"Upgrading to new socket server: {newUrl}");
                    this.config.SocketUrl = newUrl;
                }
#pragma warning restore CS0612 // Type or member is obsolete

                #if UNITY_WEBGL && !UNITY_EDITOR
                var providerEngine = new MetaMask.Unity.Providers.JsSDKProvider(this);
                this.wallet = new MetaMaskWallet(this.dataManager, transport, providerEngine);
                #else
                // Setup the wallet
                this.wallet = new MetaMaskWallet(this.dataManager, this.config, 
                    UnityEciesProvider.Singleton, 
                    transport, socket, this.config.SocketUrl);
                #endif

                if (!string.IsNullOrWhiteSpace(this.config.UserAgent))
                    this.wallet.UserAgent = this.config.UserAgent;
                
                // Grab session data
                this.session = this.wallet.Session;
                this.sessionData = this.wallet.Session.Data;
                
                this.wallet.ProviderEngine.AnalyticsPlatform = $"unity_{Application.unityVersion}";
                this.wallet.ProviderEngine.DappId = Application.identifier;
                
                if (!string.IsNullOrWhiteSpace(_infuraProjectId))
                {
                    _rpcUrl ??= new List<MetaMaskUnityRpcUrlConfig>();

                    foreach (var chainId in Infura.ChainIdToName.Keys)
                    {
                        var chainName = Infura.ChainIdToName[chainId];

                        _rpcUrl = _rpcUrl.Where(r => !ChainInfo.ChainIdMatch(r.ChainId, chainId)).ToList();
                        _rpcUrl.Add(new MetaMaskUnityRpcUrlConfig()
                        {
                            ChainId = ChainInfo.ChainIdToHex(chainId),
                            RpcUrl = Infura.Url(_infuraProjectId, chainName)
                        });
                    }
                }
                
                // Setup the fallback provider, if set
                if (_rpcUrl != null && _rpcUrl.Count > 0)
                {
                    var rpcUrlMap = _rpcUrl.ToDictionary(
                        c => ChainInfo.ChainToId(c.ChainId),
                        c => c.RpcUrl
                    );
                    
                    this.wallet.FallbackProvider = new HttpProvider(rpcUrlMap, this.wallet);
                }

                if (this.MetaMaskUnityBeforeInitialized != null)
                    this.MetaMaskUnityBeforeInitialized(this, EventArgs.Empty);
                
                _eventHandler.SetupEvents();
                
                transport.Initialize();

                // Wrap Requesting event
                MetaMaskUnityUITransport.DefaultInstance.Requesting +=
                    (sender, args) => Requesting?.Invoke(sender, args);

                this.initialized = true;
                
                if (this.MetaMaskUnityInitialized != null)
                    this.MetaMaskUnityInitialized(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MetaMaskDebug.LogError("MetaMaskUnity initialization failed");
                MetaMaskDebug.LogException(ex);
                this.initialized = false;
            }
        }
        #endregion

        #region Wallet API

        /// <summary>Connects to the wallet.</summary>
        public void Connect()
        {
            this.wallet.Connect();
        }

        public Task<string> ConnectAndSign(string message)
        {
            return this.wallet.ConnectAndSign(message);
        }

        public Task<TR> ConnectWith<TR>(string method, params object[] @params)
        {
            return this.wallet.ConnectWith<TR>(method, @params);
        }

        public Task ConnectAndBatch(BatchRequester requests)
        {
            return this.wallet.ConnectAndBatch(requests);
        }

        /// <summary>Disconnects the wallet.</summary>
        public void Disconnect(bool endSession = false)
        {
            if (endSession)
                EndSession();
            
            if (this.wallet.IsConnected)
                this.wallet.Disconnect();
        }
        
        public void EndSession()
        {
            this.wallet.EndSession();
        }

        public bool IsInUnityThread()
        {
            return Application.isEditor || (unityThread != null && Thread.CurrentThread.ManagedThreadId == unityThread.ManagedThreadId);
        }

        internal void ForceClearSession()
        {
            if (this.wallet != null)
                // We are inside editor code, we are safe to clear session here.
#pragma warning disable CS0618
                this.wallet.ClearSession();
#pragma warning restore CS0618
            else
            {
                if (this.dataManager == null)
                    this.dataManager = new MetaMaskDataManager(MetaMaskUnityStorage.Instance, this.config.Encrypt, this.config.EncryptionPassword);
                    
                this.dataManager.Delete(EncryptedProvider.SessionId);
            }
        }
        
        public object Request(string method, object[] parameters = null)
        {
            return this.wallet.Request(method, parameters);
        }

        public Task<TR> Request<TR>(string method, object[] parameters = null)
        {
            return this.wallet.Request<TR>(method, parameters);
        }

        public BatchRequester BatchRequests()
        {
            return this.wallet.BatchRequests();
        }

        public void SaveSession()
        {
            this.wallet.SaveSession();
        }

        /// <summary>Makes a request to the users connected wallet.</summary>
        /// <param name="request">The ethereum request to send to the user wallet.</param>
        public Task<object> Request(MetaMaskEthereumRequest request)
        {
            return this.wallet.Request(request);
        }

        public bool clearSessionData = false;

        private void OnValidate()
        {
            if (clearSessionData && Application.isEditor)
            {
                ForceClearSession();
                clearSessionData = false;
            }

            if (_rpcUrl != null && _rpcUrl.Count > 0 && !string.IsNullOrWhiteSpace(_infuraProjectId))
            {
                Debug.LogWarning("The InfuraProjectId will be used over the RpcUrl list if it can. Please set only one.");
            }
        }

        public bool IsWebGL()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            return true;
            #else
            return false;
            #endif
        }

        #endregion

        #region Protected Methods

        /// <summary>Creates a new instance of the <see cref="MetaMaskUnity"/> class.</summary>
        /// <returns>A new instance of the <see cref="MetaMaskUnity"/> class.</returns>
        protected static MetaMaskUnity CreateNewInstance()
        {
            var go = new GameObject(nameof(MetaMaskUnity));
            DontDestroyOnLoad(go);
            return go.AddComponent<MetaMaskUnity>();
        }

        /// <summary>Releases all resources used by the object.</summary>
        protected void Release()
        {
            this.wallet.Dispose();
        }

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