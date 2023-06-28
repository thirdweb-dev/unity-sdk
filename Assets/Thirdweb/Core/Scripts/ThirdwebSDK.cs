using System.Collections.Generic;
using System.Numerics;
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
            /// The API key for Magic Link authentication.
            /// </summary>
            public string magicLinkApiKey;

            /// <summary>
            /// The project ID for WalletConnect authentication.
            /// </summary>
            public string walletConnectProjectId;

            /// <summary>
            /// The client ID for Paper authentication.
            /// </summary>
            public string paperClientId;

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
            /// The API key for Thirdweb services.
            /// </summary>
            public string thirdwebApiKey;

            /// <summary>
            /// Indicates whether gasless transactions are enabled for smart wallets.
            /// </summary>
            public bool gasless;

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

        private string chainOrRPC;

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
            this.storage = new Storage(options.storage);

            string rpc = !chainOrRPC.StartsWith("https://") ? $"https://{chainOrRPC}.rpc.thirdweb.com/339d65590ba0fa79e4c8be0af33d64eda709e13652acb02c6be63f5a1fbef9c3" : chainOrRPC;

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
}
