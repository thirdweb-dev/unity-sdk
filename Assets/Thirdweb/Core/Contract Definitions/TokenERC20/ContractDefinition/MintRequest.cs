using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.TokenERC20.ContractDefinition
{
    public partial class MintRequest : MintRequestBase { }

    public class MintRequestBase
    {
        [Parameter("address", "to", 1)]
        public virtual string To { get; set; }

        [Parameter("address", "primarySaleRecipient", 2)]
        public virtual string PrimarySaleRecipient { get; set; }

        [Parameter("uint256", "quantity", 3)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("uint256", "price", 4)]
        public virtual BigInteger Price { get; set; }

        [Parameter("address", "currency", 5)]
        public virtual string Currency { get; set; }

        [Parameter("uint128", "validityStartTimestamp", 6)]
        public virtual BigInteger ValidityStartTimestamp { get; set; }

        [Parameter("uint128", "validityEndTimestamp", 7)]
        public virtual BigInteger ValidityEndTimestamp { get; set; }

        [Parameter("bytes32", "uid", 8)]
        public virtual byte[] Uid { get; set; }
    }
}
