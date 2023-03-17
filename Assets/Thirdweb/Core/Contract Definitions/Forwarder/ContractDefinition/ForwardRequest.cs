using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.Forwarder.ContractDefinition
{
    public partial class ForwardRequest : ForwardRequestBase { }

    public class ForwardRequestBase
    {
        [Parameter("address", "from", 1)]
        public virtual string From { get; set; }

        [Parameter("address", "to", 2)]
        public virtual string To { get; set; }

        [Parameter("uint256", "value", 3)]
        public virtual BigInteger Value { get; set; }

        [Parameter("uint256", "gas", 4)]
        public virtual BigInteger Gas { get; set; }

        [Parameter("uint256", "nonce", 5)]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("bytes", "data", 6)]
        public virtual byte[] Data { get; set; }
    }
}
