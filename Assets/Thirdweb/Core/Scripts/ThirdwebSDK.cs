using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using Thirdweb.Pay;
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
            /// Gasless relayer configuration options for Thirdweb Engine..
            /// </summary>
            public RelayerOptions? gasless;

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
            /// WalletConnect Project ID (https://cloud.walletconnect.com/app).
            /// </summary>
            public string walletConnectProjectId;

            /// <summary>
            /// WalletConnect WebGL QR Modal: enable recommended explorer wallet buttons.
            /// </summary>
            public bool walletConnectEnableExplorer;

            /// <summary>
            /// WalletConnect WebGL QR Modal: wallets to display in the WC modal (https://walletconnect.com/explorer).
            /// </summary>
            public string[] walletConnectExplorerRecommendedWalletIds;

            /// <summary>
            /// WalletConnect WebGL QR Modal: mapping of wallet id to wallet image.
            /// </summary>
            public Dictionary<string, string> walletConnectWalletImages;

            /// <summary>
            /// WalletConnect WebGL QR Modal: custom desktop wallets to display.
            /// </summary>
            public WalletConnectWalletOptions[] walletConnectDesktopWallets;

            /// <summary>
            /// WalletConnect WebGL QR Modal: custom mobile wallets to display.
            /// </summary>
            public WalletConnectWalletOptions[] walletConnectMobileWallets;

            /// <summary>
            /// WalletConnect WebGL QR Modal: set theme to 'light' or 'dark'.
            /// </summary>
            public string walletConnectThemeMode;

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
        /// Optional wallet configuration options for WalletConnect wallets, useful for displaying specific wallets only.
        /// </summary>
        [System.Serializable]
        public struct WalletConnectWalletOptions
        {
            public string id;
            public string name;
            public WalletConnectWalletLinks links;
        }

        [System.Serializable]
        public class WalletConnectWalletLinks
        {
            public string native;
            public string universal;
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
            /// The address of your ERC20 paymaster contract if used.
            /// </summary>
            public string erc20PaymasterAddress;

            /// <summary>
            /// The address of your ERC20 token if using ERC20 paymaster.
            /// </summary>
            public string erc20TokenAddress;

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
        }

        /// <summary>
        /// Thirdweb Engine Relayer configuration options.
        /// </summary>
        [System.Serializable]
        public struct RelayerOptions
        {
            public EngineRelayerOptions engine;
        }

        [System.Serializable]
        public struct EngineRelayerOptions
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

        private readonly string chainOrRPC;

        /// <summary>
        /// Connect and interact with a user's wallet.
        /// </summary>
        public Wallet Wallet { get; internal set; }

        /// <summary>
        /// Download files from anywhere, upload files to IPFS.
        /// </summary>
        public Storage Storage { get; internal set; }

        /// <summary>
        /// Pay with crypto or fiat using the Thirdweb Pay service.
        /// </summary>
        public ThirdwebPay Pay { get; internal set; }

        /// <summary>
        /// Interact with blocks on the active chain.
        /// </summary>
        public Blocks Blocks { get; internal set; }

        public ThirdwebSession Session { get; internal set; }

        internal const string version = "4.16.4";

        /// <summary>
        /// Create an instance of the Thirdweb SDK.
        /// </summary>
        /// <param name="chainOrRPC">The chain name or RPC URL to connect to.</param>
        /// <param name="chainId">The chain ID.</param>
        /// <param name="options">Configuration options.</param>
        public ThirdwebSDK(string chainOrRPC, BigInteger chainId, Options options)
        {
            if (chainId == null || chainId == 0)
                throw new UnityException("Chain ID required!");

            this.chainOrRPC = chainOrRPC;

            string rpc = !chainOrRPC.StartsWith("https://")
                ? (string.IsNullOrEmpty(options.clientId) ? $"https://{chainOrRPC}.rpc.thirdweb.com/" : $"https://{chainOrRPC}.rpc.thirdweb.com/{options.clientId}")
                : chainOrRPC;

            if (options.clientId != null && Utils.IsThirdwebRequest(rpc))
                rpc = rpc.AppendBundleIdQueryParam(options.bundleId);

            if (Utils.IsWebGLBuild())
            {
                Bridge.Initialize(rpc, options);
            }

            this.Session = new ThirdwebSession(this, options, chainId, rpc);
            this.Wallet = new Wallet(this);
            this.Storage = new Storage(this);
            this.Pay = new ThirdwebPay(this);
            this.Blocks = new Blocks(this);

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
            return new Contract(this, Session.ChainId, address, abi);
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
