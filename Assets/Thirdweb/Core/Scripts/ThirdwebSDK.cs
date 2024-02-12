using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using UnityEngine;

namespace Thirdweb
{
    /// <summary>
    /// The entry point for the Thirdweb SDK.
    /// </summary>
    public class ThirdwebSDK
    {
        /// <summary>
        /// Options for configuring the Thirdweb SDK.
        /// </summary>
        [System.Serializable]
        public struct Options
        {
            /// <summary>
            /// Gasless configuration options for the Thirdweb SDK.
            /// </summary>
            public GaslessOptions? gasless;

            /// <summary>
            /// Storage configuration options for the Thirdweb SDK.
            /// </summary>
            public StorageOptions? storage;

            /// <summary>
            /// Wallet configuration options for the Thirdweb SDK.
            /// </summary>
            public WalletOptions? wallet;

            /// <summary>
            /// Smart wallet configuration options for the Thirdweb SDK.
            /// </summary>
            public SmartWalletConfig? smartWalletConfig;

            /// <summary>
            /// The Client ID for Thirdweb services. Generate one from the thirdweb dashboard.
            /// </summary>
            public string clientId;

            /// <summary>
            /// Optional Bundle ID override for thirdweb services.
            /// </summary>
            public string bundleId;

            public ThirdwebChainData[] supportedChains;
        }

        /// <summary>
        /// Configuration options for wallets.
        /// </summary>
        [System.Serializable]
        public struct WalletOptions
        {
            /// <summary>
            /// The name of the app that will be displayed in different wallet providers.
            /// </summary>
            public string appName;

            /// <summary>
            /// The description of the app.
            /// </summary>
            public string appDescription;

            /// <summary>
            /// The URL of the app.
            /// </summary>
            public string appUrl;

            /// <summary>
            /// An array of URLs for app icons.
            /// </summary>
            public string[] appIcons;

            /// <summary>
            /// The project ID for WalletConnect authentication.
            /// </summary>
            public string walletConnectProjectId;

            /// <summary>
            /// Wallets to display in the WC modal (https://walletconnect.com/explorer)
            /// </summary>
            public string[] walletConnectExplorerRecommendedWalletIds;

            /// <summary>
            /// When using OAuth2 (e.g. Google) to login on mobile, you can provide a redirect URL such as 'myapp://'.
            /// </summary>
            public string customScheme;

            /// <summary>
            /// Additional data to pass to the wallet provider.
            /// </summary>
            public Dictionary<string, object> extras;
        }

        /// <summary>
        /// Smart wallet configuration options.
        /// </summary>
        [System.Serializable]
        public struct SmartWalletConfig
        {
            /// <summary>
            /// The address of the factory contract for smart wallets.
            /// </summary>
            public string factoryAddress;

            /// <summary>
            /// Indicates whether gasless transactions are enabled for smart wallets.
            /// </summary>
            public bool gasless;

            /// <summary>
            /// Indicates whether to deploy the smart wallet upon signing any type of message.
            /// </summary>
            public bool deployOnSign;

            /// <summary>
            /// The URL of the bundler service.
            /// </summary>
            public string bundlerUrl;

            /// <summary>
            /// The URL of the paymaster service.
            /// </summary>
            public string paymasterUrl;

            /// <summary>
            /// The address of the entry point contract.
            /// </summary>
            public string entryPointAddress;
        }

        /// <summary>
        /// Storage configuration options.
        /// </summary>
        [System.Serializable]
        public struct StorageOptions
        {
            /// <summary>
            /// The URL of the IPFS gateway.
            /// </summary>
            public string ipfsGatewayUrl;

            /// <summary>
            /// Custom IPFS Downloader
            /// </summary>
            public IStorageDownloader downloaderOverride;

            /// <summary>
            /// Custom IPFS Uploader
            /// </summary>
            public IStorageUploader uploaderOverride;
        }

        /// <summary>
        /// Gasless configuration options.
        /// </summary>
        [System.Serializable]
        public struct GaslessOptions
        {
            /// <summary>
            /// OpenZeppelin Defender Gasless configuration options.
            /// </summary>
            public OZDefenderOptions? openzeppelin;

            /// <summary>
            /// [Obsolete] Biconomy Gasless configuration options. Biconomy is not fully supported and will be removed soon. Use OpenZeppelin Defender instead.
            /// </summary>
            [System.Obsolete("Biconomy is not fully supported and will be removed soon. Use OpenZeppelin Defender instead.")]
            public BiconomyOptions? biconomy;

