using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.DropERC20.ContractDefinition
{
    public partial class Checkpoint : CheckpointBase { }

    public class CheckpointBase
    {
        [Parameter("uint32", "fromBlock", 1)]
        public virtual uint FromBlock { get; set; }

        [Parameter("uint224", "votes", 2)]
        public virtual BigInteger Votes { get; set; }
    }
}
