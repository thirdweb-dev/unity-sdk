using System.Threading.Tasks;
using UnityEngine;
using WalletConnectUnity.Core;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models;
using System;
using Thirdweb.Redcode.Awaiting;
using System.Linq;
using Nethereum.Hex.HexTypes;
using WalletConnectUnity.Modal;
using System.Collections.Generic;

namespace Thirdweb.Wallets
{
    public class WalletConnectUI : MonoBehaviour
    {
        public GameObject WalletConnectUIParent;

        public static WalletConnectUI Instance { get; private set; }

        protected Exception _exception;
        protected bool _isConnected;
        protected string[] _supportedChains;
        protected string[] _includedWalletIds;

        protected virtual async void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }

            await WalletConnectModal.InitializeAsync();
        }

        public virtual async Task Connect(string[] eip155ChainsSupported, string[] includedWalletIds)
        {
            _exception = null;
            _isConnected = false;
            _supportedChains = eip155ChainsSupported;
            _includedWalletIds = includedWalletIds;

            WalletConnectUIParent.SetActive(true);

            WalletConnectModal.Ready += OnReady;
            WalletConnect.Instance.ActiveSessionChanged += OnActiveSessionChanged;
            WalletConnect.Instance.SessionDisconnected += OnSessionDisconnected;

            if (WalletConnect.Instance.IsInitialized)
                CreateNewSession();

            await new WaitUntil(() => _isConnected || _exception != null);

            WalletConnectModal.Ready -= OnReady;
            WalletConnect.Instance.ActiveSessionChanged -= OnActiveSessionChanged;
            WalletConnect.Instance.SessionDisconnected -= OnSessionDisconnected;

            WalletConnectUIParent.SetActive(false);

            if (_exception != null)
                throw _exception;
        }

        protected virtual void OnSessionDisconnected(object sender, EventArgs e)
        {
            _isConnected = false;
            ThirdwebDebug.Log("Session disconnected");
        }

        protected virtual void OnActiveSessionChanged(object sender, SessionStruct sessionStruct)
        {
            if (!string.IsNullOrEmpty(sessionStruct.Topic))
            {
                _isConnected = true;
                ThirdwebDebug.Log("Session connected");
            }
            else
            {
                _isConnected = false;
                ThirdwebDebug.Log("No topic found in sessionStruct");
            }
        }

        protected virtual async void OnReady(object sender, ModalReadyEventArgs args)
        {
            if (args.SessionResumed)
            {
                // Session exists
                await WalletConnect.Instance.DisconnectAsync();
                ThirdwebDebug.Log("Resetting session");
            }

            CreateNewSession();
        }

        protected virtual void CreateNewSession()
        {
            ThirdwebDebug.Log("Creating new session");
            var optionalNamespaces = new Dictionary<string, ProposedNamespace>
            {
                {
                    "eip155",
                    new ProposedNamespace
                    {
                        Methods = new[] { "eth_sendTransaction", "personal_sign", "eth_signTypedData_v4", "wallet_switchEthereumChain", "wallet_addEthereumChain" },
                        Chains = _supportedChains,
                        Events = new[] { "chainChanged", "accountsChanged" },
                    }
                }
            };

            var connectOptions = new ConnectOptions { OptionalNamespaces = optionalNamespaces, };

            // Open modal
            WalletConnectModal.Open(new WalletConnectModalOptions { ConnectOptions = connectOptions, IncludedWalletIds = _includedWalletIds });
        }

        public virtual async void Cancel()
        {
            _exception = new UnityException("User cancelled");
            WalletConnectModal.Ready -= OnReady;
            WalletConnect.Instance.ActiveSessionChanged -= OnActiveSessionChanged;
            WalletConnect.Instance.SessionDisconnected -= OnSessionDisconnected;

            try
            {
                await WalletConnect.Instance.DisconnectAsync();
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogWarning($"Error disconnecting WalletConnect: {e}");
            }
        }
    }
}
