using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RotaryHeart.Lib.SerializableDictionary;
using TMPro;
using System.Numerics;
using UnityEngine.Events;

namespace Thirdweb.Examples
{
    [System.Serializable]
    public class WalletProviderUI
    {
        public GameObject objectToShow;
        public Button connectButton;
        public TMP_InputField emailInput;
        public Sprite sprite;
    }

    [System.Serializable]
    public class NetworkIcon
    {
        public string chain;
        public Sprite sprite;
    }

    [System.Serializable]
    public class WalletProviderUIDictionary : SerializableDictionaryBase<WalletProvider, WalletProviderUI> { }

    public class Prefab_ConnectWallet : MonoBehaviour
    {
        [Header("SETUP")]
        [Header("Wallets you want to support")]
        public List<WalletProvider> SupportedWallets = new List<WalletProvider>()
        {
            WalletProvider.Paper,
            WalletProvider.Injected,
            WalletProvider.Metamask,
            WalletProvider.Coinbase,
            WalletProvider.WalletConnect,
            WalletProvider.SmartWallet,
            WalletProvider.LocalWallet,
            WalletProvider.Hyperplay
        };

        [Header("Additional event callbacks")]
        public UnityEvent OnConnect;
        public UnityEvent OnDisconnect;
        public UnityEvent OnSwitchNetwork;
        public UnityEvent OnConnectFailed;
        public UnityEvent OnDisconnectFailed;
        public UnityEvent OnSwitchNetworkFailed;

        [Header("UI ELEMENTS (DANGER ZONE)")]
        [Header("Connecting State")]
        public Button ConnectButton;
        public GameObject ConnectPanel;
        public GameObject OrGameObject;
        public WalletProviderUIDictionary SupportedWalletsUI;

        [Header("Connected State")]
        public List<NetworkIcon> NetworkIcons;
        public Button ConnectedButton;
        public Image ConnectedButtonNetworkImage;
        public TMP_Text ConnectedButtonBalanceText;
        public TMP_Text ConnectedButtonAddressText;
        public Image ConnectedButtonWalletIcon;

        [Header("Connected State Dropdown")]
        public GameObject ConnectedDropdownPanel;
        public Button ConnectedDropdownCopy;
        public Button ConnectedDropdownDisconnect;
        public Image ConnectedDropdownNetworkImage;
        public TMP_Text ConnectedDropdownNetworkText;
        public TMP_Text ConnectedDropdownBalanceText;
        public TMP_Text ConnectedDropdownAddressText;
        public Image ConnectedDropdownWalletIcon;
        public Button ConnectedDropdownSwitchWalletButton;
        public Button ConnectedDropdownExportButton;

        [Header("Switch Network Panel")]
        public GameObject SwitchNetworkPanel;
        public Transform SwitchNetworkContent;
        public GameObject SwitchNetworkButtonPrefab;

        [Header("Custom LocalWallet UI")]
        public GameObject LocalWalletUISaved;
        public TMP_InputField LocalWalletSavedPasswordInput;
        public Button LocalWalletSavedConnectButton;
        public Button LocalWalletSavedCreateNewButton;
        public GameObject LocalWalletSavedPasswordWrong;
        public GameObject LocalWalletUINew;
        public TMP_InputField LocalWalletNewPasswordInput;
        public Button LocalWalletNewConnectButton;

        private ChainData _currentChainData;
        private WalletProvider _walletProvider;
        private WalletProvider _personalWalletProvider;
        private string _address;
        private string _email;
        private string _password;

