using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.DropERC20.ContractDefinition
{
    public partial class ClaimCondition : ClaimConditionBase { }

    public class ClaimConditionBase
    {
        [Parameter("uint256", "startTimestamp", 1)]
        public virtual BigInteger StartTimestamp { get; set; }

        [Parameter("uint256", "maxClaimableSupply", 2)]
        public virtual BigInteger MaxClaimableSupply { get; set; }

        [Parameter("uint256", "supplyClaimed", 3)]
        public virtual BigInteger SupplyClaimed { get; set; }

        [Parameter("uint256", "quantityLimitPerWallet", 4)]
        public virtual BigInteger QuantityLimitPerWallet { get; set; }

        [Parameter("bytes32", "merkleRoot", 5)]
        public virtual byte[] MerkleRoot { get; set; }

        [Parameter("uint256", "pricePerToken", 6)]
        public virtual BigInteger PricePerToken { get; set; }

        [Parameter("address", "currency", 7)]
        public virtual string Currency { get; set; }

        [Parameter("string", "metadata", 8)]
        public virtual string Metadata { get; set; }
    }
}
