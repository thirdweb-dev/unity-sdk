using System.Numerics;
using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Ethereum.Types
{
    public class EIP712Domain
    {
        [EvmType("string", "name", 1)]
        [JsonProperty("name", Order = 1)]
        public virtual string Name { get; set; }

        [EvmType("string", "version", 2)]
        [JsonProperty("version", Order = 2)]
        public virtual string Version { get; set; }

        [EvmType("uint256", "chainId", 3)]
        [JsonProperty("chainId", Order = 3)]
        public virtual BigInteger? ChainId { get; set; }

        [EvmType("address", "verifyingContract", 4)]
        [JsonProperty("verifyingContract", Order = 4)]
        public virtual string VerifyingContract { get; set; }

        public EIP712Domain() { }

        public EIP712Domain(string name, string version, int chainId, string verifyingContract)
        {
            Name = name;
            Version = version;
            ChainId = chainId;
            VerifyingContract = verifyingContract;
        }
    }
}
