using UnityEngine;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Thirdweb.Unity
{
    public enum WalletProvider
    {
        PrivateKeyWallet,
        InAppWallet,
        WalletConnectWallet,
        MetaMaskWallet,
    }

    public class InAppWalletOptions
    {
        public string Email;
        public string PhoneNumber;
        public AuthProvider AuthProvider;
        public string JwtOrPayload;
        public string EncryptionKey;
        public string StorageDirectoryPath;
        public IThirdwebWallet SiweSigner;

        public InAppWalletOptions(
            string email = null,
            string phoneNumber = null,
            AuthProvider authprovider = AuthProvider.Default,
            string jwtOrPayload = null,
            string encryptionKey = null,
            string storageDirectoryPath = null,
            IThirdwebWallet siweSigner = null
        )
        {
            Email = email;
            PhoneNumber = phoneNumber;
            AuthProvider = authprovider;
            JwtOrPayload = jwtOrPayload;
            EncryptionKey = encryptionKey;
            StorageDirectoryPath = storageDirectoryPath ?? Application.persistentDataPath;
            SiweSigner = siweSigner;
        }
    }

    public class SmartWalletOptions
    {
        public bool SponsorGas;
        public string FactoryAddress;
        public string AccountAddressOverride;
        public string EntryPoint;
        public string BundlerUrl;
        public string PaymasterUrl;
        public string ERC20PaymasterAddress;
        public string ERC20PaymasterToken;

        public SmartWalletOptions(
            bool sponsorGas,
            string factoryAddress = null,
            string accountAddressOverride = null,
            string entryPoint = null,
            string bundlerUrl = null,
            string paymasterUrl = null,
            string erc20PaymasterAddress = null,
            string erc20PaymasterToken = null
        )
        {
            SponsorGas = sponsorGas;
            FactoryAddress = factoryAddress;
            AccountAddressOverride = accountAddressOverride;
            EntryPoint = entryPoint;
            BundlerUrl = bundlerUrl;
            PaymasterUrl = paymasterUrl;
            ERC20PaymasterAddress = erc20PaymasterAddress;
            ERC20PaymasterToken = erc20PaymasterToken;
        }
    }

    public class WalletOptions
    {
        public WalletProvider Provider;
        public BigInteger ChainId;
        public InAppWalletOptions InAppWalletOptions;
        public SmartWalletOptions SmartWalletOptions;

        public WalletOptions(WalletProvider provider, BigInteger chainId, InAppWalletOptions inAppWalletOptions = null, SmartWalletOptions smartWalletOptions = null)
        {
            Provider = provider;
            ChainId = chainId;
            InAppWalletOptions = inAppWalletOptions ?? new InAppWalletOptions();
            SmartWalletOptions = smartWalletOptions;
        }
    }

    public class ThirdwebManager : MonoBehaviour
    {
        [field: SerializeField, Header("Client Settings")]
        private string ClientId { get; set; }

        [field: SerializeField]
        private string BundleId { get; set; }

        [field: SerializeField]
        private bool InitializeOnAwake { get; set; } = true;

        [field: SerializeField]
        private bool ShowDebugLogs { get; set; } = true;

        [field: SerializeField]
        private bool OptOutUsageAnalytics { get; set; } = false;

        [field: SerializeField, Header("Wallet Settings")]
        private ulong[] SupportedChains { get; set; } = new ulong[] { 421614 };

        public ThirdwebClient Client { get; private set; }

        public IThirdwebWallet ActiveWallet { get; private set; }

        public static ThirdwebManager Instance { get; private set; }

        private const string THIRDWEB_UNITY_SDK_VERSION = "5.0.0-beta.3";

        private bool _initialized;

        private Dictionary<string, IThirdwebWallet> _walletMapping;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            ThirdwebDebug.IsEnabled = ShowDebugLogs;

            if (InitializeOnAwake)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(BundleId))
            {
                ThirdwebDebug.LogError("ClientId and BundleId must be set in order to initialize ThirdwebManager. Get your API key from https://thirdweb.com/create-api-key");
                return;
            }

            Client = ThirdwebClient.Create(
                clientId: ClientId,
                bundleId: BundleId,
                httpClient: Application.platform == RuntimePlatform.WebGLPlayer ? new Helpers.UnityThirdwebHttpClient() : new ThirdwebHttpClient(),
                headers: new Dictionary<string, string>
                {
                    { "x-sdk-name", Application.platform == RuntimePlatform.WebGLPlayer ? "UnitySDK_WebGL" : "UnitySDK" },
                    { "x-sdk-os", Application.platform.ToString() },
                    { "x-sdk-platform", "unity" },
                    { "x-sdk-version", THIRDWEB_UNITY_SDK_VERSION },
                    { "x-client-id", ClientId },
                    { "x-bundle-id", BundleId }
                }
            );

            ThirdwebDebug.Log("ThirdwebManager initialized.");

            _walletMapping = new Dictionary<string, IThirdwebWallet>();

            _initialized = true;
        }

        public async Task<ThirdwebContract> GetContract(string address, BigInteger chainId, string abi = null)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("ThirdwebManager is not initialized.");
            }

            return await ThirdwebContract.Create(Client, address, chainId, abi);
        }

        public IThirdwebWallet GetActiveWallet()
        {
            return ActiveWallet;
        }

        public void SetActiveWallet(IThirdwebWallet wallet)
        {
            ActiveWallet = wallet;
        }

        public IThirdwebWallet GetWallet(string address)
        {
            if (_walletMapping.TryGetValue(address, out var wallet))
            {
                return wallet;
            }

            throw new KeyNotFoundException($"Wallet with address {address} not found.");
        }

        public async Task<IThirdwebWallet> AddWallet(IThirdwebWallet wallet)
        {
            var address = await wallet.GetAddress();
            _walletMapping.TryAdd(address, wallet);
            return wallet;
        }

        public void RemoveWallet(string address)
        {
            if (_walletMapping.ContainsKey(address))
            {
                _walletMapping.Remove(address, out var wallet);
            }
        }

        public async Task<IThirdwebWallet> ConnectWallet(WalletOptions walletOptions)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("ThirdwebManager is not initialized.");
            }

            if (walletOptions == null)
            {
                throw new ArgumentNullException(nameof(walletOptions));
            }

            if (walletOptions.ChainId <= 0)
            {
                throw new ArgumentException("ChainId must be greater than 0.");
            }

            IThirdwebWallet wallet = null;

            switch (walletOptions.Provider)
            {
                case WalletProvider.PrivateKeyWallet:
                    wallet = await PrivateKeyWallet.Generate(client: Client);
                    break;
                case WalletProvider.InAppWallet:
                    wallet = await InAppWallet.Create(
                        client: Client,
                        email: walletOptions.InAppWalletOptions?.Email,
                        phoneNumber: walletOptions.InAppWalletOptions?.PhoneNumber,
                        authProvider: walletOptions.InAppWalletOptions?.AuthProvider ?? AuthProvider.Default,
                        storageDirectoryPath: Application.persistentDataPath
                    );
                    break;
                case WalletProvider.WalletConnectWallet:
                    var supportedChains = SupportedChains.Select(chain => new BigInteger(chain)).ToArray();
                    wallet = await WalletConnectWallet.Create(client: Client, initialChainId: walletOptions.ChainId, supportedChains: supportedChains);
                    break;
                case WalletProvider.MetaMaskWallet:
                    wallet = await MetaMaskWallet.Create(client: Client, activeChainId: walletOptions.ChainId);
                    break;
            }

            if (walletOptions.Provider == WalletProvider.InAppWallet && !await wallet.IsConnected())
            {
                ThirdwebDebug.Log("Session does not exist or is expired, proceeding with InAppWallet authentication.");

                var inAppWallet = wallet as InAppWallet;

                if (walletOptions.InAppWalletOptions.AuthProvider == AuthProvider.Default)
                {
                    await inAppWallet.SendOTP();
                    _ = await InAppWalletModal.LoginWithOtp(inAppWallet);
                }
                else if (walletOptions.InAppWalletOptions.AuthProvider == AuthProvider.Siwe)
                {
                    _ = await inAppWallet.LoginWithSiwe(walletOptions.ChainId);
                }
                else
                {
                    _ = await inAppWallet.LoginWithOauth(
                        isMobile: Application.isMobilePlatform,
                        browserOpenAction: (url) => Application.OpenURL(url),
                        mobileRedirectScheme: BundleId + "://",
                        browser: new CrossPlatformUnityBrowser()
                    );
                }
            }

            var address = await wallet.GetAddress();
            ThirdwebDebug.Log($"Wallet address: {address}");

            var isSmartWallet = walletOptions.SmartWalletOptions != null;

            if (!OptOutUsageAnalytics)
            {
                TrackUsage("connectWallet", "connect", isSmartWallet ? "smartWallet" : walletOptions.Provider.ToString()[..1].ToLower() + walletOptions.Provider.ToString()[1..], address);
            }

            if (isSmartWallet)
            {
                ThirdwebDebug.Log("Upgrading to SmartWallet.");
                return await UpgradeToSmartWallet(wallet, walletOptions.ChainId, walletOptions.SmartWalletOptions);
            }
            else
            {
                await AddWallet(wallet);
                SetActiveWallet(wallet);
                return wallet;
            }
        }

        public async Task<SmartWallet> UpgradeToSmartWallet(IThirdwebWallet personalWallet, BigInteger chainId, SmartWalletOptions smartWalletOptions)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("ThirdwebManager is not initialized.");
            }

            if (personalWallet.AccountType == ThirdwebAccountType.SmartAccount)
            {
                ThirdwebDebug.LogWarning("Wallet is already a SmartWallet.");
                return personalWallet as SmartWallet;
            }

            if (smartWalletOptions == null)
            {
                throw new ArgumentNullException(nameof(smartWalletOptions));
            }

            if (chainId <= 0)
            {
                throw new ArgumentException("ChainId must be greater than 0.");
            }

            var wallet = await SmartWallet.Create(
                personalWallet: personalWallet,
                chainId: chainId,
                gasless: smartWalletOptions.SponsorGas,
                factoryAddress: smartWalletOptions.FactoryAddress,
                accountAddressOverride: smartWalletOptions.AccountAddressOverride,
                entryPoint: smartWalletOptions.EntryPoint,
                bundlerUrl: smartWalletOptions.BundlerUrl,
                paymasterUrl: smartWalletOptions.PaymasterUrl,
                erc20PaymasterAddress: smartWalletOptions.ERC20PaymasterAddress,
                erc20PaymasterToken: smartWalletOptions.ERC20PaymasterToken
            );

            await AddWallet(wallet);
            SetActiveWallet(wallet);

            return wallet;
        }

        public async Task<List<LinkedAccount>> LinkAccount(InAppWallet mainWallet, InAppWallet walletToLink, string otp = null, BigInteger? chainId = null, string jwtOrPayload = null)
        {
            return await mainWallet.LinkAccount(
                walletToLink: walletToLink,
                otp: otp,
                isMobile: Application.isMobilePlatform,
                browserOpenAction: (url) => Application.OpenURL(url),
                mobileRedirectScheme: BundleId + "://",
                browser: new CrossPlatformUnityBrowser(),
                chainId: chainId,
                jwt: jwtOrPayload,
                payload: jwtOrPayload
            );
        }

        private async void TrackUsage(string source, string action, string walletType, string walletAddress)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(action) || string.IsNullOrEmpty(walletType) || string.IsNullOrEmpty(walletAddress))
            {
                ThirdwebDebug.LogWarning("Invalid usage analytics parameters.");
                return;
            }

            try
            {
                var content = new System.Net.Http.StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(
                        new
                        {
                            source,
                            action,
                            walletAddress,
                            walletType,
                        }
                    ),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );
                _ = await Client.HttpClient.PostAsync("https://c.thirdweb.com/event", content);
            }
            catch
            {
                ThirdwebDebug.LogWarning($"Failed to report usage analytics.");
            }
        }
    }
}
