using UnityEngine;

namespace WalletConnect
{
    public class Chain
    {
        public static readonly string EvmNamespace = "eip155";
        
        
        public static readonly Chain Ethereum = new Chain(EvmNamespace, "1", nameof(Ethereum), new Color(0.38f, 0.49f, 0.92f));
        public static readonly Chain Optimism = new Chain(EvmNamespace, "10", nameof(Optimism), new Color(0.91f, 0f, 0f));
        public static readonly Chain xDai = new Chain(EvmNamespace, "100", nameof(xDai), new Color(0.28f, 0.66f, 0.65f));
        public static readonly Chain Polygon = new Chain(EvmNamespace, "137", nameof(Polygon), new Color(0.51f, 0.28f, 0.9f));
        public static readonly Chain Arbitrum = new Chain(EvmNamespace, "42161", nameof(Arbitrum), new Color(0.17f, 0.22f, 0.29f));
        public static readonly Chain Celo = new Chain(EvmNamespace, "42220", nameof(Celo), new Color(0.21f, 0.82f, 0.5f));
        
        public static readonly Chain EthereumGoerli = new Chain(EvmNamespace, "5", "Ethereum Goerli", new Color(0.38f, 0.49f, 0.92f), true);
        public static readonly Chain OptimismGoerli = new Chain(EvmNamespace, "420", "Optimism Goerli", new Color(0.91f, 0f, 0f), true);
        public static readonly Chain Mumbai = new Chain(EvmNamespace, "80001", nameof(Mumbai), new Color(0.51f, 0.28f, 0.9f), true);
        public static readonly Chain ArbitrumRinkeby = new Chain(EvmNamespace, "421611", "Arbitrum Rinkeby", new Color(0.17f, 0.22f, 0.29f), true);
        public static readonly Chain CeloAlfajores = new Chain(EvmNamespace, "44787", "Celo Alfajores", new Color(0.21f, 0.82f, 0.5f), true);
        
        // TODO Find chain namespaces and ids for the following
        //public static readonly Chain CosmosHub = new Chain("cosmos", "cosmoshub-4", "Cosmos Hub");
        /*public static readonly Chain SolanaMainnet = new Chain(EvmNamespace, "42220", "Solana Mainnet");
        public static readonly Chain PolkadotMainnet = new Chain(EvmNamespace, "42220", "Polkadot Mainnet");
        public static readonly Chain MultiversXMainnet = new Chain(EvmNamespace, "42220", "MultiversX Mainnet");
        public static readonly Chain TronMainnet = new Chain(EvmNamespace, "42220", "Tron Mainnet");
        public static readonly Chain Tezos = new Chain(EvmNamespace, "42220", nameof(Tezos));*/

        public static readonly Chain[] All = new[]
        {
            Ethereum,
            EthereumGoerli,
            Optimism,
            OptimismGoerli,
            Polygon,
            Mumbai,
            Arbitrum,
            ArbitrumRinkeby,
            Celo,
            CeloAlfajores,
            xDai
        };
        
        public Chain(string chainNamespace, string chainId, string name, Color primaryColor, bool testnet = false)
        {
            ChainNamespace = chainNamespace;
            Name = name;
            PrimaryColor = primaryColor;
            ChainId = chainId;
            IsTestnet = testnet;
        }
        
        public Color PrimaryColor { get; }

        public string ChainId { get; }
        
        public string Name { get; }

        public string IconUrl => $"https://blockchain-api.xyz/logos/{FullChainId}.png";
        
        public string ChainNamespace { get; }
        
        public bool IsTestnet { get; }
        public string FullChainId => $"{ChainNamespace}:{ChainId}";
    }
}