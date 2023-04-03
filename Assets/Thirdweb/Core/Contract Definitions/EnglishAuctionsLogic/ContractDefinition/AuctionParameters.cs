using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.EnglishAuctionsLogic.ContractDefinition
{
    public partial class AuctionParameters : AuctionParametersBase { }

    public class AuctionParametersBase
    {
        [Parameter("address", "assetContract", 1)]
        public virtual string AssetContract { get; set; }

        [Parameter("uint256", "tokenId", 2)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("uint256", "quantity", 3)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("address", "currency", 4)]
        public virtual string Currency { get; set; }

        [Parameter("uint256", "minimumBidAmount", 5)]
        public virtual BigInteger MinimumBidAmount { get; set; }

        [Parameter("uint256", "buyoutBidAmount", 6)]
        public virtual BigInteger BuyoutBidAmount { get; set; }

        [Parameter("uint64", "timeBufferInSeconds", 7)]
        public virtual ulong TimeBufferInSeconds { get; set; }

        [Parameter("uint64", "bidBufferBps", 8)]
        public virtual ulong BidBufferBps { get; set; }

        [Parameter("uint64", "startTimestamp", 9)]
        public virtual ulong StartTimestamp { get; set; }

        [Parameter("uint64", "endTimestamp", 10)]
        public virtual ulong EndTimestamp { get; set; }
    }
}
