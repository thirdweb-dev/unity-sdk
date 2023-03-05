using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.Marketplace.ContractDefinition
{
    public partial class Listing : ListingBase { }

    public class ListingBase
    {
        [Parameter("uint256", "listingId", 1)]
        public virtual BigInteger ListingId { get; set; }

        [Parameter("address", "tokenOwner", 2)]
        public virtual string TokenOwner { get; set; }

        [Parameter("address", "assetContract", 3)]
        public virtual string AssetContract { get; set; }

        [Parameter("uint256", "tokenId", 4)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("uint256", "startTime", 5)]
        public virtual BigInteger StartTime { get; set; }

        [Parameter("uint256", "endTime", 6)]
        public virtual BigInteger EndTime { get; set; }

        [Parameter("uint256", "quantity", 7)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("address", "currency", 8)]
        public virtual string Currency { get; set; }

        [Parameter("uint256", "reservePricePerToken", 9)]
        public virtual BigInteger ReservePricePerToken { get; set; }

        [Parameter("uint256", "buyoutPricePerToken", 10)]
        public virtual BigInteger BuyoutPricePerToken { get; set; }

        [Parameter("uint8", "tokenType", 11)]
        public virtual byte TokenType { get; set; }

        [Parameter("uint8", "listingType", 12)]
        public virtual byte ListingType { get; set; }
    }
}
