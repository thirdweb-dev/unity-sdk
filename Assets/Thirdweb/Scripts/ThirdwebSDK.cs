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

        /// <summary>
        /// Create an instance of the thirdweb SDK. Requires a webGL browser context.
        /// </summary>
        /// <param name="chainOrRPC">The chain name or RPC url to connect to</param>
        /// <param name="options">Configuration options</param>
        public ThirdwebSDK(string chainOrRPC, Options options = new Options()) 
        {
            this.chainOrRPC = chainOrRPC;
            this.wallet = new Wallet();
            this.deployer = new Deployer();
            Bridge.Initialize(chainOrRPC, options);
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