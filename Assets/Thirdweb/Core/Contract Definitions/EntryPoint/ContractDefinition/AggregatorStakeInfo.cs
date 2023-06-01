using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.EntryPoint.ContractDefinition
{
    public partial class AggregatorStakeInfo : AggregatorStakeInfoBase { }

    public class AggregatorStakeInfoBase
    {
        [Parameter("address", "aggregator", 1)]
        public virtual string Aggregator { get; set; }

        [Parameter("tuple", "stakeInfo", 2)]
        public virtual StakeInfo StakeInfo { get; set; }
    }
}
