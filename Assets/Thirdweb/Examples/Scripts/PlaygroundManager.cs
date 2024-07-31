using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Thirdweb.Unity.Examples
{
    [System.Serializable]
    public class WalletPanelUI
    {
        public WalletProvider Provider;
        public GameObject Panel;
        public Button GetAddressButton;
        public Button SignButton;
        public Button GetBalanceButton;
        public Button BackButton;
        public TMP_Text LogText;
    }

    public class PlaygroundManager : MonoBehaviour
    {
        [field: SerializeField, Header("Wallet Options")]
        public ulong ActiveChainId { get; private set; } = 421614;

        [field: SerializeField, Header("Connect Wallet")]
        public GameObject ConnectWalletPanel { get; private set; }

        [field: SerializeField]
        private Button PrivateKeyWalletButton;

        [field: SerializeField]
        private Button InAppWalletButton;

        [field: SerializeField]
        private Button WalletConnectButton;

        [field: SerializeField, Header("Wallet Panels")]
        public List<WalletPanelUI> WalletPanels;

        private ThirdwebChainData _chainDetails;

        private void Awake()
        {
            InitializePanels();
        }

        private async void Start()
        {
            _chainDetails = await Utils.FetchThirdwebChainDataAsync(client: ThirdwebManager.Instance.Client, chainId: ActiveChainId);
        }

        private void InitializePanels()
        {
            foreach (var walletPanel in WalletPanels)
            {
                walletPanel.Panel.SetActive(false);
            }

            ConnectWalletPanel.SetActive(true);

            PrivateKeyWalletButton.onClick.RemoveAllListeners();
            PrivateKeyWalletButton.onClick.AddListener(() => ConnectWallet(WalletProvider.PrivateKeyWallet));

            InAppWalletButton.onClick.RemoveAllListeners();
            InAppWalletButton.onClick.AddListener(() => ConnectWallet(WalletProvider.InAppWallet));

            WalletConnectButton.onClick.RemoveAllListeners();
            WalletConnectButton.onClick.AddListener(() => ConnectWallet(WalletProvider.WalletConnectWallet));
        }

        private async void ConnectWallet(WalletProvider provider)
        {
            // Connect the wallet

            var walletOptions = GetWalletOptions(provider);
            var wallet = await ThirdwebManager.Instance.ConnectWallet(walletOptions);

            // Initialize the wallet panel

            ConnectWalletPanel.SetActive(false);

            foreach (var walletPanel in WalletPanels)
            {
                walletPanel.Panel.SetActive(walletPanel.Provider == provider);
            }

            // Setup actions

            var currentPanel = WalletPanels.Find(panel => panel.Provider == provider);
            currentPanel.LogText.text = string.Empty;

            currentPanel.BackButton.onClick.RemoveAllListeners();
            currentPanel.BackButton.onClick.AddListener(InitializePanels);

            currentPanel.GetAddressButton.onClick.RemoveAllListeners();
            currentPanel.GetAddressButton.onClick.AddListener(async () =>
            {
                var address = await wallet.GetAddress();
                currentPanel.LogText.text = $"Address: {address}";
            });

            currentPanel.SignButton.onClick.RemoveAllListeners();
            currentPanel.SignButton.onClick.AddListener(async () =>
            {
                var message = "Hello World!";
                var signature = await wallet.PersonalSign(message);
                currentPanel.LogText.text = $"Signature: {signature}";
            });

            currentPanel.GetBalanceButton.onClick.RemoveAllListeners();
            currentPanel.GetBalanceButton.onClick.AddListener(async () =>
            {
                var balance = await wallet.GetBalance(chainId: ActiveChainId);
                currentPanel.LogText.text = $"Balance: {balance} {_chainDetails.NativeCurrency.Symbol}";
            });
        }

        private WalletOptions GetWalletOptions(WalletProvider provider)
        {
            switch (provider)
            {
                case WalletProvider.PrivateKeyWallet:
                    return new WalletOptions(provider: WalletProvider.PrivateKeyWallet, chainId: ActiveChainId);
                case WalletProvider.InAppWallet:
                    var inAppWalletOptions = new InAppWalletOptions(authprovider: AuthProvider.Google);
                    return new WalletOptions(provider: WalletProvider.InAppWallet, chainId: ActiveChainId, inAppWalletOptions: inAppWalletOptions);
                case WalletProvider.WalletConnectWallet:
                    return new WalletOptions(provider: WalletProvider.WalletConnectWallet, chainId: ActiveChainId);
                default:
                    throw new System.NotImplementedException("Wallet provider not implemented for this example.");
            }
        }
    }
}
