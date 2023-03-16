using WalletConnectSharp.Core.Models.Ethereum;

namespace WalletConnectSharp.Core
{
    /// <summary>
    /// A static class containing common Chain Ids
    /// </summary>
    public static class Chains
    {
        /// <summary>
        /// The Ethereum Mainnet Chain with a Chain Id of 0x1 (1)
        /// </summary>
        public static EthChain EthereumChainId = new EthChain() { chainId = "0x1" };

        /// <summary>
        /// The Expanse Chain with a Chain Id of 0x2 (2)
        /// </summary>
        public static EthChain ExpanseChainId = new EthChain() { chainId = "0x2" };

        /// <summary>
        /// The Ropsten Chain with a Chain Id of 0x3 (3)
        /// </summary>
        public static EthChain RopstenChainId = new EthChain() { chainId = "0x3" };

        /// <summary>
        /// The Rinkeby Chain with a Chain Id of 0x4 (4)
        /// </summary>
        public static EthChain RinkebyChainId = new EthChain() { chainId = "0x4" };

        /// <summary>
        /// The Görli Chain with a Chain Id of 0x5 (5)
        /// </summary>
        public static EthChain GörliChainId = new EthChain() { chainId = "0x5" };

        /// <summary>
        /// The Kotti Chain with a Chain Id of 0x6 (6)
        /// </summary>
        public static EthChain KottiChainId = new EthChain() { chainId = "0x6" };

        /// <summary>
        /// The Kotti Chain with a Chain Id of 0xa (10)
        /// </summary>
        public static EthChain OptimismChainId = new EthChain() { chainId = "0xa" };

        /// <summary>
        /// The Gnosis Chain with a Chain Id of 0x64 (100)
        /// </summary>
        public static EthChain GnosisChainId = new EthChain() { chainId = "0x64" };

        /// <summary>
        /// The Polygon Chain with a Chain Id of 0x89 (137)
        /// </summary>
        public static EthChain PolygonChainId = new EthChain() { chainId = "0x89" };

        /// <summary>
        /// The Arbitrum One Chain with a Chain Id of 0xa4b1 (42161)
        /// </summary>
        public static EthChain ArbitrumOneChainId = new EthChain() { chainId = "0xa4b1" };
    }
}
