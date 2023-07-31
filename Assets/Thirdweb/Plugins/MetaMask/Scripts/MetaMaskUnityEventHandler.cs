using System;
using System.Collections.Generic;
using System.Linq;
using MetaMask.SocketIOClient;
using MetaMask.Unity.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace MetaMask.Unity
{
    public class MetaMaskUnityEventHandler : BindableMonoBehavior, IMetaMaskEventsHandler
    {
        [Inject]
        private MetaMaskUnity _metaMask;

        public IMetaMaskEventsHandler Events => this;
        public EventHandler<MetaMaskConnectEventArgs> StartConnectingHandler { get; set; }
        public EventHandler WalletReadyHandler { get; set; }
        public EventHandler WalletPausedHandler { get; set; }
        public EventHandler WalletConnectedHandler { get; set; }
        public EventHandler WalletDisconnectedHandler { get; set; }
        public EventHandler ChainIdChangedHandler { get; set; }
        public EventHandler AccountChangedHandler { get; set; }
        public EventHandler WalletAuthorizedHandler { get; set; }
        public EventHandler WalletUnauthorizedHandler { get; set; }
        public EventHandler<MetaMaskEthereumRequestResultEventArgs> EthereumRequestResultReceivedHandler { get; set; }
        public EventHandler<MetaMaskEthereumRequestFailedEventArgs> EthereumRequestFailedHandler { get; set; }
        
        public MetaMaskConnectedEvent MetaMaskConnected = new MetaMaskConnectedEvent();
        public MetaMaskWalletReadyEvent MetaMaskWalletReady = new MetaMaskWalletReadyEvent();
        public MetaMaskWalletPausedEvent MetaMaskWalletPaused = new MetaMaskWalletPausedEvent();
        public MetaMaskConnectingEvent MetamaskConnecting = new MetaMaskConnectingEvent();
        public MetaMaskWalletDisconnectedEvent MetaMaskWalletDisconnected = new MetaMaskWalletDisconnectedEvent();
        public MetaMaskWalletAccountChangedEvent MetaMaskWalletAccountChanged = new MetaMaskWalletAccountChangedEvent();
        public MetaMaskChainIdChangedEvent MetaMaskChainIdChanged = new MetaMaskChainIdChangedEvent();
        public MetaMaskWalletAuthorizedEvent MetaMaskWalletAuthorized = new MetaMaskWalletAuthorizedEvent();
        public MetaMaskWalletUnauthorizedEvent MetaMaskWalletUnauthorized = new MetaMaskWalletUnauthorizedEvent();
        public MetaMaskWalletEthereumRequestResultEvent MetaMaskWalletEthereumRequestResult =
            new MetaMaskWalletEthereumRequestResultEvent();
        public MetaMaskWalletRequestFailedEvent MetaMaskWalletRequestFailed = new MetaMaskWalletRequestFailedEvent();

        private List<Action> TeardownActions;

        private void Start()
        {
            SetupEvents();
        }

        private void OnDestroy()
        {
            TeardownEvents();
        }

        private void SetupEvents()
        {
            // 1. Unity Event
            // 2. Getter for .NET Event Handler
            // 3. Getter for Unity Event Handler
            // 4. Function to update .NET Event Handler
            var allEvents = new (UnityEvent, Func<EventHandler>, Func<EventHandler>, Action<EventHandler>)[]
            {
                (MetaMaskConnected, () => this._metaMask.Wallet.Events.WalletConnectedHandler, () => WalletConnectedHandler, (eh) => _metaMask.Wallet.Events.WalletConnectedHandler = eh),
                (MetaMaskWalletReady, () => this._metaMask.Wallet.Events.WalletReadyHandler, () => WalletReadyHandler, (eh) => _metaMask.Wallet.Events.WalletReadyHandler = eh),
                (MetaMaskWalletPaused, () => this._metaMask.Wallet.Events.WalletPausedHandler, () => WalletPausedHandler, (eh) => _metaMask.Wallet.Events.WalletPausedHandler = eh),
                (MetaMaskWalletDisconnected, () => this._metaMask.Wallet.Events.WalletDisconnectedHandler, () => WalletDisconnectedHandler, (eh) => _metaMask.Wallet.Events.WalletDisconnectedHandler = eh),
                (MetaMaskWalletAccountChanged, () => this._metaMask.Wallet.Events.AccountChangedHandler, () => AccountChangedHandler, (eh) => _metaMask.Wallet.Events.AccountChangedHandler = eh),
                (MetaMaskChainIdChanged, () => this._metaMask.Wallet.Events.ChainIdChangedHandler, () => ChainIdChangedHandler, (eh) => _metaMask.Wallet.Events.ChainIdChangedHandler = eh),
                (MetaMaskWalletAuthorized, () => this._metaMask.Wallet.Events.WalletAuthorizedHandler, () => WalletAuthorizedHandler, (eh) => _metaMask.Wallet.Events.WalletAuthorizedHandler = eh),
                (MetaMaskWalletUnauthorized, () => this._metaMask.Wallet.Events.WalletUnauthorizedHandler, () => WalletUnauthorizedHandler, (eh) => _metaMask.Wallet.Events.WalletUnauthorizedHandler = eh),
            };

            TeardownActions = allEvents.Select((e) => SetupEvent(e.Item1, e.Item2, e.Item3, e.Item4)).ToList();

            TeardownActions.Add(SetupEvent(MetamaskConnecting,
                () => this._metaMask.Wallet.Events.StartConnectingHandler, () => StartConnectingHandler,
                (eh) => this._metaMask.Wallet.Events.StartConnectingHandler = eh));

            TeardownActions.Add(SetupEvent(MetaMaskWalletEthereumRequestResult,
                () => this._metaMask.Wallet.Events.EthereumRequestResultReceivedHandler,
                () => EthereumRequestResultReceivedHandler,
                (eh) => this._metaMask.Wallet.Events.EthereumRequestResultReceivedHandler = eh));

            TeardownActions.Add(SetupEvent(MetaMaskWalletRequestFailed, () => this._metaMask.Wallet.Events.EthereumRequestFailedHandler,
                () => EthereumRequestFailedHandler,
                (eh) => this._metaMask.Wallet.Events.EthereumRequestFailedHandler = eh));
        }

        private void TeardownEvents()
        {
            if (TeardownActions == null)
                return;
            
            foreach (var action in TeardownActions.Where(action => action != null))
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Debug.LogError("Error during MetaMask Event teardown");
                    Debug.LogError(e);
                }
            }
            
            TeardownActions.Clear();
        }

        private Action SetupEvent(UnityEvent @event, Func<EventHandler> sourceGetter, Func<EventHandler> targetGetter, Action<EventHandler> setter)
        {
            void EventTriggered(object sender, EventArgs e)
            {
                UnityThread.executeInUpdate(() =>
                {
                    @event?.Invoke();
                });
            }

            var source = sourceGetter();
            source += EventTriggered;
            
            @event.AddListener(() =>
            {
                var handler = targetGetter();
                handler?.Invoke(this, EventArgs.Empty);
            });

            setter(source);

            return () =>
            {
                var currentSource = sourceGetter();
                currentSource -= EventTriggered;
                setter(currentSource);
            };
        }
        
        private Action SetupEvent<T>(UnityEvent<T> @event, Func<EventHandler<T>> sourceGetter, 
            Func<EventHandler<T>> targetGetter, Action<EventHandler<T>> updater) where T : EventArgs
        {
            void EventTriggered(object sender, T e)
            {
                UnityThread.executeInUpdate(() =>
                {
                    @event.Invoke(e);
                });
            }

            var source = sourceGetter();
            source += EventTriggered;
            
            @event.AddListener((e) =>
            {
                var handler = targetGetter();
                handler?.Invoke(this, e);
            });

            updater(source);

            return () =>
            {
                var currentSource = sourceGetter();
                currentSource -= EventTriggered;
                updater(currentSource);
            };
        }
    }
    
        /// <summary>
    /// UnityEvent definition for when metamask sdk is connecting to a session.
    /// </summary>
    [Serializable]
    public class MetaMaskConnectingEvent : UnityEvent<MetaMaskConnectEventArgs> {}
    
    /// <summary>
    /// UnityEvent definition for when metamask wallet is ready for user interaction.
    /// </summary>
    [Serializable]
    public class MetaMaskWalletReadyEvent : UnityEvent {}
    
    /// <summary>
    /// UnityEvent definition for when metamask wallet has been closed or paused, and user interaction will require
    /// a resume.
    /// </summary>
    [Serializable]
    public class MetaMaskWalletPausedEvent : UnityEvent {}
    
    /// <summary>
    /// UnityEvent definition for when metamask wallet has connected, but may not be ready for user interaction.
    /// </summary>
    [Serializable]
    public class MetaMaskConnectedEvent : UnityEvent {}
    
    /// <summary>
    /// UnityEvent definition for when metamask sdk has disconnected
    /// </summary>
    [Serializable]
    public class MetaMaskWalletDisconnectedEvent : UnityEvent {}
    
    /// <summary>
    /// UnityEvent definition for when metamask wallet's chain id has changed
    /// </summary>
    [Serializable]
    public class MetaMaskChainIdChangedEvent : UnityEvent {}
    
    /// <summary>
    /// UnityEvent definition for when metamask wallet's account address has changed
    /// </summary>
    [Serializable]
    public class MetaMaskWalletAccountChangedEvent : UnityEvent {}
    
    /// <summary>
    /// UnityEvent definition for when metamask wallet has been authorized
    /// </summary>
    [Serializable]
    public class MetaMaskWalletAuthorizedEvent : UnityEvent {}
    
    /// <summary>
    /// UnityEvent definition for when metamask wallet has not been authorized and the session
    /// connect was rejected
    /// </summary>
    [Serializable]
    public class MetaMaskWalletUnauthorizedEvent : UnityEvent {}
    
    /// <summary>
    /// UnityEvent definition for when metamask wallet has not been authorized and the session
    /// connect was rejected
    /// </summary>
    [Serializable]
    public class MetaMaskWalletEthereumRequestResultEvent : UnityEvent<MetaMaskEthereumRequestResultEventArgs> {}
    
    /// <summary>
    /// UnityEvent definition for when metamask wallet has not been authorized and the session
    /// connect was rejected
    /// </summary>
    [Serializable]
    public class MetaMaskWalletRequestFailedEvent : UnityEvent<MetaMaskEthereumRequestFailedEventArgs> {}
}