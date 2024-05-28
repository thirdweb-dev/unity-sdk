using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectUnity.Core;

namespace WalletConnectUnity.Modal.Sample
{
    public class Dapp : MonoBehaviour
    {
        [Space] [SerializeField] private NetworkListItem _networkListItemPrefab;

        [Space] [SerializeField] private Transform _networkListContainer;
        [SerializeField] private Button _continueButton;

        [Space] [SerializeField] private GameObject _dappButtons;
        [SerializeField] private GameObject _networkList;

        private readonly HashSet<Chain> _selectedChains = new();

        private void Start()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;

            // When WalletConnectModal is ready, enable buttons and subscribe to other events.
            // WalletConnectModal.SignClient can be null if WalletConnectModal is not ready.
            if (WalletConnectModal.IsReady)
            {
                var connected = WalletConnect.Instance.IsConnected;
                InitialiseDapp(connected);
            }
            else
            {
                WalletConnectModal.Ready += (_, args) => { InitialiseDapp(args.SessionResumed); };
            }
        }

        private async void InitialiseDapp(bool connected)
        {
            // Use WalletConnect client id as Sentry user id for internal testing
            // var clientId = await WalletConnect.Instance.SignClient.Core.Crypto.GetClientId();
            // if (!string.IsNullOrWhiteSpace(clientId))
            //     Sentry.SentrySdk.ConfigureScope(scope =>
            //     {
            //         scope.User = new Sentry.SentryUser
            //         {
            //             Id = clientId
            //         };
            //     });

            // SessionResumed is true if Modal resumed session from storage
            if (connected)
                EnableDappButtons();
            else
                EnableNetworksList();

            // Invoked after wallet connected
            WalletConnect.Instance.ActiveSessionChanged += (_, @struct) =>
            {
                if (string.IsNullOrEmpty(@struct.Topic))
                    return;

                Debug.Log($"[WalletConnectModalSample] Session connected. Topic: {@struct.Topic}");
                EnableDappButtons();
            };

            // Invoked after wallet disconnected
            WalletConnect.Instance.SessionDisconnected += (_, _) =>
            {
                Debug.Log($"[WalletConnectModalSample] Session deleted.");
                EnableNetworksList();
            };
        }

        private void EnableDappButtons()
        {
            _networkList.SetActive(false);
            _dappButtons.SetActive(true);
        }

        private void EnableNetworksList()
        {
            _dappButtons.SetActive(false);
            _networkList.SetActive(true);

            if (_networkListContainer.childCount == 0)
            {
                foreach (var chain in ChainConstants.Chains.All)
                {
                    var item = Instantiate(_networkListItemPrefab, _networkListContainer);
                    item.Initialize(chain, OnNetworkSelected);
                }

                // Non-evm chains example.
                // Full chain list available at: https://docs.walletconnect.com/advanced/multichain/chain-list
                var algorandChain = new Chain(
                    ChainConstants.Namespaces.Algorand,
                    ChainConstants.References.Algorand,
                    "Algorand",
                    new Currency("Algo", "ALGO", 6),
                    new BlockExplorer("Pera Explorer", "https://explorer.perawallet.app/"),
                    "https://mainnet-api.algonode.cloud",
                    false,
                    "https://raw.githubusercontent.com/WalletConnect/WalletConnectUnity/project/modal-sample/.github/media/algorand-logo.jpeg"
                );

                var itemAlgorand = Instantiate(_networkListItemPrefab, _networkListContainer);
                itemAlgorand.Initialize(algorandChain, OnNetworkSelected);
            }
        }

        private void OnNetworkSelected(Chain chain, bool selected)
        {
            if (selected)
                _selectedChains.Add(chain);
            else
                _selectedChains.Remove(chain);

            _continueButton.interactable = _selectedChains.Count != 0;
        }

        public void OnContinueButton()
        {
            var options = new WalletConnectModalOptions
            {
                ConnectOptions = BuildConnectOptions(),
                ExcludedWalletIds = new[]
                {
                    // Exclude Coinbase wallet because it doesn't support WalletConnect protocol
                    "fd20dc426fb37566d803205b19bbc1d4096b248ac04548e3cfb6b3a38bd033aa"
                }
            };

            WalletConnectModal.Open(options);
        }

        private ConnectOptions BuildConnectOptions()
        {
            // Using optional namespaces. Wallet will approve only chains it supports.
            var optionalNamespaces = new Dictionary<string, ProposedNamespace>();

            var selectedEvmChains = _selectedChains.Where(c => c.ChainNamespace == ChainConstants.Namespaces.Evm).ToArray();
            if (selectedEvmChains.Any())
            {
                var methods = new[]
                {
                    "wallet_switchEthereumChain",
                    "wallet_addEthereumChain",
                    "eth_sendTransaction",
                    "personal_sign"
                };

                var events = new[]
                {
                    "chainChanged", "accountsChanged"
                };

                var chainIds = selectedEvmChains.Select(c => c.ChainId).ToArray();

                optionalNamespaces.Add(ChainConstants.Namespaces.Evm, new ProposedNamespace
                {
                    Chains = chainIds,
                    Events = events,
                    Methods = methods
                });
            }

            // Non-evm chain example.
            var algorandChains = _selectedChains.Where(c => c.ChainNamespace == ChainConstants.Namespaces.Algorand).ToArray();
            if (algorandChains.Any())
            {
                var methods = new[]
                {
                    "algo_signTxn"
                };

                var events = new[]
                {
                    "accountsChanged"
                };

                var chainIds = algorandChains.Select(c => c.ChainId).ToArray();

                optionalNamespaces.Add(ChainConstants.Namespaces.Algorand, new ProposedNamespace
                {
                    Chains = chainIds,
                    Events = events,
                    Methods = methods
                });
            }

            if (optionalNamespaces.Count == 0)
                throw new InvalidOperationException("No chains selected");

            return new ConnectOptions
            {
                OptionalNamespaces = optionalNamespaces
            };
        }
    }
}