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
using System.Linq;

namespace Thirdweb.Unity.Examples
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

        public enum WalletProvider
        {
            PrivateKeyWallet,
            InAppWallet,
            WalletConnectWallet,
            SmartWallet
        }

        public class ConnectionParameters
        {
            public WalletProvider provider;
            public string email;
            public string phoneNumber;
            public AuthProvider authProvider;
        }

        [Serializable]
        public class WalletProviderUIDictionary : SerializableDictionaryBase<WalletProvider, GameObject> { }

        [Header("Enabled Wallet Providers. Press Play to see changes.")]
        public List<WalletProvider> enabledWalletProviders = new() { WalletProvider.PrivateKeyWallet, WalletProvider.InAppWallet, WalletProvider.WalletConnectWallet };

        [Header("Use ERC-4337 (Account Abstraction) compatible smart wallets.\nEnabling this will connect user to the associated smart wallet as per your ThirwebManager settings.")]
        public bool useSmartWallets = false;

        [Header("Chains to connect to.")]
        public string activeChainId = "421614";
        public string[] allSupportedChainIds = new[] { "421614" };

        [Header("Events")]
        public UnityEvent<IThirdwebWallet> OnConnected;
        public UnityEvent OnDisconnected;
        public UnityEvent<IThirdwebWallet, ConnectionParameters> OnConnectionRequested;
        public UnityEvent<IThirdwebWallet, ConnectionParameters> OnConnectionStarted;
        public UnityEvent<Exception> OnConnectionFailed;

        [Header("UI")]
        public WalletProviderUIDictionary walletProviderUI;
        public TMP_InputField emailInput;
        public List<Image> walletImages;
        public List<TMP_Text> addressTexts;
        public List<TMP_Text> balanceTexts;
        public List<WalletIcon> walletIcons;

        private string _address;

        private IThirdwebWallet _activeWallet;
        private ThirdwebClient _client;

        private void Awake()
        {
            OnConnectionRequested.AddListener(Connect);
            OnConnectionFailed.AddListener(e => ThirdwebDebug.LogError($"Connection failed: {e}"));
        }

        private void Start()
        {
            _address = null;
            _client = ThirdwebManager.Instance.Client;

            foreach (var walletProvider in walletProviderUI)
                walletProvider.Value.SetActive(enabledWalletProviders.Contains(walletProvider.Key));
        }

        // Connection

        public async void ConnectGuest()
        {
            try
            {
                var privateKeyWallet = await PrivateKeyWallet.Generate(client: _client);
                OnConnectionRequested.Invoke(privateKeyWallet, new ConnectionParameters() { provider = WalletProvider.PrivateKeyWallet });
            }
            catch (Exception e)
            {
                OnConnectionFailed.Invoke(e);
                return;
            }
        }

        public async void ConnectOauth(string authProviderStr)
        {
            try
            {
                var inAppWallet = await InAppWallet.Create(
                    client: _client,
                    email: null,
                    phoneNumber: null,
                    authprovider: Enum.Parse<AuthProvider>(authProviderStr),
                    storageDirectoryPath: Application.persistentDataPath
                );

                OnConnectionRequested.Invoke(
                    inAppWallet,
                    new ConnectionParameters()
                    {
                        provider = WalletProvider.InAppWallet,
                        email = null,
                        phoneNumber = null,
                        authProvider = Enum.Parse<AuthProvider>(authProviderStr),
                    }
                );
            }
            catch (Exception e)
            {
                OnConnectionFailed.Invoke(e);
                return;
            }
        }

        public async void ConnectEmailOrPhone()
        {
            try
            {
                var input = emailInput.text;
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^\S+@\S+\.\S+$");
                var isEmail = emailRegex.IsMatch(input.Replace("+", ""));

                if (isEmail)
                {
                    var inAppWallet = await InAppWallet.Create(
                        client: _client,
                        email: input,
                        phoneNumber: null,
                        authprovider: AuthProvider.Default,
                        storageDirectoryPath: Application.persistentDataPath
                    );
                    OnConnectionRequested.Invoke(
                        inAppWallet,
                        new ConnectionParameters()
                        {
                            provider = WalletProvider.InAppWallet,
                            email = input,
                            phoneNumber = null,
                            authProvider = AuthProvider.Default,
                        }
                    );
                }
                else
                {
                    var inAppWallet = await InAppWallet.Create(
                        client: _client,
                        email: null,
                        phoneNumber: input,
                        authprovider: AuthProvider.Default,
                        storageDirectoryPath: Application.persistentDataPath
                    );
                    OnConnectionRequested.Invoke(
                        inAppWallet,
                        new ConnectionParameters()
                        {
                            provider = WalletProvider.InAppWallet,
                            email = null,
                            phoneNumber = input,
                            authProvider = AuthProvider.Default,
                        }
                    );
                }
            }
            catch (Exception e)
            {
                OnConnectionFailed.Invoke(e);
                return;
            }
        }

        public async void ConnectExternal(string walletProviderStr)
        {
            try
            {
                var walletProvider = Enum.Parse<WalletProvider>(walletProviderStr);
                switch (walletProvider)
                {
                    case WalletProvider.WalletConnectWallet:
                        var wcWallet = await WalletConnectWallet.Create(client: _client);
                        OnConnectionRequested.Invoke(wcWallet, new ConnectionParameters() { provider = WalletProvider.WalletConnectWallet });
                        break;
                    default:
                        throw new NotImplementedException($"Wallet provider {walletProvider} not implemented.");
                }
            }
            catch (Exception e)
            {
                OnConnectionFailed.Invoke(e);
                return;
            }
        }

        public void Disconnect()
        {
            if (_activeWallet != null)
            {
                ThirdwebDebug.Log("Disconnecting...");
                try
                {
                    _activeWallet = null;
                    OnDisconnected.Invoke();
                }
                catch (Exception e)
                {
                    ThirdwebDebug.LogError($"Failed to disconnect: {e}");
                }
            }
            else
            {
                ThirdwebDebug.LogWarning("No active wallet to disconnect.");
            }
        }

        private async void Connect(IThirdwebWallet wallet, ConnectionParameters connectionParameters)
        {
            try
            {
                OnConnectionStarted.Invoke(wallet, connectionParameters);

                _address = wallet switch
                {
                    WalletConnectWallet wcWallet => await wcWallet.Connect(BigInteger.Parse(activeChainId), allSupportedChainIds.Select(x => BigInteger.Parse(x)).ToArray()),
                    InAppWallet inAppWallet => await InAppWalletModal.Instance.Connect(wallet: inAppWallet, authprovider: connectionParameters.authProvider),
                    PrivateKeyWallet privateKeyWallet => await privateKeyWallet.GetAddress(),
                    _ => throw new NotImplementedException($"Wallet type {wallet.GetType()} not implemented."),
                };

                _activeWallet = wallet;

                if (useSmartWallets)
                {
                    var smartWallet = await SmartWallet.Create(_client, _activeWallet, BigInteger.Parse(activeChainId), true);
                    _address = await smartWallet.GetAddress();
                    _activeWallet = smartWallet;
                }

                PostConnect(connectionParameters);
            }
            catch (Exception e)
            {
                OnConnectionFailed.Invoke(e);
                return;
            }
        }

        private async void PostConnect(ConnectionParameters wc = null)
        {
            ThirdwebDebug.Log($"Connected to {_address}");

            var addy = $"{_address[..4]}...{_address[^4..]}";

            foreach (var addressText in addressTexts)
                addressText.text = addy;

            var bal = await _activeWallet.GetBalance(_client, BigInteger.Parse(activeChainId));
            var balStr = $"{bal.ToString().ToEth()} ETH";
            foreach (var balanceText in balanceTexts)
                balanceText.text = balStr;

            if (wc != null)
            {
                var currentWalletIcon = walletIcons.Find(x => x.provider == (useSmartWallets ? WalletProvider.SmartWallet : wc.provider))?.sprite ?? walletIcons[0].sprite;
                foreach (var walletImage in walletImages)
                    walletImage.sprite = currentWalletIcon;
            }

            OnConnected.Invoke(_activeWallet);

            ThirdwebDebug.Log($"Connected to {_activeWallet.GetType()} with address {_address}");
        }

        public void CopyAddress()
        {
            GUIUtility.systemCopyBuffer = _address;
            ThirdwebDebug.Log($"Copied address to clipboard: {_address}");
        }
    }
}
