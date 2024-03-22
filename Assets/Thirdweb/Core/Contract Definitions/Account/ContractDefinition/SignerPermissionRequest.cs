using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.Account.ContractDefinition
{
    public partial class SignerPermissionRequest : SignerPermissionRequestBase { }

    public class SignerPermissionRequestBase
    {
        [Parameter("address", "signer", 1)]
        public virtual string Signer { get; set; }

        [Parameter("uint8", "isAdmin", 2)]
        public virtual byte IsAdmin { get; set; }

        [Parameter("address[]", "approvedTargets", 3)]
        public virtual List<string> ApprovedTargets { get; set; }

        [Parameter("uint256", "nativeTokenLimitPerTransaction", 4)]
        public virtual BigInteger NativeTokenLimitPerTransaction { get; set; }

        [Parameter("uint128", "permissionStartTimestamp", 5)]
        public virtual BigInteger PermissionStartTimestamp { get; set; }

        [Parameter("uint128", "permissionEndTimestamp", 6)]
        public virtual BigInteger PermissionEndTimestamp { get; set; }

        [Parameter("uint128", "reqValidityStartTimestamp", 7)]
        public virtual BigInteger ReqValidityStartTimestamp { get; set; }

        [Parameter("uint128", "reqValidityEndTimestamp", 8)]
        public virtual BigInteger ReqValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 9)]
        public virtual byte[] Uid { get; set; }
    }
}
