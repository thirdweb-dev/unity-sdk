using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal
{
    public sealed class WalletConnectModal : MonoBehaviour
    {
        [field: SerializeField] private bool InitializeOnAwake { get; set; } = true;

        [field: SerializeField] private bool ResumeSessionOnInit { get; set; } = true;

        [field: SerializeField, Space] private WCModal Modal { get; set; }

        [field: SerializeField] private SerializableDictionary<ViewType, WCModalView> Views { get; set; } = new();

        public static ISignClient SignClient => WalletConnect.Instance.SignClient;
        
        public static UnityWebRequestWalletsFactory WalletsRequestsFactory { get; private set; }

        internal static ConnectionController ConnectionController { get; private set; }

        internal static WalletConnectModal Instance { get; private set; }

        private static WalletConnectModalOptions Options { get; set; }

        public static bool IsReady { get; set; }

        [Obsolete("Use WalletConnect.Instance.SessionConnected instead")]
        public static event EventHandler Connected;
        
        // TODO: make ConnectionError generic
        public static event EventHandler ConnectionError;
        public static event EventHandler<ModalReadyEventArgs> Ready;
        public static event EventHandler ModalOpened;
        public static event EventHandler ModalClosed;

        private async void Awake()
        {
            if (!TryConfigureSingleton())
                return;
            
            if (InitializeOnAwake)
                await InitializeAsync();
        }

        public static async Task InitializeAsync()
        {
            UnityWebRequestExtensions.sdkVersion = "unity-wcm-v1.1.6"; // TODO: update this from CI

            await WalletConnect.Instance.InitializeAsync();

            ConnectionController = new ConnectionController(SignClient);

            WalletConnect.Instance.SessionConnected += Instance.OnSessionConnected;
            SignClient.SessionConnectionErrored += Instance.OnSessionErrored;
            Instance.Modal.Opened += (_, _) => ModalOpened?.Invoke(Instance, EventArgs.Empty);
            Instance.Modal.Closed += (_, _) => ModalClosed?.Invoke(Instance, EventArgs.Empty);

            var sessionResumed = false;
            if (Instance.ResumeSessionOnInit)
                sessionResumed = await WalletConnect.Instance.TryResumeSessionAsync();

            IsReady = true;
            Ready?.Invoke(Instance, new ModalReadyEventArgs
            {
                SessionResumed = sessionResumed
            });
        }

        private bool TryConfigureSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                return true;
            }
            else
            {
                Debug.LogError("[WalletConnectUnity] WalletConnectModal already exists. Destroying...");
                Destroy(gameObject);
                return false;
            }
        }

        public static void Open(WalletConnectModalOptions options, ViewType view = ViewType.Connect)
        {
            if (!IsReady)
            {
                Debug.LogError("[WalletConnectUnity] WalletConnectModal is not ready yet.");
                return;
            }

            if (Instance.Modal.IsOpen)
            {
                Debug.LogWarning("[WalletConnectUnity] WalletConnectModal is already open.");
                return;
            }

            if (Options != options)
            {
                Options = options;

                WalletsRequestsFactory = new UnityWebRequestWalletsFactory(
                    includedWalletIds: options.IncludedWalletIds,
                    excludedWalletIds: options.ExcludedWalletIds
                );

                ConnectionController.InitiateConnection(options.ConnectOptions);
            }

            var modalView = Instance.Views[view];
            Instance.Modal.OpenView(modalView);
        }

        public static async void Disconnect()
        {
            try
            {
                await WalletConnect.Instance.DisconnectAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnSessionConnected(object sender, SessionStruct _)
        {
            Modal.CloseModal();
            Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnSessionErrored(object sender, Exception e)
        {
            WalletConnect.UnitySyncContext.Post(_ =>
            {
                Modal.CloseModal();
                ConnectionError?.Invoke(this, EventArgs.Empty);
            }, null);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SignClient?.Dispose();
            }
        }
    }

    public enum ViewType : sbyte
    {
        Connect = 1,
    }

    public class ModalReadyEventArgs : EventArgs
    {
        public bool SessionResumed { get; set; }
    }
}