using UnityEngine;
using System.Collections.Generic;
using System.Numerics;

namespace Thirdweb
{
    [System.Serializable]
    public class ChainData
    {
        public string identifier;
        public string chainId;
        public string rpcOverride;

        public ChainData(string identifier, string chainId, string rpcOverride)
        {
            this.identifier = identifier;
            this.chainId = chainId;
            this.rpcOverride = rpcOverride;
        }
    }

    public class ThirdwebManager : MonoBehaviour
    {
        [Tooltip("The chain to initialize the SDK with")]
        public string activeChain = "arbitrum-sepolia";

        [Tooltip("Support any chain by adding it to this list from the inspector")]
        public List<ChainData> supportedChains = new() { new ChainData("arbitrum-sepolia", "421614", null), };

        [Tooltip("Thirdweb Client ID (https://thirdweb.com/create-api-key/). Used for default thirdweb services such as RPC, Storage and Account Abstraction.")]
        public string clientId;

        [Tooltip("Whether the SDK should initialize on awake or not")]
        public bool initializeOnAwake = false;

        [Tooltip("Whether to show thirdweb sdk debug logs")]
        public bool showDebugLogs = false;

        [Tooltip("Optional Bundle ID override for thirdweb services")]
        public string bundleIdOverride = null;

        [Tooltip("General Thirdweb Settings")]
        public ThirdwebConfig thirdwebConfig;

        [Tooltip("The name of your app")]
        public string appName = null;

        [Tooltip("The description of your app")]
        public string appDescription = null;

        [Tooltip("Favicons for your app")]
        public string[] appIcons = new string[] { };

        [Tooltip("The url of your app")]
        public string appUrl = null;

        [Tooltip("IPFS Gateway Override")]
        public string storageIpfsGatewayUrl = null;

        [Tooltip("Find out more about relayers here https://portal.thirdweb.com/engine/features/relayers")]
        public string relayerUrl = null;

        [Tooltip("Forwarder Contract Address (Defaults to 0xD04F98C88cE1054c90022EE34d566B9237a1203C if left empty)")]
        public string forwarderAddress = null;

        [Tooltip("Forwarder Domain Override (Defaults to GSNv2 Forwarder if left empty)")]
        public string forwarderDomainOverride = null;

        [Tooltip("Forwarder Version (Defaults to 0.0.1 if left empty)")]
        public string forwaderVersionOverride = null;

        [Tooltip("WalletConnect Project ID (https://cloud.walletconnect.com/app)")]
        public string walletConnectProjectId = null;

        [Tooltip("WalletConnect WebGL QR Modal: enable recommended explorer wallet buttons")]
        public bool walletConnectEnableExplorer = false;

        [Tooltip("WalletConnect WebGL QR Modal: wallets to display in the WC modal (https://walletconnect.com/explorer)")]
        public string[] walletConnectExplorerRecommendedWalletIds = null;

        [Tooltip("WalletConnect WebGL QR Modal: mapping of wallet id to wallet image")]
        public List<StringPair> walletConnectWalletImages = null;

        [Tooltip("WalletConnect WebGL QR Modal: custom desktop wallets to display.")]
        public ThirdwebSDK.WalletConnectWalletOptions[] walletConnectDesktopWallets;

        [Tooltip("WalletConnect WebGL QR Modal: custom mobile wallets to display.")]
        public ThirdwebSDK.WalletConnectWalletOptions[] walletConnectMobileWallets;

        [Tooltip("WalletConnect Theme Mode (light or dark)")]
        public string walletConnectThemeMode = null;

        [Tooltip("Factory Contract Address")]
        public string factoryAddress;

        [Tooltip("Whether it should use a paymaster for gasless transactions or not")]
        public bool gasless;

        [Tooltip("Optional - If you want to use a custom erc20 paymaster, you can provide the contract address here")]
        public string erc20PaymasterAddress;

        [Tooltip("Optional - If you want to use a custom erc20 token for your paymaster, you can provide the contract address here")]
        public string erc20TokenAddress;

        [Tooltip("Optional - If you want to use a custom relayer, you can provide the URL here")]
        public string bundlerUrl;

        [Tooltip("Optional - If you want to use a custom paymaster, you can provide the URL here")]
        public string paymasterUrl;

        [Tooltip("Optional - If you want to use a custom entry point, you can provide the contract address here")]
        public string entryPointAddress;

        [Tooltip("Instantiates the WalletConnect SDK for Native platforms.")]
        public GameObject WalletConnectPrefab;

        [Tooltip("Instantiates the Metamask SDK for Native platforms.")]
        public GameObject MetamaskPrefab;

        [Tooltip("Instantiates the InAppWallet SDK for Native platforms.")]
        public GameObject InAppWalletPrefab;

        public ThirdwebSDK SDK;

