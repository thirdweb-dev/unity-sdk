using System;

using MetaMask.Cryptography;
using MetaMask.IO;
using MetaMask.Logging;
using MetaMask.Models;
using MetaMask.Sockets;
using MetaMask.Transports;
using MetaMask.Transports.Unity.UI;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetaMask.Unity
{

    public class MetaMaskUnity : MonoBehaviour
    {

        #region Fields

        protected static MetaMaskUnity instance;

        /// <summary>The configuration for the MetaMask client.</summary>
        [SerializeField]
        protected MetaMaskConfig config;
        /// <summary>Whether or not to initialize the wallet on start.</summary>
        /// <remarks>This is useful for testing.</remarks>
        [SerializeField]
        protected bool initializeOnStart = true;


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
                    else
                    {
                        instance = CreateNewInstance();
                    }
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
            if (this.initializeOnStart)
            {
                Initialize(Config);
            }
        }


        /// <summary>Saves the current session.</summary>
        protected void OnApplicationQuit()
        {
            SaveSession();
            Release();
        }

        #endregion

        #region Public Methods

        /// <summary>Initializes the MetaMask client.</summary>
        /// <param name="config">The configuration to use.</param>
        /// <param name="transport">The transport to use.</param>
        /// <param name="socket">The socket to use.</param>
        public void Initialize()
        {
            var transport = Resources.Load<MetaMaskUnityUITransport>("MetaMask/Transports/UnityUI");
            var socket = new MetaMaskUnitySocketIO();
            Initialize(Config, transport, socket);
        }

        /// <summary>Initializes the MetaMask client.</summary>
        /// <param name="config">The configuration to use.</param>
        public void Initialize(MetaMaskConfig config)
        {
            var transport = Resources.Load<MetaMaskUnityUITransport>("MetaMask/Transports/UnityUI");
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

            try
            {

                // Initialize the transport
                transport.Initialize();

                // Configure persistent data manager
                this.dataManager = new MetaMaskDataManager(MetaMaskPlayerPrefsStorage.Singleton, this.config.Encrypt, this.config.EncryptionPassword);

                // Load and configure the session
                LoadSession();

                // Setup the wallet
                this.wallet = new MetaMaskWallet(this.session, transport, socket, this.config.SocketUrl);
                this.wallet.AnalyticsPlatform = "unity";
                this.initialized = true;
            }
            catch (Exception ex)
            {
                MetaMaskDebug.LogError("MetaMaskUnity initialization failed");
                MetaMaskDebug.LogException(ex);
                this.initialized = false;
            }
        }

        /// <summary>Saves the current session.</summary>
        public void SaveSession()
        {
            this.dataManager.Save(this.config.SessionIdentifier, this.session.Data);
        }

        /// <summary>Loads the session.</summary>
        public void LoadSession()
        {
            if (Config.AppName == "example" || Config.AppUrl == "example.com")
            {
                if (SceneManager.GetActiveScene().name.ToLower() != "metamask main (sample)")
                    throw new ArgumentException(
                        "Cannot use example App name or App URL, please update app info in Window > MetaMask > Setup Window under Credentials");

            }
            
            if (this.sessionData == null)
            {
                if (this.session != null && this.session.Data != null)
                {
                    this.sessionData = this.session.Data;
                }
                else
                {
                    this.sessionData = new MetaMaskSessionData(Config.AppName, Config.AppUrl);
                }
            }
            this.dataManager.LoadInto(this.config.SessionIdentifier, this.sessionData);
            if (this.session == null)
            {
                this.session = new MetaMaskSession(UnityEciesProvider.Singleton, this.sessionData);
            }
        }

        #endregion

        #region Wallet API

        /// <summary>Connects to the wallet.</summary>
        public void Connect()
        {
            this.wallet.Connect();
        }

        /// <summary>Disconnects the wallet.</summary>
        public void Disconnect()
        {
            this.wallet.Disconnect();
        }

        /// <summary>Makes a request to the users connected wallet.</summary>
        /// <param name="request">The ethereum request to send to the user wallet.</param>
        public void Request(MetaMaskEthereumRequest request)
        {
            this.wallet.Request(request);
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

        #endregion

    }

}