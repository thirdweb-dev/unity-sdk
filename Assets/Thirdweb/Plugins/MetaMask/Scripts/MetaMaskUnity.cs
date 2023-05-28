using System;
using System.Threading.Tasks;
using MetaMask.Cryptography;
using MetaMask.IO;
using MetaMask.Logging;
using MetaMask.Models;
using MetaMask.Sockets;
using MetaMask.Transports;
using MetaMask.Transports.Unity.UI;
using UnityEngine;

namespace MetaMask.Unity
{
    public class MetaMaskUnity : MonoBehaviour
    {
        #region Fields

        protected static MetaMaskUnity instance;

        /// <summary>The configuration for the MetaMask client.</summary>
        [SerializeField]
        protected MetaMaskConfig config;

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
            get { return instance; }
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
                DontDestroyOnLoad(this.gameObject);
            }
            else if (instance != this)
            {
                Debug.LogError("There are more than 1 instances of " + nameof(MetaMaskUnity) + " inside the scene, there should be only one.");
                Destroy(gameObject);
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

        public EventHandler OnConnectionAttempted;
        public EventHandler OnDisconnectionAttempted;

        /// <summary>Connects to the wallet.</summary>
        public void Connect()
        {
            this.wallet.Connect();
            OnConnectionAttempted.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Disconnects the wallet.</summary>
        public void Disconnect()
        {
            this.wallet.Disconnect();
            OnDisconnectionAttempted.Invoke(this, EventArgs.Empty);
        }

        public void OpenDeepLink()
        {
            if (MetaMask.Transports.Unity.UI.MetaMaskUnityUITransport.DefaultInstance != null)
            {
                MetaMask.Transports.Unity.UI.MetaMaskUnityUITransport.DefaultInstance.OpenDeepLink();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>Releases all resources used by the object.</summary>
        protected void Release()
        {
            this.wallet.Dispose();
        }

        #endregion
    }
}
