using System.Collections.Generic;
using Nethereum.Siwe;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
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
            public Dictionary<string, object> extras; // extra data to pass to the wallet provider
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

        public class NativeSession
        {
            public int lastChainId = -1;
            public string lastRPC = null;
            public Account account = null;
            public Web3 web3 = null;
            public Options options = new Options();
            public SiweMessageService siweSession = new SiweMessageService();

            public NativeSession(int lastChainId, string lastRPC, Account account, Web3 web3, Options options, SiweMessageService siweSession)
            {
                this.lastChainId = lastChainId;
                this.lastRPC = lastRPC;
                this.account = account;
                this.web3 = web3;
                this.options = options;
                this.siweSession = siweSession;
            }
        }

        public NativeSession nativeSession;

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

            if (!Utils.IsWebGLBuild())
            {
                if (chainId == -1)
                    throw new UnityException("Chain ID override required for native platforms!");

                string rpc = !chainOrRPC.StartsWith("https://") ? $"https://{chainOrRPC}.rpc.thirdweb.com/339d65590ba0fa79e4c8be0af33d64eda709e13652acb02c6be63f5a1fbef9c3" : chainOrRPC;
                nativeSession = new NativeSession(chainId, rpc, null, new Web3(rpc), options, new SiweMessageService());
                // Set default WalletOptions
                nativeSession.options.wallet = new WalletOptions()
                {
                    appName = options.wallet?.appName ?? "Thirdweb Game",
                    appDescription = options.wallet?.appDescription ?? "Thirdweb Game Demo",
                    appIcons = options.wallet?.appIcons ?? new string[] { "https://thirdweb.com/favicon.ico" },
                    appUrl = options.wallet?.appUrl ?? "https://thirdweb.com"
                };
            }
            else
            {
                Bridge.Initialize(chainOrRPC, options);
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
