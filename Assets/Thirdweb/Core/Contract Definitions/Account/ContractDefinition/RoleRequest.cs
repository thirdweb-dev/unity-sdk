using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.Account.ContractDefinition
{
    public partial class RoleRequest : RoleRequestBase { }

    public class RoleRequestBase
    {
        [Parameter("bytes32", "role", 1)]
        public virtual byte[] Role { get; set; }

        [Parameter("address", "target", 2)]
        public virtual string Target { get; set; }

        [Parameter("uint8", "action", 3)]
        public virtual byte Action { get; set; }

        [Parameter("uint128", "validityStartTimestamp", 4)]
        public virtual BigInteger ValidityStartTimestamp { get; set; }

        [Parameter("uint128", "validityEndTimestamp", 5)]
        public virtual BigInteger ValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 6)]
        public virtual byte[] Uid { get; set; }
    }
}
