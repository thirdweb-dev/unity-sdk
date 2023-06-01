using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.EntryPoint.ContractDefinition
{
    public partial class ReturnInfo : ReturnInfoBase { }

    public class ReturnInfoBase
    {
        [Parameter("uint256", "preOpGas", 1)]
        public virtual BigInteger PreOpGas { get; set; }

        [Parameter("uint256", "prefund", 2)]
        public virtual BigInteger Prefund { get; set; }

        [Parameter("bool", "sigFailed", 3)]
        public virtual bool SigFailed { get; set; }

        [Parameter("uint48", "validAfter", 4)]
        public virtual ulong ValidAfter { get; set; }

        [Parameter("uint48", "validUntil", 5)]
        public virtual ulong ValidUntil { get; set; }

        [Parameter("bytes", "paymasterContext", 6)]
        public virtual byte[] PaymasterContext { get; set; }
    }
}