        private void Start()
        {
            ConnectPanel.SetActive(false);

            ConnectButton.gameObject.SetActive(true);
            ConnectPanel.SetActive(false);
            ConnectedButton.gameObject.SetActive(false);
            ConnectedDropdownPanel.SetActive(false);
            SwitchNetworkPanel.SetActive(false);
            LocalWalletUISaved.SetActive(false);
            LocalWalletUINew.SetActive(false);

            ConnectButton.onClick.RemoveAllListeners();
            ConnectButton.onClick.AddListener(() => ToggleConnectPanel(true));

            if (SupportedWallets == null || SupportedWalletsUI.Count == 0)
                throw new UnityException("Please add at least one supported wallet!");

            foreach (var walletUI in SupportedWalletsUI)
            {
                walletUI.Value.objectToShow.SetActive(SupportedWallets.Contains(walletUI.Key));
                walletUI.Value.connectButton.onClick.AddListener(() => ValidateConnection(walletUI.Key));
            }

            bool usingEmailWallet = SupportedWallets.Contains(WalletProvider.MagicLink) || SupportedWallets.Contains(WalletProvider.Paper);
            bool usingNormalWallet =
                SupportedWallets.Contains(WalletProvider.Metamask)
                || SupportedWallets.Contains(WalletProvider.Coinbase)
                || SupportedWallets.Contains(WalletProvider.WalletConnect)
                || SupportedWallets.Contains(WalletProvider.Injected)
                || SupportedWallets.Contains(WalletProvider.SmartWallet)
                || SupportedWallets.Contains(WalletProvider.Hyperplay);

            OrGameObject.SetActive(usingEmailWallet && usingNormalWallet);

            _currentChainData = ThirdwebManager.Instance.supportedChains.Find(x => x.identifier == ThirdwebManager.Instance.activeChain);
            _address = null;
            _email = null;
            _password = null;
        }

        public void ToggleConnectPanel(bool active)
        {
            ConnectPanel.SetActive(active);
        }

        private void ValidateConnection(WalletProvider walletProvider)
        {
            Debug.Log("ValidateConnection: " + walletProvider);

            ConnectPanel.SetActive(false);

            WalletProvider personalWallet = WalletProvider.LocalWallet;

            if (walletProvider == WalletProvider.Paper || personalWallet == WalletProvider.Paper)
            {
                if (SupportedWalletsUI[WalletProvider.Paper].emailInput == null || string.IsNullOrEmpty(SupportedWalletsUI[WalletProvider.Paper].emailInput.text))
                {
                    Debug.LogWarning("Could not connect, no email provided!");
                    ConnectPanel.SetActive(false);
                    return;
                }
                else
                {
                    _email = SupportedWalletsUI[WalletProvider.Paper].emailInput.text;
                }
            }
            else if (walletProvider == WalletProvider.MagicLink || personalWallet == WalletProvider.MagicLink)
            {
                if (SupportedWalletsUI[WalletProvider.MagicLink].emailInput == null || string.IsNullOrEmpty(SupportedWalletsUI[WalletProvider.MagicLink].emailInput.text))
                {
                    Debug.LogWarning("Could not connect, no email provided!");
                    ConnectPanel.SetActive(false);
                    return;
                }
                else
                {
                    _email = SupportedWalletsUI[WalletProvider.MagicLink].emailInput.text;
                }
            }
            else if (walletProvider == WalletProvider.LocalWallet)
            {
                if (!Utils.IsWebGLBuild() && Utils.HasStoredAccount())
                    ToggleLocalWalletUISaved(true);
                else
                    ToggleLocalWalletUINew(true);
                return;
            }

            ConnectWallet(walletProvider, _password, _email, personalWallet);
        }

        public void ToggleLocalWalletUISaved(bool active)
        {
            LocalWalletUISaved.SetActive(active);
            LocalWalletSavedConnectButton.onClick.RemoveAllListeners();
            LocalWalletSavedConnectButton.onClick.AddListener(() =>
            {
                _password = string.IsNullOrEmpty(LocalWalletSavedPasswordInput.text) ? null : LocalWalletSavedPasswordInput.text;
                try
                {
                    Utils.UnlockOrGenerateLocalAccount(BigInteger.Parse(_currentChainData.chainId), _password);
                }
                catch (UnityException)
                {
                    LocalWalletSavedPasswordWrong.SetActive(true);
                    return;
                }
                ConnectWallet(WalletProvider.LocalWallet, _password, null, WalletProvider.LocalWallet);
                LocalWalletUISaved.SetActive(false);
            });

            LocalWalletSavedCreateNewButton.onClick.AddListener(() =>
            {
                LocalWalletUISaved.SetActive(false);
                ToggleLocalWalletUINew(true);
            });

            if (!active)
                ToggleConnectPanel(true);
        }

