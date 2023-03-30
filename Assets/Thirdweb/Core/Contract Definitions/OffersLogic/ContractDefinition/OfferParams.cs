using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.OffersLogic.ContractDefinition
{
    public partial class OfferParams : OfferParamsBase { }

    public class OfferParamsBase
    {
        [Parameter("address", "assetContract", 1)]
        public virtual string AssetContract { get; set; }

        [Parameter("uint256", "tokenId", 2)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("uint256", "quantity", 3)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("address", "currency", 4)]
        public virtual string Currency { get; set; }

        [Parameter("uint256", "totalPrice", 5)]
        public virtual BigInteger TotalPrice { get; set; }

        [Parameter("uint256", "expirationTimestamp", 6)]
        public virtual BigInteger ExpirationTimestamp { get; set; }
    }
}
