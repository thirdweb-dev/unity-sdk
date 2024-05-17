using UnityEngine;
using Thirdweb;
using System;
using TMPro;
using UnityEngine.Events;
using Thirdweb.Redcode.Awaiting;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Numerics;
using RotaryHeart.Lib.SerializableDictionary;

namespace Thirdweb.Examples
{
    public class Prefab_ConnectWallet : MonoBehaviour
    {
        [Serializable]
        public class NetworkIcon
        {
            public string chain;
            public Sprite sprite;
        }

        [Serializable]
        public class WalletIcon
        {
            public WalletProvider provider;
            public Sprite sprite;
        }

        [System.Serializable]
        public class WalletProviderUIDictionary : SerializableDictionaryBase<WalletProvider, GameObject> { }

        [Header("Enabled Wallet Providers. Press Play to see changes.")]
        public List<WalletProvider> enabledWalletProviders = new List<WalletProvider> { WalletProvider.LocalWallet, WalletProvider.InAppWallet, WalletProvider.SmartWallet };

        [Header("Use ERC-4337 (Account Abstraction) compatible smart wallets.\nEnabling this will connect user to the associated smart wallet as per your ThirwebManager settings.")]
        public bool useSmartWallets = false;

        [Header("End session on disconnect. If enabled, user will have to re-authenticate on next connect.")]
        public bool endSessionOnDisconnect = false;

        [Header("Events")]
        public UnityEvent onStart;
        public UnityEvent<WalletConnection> onConnectionRequested;
        public UnityEvent<string> onConnected;
        public UnityEvent<Exception> onConnectionError;
        public UnityEvent onDisconnected;
        public UnityEvent onSwitchNetwork;

        [Header("UI")]
        public WalletProviderUIDictionary walletProviderUI;
        public TMP_InputField emailInput;
        public GameObject exportButton;
        public List<Image> walletImages;
        public List<TMP_Text> addressTexts;
        public List<TMP_Text> balanceTexts;
        public Button networkSwitchButton;
        public Transform switchNetworkContent;
        public GameObject switchNetworkButtonPrefab;
        public List<NetworkIcon> networkIcons;
        public List<WalletIcon> walletIcons;
        public Image currentNetworkIcon;
        public TMP_Text currentNetworkText;

        private string _address;
        private string _password;
        private ChainData _currentChainData;

        private void Start()
        {
            _address = null;
            _password = null;

            _currentChainData = ThirdwebManager.Instance.supportedChains.Find(x => x.identifier == ThirdwebManager.Instance.activeChain);

            networkSwitchButton.interactable = ThirdwebManager.Instance.supportedChains.Count > 1;

            foreach (var walletProvider in walletProviderUI)
                walletProvider.Value.SetActive(enabledWalletProviders.Contains(walletProvider.Key));

            onStart.Invoke();
        }

        // Connection

        public void ConnectGuest(string password)
        {
            _password = password;
            var wc = useSmartWallets
                ? new WalletConnection(provider: WalletProvider.SmartWallet, chainId: BigInteger.Parse(_currentChainData.chainId), password: _password, personalWallet: WalletProvider.LocalWallet)
                : new WalletConnection(provider: WalletProvider.LocalWallet, chainId: BigInteger.Parse(_currentChainData.chainId), password: _password);
            Connect(wc);
        }

        public void ConnectOauth(string authProviderStr)
        {
            var wc = useSmartWallets
                ? new WalletConnection(
                    provider: WalletProvider.SmartWallet,
                    chainId: BigInteger.Parse(_currentChainData.chainId),
                    authOptions: new AuthOptions(Enum.Parse<AuthProvider>(authProviderStr)),
                    personalWallet: WalletProvider.InAppWallet
                )
                : new WalletConnection(
                    provider: WalletProvider.InAppWallet,
                    chainId: BigInteger.Parse(_currentChainData.chainId),
                    authOptions: new AuthOptions(Enum.Parse<AuthProvider>(authProviderStr))
                );
            Connect(wc);
        }

        public void ConnectEmail()
        {
            string input = emailInput.text;
            bool isEmail = Utils.IsValidEmail(input);

            WalletConnection wc;
            if (isEmail)
            {
                wc = useSmartWallets
                    ? new WalletConnection(
                        provider: WalletProvider.SmartWallet,
                        chainId: BigInteger.Parse(_currentChainData.chainId),
                        email: emailInput.text,
                        authOptions: new AuthOptions(AuthProvider.EmailOTP),
                        personalWallet: WalletProvider.InAppWallet
                    )
                    : new WalletConnection(
                        provider: WalletProvider.InAppWallet,
                        chainId: BigInteger.Parse(_currentChainData.chainId),
                        email: emailInput.text,
                        authOptions: new AuthOptions(AuthProvider.EmailOTP)
                    );
            }
            else
            {
                wc = useSmartWallets
                    ? new WalletConnection(
                        provider: WalletProvider.SmartWallet,
                        chainId: BigInteger.Parse(_currentChainData.chainId),
                        phoneNumber: input,
                        authOptions: new AuthOptions(AuthProvider.PhoneOTP),
                        personalWallet: WalletProvider.InAppWallet
                    )
                    : new WalletConnection(
                        provider: WalletProvider.InAppWallet,
                        chainId: BigInteger.Parse(_currentChainData.chainId),
                        phoneNumber: input,
                        authOptions: new AuthOptions(AuthProvider.PhoneOTP)
                    );
            }

            Connect(wc);
        }

