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
        [Inject(optional = true)]
        private MetaMaskUnityEventListener _eventListener;
        
        protected IMetaMaskSDK _metaMask => MetaMaskUnity.Instance;
        
        public event EventHandler<MetaMaskConnectEventArgs> StartConnecting;
        public event EventHandler WalletReady;
        public event EventHandler WalletPaused;
        public event EventHandler WalletConnected;
        public event EventHandler WalletDisconnected;
        public event EventHandler ChainIdChanged;
        public event EventHandler AccountChanged;
        public event EventHandler WalletAuthorized;
        public event EventHandler WalletUnauthorized;
        public event EventHandler<MetaMaskEthereumRequestResultEventArgs> EthereumRequestResultReceived;
        public event EventHandler<MetaMaskEthereumRequestFailedEventArgs> EthereumRequestFailed;

        public IMetaMaskEventsHandler Events => this;

        private List<Action> TeardownActions;

        private void OnDestroy()
        {
            TeardownEvents();
        }

        internal void SetupEvents()
        {
            // 1. Unity Event
            // 2. Getter for .NET Event Handler
            // 3. Getter for Unity Event Handler
            // 4. Function to update .NET Event Handler
            var allEvents = new (Action<EventHandler, bool>, Func<EventHandler>)[]
            {
                ((eh, set) =>
                {
                    if (set)
                        this._metaMask.Wallet.Events.WalletConnected += eh;
                    else
                        this._metaMask.Wallet.Events.WalletConnected -= eh;
                }, () => WalletConnected),
                ((eh, set) =>
                {
                    if (set)
                        this._metaMask.Wallet.Events.WalletReady += eh;
                    else
                        this._metaMask.Wallet.Events.WalletReady -= eh;
                }, () => WalletReady),
                ((eh, set) =>
                {
                    if (set)
                        this._metaMask.Wallet.Events.WalletPaused += eh;
                    else
                        this._metaMask.Wallet.Events.WalletPaused -= eh;
                }, () => WalletPaused),
                ((eh, set) =>
                {
                    if (set)
                        this._metaMask.Wallet.Events.WalletDisconnected += eh;
                    else
                        this._metaMask.Wallet.Events.WalletDisconnected -= eh;
                }, () => WalletDisconnected),
                ((eh, set) =>
                {
                    if (set)
                        this._metaMask.Wallet.Events.AccountChanged += eh;
                    else
                        this._metaMask.Wallet.Events.AccountChanged -= eh;
                }, () => AccountChanged),
                ((eh, set) =>
                {
                    if (set)
                        this._metaMask.Wallet.Events.ChainIdChanged += eh;
                    else
                        this._metaMask.Wallet.Events.ChainIdChanged -= eh;
                }, () => ChainIdChanged),
                ((eh, set) =>
                {
                    if (set)
                        this._metaMask.Wallet.Events.WalletAuthorized += eh;
                    else
                        this._metaMask.Wallet.Events.WalletAuthorized -= eh;
                }, () => WalletAuthorized),
                ((eh, set) =>
                {
                    if (set)
                        this._metaMask.Wallet.Events.WalletUnauthorized += eh;
                    else
                        this._metaMask.Wallet.Events.WalletUnauthorized -= eh;
                }, () => WalletUnauthorized),
            };

            TeardownActions = allEvents.Select((e) => SetupEvent(e.Item1, e.Item2)).ToList();

            TeardownActions.Add(SetupEvent(
                (eh, set) =>
                {
                    if (set)
                        this._metaMask.Wallet.Events.StartConnecting += eh;
                    else
                        this._metaMask.Wallet.Events.StartConnecting -= eh;
                }, () => StartConnecting));

            TeardownActions.Add(SetupEvent(
                (eh, set) =>
                {
                    if (set)
                        this._metaMask.Wallet.Events.EthereumRequestResultReceived += eh;
                    else
                        this._metaMask.Wallet.Events.EthereumRequestResultReceived -= eh;
                },
                () => EthereumRequestResultReceived));

            TeardownActions.Add(SetupEvent((eh, set) =>
                {
                    if (set)
                        this._metaMask.Wallet.Events.EthereumRequestFailed += eh;
                    else
                        this._metaMask.Wallet.Events.EthereumRequestFailed -= eh;
                },
                () => EthereumRequestFailed));
            
            if (_eventListener != null)
                _eventListener.SetupEvents();
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

        private Action SetupEvent(Action<EventHandler, bool> sourceUpdater, Func<EventHandler> targetGetter)
        {
            void EventTriggered(object sender, EventArgs e)
            {
                UnityThread.executeInUpdate(() =>
                {
                    var handler = targetGetter();
                    handler?.Invoke(this, EventArgs.Empty);
                });
            }

            sourceUpdater(EventTriggered, true);

            return () =>
            {
                sourceUpdater(EventTriggered, false);
            };
        }
        
        private Action SetupEvent<T>(Action<EventHandler<T>, bool> sourceUpdater, 
            Func<EventHandler<T>> targetGetter) where T : EventArgs
        {
            void EventTriggered(object sender, T e)
            {
                UnityThread.executeInUpdate(() =>
                {
                    var handler = targetGetter();
                    handler?.Invoke(sender, e);
                });
            }

            sourceUpdater(EventTriggered, true);

            return () =>
            {
                sourceUpdater(EventTriggered, false);
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
    
    /// <summary>
    /// UnityEvent definition for when metamask wallet has not been authorized and the session
    /// connect was rejected
    /// </summary>
    [Serializable]
    public class MetaMaskStartConnectingEvent : UnityEvent<MetaMaskConnectEventArgs> {}
}