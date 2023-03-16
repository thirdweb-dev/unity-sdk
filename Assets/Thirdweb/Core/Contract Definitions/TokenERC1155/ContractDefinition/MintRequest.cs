using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.TokenERC1155.ContractDefinition
{
    public partial class MintRequest : MintRequestBase { }

    public class MintRequestBase
    {
        [Parameter("address", "to", 1)]
        public virtual string To { get; set; }

        [Parameter("address", "royaltyRecipient", 2)]
        public virtual string RoyaltyRecipient { get; set; }

        [Parameter("uint256", "royaltyBps", 3)]
        public virtual BigInteger RoyaltyBps { get; set; }

        [Parameter("address", "primarySaleRecipient", 4)]
        public virtual string PrimarySaleRecipient { get; set; }

        [Parameter("uint256", "tokenId", 5)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("string", "uri", 6)]
        public virtual string Uri { get; set; }

        [Parameter("uint256", "quantity", 7)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("uint256", "pricePerToken", 8)]
        public virtual BigInteger PricePerToken { get; set; }

        [Parameter("address", "currency", 9)]
        public virtual string Currency { get; set; }

        [Parameter("uint128", "validityStartTimestamp", 10)]
        public virtual BigInteger ValidityStartTimestamp { get; set; }

        [Parameter("uint128", "validityEndTimestamp", 11)]
        public virtual BigInteger ValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 12)]
        public virtual byte[] Uid { get; set; }
    }
}