        public static ThirdwebManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                ThirdwebDebug.LogWarning("Two ThirdwebManager instances were found, removing this one.");
                Destroy(this.gameObject);
                return;
            }

            if (initializeOnAwake)
                Initialize(activeChain);
        }

        public void Initialize(string chainIdentifier)
        {
            // Pass supported chains with replaced RPCs

            var options = new ThirdwebSDK.Options();

            // Set up Client ID and Bundle ID

            options.clientId = string.IsNullOrEmpty(clientId) ? null : clientId;
            options.bundleId = string.IsNullOrEmpty(bundleIdOverride) ? Application.identifier.ToLower() : bundleIdOverride;

            // Set up supported chains

            if (supportedChains.Find(x => x.identifier == chainIdentifier) == null)
                throw new UnityException("Please add your active chain to the supported chains list! See https://thirdweb.com/dashboard/rpc for a list of supported chains.");

            activeChain = chainIdentifier;
            string activeChainId = null;
            string activeChainRpc = null;

            var supportedChainData = new List<ThirdwebChainData>();
            foreach (var chainData in this.supportedChains)
            {
                if (string.IsNullOrEmpty(chainData.identifier))
                    throw new UnityException($"You must provide a valid chain identifier! See https://thirdweb.com/dashboard/rpc for a list of supported chains.");

                if (string.IsNullOrEmpty(chainData.chainId) || !BigInteger.TryParse(chainData.chainId, out _))
                    throw new UnityException($"Could not add {chainData.identifier} to supported chains, you must provide a valid chain ID!");

                if (!string.IsNullOrEmpty(chainData.rpcOverride) && !chainData.rpcOverride.StartsWith("https://"))
                    throw new UnityException($"Could not add {chainData.identifier} to supported chains, RPC overrides must start with https:// or be left empty to use thirdweb RPCs!");

                string rpc = string.IsNullOrEmpty(chainData.rpcOverride)
                    ? (string.IsNullOrEmpty(clientId) ? $"https://{chainData.chainId}.rpc.thirdweb.com/" : $"https://{chainData.chainId}.rpc.thirdweb.com/{clientId}")
                    : chainData.rpcOverride;

                if (options.clientId != null && Utils.IsThirdwebRequest(rpc))
                    rpc = rpc.AppendBundleIdQueryParam(options.bundleId);

                if (chainData.identifier == activeChain)
                {
                    activeChainId = chainData.chainId;
                    activeChainRpc = rpc;
                }

                try
                {
                    supportedChainData.Add(ThirdwebSession.FetchChainData(BigInteger.Parse(chainData.chainId), rpc));
                }
                catch (System.Exception e)
                {
                    ThirdwebDebug.LogWarning($"Failed to fetch chain data for {chainData.identifier} ({chainData.chainId}) - {e}, skipping...");
                    continue;
                }
            }

            options.supportedChains = supportedChainData.ToArray();

            // Set up storage and gasless options (if any)

            if (!string.IsNullOrEmpty(storageIpfsGatewayUrl))
            {
                options.storage = new ThirdwebSDK.StorageOptions() { ipfsGatewayUrl = storageIpfsGatewayUrl };
            }
            if (!string.IsNullOrEmpty(relayerUrl))
            {
                options.gasless = new ThirdwebSDK.RelayerOptions()
                {
                    engine = new ThirdwebSDK.EngineRelayerOptions()
                    {
                        relayerUrl = this.relayerUrl,
                        relayerForwarderAddress = string.IsNullOrEmpty(this.forwarderAddress) ? "0xD04F98C88cE1054c90022EE34d566B9237a1203C" : this.forwarderAddress,
                        domainName = string.IsNullOrEmpty(this.forwarderDomainOverride) ? "GSNv2 Forwarder" : this.forwarderDomainOverride,
                        domainVersion = string.IsNullOrEmpty(this.forwaderVersionOverride) ? "0.0.1" : this.forwaderVersionOverride
                    }
                };
            }

            // Set up wallet data

            thirdwebConfig = Resources.Load<ThirdwebConfig>("ThirdwebConfig");

            // Setup WalletConnect options

            var wcImages = new Dictionary<string, string>();
            if (walletConnectWalletImages != null && walletConnectWalletImages.Count > 0)
                foreach (var pair in walletConnectWalletImages)
                    wcImages.Add(pair.key, pair.value);

            if (walletConnectDesktopWallets != null && walletConnectDesktopWallets.Length > 0)
            {
                for (int i = 0; i < walletConnectDesktopWallets.Length; i++)
                {
                    if (walletConnectDesktopWallets[i].links == null)
                        continue;

                    if (string.IsNullOrEmpty(walletConnectDesktopWallets[i].links.native) && string.IsNullOrEmpty(walletConnectDesktopWallets[i].links.universal))
                    {
                        walletConnectDesktopWallets[i].links = null;
                        continue;
                    }

                    walletConnectDesktopWallets[i].links.native = string.IsNullOrEmpty(walletConnectDesktopWallets[i].links.native) ? null : walletConnectDesktopWallets[i].links.native;
                    walletConnectDesktopWallets[i].links.universal = string.IsNullOrEmpty(walletConnectDesktopWallets[i].links.universal) ? null : walletConnectDesktopWallets[i].links.universal;
                }
            }

            if (walletConnectMobileWallets != null && walletConnectMobileWallets.Length > 0)
            {
                for (int i = 0; i < walletConnectMobileWallets.Length; i++)
                {
                    if (walletConnectMobileWallets[i].links == null)
                        continue;

                    if (string.IsNullOrEmpty(walletConnectMobileWallets[i].links.native) && string.IsNullOrEmpty(walletConnectMobileWallets[i].links.universal))
                    {
                        walletConnectMobileWallets[i].links = null;
                        continue;
                    }

                    walletConnectMobileWallets[i].links.native = string.IsNullOrEmpty(walletConnectMobileWallets[i].links.native) ? null : walletConnectMobileWallets[i].links.native;
                    walletConnectMobileWallets[i].links.universal = string.IsNullOrEmpty(walletConnectMobileWallets[i].links.universal) ? null : walletConnectMobileWallets[i].links.universal;
                }
            }

            options.wallet = new ThirdwebSDK.WalletOptions()
            {
                appName = string.IsNullOrEmpty(appName) ? "thirdweb powered dApp" : appName,
                appDescription = string.IsNullOrEmpty(appDescription) ? "thirdweb powered dApp" : appDescription,
                appIcons = (appIcons == null || appIcons.Length == 0 || string.IsNullOrEmpty(appIcons[0])) ? new string[] { "https://thirdweb.com/favicon.ico" } : appIcons,
                appUrl = string.IsNullOrEmpty(appUrl) ? "https://thirdweb.com" : appUrl,
                walletConnectProjectId = string.IsNullOrEmpty(walletConnectProjectId) ? "145769e410f16970a79ff77b2d89a1e0" : walletConnectProjectId,
                walletConnectEnableExplorer = walletConnectEnableExplorer,
                walletConnectExplorerRecommendedWalletIds =
                    (walletConnectExplorerRecommendedWalletIds == null || walletConnectExplorerRecommendedWalletIds.Length == 0 || string.IsNullOrEmpty(walletConnectExplorerRecommendedWalletIds[0]))
                        ? null
                        : walletConnectExplorerRecommendedWalletIds,
                walletConnectWalletImages = wcImages == null || wcImages.Count == 0 ? null : wcImages,
                walletConnectDesktopWallets = (walletConnectDesktopWallets == null || walletConnectDesktopWallets.Length == 0) ? null : walletConnectDesktopWallets,
                walletConnectMobileWallets = (walletConnectMobileWallets == null || walletConnectMobileWallets.Length == 0) ? null : walletConnectMobileWallets,
                walletConnectThemeMode = (string.IsNullOrEmpty(walletConnectThemeMode) || (walletConnectThemeMode != "light" && walletConnectThemeMode != "dark")) ? null : walletConnectThemeMode,
                customScheme = string.IsNullOrEmpty(thirdwebConfig.customScheme) ? null : thirdwebConfig.customScheme,
            };

            options.smartWalletConfig = new ThirdwebSDK.SmartWalletConfig()
            {
                factoryAddress = string.IsNullOrEmpty(factoryAddress) ? Thirdweb.AccountAbstraction.Constants.DEFAULT_FACTORY_ADDRESS : factoryAddress,
                gasless = gasless,
                erc20PaymasterAddress = string.IsNullOrEmpty(erc20PaymasterAddress) ? null : erc20PaymasterAddress,
                erc20TokenAddress = string.IsNullOrEmpty(erc20TokenAddress) ? null : erc20TokenAddress,
                bundlerUrl = string.IsNullOrEmpty(bundlerUrl) ? $"https://{activeChainId}.bundler.thirdweb.com" : bundlerUrl,
                paymasterUrl = string.IsNullOrEmpty(paymasterUrl) ? $"https://{activeChainId}.bundler.thirdweb.com" : paymasterUrl,
                entryPointAddress = string.IsNullOrEmpty(entryPointAddress) ? Thirdweb.AccountAbstraction.Constants.DEFAULT_ENTRYPOINT_ADDRESS : entryPointAddress,
            };

            // Pass active chain rpc and chainId

            SDK = new ThirdwebSDK(activeChainRpc, BigInteger.Parse(activeChainId), options);
        }

        public void Initialize(string activeChainRpc, BigInteger activeChainId, ThirdwebSDK.Options options)
        {
            SDK = new ThirdwebSDK(activeChainRpc, activeChainId, options);
        }

        [System.Serializable]
        public struct StringPair
        {
            public string key;
            public string value;
        }
    }
}
