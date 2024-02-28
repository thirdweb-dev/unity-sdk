using System;
using System.Collections.Generic;
using System.Linq;
using MetaMask.SocketIOClient;
using MetaMask.Unity.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace MetaMask.Unity
{
    public class MetaMaskUnityEventListener : BindableMonoBehavior
    {
        [Inject]
        private MetaMaskUnityEventHandler _eventHandler;
        
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
        public MetaMaskStartConnectingEvent MetaMaskWalletStartConnecting = new MetaMaskStartConnectingEvent();
        
        private List<Action> TeardownActions;
        
        internal void SetupEvents()
        {
            if (_eventHandler == null)
                UnityBinder.Inject(this);
            
            var allEvents = new (UnityEvent, Action<EventHandler, bool>)[]
            {
                (MetaMaskConnected, (eh, set) =>
                {
                    if (set)
                        this._eventHandler.WalletConnected += eh;
                    else
                        this._eventHandler.WalletConnected -= eh;
                }),
                (MetaMaskWalletReady, (eh, set) =>
                {
                    if (set)
                        this._eventHandler.WalletReady += eh;
                    else
                        this._eventHandler.WalletReady -= eh;
                }),
                (MetaMaskWalletPaused, (eh, set) =>
                {
                    if (set)
                        this._eventHandler.WalletPaused += eh;
                    else
                        this._eventHandler.WalletPaused -= eh;
                }),
                (MetaMaskWalletDisconnected, (eh, set) =>
                {
                    if (set)
                        this._eventHandler.WalletDisconnected += eh;
                    else
                        this._eventHandler.WalletDisconnected -= eh;
                }),
                (MetaMaskWalletAccountChanged, (eh, set) =>
                {
                    if (set)
                        this._eventHandler.AccountChanged += eh;
                    else
                        this._eventHandler.AccountChanged -= eh;
                }),
                (MetaMaskChainIdChanged, (eh, set) =>
                {
                    if (set)
                        this._eventHandler.ChainIdChanged += eh;
                    else
                        this._eventHandler.ChainIdChanged -= eh;
                }),
                (MetaMaskWalletAuthorized, (eh, set) =>
                {
                    if (set)
                        this._eventHandler.WalletAuthorized += eh;
                    else
                        this._eventHandler.WalletAuthorized -= eh;
                }),
                (MetaMaskWalletUnauthorized, (eh, set) =>
                {
                    if (set)
                        this._eventHandler.WalletUnauthorized += eh;
                    else
                        this._eventHandler.WalletUnauthorized -= eh;
                }),
            };

            TeardownActions = allEvents.Select((e) => SetupEvent(e.Item1, e.Item2)).ToList();

            TeardownActions.Add(SetupEvent(
                MetaMaskWalletStartConnecting,
                (eh, set) =>
                {
                    if (set)
                        this._eventHandler.StartConnecting += eh;
                    else
                        this._eventHandler.StartConnecting -= eh;
                }));

            TeardownActions.Add(SetupEvent(
                MetaMaskWalletEthereumRequestResult,
                (eh, set) =>
                {
                    if (set)
                        this._eventHandler.EthereumRequestResultReceived += eh;
                    else
                        this._eventHandler.EthereumRequestResultReceived -= eh;
                }));

            TeardownActions.Add(SetupEvent(
                MetaMaskWalletRequestFailed,
                (eh, set) =>
                {
                    if (set)
                        this._eventHandler.EthereumRequestFailed += eh;
                    else
                        this._eventHandler.EthereumRequestFailed -= eh;
                }));
        }
        
        private Action SetupEvent(UnityEvent @event, Action<EventHandler, bool> sourceUpdater)
        {
            void EventTriggered(object sender, EventArgs e)
            {
                UnityThread.executeInUpdate(() =>
                {
                    @event?.Invoke();
                });
            }

            sourceUpdater(EventTriggered, true);

            return () =>
            {
                sourceUpdater(EventTriggered, false);
            };
        }
        
        private Action SetupEvent<T>(UnityEvent<T> @event, Action<EventHandler<T>, bool> sourceUpdater) where T : EventArgs
        {
            void EventTriggered(object sender, T e)
            {
                UnityThread.executeInUpdate(() =>
                {
                    @event?.Invoke(e);
                });
            }

            sourceUpdater(EventTriggered, true);

            return () =>
            {
                sourceUpdater(EventTriggered, false);
            };
        }
        
        private void OnDestroy()
        {
            TeardownEvents();
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
    }
}