        public void ConnectExternal(string walletProviderStr)
        {
            var wc = useSmartWallets
                ? new WalletConnection(provider: WalletProvider.SmartWallet, chainId: BigInteger.Parse(_currentChainData.chainId), personalWallet: Enum.Parse<WalletProvider>(walletProviderStr))
                : new WalletConnection(provider: Enum.Parse<WalletProvider>(walletProviderStr), chainId: BigInteger.Parse(_currentChainData.chainId));
            Connect(wc);
        }

        public async void Disconnect()
        {
            ThirdwebDebug.Log("Disconnecting...");
            try
            {
                _address = null;
                _password = null;
                await ThirdwebManager.Instance.SDK.Wallet.Disconnect(endSession: endSessionOnDisconnect);
                onDisconnected.Invoke();
            }
            catch (System.Exception e)
            {
                ThirdwebDebug.LogError($"Failed to disconnect: {e}");
            }
        }

        private async void Connect(WalletConnection wc)
        {
            ThirdwebDebug.Log($"Connecting to {wc.provider}...");

            onConnectionRequested.Invoke(wc);

            await new WaitForSeconds(0.5f);

            try
            {
                _address = await ThirdwebManager.Instance.SDK.Wallet.Connect(wc);
                exportButton.SetActive(wc.provider == WalletProvider.LocalWallet);
            }
            catch (Exception e)
            {
                _address = null;
                ThirdwebDebug.LogError($"Failed to connect: {e}");
                onConnectionError.Invoke(e);
                return;
            }

            PostConnect(wc);
        }

        private async void PostConnect(WalletConnection wc = null)
        {
            ThirdwebDebug.Log($"Connected to {_address}");

            var addy = _address.ShortenAddress();
            foreach (var addressText in addressTexts)
                addressText.text = addy;

            var bal = await ThirdwebManager.Instance.SDK.Wallet.GetBalance();
            var balStr = $"{bal.value.ToEth()} {bal.symbol}";
            foreach (var balanceText in balanceTexts)
                balanceText.text = balStr;

            if (wc != null)
            {
                var currentWalletIcon = walletIcons.Find(x => x.provider == wc.provider)?.sprite ?? walletIcons[0].sprite;
                foreach (var walletImage in walletImages)
                    walletImage.sprite = currentWalletIcon;
            }

            currentNetworkIcon.sprite = networkIcons.Find(x => x.chain == _currentChainData.identifier)?.sprite ?? networkIcons[0].sprite;
            currentNetworkText.text = PrettifyNetwork(_currentChainData.identifier);

            onConnected.Invoke(_address);
        }

        // Network switching

        public void ToggleSwitchNetworkPanel()
        {
            foreach (Transform item in switchNetworkContent)
                Destroy(item.gameObject);

            foreach (var chain in ThirdwebManager.Instance.supportedChains)
            {
                if (chain.identifier == _currentChainData.identifier)
                    continue;

                var chainData = ThirdwebManager.Instance.supportedChains.Find(x => x.identifier == chain.identifier);
                var chainButton = Instantiate(switchNetworkButtonPrefab, switchNetworkContent);
                var chainButtonImage = chainButton.transform.Find("Image_Network");
                var chainButtonText = chainButton.transform.Find("Text_Network");
                chainButtonText.GetComponentInChildren<TMP_Text>().text = PrettifyNetwork(chain.identifier);
                chainButtonImage.GetComponentInChildren<Image>().sprite = networkIcons.Find(x => x.chain == chain.identifier)?.sprite ?? networkIcons[0].sprite;
                chainButton.GetComponent<Button>().onClick.RemoveAllListeners();
                chainButton.GetComponent<Button>().onClick.AddListener(() => SwitchNetwork(chainData));
            }
        }

        private async void SwitchNetwork(ChainData chainData)
        {
            ThirdwebDebug.Log($"Switching to network: {chainData.identifier}...");
            try
            {
                await ThirdwebManager.Instance.SDK.Wallet.SwitchNetwork(BigInteger.Parse(chainData.chainId));
                ThirdwebManager.Instance.activeChain = chainData.identifier;
                _currentChainData = ThirdwebManager.Instance.supportedChains.Find(x => x.identifier == ThirdwebManager.Instance.activeChain);
                ThirdwebDebug.Log($"Switched to network: {chainData.identifier}");
                onSwitchNetwork?.Invoke();
                PostConnect();
            }
            catch (System.Exception e)
            {
                ThirdwebDebug.LogWarning($"Could not switch network! {e}");
            }
        }

        // Utility

        public async void ExportWallet()
        {
            ThirdwebDebug.Log("Exporting wallet...");
            string json = await ThirdwebManager.Instance.SDK.Wallet.Export(_password);
            await Utils.CopyToClipboard(json);
            ThirdwebDebug.Log($"Copied wallet to clipboard: {json}");
        }

        public async void CopyAddress()
        {
            await Utils.CopyToClipboard(_address);
            ThirdwebDebug.Log($"Copied address to clipboard: {_address}");
        }

        private string PrettifyNetwork(string networkIdentifier)
        {
            var replaced = networkIdentifier.Replace("-", " ");
            return replaced.Substring(0, 1).ToUpper() + replaced.Substring(1);
        }
    }
}
