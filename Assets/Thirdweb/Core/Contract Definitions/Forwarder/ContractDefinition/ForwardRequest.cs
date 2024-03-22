using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Newtonsoft.Json;

namespace Thirdweb.Contracts.Forwarder.ContractDefinition
{
    public partial class ForwardRequest : ForwardRequestBase { }

    public class ForwardRequestBase
    {
        [Parameter("address", "from", 1)]
        [JsonProperty("from")]
        public virtual string From { get; set; }

        [Parameter("address", "to", 2)]
        [JsonProperty("to")]
        public virtual string To { get; set; }

        [Parameter("uint256", "value", 3)]
        [JsonProperty("value")]
        public virtual BigInteger Value { get; set; }

        [Parameter("uint256", "gas", 4)]
        [JsonProperty("gas")]
        public virtual BigInteger Gas { get; set; }

        [Parameter("uint256", "nonce", 5)]
        [JsonProperty("nonce")]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("bytes", "data", 6)]
        [JsonProperty("data")]
        public virtual string Data { get; set; }
    }
}
