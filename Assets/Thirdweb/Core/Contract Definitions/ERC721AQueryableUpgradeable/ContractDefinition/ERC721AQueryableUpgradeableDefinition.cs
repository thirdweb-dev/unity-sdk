using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts;
using System.Threading;

namespace Thirdweb.Contracts.ERC721AQueryableUpgradeable.ContractDefinition
{
    public partial class ERC721AQueryableUpgradeableDeployment : ERC721AQueryableUpgradeableDeploymentBase
    {
        public ERC721AQueryableUpgradeableDeployment()
            : base(BYTECODE) { }

        public ERC721AQueryableUpgradeableDeployment(string byteCode)
            : base(byteCode) { }
    }

    public class ERC721AQueryableUpgradeableDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";

        public ERC721AQueryableUpgradeableDeploymentBase()
            : base(BYTECODE) { }

        public ERC721AQueryableUpgradeableDeploymentBase(string byteCode)
            : base(byteCode) { }
    }

    public partial class ApproveFunction : ApproveFunctionBase { }

    [Function("approve")]
    public class ApproveFunctionBase : FunctionMessage
    {
        [Parameter("address", "to", 1)]
        public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 2)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class BalanceOfFunction : BalanceOfFunctionBase { }

    [Function("balanceOf", "uint256")]
    public class BalanceOfFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner", 1)]
        public virtual string Owner { get; set; }
    }

    public partial class ExplicitOwnershipOfFunction : ExplicitOwnershipOfFunctionBase { }

    [Function("explicitOwnershipOf", typeof(ExplicitOwnershipOfOutputDTO))]
    public class ExplicitOwnershipOfFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "tokenId", 1)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class GetApprovedFunction : GetApprovedFunctionBase { }

    [Function("getApproved", "address")]
    public class GetApprovedFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "tokenId", 1)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class IsApprovedForAllFunction : IsApprovedForAllFunctionBase { }

    [Function("isApprovedForAll", "bool")]
    public class IsApprovedForAllFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner", 1)]
        public virtual string Owner { get; set; }

        [Parameter("address", "operator", 2)]
        public virtual string Operator { get; set; }
    }

    public partial class NameFunction : NameFunctionBase { }

    [Function("name", "string")]
    public class NameFunctionBase : FunctionMessage { }

    public partial class OwnerOfFunction : OwnerOfFunctionBase { }

    [Function("ownerOf", "address")]
    public class OwnerOfFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "tokenId", 1)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class SafeTransferFromFunction : SafeTransferFromFunctionBase { }

    [Function("safeTransferFrom")]
    public class SafeTransferFromFunctionBase : FunctionMessage
    {
        [Parameter("address", "from", 1)]
        public virtual string From { get; set; }

        [Parameter("address", "to", 2)]
        public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 3)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class SafeTransferFrom1Function : SafeTransferFrom1FunctionBase { }

    [Function("safeTransferFrom")]
    public class SafeTransferFrom1FunctionBase : FunctionMessage
    {
        [Parameter("address", "from", 1)]
        public virtual string From { get; set; }

        [Parameter("address", "to", 2)]
        public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 3)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("bytes", "_data", 4)]
        public virtual byte[] Data { get; set; }
    }

    public partial class SetApprovalForAllFunction : SetApprovalForAllFunctionBase { }

    [Function("setApprovalForAll")]
    public class SetApprovalForAllFunctionBase : FunctionMessage
    {
        [Parameter("address", "operator", 1)]
        public virtual string Operator { get; set; }

        [Parameter("bool", "approved", 2)]
        public virtual bool Approved { get; set; }
    }

    public partial class SupportsInterfaceFunction : SupportsInterfaceFunctionBase { }

    [Function("supportsInterface", "bool")]
    public class SupportsInterfaceFunctionBase : FunctionMessage
    {
        [Parameter("bytes4", "interfaceId", 1)]
        public virtual byte[] InterfaceId { get; set; }
    }

    public partial class SymbolFunction : SymbolFunctionBase { }

    [Function("symbol", "string")]
    public class SymbolFunctionBase : FunctionMessage { }

    public partial class TokenURIFunction : TokenURIFunctionBase { }

    [Function("tokenURI", "string")]
    public class TokenURIFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "tokenId", 1)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class TokensOfOwnerFunction : TokensOfOwnerFunctionBase { }

    [Function("tokensOfOwner", "uint256[]")]
    public class TokensOfOwnerFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner", 1)]
        public virtual string Owner { get; set; }
    }

    public partial class TokensOfOwnerInFunction : TokensOfOwnerInFunctionBase { }

    [Function("tokensOfOwnerIn", "uint256[]")]
    public class TokensOfOwnerInFunctionBase : FunctionMessage
    {
        [Parameter("address", "owner", 1)]
        public virtual string Owner { get; set; }

        [Parameter("uint256", "start", 2)]
        public virtual BigInteger Start { get; set; }

        [Parameter("uint256", "stop", 3)]
        public virtual BigInteger Stop { get; set; }
    }

    public partial class TotalSupplyFunction : TotalSupplyFunctionBase { }

    [Function("totalSupply", "uint256")]
    public class TotalSupplyFunctionBase : FunctionMessage { }

    public partial class TransferFromFunction : TransferFromFunctionBase { }

    [Function("transferFrom")]
    public class TransferFromFunctionBase : FunctionMessage
    {
        [Parameter("address", "from", 1)]
        public virtual string From { get; set; }

        [Parameter("address", "to", 2)]
        public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 3)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class ApprovalEventDTO : ApprovalEventDTOBase { }

    [Event("Approval")]
    public class ApprovalEventDTOBase : IEventDTO
    {
        [Parameter("address", "owner", 1, true)]
        public virtual string Owner { get; set; }

        [Parameter("address", "approved", 2, true)]
        public virtual string Approved { get; set; }

        [Parameter("uint256", "tokenId", 3, true)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class ApprovalForAllEventDTO : ApprovalForAllEventDTOBase { }

    [Event("ApprovalForAll")]
    public class ApprovalForAllEventDTOBase : IEventDTO
    {
        [Parameter("address", "owner", 1, true)]
        public virtual string Owner { get; set; }

        [Parameter("address", "operator", 2, true)]
        public virtual string Operator { get; set; }

        [Parameter("bool", "approved", 3, false)]
        public virtual bool Approved { get; set; }
    }

    public partial class ConsecutiveTransferEventDTO : ConsecutiveTransferEventDTOBase { }

    [Event("ConsecutiveTransfer")]
    public class ConsecutiveTransferEventDTOBase : IEventDTO
    {
        [Parameter("uint256", "fromTokenId", 1, true)]
        public virtual BigInteger FromTokenId { get; set; }

        [Parameter("uint256", "toTokenId", 2, false)]
        public virtual BigInteger ToTokenId { get; set; }

        [Parameter("address", "from", 3, true)]
        public virtual string From { get; set; }

        [Parameter("address", "to", 4, true)]
        public virtual string To { get; set; }
    }

    public partial class TransferEventDTO : TransferEventDTOBase { }

    [Event("Transfer")]
    public class TransferEventDTOBase : IEventDTO
    {
        [Parameter("address", "from", 1, true)]
        public virtual string From { get; set; }

        [Parameter("address", "to", 2, true)]
        public virtual string To { get; set; }

        [Parameter("uint256", "tokenId", 3, true)]
        public virtual BigInteger TokenId { get; set; }
    }

    public partial class ApprovalCallerNotOwnerNorApprovedError : ApprovalCallerNotOwnerNorApprovedErrorBase { }

    [Error("ApprovalCallerNotOwnerNorApproved")]
    public class ApprovalCallerNotOwnerNorApprovedErrorBase : IErrorDTO { }

    public partial class ApprovalQueryForNonexistentTokenError : ApprovalQueryForNonexistentTokenErrorBase { }

    [Error("ApprovalQueryForNonexistentToken")]
    public class ApprovalQueryForNonexistentTokenErrorBase : IErrorDTO { }

    public partial class BalanceQueryForZeroAddressError : BalanceQueryForZeroAddressErrorBase { }

    [Error("BalanceQueryForZeroAddress")]
    public class BalanceQueryForZeroAddressErrorBase : IErrorDTO { }

    public partial class InvalidQueryRangeError : InvalidQueryRangeErrorBase { }

    [Error("InvalidQueryRange")]
    public class InvalidQueryRangeErrorBase : IErrorDTO { }

    public partial class MintERC2309QuantityExceedsLimitError : MintERC2309QuantityExceedsLimitErrorBase { }

    [Error("MintERC2309QuantityExceedsLimit")]
    public class MintERC2309QuantityExceedsLimitErrorBase : IErrorDTO { }

    public partial class MintToZeroAddressError : MintToZeroAddressErrorBase { }

    [Error("MintToZeroAddress")]
    public class MintToZeroAddressErrorBase : IErrorDTO { }

    public partial class MintZeroQuantityError : MintZeroQuantityErrorBase { }

    [Error("MintZeroQuantity")]
    public class MintZeroQuantityErrorBase : IErrorDTO { }

    public partial class OwnerQueryForNonexistentTokenError : OwnerQueryForNonexistentTokenErrorBase { }

    [Error("OwnerQueryForNonexistentToken")]
    public class OwnerQueryForNonexistentTokenErrorBase : IErrorDTO { }

    public partial class OwnershipNotInitializedForExtraDataError : OwnershipNotInitializedForExtraDataErrorBase { }

    [Error("OwnershipNotInitializedForExtraData")]
    public class OwnershipNotInitializedForExtraDataErrorBase : IErrorDTO { }

    public partial class TransferCallerNotOwnerNorApprovedError : TransferCallerNotOwnerNorApprovedErrorBase { }

    [Error("TransferCallerNotOwnerNorApproved")]
    public class TransferCallerNotOwnerNorApprovedErrorBase : IErrorDTO { }

    public partial class TransferFromIncorrectOwnerError : TransferFromIncorrectOwnerErrorBase { }

    [Error("TransferFromIncorrectOwner")]
    public class TransferFromIncorrectOwnerErrorBase : IErrorDTO { }

    public partial class TransferToNonERC721ReceiverImplementerError : TransferToNonERC721ReceiverImplementerErrorBase { }

    [Error("TransferToNonERC721ReceiverImplementer")]
    public class TransferToNonERC721ReceiverImplementerErrorBase : IErrorDTO { }

    public partial class TransferToZeroAddressError : TransferToZeroAddressErrorBase { }

    [Error("TransferToZeroAddress")]
    public class TransferToZeroAddressErrorBase : IErrorDTO { }

    public partial class URIQueryForNonexistentTokenError : URIQueryForNonexistentTokenErrorBase { }

    [Error("URIQueryForNonexistentToken")]
    public class URIQueryForNonexistentTokenErrorBase : IErrorDTO { }

    public partial class BalanceOfOutputDTO : BalanceOfOutputDTOBase { }

    [FunctionOutput]
    public class BalanceOfOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class ExplicitOwnershipOfOutputDTO : ExplicitOwnershipOfOutputDTOBase { }

    [FunctionOutput]
    public class ExplicitOwnershipOfOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("tuple", "ownership", 1)]
        public virtual TokenOwnership Ownership { get; set; }
    }

    public partial class GetApprovedOutputDTO : GetApprovedOutputDTOBase { }

    [FunctionOutput]
    public class GetApprovedOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class IsApprovedForAllOutputDTO : IsApprovedForAllOutputDTOBase { }

    [FunctionOutput]
    public class IsApprovedForAllOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }

    public partial class NameOutputDTO : NameOutputDTOBase { }

    [FunctionOutput]
    public class NameOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class OwnerOfOutputDTO : OwnerOfOutputDTOBase { }

    [FunctionOutput]
    public class OwnerOfOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class SupportsInterfaceOutputDTO : SupportsInterfaceOutputDTOBase { }

    [FunctionOutput]
    public class SupportsInterfaceOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }

    public partial class SymbolOutputDTO : SymbolOutputDTOBase { }

    [FunctionOutput]
    public class SymbolOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class TokenURIOutputDTO : TokenURIOutputDTOBase { }

    [FunctionOutput]
    public class TokenURIOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class TokensOfOwnerOutputDTO : TokensOfOwnerOutputDTOBase { }

    [FunctionOutput]
    public class TokensOfOwnerOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256[]", "", 1)]
        public virtual List<BigInteger> ReturnValue1 { get; set; }
    }

    public partial class TokensOfOwnerInOutputDTO : TokensOfOwnerInOutputDTOBase { }

    [FunctionOutput]
    public class TokensOfOwnerInOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256[]", "", 1)]
        public virtual List<BigInteger> ReturnValue1 { get; set; }
    }

    public partial class TotalSupplyOutputDTO : TotalSupplyOutputDTOBase { }

    [FunctionOutput]
    public class TotalSupplyOutputDTOBase : IFunctionOutputDTO
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }
}
