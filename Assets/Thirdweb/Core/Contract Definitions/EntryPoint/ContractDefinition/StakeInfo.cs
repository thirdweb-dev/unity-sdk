using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.EntryPoint.ContractDefinition
{
    public partial class StakeInfo : StakeInfoBase { }

    public class StakeInfoBase
    {
        [Parameter("uint256", "stake", 1)]
        public virtual BigInteger Stake { get; set; }

        [Parameter("uint256", "unstakeDelaySec", 2)]
        public virtual BigInteger UnstakeDelaySec { get; set; }
    }
}
