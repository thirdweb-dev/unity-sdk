using UnityEngine;
using System;
using TMPro;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Numerics;
using RotaryHeart.Lib.SerializableDictionary;
using System.Linq;
using System.Threading.Tasks;

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

        [Tooltip("Invoked when the user submits an invalid OTP and can retry.")]
        public UnityEvent OnOTPVerificationFailed;

        [Header("UI")]
        public WalletProviderUIDictionary walletProviderUI;
        public TMP_InputField emailInput;
        public List<Image> walletImages;
        public List<TMP_Text> addressTexts;
        public List<TMP_Text> balanceTexts;
        public List<WalletIcon> walletIcons;

        [Header("InAppWallet UI")]
        public GameObject OTPPanel;
        public TMP_InputField OTPInput;
        public Button OTPSubmitButton;

        public IThirdwebWallet ActiveWallet => _activeWallet;

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
                    if (_activeWallet is InAppWallet)
                    {
                        // Logout
                        (_activeWallet as InAppWallet).Disconnect();
                    }
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

                if (await wallet.IsConnected())
                {
                    _address = await wallet.GetAddress();
                }
                else if (wallet is WalletConnectWallet)
                {
                    _address = await (wallet as WalletConnectWallet).Connect(BigInteger.Parse(activeChainId), allSupportedChainIds.Select(x => BigInteger.Parse(x)).ToArray());
                }
                else if (wallet is InAppWallet)
                {
                    _address = await LoginWithInAppWallet(wallet as InAppWallet, connectionParameters);
                }
                else if (wallet is PrivateKeyWallet)
                {
                    _address = await (wallet as PrivateKeyWallet).GetAddress();
                }
                else
                {
                    throw new NotImplementedException($"Wallet type {wallet.GetType()} not implemented.");
                }

                _activeWallet = wallet;

                if (useSmartWallets)
                {
                    var smartWallet = await SmartWallet.Create(_activeWallet, BigInteger.Parse(activeChainId), true);
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

        private async Task<string> LoginWithInAppWallet(InAppWallet wallet, ConnectionParameters connectionParameters)
        {
            OTPPanel.SetActive(false);
            OTPSubmitButton.onClick.RemoveAllListeners();
            OTPInput.text = "";

            var authProvider = connectionParameters.authProvider;

            if (authProvider == AuthProvider.Default)
            {
                OTPPanel.SetActive(true);
                await wallet.SendOTP();
                ThirdwebDebug.Log("Please submit the OTP sent to your email or phone.");
                OTPSubmitButton.onClick.AddListener(async () => await SubmitOTP(wallet));
            }
            else
            {
                await wallet.LoginWithOauth(
                    isMobile: Application.isMobilePlatform,
                    browserOpenAction: (url) => Application.OpenURL(url),
                    mobileRedirectScheme: "com.thirdweb.unitysdk://",
                    browser: new CrossPlatformUnityBrowser()
                );
            }

            while (!await wallet.IsConnected() && Application.isPlaying)
            {
                await Task.Delay(250);
            }

            OTPPanel.SetActive(false);
            return await wallet.GetAddress();
        }

        private async Task<InAppWallet> SubmitOTP(InAppWallet wallet)
        {
            OTPInput.interactable = false;
            OTPSubmitButton.interactable = false;

            try
            {
                var otp = OTPInput.text;
                (var inAppWalletAddress, var canRetry) = await wallet.SubmitOTP(otp);
                if (inAppWalletAddress == null && canRetry)
                {
                    ThirdwebDebug.Log("Please submit the OTP again.");
                    OTPInput.text = "";
                    OnOTPVerificationFailed.Invoke();
                }
                return wallet;
            }
            finally
            {
                OTPInput.interactable = true;
                OTPSubmitButton.interactable = true;
            }
        }

        private async void PostConnect(ConnectionParameters wc = null)
        {
            ThirdwebDebug.Log($"Connected to {_address}");

            var addy = $"{_address[..4]}...{_address[^4..]}";

            foreach (var addressText in addressTexts)
                addressText.text = addy;

            var bal = await _activeWallet.GetBalance(BigInteger.Parse(activeChainId));
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
