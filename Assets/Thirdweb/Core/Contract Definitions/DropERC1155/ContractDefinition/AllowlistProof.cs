using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.DropERC1155.ContractDefinition
{
    public partial class AllowlistProof : AllowlistProofBase { }

    public class AllowlistProofBase
    {
        [Parameter("bytes32[]", "proof", 1)]
        public virtual List<byte[]> Proof { get; set; }

        [Parameter("uint256", "quantityLimitPerWallet", 2)]
        public virtual BigInteger QuantityLimitPerWallet { get; set; }

        [Parameter("uint256", "pricePerToken", 3)]
        public virtual BigInteger PricePerToken { get; set; }

        [Parameter("address", "currency", 4)]
        public virtual string Currency { get; set; }
    }
}
