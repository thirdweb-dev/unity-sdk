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

        public InAppWalletOptions(
            string email = null,
            string phoneNumber = null,
            AuthProvider authprovider = AuthProvider.Default,
            string jwtOrPayload = null,
            string encryptionKey = null,
            string storageDirectoryPath = null
        )
        {
            Email = email;
            PhoneNumber = phoneNumber;
            AuthProvider = authprovider;
            JwtOrPayload = jwtOrPayload;
            EncryptionKey = encryptionKey;
            StorageDirectoryPath = storageDirectoryPath ?? Application.persistentDataPath;
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

        public SmartWalletOptions(bool sponsorGas, string factoryAddress = null, string accountAddressOverride = null, string entryPoint = null, string bundlerUrl = null, string paymasterUrl = null)
        {
            SponsorGas = sponsorGas;
            FactoryAddress = factoryAddress;
            AccountAddressOverride = accountAddressOverride;
            EntryPoint = entryPoint;
            BundlerUrl = bundlerUrl;
            PaymasterUrl = paymasterUrl;
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

        [field: SerializeField, Header("Wallet Settings")]
        private ulong[] SupportedChains { get; set; } = new ulong[] { 421614 };

        public ThirdwebClient Client { get; private set; }

        public IThirdwebWallet ActiveWallet { get; private set; }

        public static ThirdwebManager Instance { get; private set; }

        private const string THIRDWEB_UNITY_SDK_VERSION = "5.0.0-beta.1";

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
                ThirdwebDebug.LogError("ClientId and BundleId must be set in order to initialize ThirdwebManager.");
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

        public IThirdwebWallet AddWallet(IThirdwebWallet wallet)
        {
            var address = wallet.GetAddress().Result;
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
                        authprovider: walletOptions.InAppWalletOptions?.AuthProvider ?? AuthProvider.Default,
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

            if (!await wallet.IsConnected() && walletOptions.Provider == WalletProvider.InAppWallet)
            {
                ThirdwebDebug.Log("Session does not exist or is expired, proceeding with InAppWallet authentication.");

                var inAppWallet = wallet as InAppWallet;

                if (walletOptions.InAppWalletOptions.AuthProvider == AuthProvider.Default)
                {
                    await inAppWallet.SendOTP();
                    _ = await InAppWalletModal.VerifyOTP(inAppWallet);
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

            if (walletOptions.SmartWalletOptions != null)
            {
                wallet = await SmartWallet.Create(
                    personalWallet: wallet,
                    chainId: walletOptions.ChainId,
                    gasless: walletOptions.SmartWalletOptions.SponsorGas,
                    factoryAddress: walletOptions.SmartWalletOptions.FactoryAddress,
                    accountAddressOverride: walletOptions.SmartWalletOptions.AccountAddressOverride,
                    entryPoint: walletOptions.SmartWalletOptions.EntryPoint,
                    bundlerUrl: walletOptions.SmartWalletOptions.BundlerUrl,
                    paymasterUrl: walletOptions.SmartWalletOptions.PaymasterUrl
                );
            }

            AddWallet(wallet);
            SetActiveWallet(wallet);

            return wallet;
        }
    }
}
