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
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace MetaMask.Unity
{
    [RequireComponent(typeof(MetaMaskUnityEventHandler))]
    [RequireComponent(typeof(MetaMaskHttpService))]
    public class MetaMaskUnity : MonoBehaviour, IMetaMaskEvents
    {
        public static readonly string Version = MetaMaskWallet.Version;
        public static readonly string Build = "8ea8c0b";
        
        #region Classes

        [Serializable]
        public class MetaMaskUnityRpcUrlConfig
        {
            public long ChainId;

            public string RpcUrl;
        }

        #endregion
        #region Fields

        protected static MetaMaskUnity instance;

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

        #endregion

        #region Properties

        /// <summary>Gets the singleton instance of the <see cref="MetaMaskUnity"/> class.</summary>
        /// <returns>The singleton instance of the <see cref="MetaMaskUnity"/> class.</returns>
        public static MetaMaskUnity Instance
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
                }
                return instance;
            }
        }

        /// <summary>Gets the configuration for the MetaMask client.</summary>
        /// <returns>The configuration for the MetaMask client.</returns>
        public MetaMaskConfig Config
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
            else if (instance != this)
            {
                Debug.LogError("There are more than 1 instances of " + nameof(MetaMaskUnity) + " inside the scene, there should be only one.");
                Destroy(gameObject);
            }
            if (this.initializeOnAwake)
            {
                Initialize(Config);
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
            Initialize(Config, transport, socket);
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
            Initialize(Config, transport, socket);
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
                
                //#if UNITY_WEBGL && !UNITY_EDITOR
                //var providerEngine = new MetaMask.Unity.Providers.JsSDKProvider(this);
                //this.wallet = new MetaMaskWallet(this.dataManager, transport, providerEngine);
                //#else
                // Setup the wallet
                this.wallet = new MetaMaskWallet(this.dataManager, this.config, 
                    UnityEciesProvider.Singleton, 
                    transport, socket, this.config.SocketUrl);
                //#endif

                if (!string.IsNullOrWhiteSpace(this.config.UserAgent))
                    this.wallet.UserAgent = this.config.UserAgent;
                
                // Grab session data
                this.session = this.wallet.Session;
                this.sessionData = this.wallet.Session.Data;
                
                this.wallet.ProviderEngine.AnalyticsPlatform = "unity";
                
                if (!string.IsNullOrWhiteSpace(_infuraProjectId))
                {
                    _rpcUrl ??= new List<MetaMaskUnityRpcUrlConfig>();

                    foreach (var chainId in Infura.ChainIdToName.Keys)
                    {
                        var chainName = Infura.ChainIdToName[chainId];

                        _rpcUrl = _rpcUrl.Where(r => r.ChainId != chainId).ToList();
                        _rpcUrl.Add(new MetaMaskUnityRpcUrlConfig()
                        {
                            ChainId = chainId,
                            RpcUrl = Infura.Url(_infuraProjectId, chainName)
                        });
                    }
                }
                
                // Setup the fallback provider, if set
                if (_rpcUrl != null && _rpcUrl.Count > 0)
                {
                    var rpcUrlMap = _rpcUrl.ToDictionary(
                        c => c.ChainId,
                        c => c.RpcUrl
                    );
                    
                    this.wallet.FallbackProvider = new HttpProvider(rpcUrlMap, this.wallet);
                }

                if (this.MetaMaskUnityBeforeInitialized != null)
                    this.MetaMaskUnityBeforeInitialized(this, EventArgs.Empty);
                
                _eventHandler.SetupEvents();
                
                // Initialize the transport
                transport.Initialize();

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

        /// <summary>Disconnects the wallet.</summary>
        public void Disconnect(bool endSession = false)
        {
            if (this.wallet.IsConnected)
                this.wallet.Disconnect();
            
            if (endSession)
                EndSession();
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

        /// <summary>Makes a request to the users connected wallet.</summary>
        /// <param name="request">The ethereum request to send to the user wallet.</param>
        public void Request(MetaMaskEthereumRequest request)
        {
            this.wallet.Request(request);
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