using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.Marketplace.ContractDefinition
{
    public partial class ListingParameters : ListingParametersBase { }

    public class ListingParametersBase
    {
        [Parameter("address", "assetContract", 1)]
        public virtual string AssetContract { get; set; }

        [Parameter("uint256", "tokenId", 2)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("uint256", "startTime", 3)]
        public virtual BigInteger StartTime { get; set; }

        [Parameter("uint256", "secondsUntilEndTime", 4)]
        public virtual BigInteger SecondsUntilEndTime { get; set; }

        [Parameter("uint256", "quantityToList", 5)]
        public virtual BigInteger QuantityToList { get; set; }

        [Parameter("address", "currencyToAccept", 6)]
        public virtual string CurrencyToAccept { get; set; }

        [Parameter("uint256", "reservePricePerToken", 7)]
        public virtual BigInteger ReservePricePerToken { get; set; }

        [Parameter("uint256", "buyoutPricePerToken", 8)]
        public virtual BigInteger BuyoutPricePerToken { get; set; }

        [Parameter("uint8", "listingType", 9)]
        public virtual byte ListingType { get; set; }
    }
}
