using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.SignatureDrop.ContractDefinition
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

        [Parameter("string", "uri", 5)]
        public virtual string Uri { get; set; }

        [Parameter("uint256", "quantity", 6)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("uint256", "pricePerToken", 7)]
        public virtual BigInteger PricePerToken { get; set; }

        [Parameter("address", "currency", 8)]
        public virtual string Currency { get; set; }

        [Parameter("uint128", "validityStartTimestamp", 9)]
        public virtual BigInteger ValidityStartTimestamp { get; set; }

        [Parameter("uint128", "validityEndTimestamp", 10)]
        public virtual BigInteger ValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 11)]
        public virtual byte[] Uid { get; set; }
    }
}
