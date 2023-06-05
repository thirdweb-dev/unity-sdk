using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.Account.ContractDefinition
{
    public partial class RoleRestrictions : RoleRestrictionsBase { }

    public class RoleRestrictionsBase
    {
        [Parameter("bytes32", "role", 1)]
        public virtual byte[] Role { get; set; }

        [Parameter("address[]", "approvedTargets", 2)]
        public virtual List<string> ApprovedTargets { get; set; }

        [Parameter("uint256", "maxValuePerTransaction", 3)]
        public virtual BigInteger MaxValuePerTransaction { get; set; }

        [Parameter("uint128", "startTimestamp", 4)]
        public virtual BigInteger StartTimestamp { get; set; }

        [Parameter("uint128", "endTimestamp", 5)]
        public virtual BigInteger EndTimestamp { get; set; }
    }
}
