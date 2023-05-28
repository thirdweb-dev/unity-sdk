using System.Collections.Generic;
using UnityEngine;

namespace Thirdweb
{
    /// <summary>
    /// The entry point for the thirdweb SDK.
    /// </summary>
    public class ThirdwebSDK
    {
        /// <summary>
        /// Options for the thirdweb SDK.
        /// </summary>
        [System.Serializable]
        public struct Options
        {
            public GaslessOptions? gasless;
            public StorageOptions? storage;
            public WalletOptions? wallet;
            public SmartWalletConfig? smartWalletConfig;
        }

        /// <summary>
        /// Wallet configuration options.
        /// </summary>
        [System.Serializable]
        public struct WalletOptions
        {
            public string appName; // the app name that will show in different wallet providers
            public string appDescription;
            public string appUrl;
            public string[] appIcons;
            public string magicLinkApiKey; // the magic link api key to use for magic link auth
            public Dictionary<string, object> extras; // extra data to pass to the wallet provider
        }

        [System.Serializable]
        public struct SmartWalletConfig
        {
            public string factoryAddress;
            public string thirdwebApiKey;
            public bool gasless;
            public string bundlerUrl;
            public string paymasterUrl;
            public string paymasterAPI;
            public string entryPointAddress;
        }

        /// <summary>
        /// Storage configuration options.
        /// </summary>
        [System.Serializable]
        public struct StorageOptions
        {
            public string ipfsGatewayUrl; // override the default ipfs gateway, should end in /ipfs/
        }

        /// <summary>
        /// Gasless configuration options.
        /// </summary>
        [System.Serializable]
        public struct GaslessOptions
        {
            public OZDefenderOptions? openzeppelin;
            public BiconomyOptions? biconomy;
            public bool experimentalChainlessSupport;
        }

        /// <summary>
        /// OpenZeppelin Defender Gasless configuration options.
        /// </summary>
        [System.Serializable]
        public struct OZDefenderOptions
        {
            public string relayerUrl;
            public string relayerForwarderAddress;
            public string domainName;
            public string domainVersion;
        }

        /// <summary>
        /// Biconomy Gasless configuration options.
        /// </summary>
        [System.Serializable]
        public struct BiconomyOptions
        {
            public string apiId;
            public string apiKey;
        }

        private string chainOrRPC;

        /// <summary>
        /// Connect and Interact with a user's wallet
        /// </summary>
        public Wallet wallet;

        /// <summary>
        /// Deploy new contracts
        /// </summary>
        public Deployer deployer;

        public Storage storage;

        public ThirdwebSession session;

        /// <summary>
        /// Create an instance of the thirdweb SDK. Requires a webGL browser context.
        /// </summary>
        /// <param name="chainOrRPC">The chain name or RPC url to connect to</param>
        /// <param name="options">Configuration options</param>
        public ThirdwebSDK(string chainOrRPC, int chainId = -1, Options options = new Options())
        {
            this.chainOrRPC = chainOrRPC;
            this.wallet = new Wallet();
            this.deployer = new Deployer();
            this.storage = new Storage(options.storage);

            if (Utils.IsWebGLBuild())
            {
                Bridge.Initialize(chainOrRPC, options);
            }
            else
            {
                if (chainId == -1)
                    throw new UnityException("Chain ID override required for native platforms!");
                string rpc = !chainOrRPC.StartsWith("https://") ? $"https://{chainOrRPC}.rpc.thirdweb.com/339d65590ba0fa79e4c8be0af33d64eda709e13652acb02c6be63f5a1fbef9c3" : chainOrRPC;
                this.session = new ThirdwebSession(options, chainId, rpc);
            }
        }

        /// <summary>
        /// Get an instance of a deployed contract.
        /// </summary>
        /// <param name="address">The contract address</param>
        /// <param name="abi">Optionally pass the ABI for contracts that cannot be auto resolved. Expected format for the ABI is escaped JSON string</param>
        /// <returns>A contract instance</returns>
        public Contract GetContract(string address, string abi = null)
        {
            return new Contract(this.chainOrRPC, address, abi);
        }
    }
}
