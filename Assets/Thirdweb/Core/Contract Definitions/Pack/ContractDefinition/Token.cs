using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.Pack.ContractDefinition
{
    public partial class Token : TokenBase { }

    public class TokenBase
    {
        [Parameter("address", "assetContract", 1)]
        public virtual string AssetContract { get; set; }

        [Parameter("uint8", "tokenType", 2)]
        public virtual byte TokenType { get; set; }

        [Parameter("uint256", "tokenId", 3)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("uint256", "totalAmount", 4)]
        public virtual BigInteger TotalAmount { get; set; }
    }
}