            /// <summary>
            /// Indicates whether experimental chainless support is enabled.
            /// </summary>
            public bool experimentalChainlessSupport;
        }

        /// <summary>
        /// OpenZeppelin Defender Gasless configuration options.
        /// </summary>
        [System.Serializable]
        public struct OZDefenderOptions
        {
            /// <summary>
            /// The URL of the relayer service.
            /// </summary>
            public string relayerUrl;

            /// <summary>
            /// The address of the relayer forwarder.
            /// </summary>
            public string relayerForwarderAddress;

            /// <summary>
            /// The domain name for the forwarder.
            /// </summary>
            public string domainName;

            /// <summary>
            /// The version of the forwarder domain.
            /// </summary>
            public string domainVersion;
        }

        /// <summary>
        /// Biconomy Gasless configuration options.
        /// </summary>
        [System.Serializable]
        public struct BiconomyOptions
        {
            /// <summary>
            /// The API ID for Biconomy.
            /// </summary>
            public string apiId;

            /// <summary>
            /// The API key for Biconomy.
            /// </summary>
            public string apiKey;
        }

        private readonly string chainOrRPC;

        /// <summary>
        /// Connect and interact with a user's wallet.
        /// </summary>
        public Wallet wallet;

        /// <summary>
        /// Deploy new contracts.
        /// </summary>
        public Deployer deployer;

        public Storage storage;

        public ThirdwebSession session;

        internal const string version = "4.6.3";

        /// <summary>
        /// Create an instance of the Thirdweb SDK.
        /// </summary>
        /// <param name="chainOrRPC">The chain name or RPC URL to connect to.</param>
        /// <param name="chainId">The chain ID.</param>
        /// <param name="options">Configuration options.</param>
        public ThirdwebSDK(string chainOrRPC, BigInteger? chainId = null, Options options = new Options())
        {
            this.chainOrRPC = chainOrRPC;
            this.wallet = new Wallet();
            this.deployer = new Deployer();
            this.storage = new Storage(options.storage, options.clientId);

            string rpc = !chainOrRPC.StartsWith("https://")
                ? (string.IsNullOrEmpty(options.clientId) ? $"https://{chainOrRPC}.rpc.thirdweb.com/" : $"https://{chainOrRPC}.rpc.thirdweb.com/{options.clientId}")
                : chainOrRPC;

            if (new System.Uri(rpc).Host.EndsWith(".thirdweb.com") && !rpc.Contains("bundleId="))
                rpc = rpc.AppendBundleIdQueryParam();

            if (Utils.IsWebGLBuild())
            {
                Bridge.Initialize(rpc, options);
            }
            else
            {
                if (chainId == null)
                    throw new UnityException("Chain ID override required for native platforms!");
                this.session = new ThirdwebSession(options, chainId.Value, rpc);
            }

            if (string.IsNullOrEmpty(options.clientId))
                ThirdwebDebug.LogWarning(
                    "No Client ID provided. You will have limited access to thirdweb services for storage, RPC, and Account Abstraction. You can get a Client ID from https://thirdweb.com/create-api-key/"
                );

            ThirdwebDebug.Log($"Thirdweb SDK Initialized.\nRPC: {rpc}\nChain ID: {chainId}\nOptions: {JsonConvert.SerializeObject(options, Formatting.Indented)}");
        }

        /// <summary>
        /// Get an instance of a deployed contract.
        /// </summary>
        /// <param name="address">The contract address.</param>
        /// <param name="abi">Optionally pass the ABI for contracts that cannot be auto-resolved. The expected format for the ABI is an escaped JSON string.</param>
        /// <returns>A contract instance.</returns>
        public Contract GetContract(string address, string abi = null)
        {
            return new Contract(this.chainOrRPC, address, abi);
        }
    }

    public class ThirdwebChainData : ThirdwebChain
    {
        public string[] blockExplorerUrls;
        public string chainName;
        public string[] iconUrls;
        public ThirdwebNativeCurrency nativeCurrency;
        public string[] rpcUrls;
    }

    public class ThirdwebChain
    {
        public string chainId;
    }

    public class ThirdwebNativeCurrency
    {
        public string name;
        public string symbol;
        public int decimals;
    }
}
