using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.ERC721AQueryableUpgradeable.ContractDefinition
{
    public partial class TokenOwnership : TokenOwnershipBase { }

    public class TokenOwnershipBase
    {
        [Parameter("address", "addr", 1)]
        public virtual string Addr { get; set; }

        [Parameter("uint64", "startTimestamp", 2)]
        public virtual ulong StartTimestamp { get; set; }

        [Parameter("bool", "burned", 3)]
        public virtual bool Burned { get; set; }

        [Parameter("uint24", "extraData", 4)]
        public virtual uint ExtraData { get; set; }
    }
}
