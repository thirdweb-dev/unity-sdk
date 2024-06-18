using System;
using Newtonsoft.Json;
using UnityEngine.Scripting;
using WalletConnectUnity.Core.Utils;

namespace WalletConnectUnity.Core.Evm
{
    public class Transaction
    {
        [JsonProperty("from")]
        public string from;

        [JsonProperty("to")]
        public string to;

        [JsonProperty("gas", NullValueHandling = NullValueHandling.Ignore)]
        public string gas;

        [JsonProperty("gasPrice", NullValueHandling = NullValueHandling.Ignore)]
        public string gasPrice;

        [JsonProperty("value")]
        public string value;

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public string data = "0x";

        [Preserve]
        public Transaction()
        {
        }
    }

    [Serializable]
    public class EthereumChain
    {
        [JsonProperty("chainId")]
        public string chainIdHex;

        [JsonProperty("chainName")]
        public string name;

        [JsonProperty("nativeCurrency")]
        public Currency nativeCurrency;

        [JsonProperty("rpcUrls")]
        public string[] rpcUrls;

        [JsonProperty("blockExplorerUrls", NullValueHandling = NullValueHandling.Ignore)]
        public string[] blockExplorerUrls;

        [JsonIgnore]
        public string chainIdDecimal;

        [Preserve]
        public EthereumChain()
        {
        }

        public EthereumChain(string chainId, string name, Currency nativeCurrency, string[] rpcUrls, string[] blockExplorerUrls = null)
        {
            chainIdDecimal = chainId;
            chainIdHex = chainId.ToHex();
            this.name = name;
            this.nativeCurrency = nativeCurrency;
            this.rpcUrls = rpcUrls;
            this.blockExplorerUrls = blockExplorerUrls;
        }

        public EthereumChain(Chain chain)
        {
            if (chain.ChainNamespace != "eip155" && chain.ChainNamespace != "eip1193")
                throw new ArgumentException("Chain namespace must be eip155 or eip1193");

            chainIdDecimal = chain.ChainReference;
            chainIdHex = chain.ChainReference.ToHex();
            name = chain.Name;
            nativeCurrency = chain.NativeCurrency;
            rpcUrls = new[] { chain.RpcUrl };

            if (!string.IsNullOrWhiteSpace(chain.BlockExplorer.url))
                blockExplorerUrls = new[] { chain.BlockExplorer.url };
        }
    }
}