        public void ToggleLocalWalletUINew(bool active)
        {
            LocalWalletUINew.SetActive(active);
            LocalWalletNewConnectButton.onClick.RemoveAllListeners();
            LocalWalletNewConnectButton.onClick.AddListener(() =>
            {
                if (!Utils.IsWebGLBuild() && Utils.HasStoredAccount())
                    Utils.DeleteLocalAccount();

                _password = string.IsNullOrEmpty(LocalWalletNewPasswordInput.text) ? null : LocalWalletNewPasswordInput.text;
                ConnectWallet(WalletProvider.LocalWallet, _password, null, WalletProvider.LocalWallet);
                LocalWalletUINew.SetActive(false);
            });

            if (!active)
                ToggleConnectPanel(true);
        }

        private async void ConnectWallet(WalletProvider walletProvider, string password, string email, WalletProvider personalWallet)
        {
            Debug.Log($"Connecting to Wallet Provider: {walletProvider}...");

            try
            {
                _walletProvider = walletProvider;
                _personalWalletProvider = personalWallet;
                _address = await ThirdwebManager.Instance.SDK.wallet.Connect(new WalletConnection(walletProvider, BigInteger.Parse(_currentChainData.chainId), password, email, personalWallet));
                ShowConnectedState();
                OnConnect?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not connect to Wallet Provider: {walletProvider}! {e}");
                ConnectPanel.SetActive(false);
                LocalWalletUISaved.SetActive(false);
                LocalWalletUINew.SetActive(false);
                OnConnectFailed?.Invoke();
            }
        }

        private async void ShowConnectedState()
        {
            Debug.Log($"Connected to: {_address}");

            var chainSprite = NetworkIcons.Find(x => x.chain == _currentChainData.identifier)?.sprite;
            var walletSprite = SupportedWalletsUI[_walletProvider].sprite;
            var balance = await ThirdwebManager.Instance.SDK.wallet.GetBalance();

            ConnectPanel.SetActive(false);
            ConnectButton.gameObject.SetActive(false);
            ConnectedDropdownPanel.SetActive(false);
            ConnectedButton.gameObject.SetActive(true);

            ConnectedButtonNetworkImage.sprite = chainSprite;
            ConnectedButtonBalanceText.text = $"{balance.value.ToEth()} {balance.symbol}";
            ConnectedButtonAddressText.text = _address.ShortenAddress();
            ConnectedButtonWalletIcon.sprite = walletSprite;

            ConnectedButton.onClick.RemoveAllListeners();
            ConnectedButton.onClick.AddListener(() => ToggleConnectedDropdown(true));
        }

        private void ToggleConnectedDropdown(bool active)
        {
            ConnectedDropdownPanel.SetActive(active);

            ConnectedButton.onClick.RemoveAllListeners();
            ConnectedButton.onClick.AddListener(() => ToggleConnectedDropdown(!active));

            ConnectedDropdownNetworkImage.sprite = ConnectedButtonNetworkImage.sprite;
            ConnectedDropdownNetworkText.text = PrettifyNetwork(_currentChainData.identifier);
            ConnectedDropdownBalanceText.text = ConnectedButtonBalanceText.text;
            ConnectedDropdownAddressText.text = ConnectedButtonAddressText.text;
            ConnectedDropdownWalletIcon.sprite = ConnectedButtonWalletIcon.sprite;

            ConnectedDropdownCopy.onClick.RemoveAllListeners();
            ConnectedDropdownCopy.onClick.AddListener(() => CopyAddress());

            ConnectedDropdownDisconnect.onClick.RemoveAllListeners();
            ConnectedDropdownDisconnect.onClick.AddListener(() => DisconnectWallet());

            ConnectedDropdownSwitchWalletButton.onClick.RemoveAllListeners();
            ConnectedDropdownSwitchWalletButton.onClick.AddListener(() => ToggleSwitchNetworkPanel(true));

            ConnectedDropdownExportButton.onClick.RemoveAllListeners();
            ConnectedDropdownExportButton.onClick.AddListener(() => ExportWallet());

            ConnectedDropdownSwitchWalletButton.gameObject.SetActive(ThirdwebManager.Instance.supportedChains.Count > 1);
            ConnectedDropdownExportButton.gameObject.SetActive(
                _walletProvider == WalletProvider.LocalWallet || (_walletProvider == WalletProvider.SmartWallet && _personalWalletProvider == WalletProvider.LocalWallet)
            );
        }

