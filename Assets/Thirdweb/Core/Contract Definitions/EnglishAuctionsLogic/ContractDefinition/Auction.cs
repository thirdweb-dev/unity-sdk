using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.EnglishAuctionsLogic.ContractDefinition
{
    public partial class Auction : AuctionBase { }

    public class AuctionBase
    {
        [Parameter("uint256", "auctionId", 1)]
        public virtual BigInteger AuctionId { get; set; }

        [Parameter("address", "auctionCreator", 2)]
        public virtual string AuctionCreator { get; set; }

        [Parameter("address", "assetContract", 3)]
        public virtual string AssetContract { get; set; }

        [Parameter("uint256", "tokenId", 4)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("uint256", "quantity", 5)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("address", "currency", 6)]
        public virtual string Currency { get; set; }

        [Parameter("uint256", "minimumBidAmount", 7)]
        public virtual BigInteger MinimumBidAmount { get; set; }

        [Parameter("uint256", "buyoutBidAmount", 8)]
        public virtual BigInteger BuyoutBidAmount { get; set; }

        [Parameter("uint64", "timeBufferInSeconds", 9)]
        public virtual ulong TimeBufferInSeconds { get; set; }

        [Parameter("uint64", "bidBufferBps", 10)]
        public virtual ulong BidBufferBps { get; set; }

        [Parameter("uint64", "startTimestamp", 11)]
        public virtual ulong StartTimestamp { get; set; }

        [Parameter("uint64", "endTimestamp", 12)]
        public virtual ulong EndTimestamp { get; set; }

        [Parameter("uint8", "tokenType", 13)]
        public virtual byte TokenType { get; set; }

        [Parameter("uint8", "status", 14)]
        public virtual byte Status { get; set; }
    }
}
