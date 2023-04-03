using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.DirectListingsLogic.ContractDefinition
{
    public partial class Listing : ListingBase { }

    public class ListingBase
    {
        [Parameter("uint256", "listingId", 1)]
        public virtual BigInteger ListingId { get; set; }

        [Parameter("address", "listingCreator", 2)]
        public virtual string ListingCreator { get; set; }

        [Parameter("address", "assetContract", 3)]
        public virtual string AssetContract { get; set; }

        [Parameter("uint256", "tokenId", 4)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("uint256", "quantity", 5)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("address", "currency", 6)]
        public virtual string Currency { get; set; }

        [Parameter("uint256", "pricePerToken", 7)]
        public virtual BigInteger PricePerToken { get; set; }

        [Parameter("uint128", "startTimestamp", 8)]
        public virtual BigInteger StartTimestamp { get; set; }

        [Parameter("uint128", "endTimestamp", 9)]
        public virtual BigInteger EndTimestamp { get; set; }

        [Parameter("bool", "reserved", 10)]
        public virtual bool Reserved { get; set; }

        [Parameter("uint8", "tokenType", 11)]
        public virtual byte TokenType { get; set; }

        [Parameter("uint8", "status", 12)]
        public virtual byte Status { get; set; }
    }
}
