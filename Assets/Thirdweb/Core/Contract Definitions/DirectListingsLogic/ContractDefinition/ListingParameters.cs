using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.DirectListingsLogic.ContractDefinition
{
    public partial class ListingParameters : ListingParametersBase { }

    public class ListingParametersBase
    {
        [Parameter("address", "assetContract", 1)]
        public virtual string AssetContract { get; set; }

        [Parameter("uint256", "tokenId", 2)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("uint256", "quantity", 3)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("address", "currency", 4)]
        public virtual string Currency { get; set; }

        [Parameter("uint256", "pricePerToken", 5)]
        public virtual BigInteger PricePerToken { get; set; }

        [Parameter("uint128", "startTimestamp", 6)]
        public virtual BigInteger StartTimestamp { get; set; }

        [Parameter("uint128", "endTimestamp", 7)]
        public virtual BigInteger EndTimestamp { get; set; }

        [Parameter("bool", "reserved", 8)]
        public virtual bool Reserved { get; set; }
    }
}