        public void ToggleSwitchNetworkPanel(bool active)
        {
            Debug.Log("ToggleSwitchNetworkPanel: " + active);
            SwitchNetworkPanel.SetActive(active);

            foreach (Transform item in SwitchNetworkContent)
                Destroy(item.gameObject);

            foreach (var chain in ThirdwebManager.Instance.supportedChains)
            {
                if (chain.identifier == _currentChainData.identifier)
                    continue;

                var chainData = ThirdwebManager.Instance.supportedChains.Find(x => x.identifier == chain.identifier);
                var chainButton = Instantiate(SwitchNetworkButtonPrefab, SwitchNetworkContent);
                var chainButtonImage = chainButton.transform.Find("Image_Network");
                var chainButtonText = chainButton.transform.Find("Text_Network");
                chainButtonText.GetComponentInChildren<TMP_Text>().text = PrettifyNetwork(chain.identifier);
                chainButtonImage.GetComponentInChildren<Image>().sprite = NetworkIcons.Find(x => x.chain == chain.identifier).sprite;
                chainButton.GetComponent<Button>().onClick.RemoveAllListeners();
                chainButton.GetComponent<Button>().onClick.AddListener(() => SwitchNetwork(chainData));
            }
        }

        private async void SwitchNetwork(ChainData chainData)
        {
            Debug.Log($"Switching to network: {chainData.identifier}...");
            try
            {
                await ThirdwebManager.Instance.SDK.wallet.SwitchNetwork(BigInteger.Parse(chainData.chainId));
                _currentChainData = chainData;
                SwitchNetworkPanel.SetActive(false);
                ShowConnectedState();
                OnSwitchNetwork?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not switch network! {e}");
                SwitchNetworkPanel.SetActive(false);
                OnSwitchNetworkFailed?.Invoke();
            }
        }

        private async void CopyAddress()
        {
            string address = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            GUIUtility.systemCopyBuffer = address;
            Debug.Log($"Copied address to clipboard: {address}");
        }

        private async void DisconnectWallet()
        {
            Debug.Log("Disconnecting wallet...");
            try
            {
                await ThirdwebManager.Instance.SDK.wallet.Disconnect();
                ConnectButton.gameObject.SetActive(true);
                ConnectedButton.gameObject.SetActive(false);
                ConnectedDropdownPanel.SetActive(false);
                OnDisconnect?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not disconnect wallet! {e}");
                ConnectedDropdownPanel.SetActive(false);
                OnDisconnectFailed?.Invoke();
            }
        }

        private async void ExportWallet()
        {
            Debug.Log("Exporting wallet...");
            string json = await ThirdwebManager.Instance.SDK.wallet.Export(_password);
            GUIUtility.systemCopyBuffer = json;
            Debug.Log($"Copied wallet to clipboard: {json}");
        }

        private string PrettifyNetwork(string networkIdentifier)
        {
            var replaced = networkIdentifier.Replace("-", " ");
            return replaced.Substring(0, 1).ToUpper() + replaced.Substring(1);
        }
    }
}
