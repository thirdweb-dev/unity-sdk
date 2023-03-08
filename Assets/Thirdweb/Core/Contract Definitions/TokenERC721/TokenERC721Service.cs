using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts;
using System.Threading;
using Thirdweb.Contracts.TokenERC721.ContractDefinition;

namespace Thirdweb.Contracts.TokenERC721
{
    public partial class TokenERC721Service
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(
            Nethereum.Web3.Web3 web3,
            TokenERC721Deployment tokenERC721Deployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            return web3.Eth.GetContractDeploymentHandler<TokenERC721Deployment>().SendRequestAndWaitForReceiptAsync(tokenERC721Deployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, TokenERC721Deployment tokenERC721Deployment)
        {
            return web3.Eth.GetContractDeploymentHandler<TokenERC721Deployment>().SendRequestAsync(tokenERC721Deployment);
        }

        public static async Task<TokenERC721Service> DeployContractAndGetServiceAsync(
            Nethereum.Web3.Web3 web3,
            TokenERC721Deployment tokenERC721Deployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, tokenERC721Deployment, cancellationTokenSource);
            return new TokenERC721Service(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.Web3 Web3 { get; }

        public ContractHandler ContractHandler { get; }

        public TokenERC721Service(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<byte[]> DEFAULT_ADMIN_ROLEQueryAsync(DEFAULT_ADMIN_ROLEFunction dEFAULT_ADMIN_ROLEFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DEFAULT_ADMIN_ROLEFunction, byte[]>(dEFAULT_ADMIN_ROLEFunction, blockParameter);
        }

        public Task<byte[]> DEFAULT_ADMIN_ROLEQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DEFAULT_ADMIN_ROLEFunction, byte[]>(null, blockParameter);
        }

        public Task<string> ApproveRequestAsync(ApproveFunction approveFunction)
        {
            return ContractHandler.SendRequestAsync(approveFunction);
        }

        public Task<TransactionReceipt> ApproveRequestAndWaitForReceiptAsync(ApproveFunction approveFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationToken);
        }

        public Task<string> ApproveRequestAsync(string @operator, BigInteger tokenId)
        {
            var approveFunction = new ApproveFunction();
            approveFunction.Operator = @operator;
            approveFunction.TokenId = tokenId;

            return ContractHandler.SendRequestAsync(approveFunction);
        }

        public Task<TransactionReceipt> ApproveRequestAndWaitForReceiptAsync(string @operator, BigInteger tokenId, CancellationTokenSource cancellationToken = null)
        {
            var approveFunction = new ApproveFunction();
            approveFunction.Operator = @operator;
            approveFunction.TokenId = tokenId;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationToken);
        }

        public Task<BigInteger> BalanceOfQueryAsync(BalanceOfFunction balanceOfFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameter);
        }

        public Task<BigInteger> BalanceOfQueryAsync(string owner, BlockParameter blockParameter = null)
        {
            var balanceOfFunction = new BalanceOfFunction();
            balanceOfFunction.Owner = owner;

            return ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameter);
        }

        public Task<string> BurnRequestAsync(BurnFunction burnFunction)
        {
            return ContractHandler.SendRequestAsync(burnFunction);
        }

        public Task<TransactionReceipt> BurnRequestAndWaitForReceiptAsync(BurnFunction burnFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(burnFunction, cancellationToken);
        }

        public Task<string> BurnRequestAsync(BigInteger tokenId)
        {
            var burnFunction = new BurnFunction();
            burnFunction.TokenId = tokenId;

            return ContractHandler.SendRequestAsync(burnFunction);
        }

        public Task<TransactionReceipt> BurnRequestAndWaitForReceiptAsync(BigInteger tokenId, CancellationTokenSource cancellationToken = null)
        {
            var burnFunction = new BurnFunction();
            burnFunction.TokenId = tokenId;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(burnFunction, cancellationToken);
        }

        public Task<byte[]> ContractTypeQueryAsync(ContractTypeFunction contractTypeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractTypeFunction, byte[]>(contractTypeFunction, blockParameter);
        }

        public Task<byte[]> ContractTypeQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractTypeFunction, byte[]>(null, blockParameter);
        }

        public Task<string> ContractURIQueryAsync(ContractURIFunction contractURIFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractURIFunction, string>(contractURIFunction, blockParameter);
        }

        public Task<string> ContractURIQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractURIFunction, string>(null, blockParameter);
        }

        public Task<byte> ContractVersionQueryAsync(ContractVersionFunction contractVersionFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractVersionFunction, byte>(contractVersionFunction, blockParameter);
        }

        public Task<byte> ContractVersionQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractVersionFunction, byte>(null, blockParameter);
        }

        public Task<string> GetApprovedQueryAsync(GetApprovedFunction getApprovedFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetApprovedFunction, string>(getApprovedFunction, blockParameter);
        }

        public Task<string> GetApprovedQueryAsync(BigInteger tokenId, BlockParameter blockParameter = null)
        {
            var getApprovedFunction = new GetApprovedFunction();
            getApprovedFunction.TokenId = tokenId;

            return ContractHandler.QueryAsync<GetApprovedFunction, string>(getApprovedFunction, blockParameter);
        }

        public Task<GetDefaultRoyaltyInfoOutputDTO> GetDefaultRoyaltyInfoQueryAsync(GetDefaultRoyaltyInfoFunction getDefaultRoyaltyInfoFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetDefaultRoyaltyInfoFunction, GetDefaultRoyaltyInfoOutputDTO>(getDefaultRoyaltyInfoFunction, blockParameter);
        }

        public Task<GetDefaultRoyaltyInfoOutputDTO> GetDefaultRoyaltyInfoQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetDefaultRoyaltyInfoFunction, GetDefaultRoyaltyInfoOutputDTO>(null, blockParameter);
        }

        public Task<GetPlatformFeeInfoOutputDTO> GetPlatformFeeInfoQueryAsync(GetPlatformFeeInfoFunction getPlatformFeeInfoFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetPlatformFeeInfoFunction, GetPlatformFeeInfoOutputDTO>(getPlatformFeeInfoFunction, blockParameter);
        }

        public Task<GetPlatformFeeInfoOutputDTO> GetPlatformFeeInfoQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetPlatformFeeInfoFunction, GetPlatformFeeInfoOutputDTO>(null, blockParameter);
        }

        public Task<byte[]> GetRoleAdminQueryAsync(GetRoleAdminFunction getRoleAdminFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRoleAdminFunction, byte[]>(getRoleAdminFunction, blockParameter);
        }

        public Task<byte[]> GetRoleAdminQueryAsync(byte[] role, BlockParameter blockParameter = null)
        {
            var getRoleAdminFunction = new GetRoleAdminFunction();
            getRoleAdminFunction.Role = role;

            return ContractHandler.QueryAsync<GetRoleAdminFunction, byte[]>(getRoleAdminFunction, blockParameter);
        }

        public Task<string> GetRoleMemberQueryAsync(GetRoleMemberFunction getRoleMemberFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRoleMemberFunction, string>(getRoleMemberFunction, blockParameter);
        }

        public Task<string> GetRoleMemberQueryAsync(byte[] role, BigInteger index, BlockParameter blockParameter = null)
        {
            var getRoleMemberFunction = new GetRoleMemberFunction();
            getRoleMemberFunction.Role = role;
            getRoleMemberFunction.Index = index;

            return ContractHandler.QueryAsync<GetRoleMemberFunction, string>(getRoleMemberFunction, blockParameter);
        }

        public Task<BigInteger> GetRoleMemberCountQueryAsync(GetRoleMemberCountFunction getRoleMemberCountFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRoleMemberCountFunction, BigInteger>(getRoleMemberCountFunction, blockParameter);
        }

        public Task<BigInteger> GetRoleMemberCountQueryAsync(byte[] role, BlockParameter blockParameter = null)
        {
            var getRoleMemberCountFunction = new GetRoleMemberCountFunction();
            getRoleMemberCountFunction.Role = role;

            return ContractHandler.QueryAsync<GetRoleMemberCountFunction, BigInteger>(getRoleMemberCountFunction, blockParameter);
        }

        public Task<GetRoyaltyInfoForTokenOutputDTO> GetRoyaltyInfoForTokenQueryAsync(GetRoyaltyInfoForTokenFunction getRoyaltyInfoForTokenFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetRoyaltyInfoForTokenFunction, GetRoyaltyInfoForTokenOutputDTO>(getRoyaltyInfoForTokenFunction, blockParameter);
        }

        public Task<GetRoyaltyInfoForTokenOutputDTO> GetRoyaltyInfoForTokenQueryAsync(BigInteger tokenId, BlockParameter blockParameter = null)
        {
            var getRoyaltyInfoForTokenFunction = new GetRoyaltyInfoForTokenFunction();
            getRoyaltyInfoForTokenFunction.TokenId = tokenId;

            return ContractHandler.QueryDeserializingToObjectAsync<GetRoyaltyInfoForTokenFunction, GetRoyaltyInfoForTokenOutputDTO>(getRoyaltyInfoForTokenFunction, blockParameter);
        }

        public Task<string> GrantRoleRequestAsync(GrantRoleFunction grantRoleFunction)
        {
            return ContractHandler.SendRequestAsync(grantRoleFunction);
        }

        public Task<TransactionReceipt> GrantRoleRequestAndWaitForReceiptAsync(GrantRoleFunction grantRoleFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(grantRoleFunction, cancellationToken);
        }

        public Task<string> GrantRoleRequestAsync(byte[] role, string account)
        {
            var grantRoleFunction = new GrantRoleFunction();
            grantRoleFunction.Role = role;
            grantRoleFunction.Account = account;

            return ContractHandler.SendRequestAsync(grantRoleFunction);
        }

        public Task<TransactionReceipt> GrantRoleRequestAndWaitForReceiptAsync(byte[] role, string account, CancellationTokenSource cancellationToken = null)
        {
            var grantRoleFunction = new GrantRoleFunction();
            grantRoleFunction.Role = role;
            grantRoleFunction.Account = account;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(grantRoleFunction, cancellationToken);
        }

        public Task<bool> HasRoleQueryAsync(HasRoleFunction hasRoleFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<HasRoleFunction, bool>(hasRoleFunction, blockParameter);
        }

        public Task<bool> HasRoleQueryAsync(byte[] role, string account, BlockParameter blockParameter = null)
        {
            var hasRoleFunction = new HasRoleFunction();
            hasRoleFunction.Role = role;
            hasRoleFunction.Account = account;

            return ContractHandler.QueryAsync<HasRoleFunction, bool>(hasRoleFunction, blockParameter);
        }

        public Task<string> InitializeRequestAsync(InitializeFunction initializeFunction)
        {
            return ContractHandler.SendRequestAsync(initializeFunction);
        }

        public Task<TransactionReceipt> InitializeRequestAndWaitForReceiptAsync(InitializeFunction initializeFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(initializeFunction, cancellationToken);
        }

        public Task<string> InitializeRequestAsync(
            string defaultAdmin,
            string name,
            string symbol,
            string contractURI,
            List<string> trustedForwarders,
            string saleRecipient,
            string royaltyRecipient,
            BigInteger royaltyBps,
            BigInteger platformFeeBps,
            string platformFeeRecipient
        )
        {
            var initializeFunction = new InitializeFunction();
            initializeFunction.DefaultAdmin = defaultAdmin;
            initializeFunction.Name = name;
            initializeFunction.Symbol = symbol;
            initializeFunction.ContractURI = contractURI;
            initializeFunction.TrustedForwarders = trustedForwarders;
            initializeFunction.SaleRecipient = saleRecipient;
            initializeFunction.RoyaltyRecipient = royaltyRecipient;
            initializeFunction.RoyaltyBps = royaltyBps;
            initializeFunction.PlatformFeeBps = platformFeeBps;
            initializeFunction.PlatformFeeRecipient = platformFeeRecipient;

            return ContractHandler.SendRequestAsync(initializeFunction);
        }

        public Task<TransactionReceipt> InitializeRequestAndWaitForReceiptAsync(
            string defaultAdmin,
            string name,
            string symbol,
            string contractURI,
            List<string> trustedForwarders,
            string saleRecipient,
            string royaltyRecipient,
            BigInteger royaltyBps,
            BigInteger platformFeeBps,
            string platformFeeRecipient,
            CancellationTokenSource cancellationToken = null
        )
        {
            var initializeFunction = new InitializeFunction();
            initializeFunction.DefaultAdmin = defaultAdmin;
            initializeFunction.Name = name;
            initializeFunction.Symbol = symbol;
            initializeFunction.ContractURI = contractURI;
            initializeFunction.TrustedForwarders = trustedForwarders;
            initializeFunction.SaleRecipient = saleRecipient;
            initializeFunction.RoyaltyRecipient = royaltyRecipient;
            initializeFunction.RoyaltyBps = royaltyBps;
            initializeFunction.PlatformFeeBps = platformFeeBps;
            initializeFunction.PlatformFeeRecipient = platformFeeRecipient;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(initializeFunction, cancellationToken);
        }

        public Task<bool> IsApprovedForAllQueryAsync(IsApprovedForAllFunction isApprovedForAllFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<IsApprovedForAllFunction, bool>(isApprovedForAllFunction, blockParameter);
        }

        public Task<bool> IsApprovedForAllQueryAsync(string owner, string @operator, BlockParameter blockParameter = null)
        {
            var isApprovedForAllFunction = new IsApprovedForAllFunction();
            isApprovedForAllFunction.Owner = owner;
            isApprovedForAllFunction.Operator = @operator;

            return ContractHandler.QueryAsync<IsApprovedForAllFunction, bool>(isApprovedForAllFunction, blockParameter);
        }

        public Task<bool> IsTrustedForwarderQueryAsync(IsTrustedForwarderFunction isTrustedForwarderFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<IsTrustedForwarderFunction, bool>(isTrustedForwarderFunction, blockParameter);
        }

        public Task<bool> IsTrustedForwarderQueryAsync(string forwarder, BlockParameter blockParameter = null)
        {
            var isTrustedForwarderFunction = new IsTrustedForwarderFunction();
            isTrustedForwarderFunction.Forwarder = forwarder;

            return ContractHandler.QueryAsync<IsTrustedForwarderFunction, bool>(isTrustedForwarderFunction, blockParameter);
        }

        public Task<string> MintToRequestAsync(MintToFunction mintToFunction)
        {
            return ContractHandler.SendRequestAsync(mintToFunction);
        }

        public Task<TransactionReceipt> MintToRequestAndWaitForReceiptAsync(MintToFunction mintToFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(mintToFunction, cancellationToken);
        }

        public Task<string> MintToRequestAsync(string to, string uri)
        {
            var mintToFunction = new MintToFunction();
            mintToFunction.To = to;
            mintToFunction.Uri = uri;

            return ContractHandler.SendRequestAsync(mintToFunction);
        }

        public Task<TransactionReceipt> MintToRequestAndWaitForReceiptAsync(string to, string uri, CancellationTokenSource cancellationToken = null)
        {
            var mintToFunction = new MintToFunction();
            mintToFunction.To = to;
            mintToFunction.Uri = uri;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(mintToFunction, cancellationToken);
        }

        public Task<string> MintWithSignatureRequestAsync(MintWithSignatureFunction mintWithSignatureFunction)
        {
            return ContractHandler.SendRequestAsync(mintWithSignatureFunction);
        }

        public Task<TransactionReceipt> MintWithSignatureRequestAndWaitForReceiptAsync(MintWithSignatureFunction mintWithSignatureFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(mintWithSignatureFunction, cancellationToken);
        }

        public Task<string> MintWithSignatureRequestAsync(MintRequest req, byte[] signature)
        {
            var mintWithSignatureFunction = new MintWithSignatureFunction();
            mintWithSignatureFunction.Req = req;
            mintWithSignatureFunction.Signature = signature;

            return ContractHandler.SendRequestAsync(mintWithSignatureFunction);
        }

        public Task<TransactionReceipt> MintWithSignatureRequestAndWaitForReceiptAsync(MintRequest req, byte[] signature, CancellationTokenSource cancellationToken = null)
        {
            var mintWithSignatureFunction = new MintWithSignatureFunction();
            mintWithSignatureFunction.Req = req;
            mintWithSignatureFunction.Signature = signature;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(mintWithSignatureFunction, cancellationToken);
        }

        public Task<string> MulticallRequestAsync(MulticallFunction multicallFunction)
        {
            return ContractHandler.SendRequestAsync(multicallFunction);
        }

        public Task<TransactionReceipt> MulticallRequestAndWaitForReceiptAsync(MulticallFunction multicallFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(multicallFunction, cancellationToken);
        }

        public Task<string> MulticallRequestAsync(List<byte[]> data)
        {
            var multicallFunction = new MulticallFunction();
            multicallFunction.Data = data;

            return ContractHandler.SendRequestAsync(multicallFunction);
        }

        public Task<TransactionReceipt> MulticallRequestAndWaitForReceiptAsync(List<byte[]> data, CancellationTokenSource cancellationToken = null)
        {
            var multicallFunction = new MulticallFunction();
            multicallFunction.Data = data;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(multicallFunction, cancellationToken);
        }

        public Task<string> NameQueryAsync(NameFunction nameFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<NameFunction, string>(nameFunction, blockParameter);
        }

        public Task<string> NameQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<NameFunction, string>(null, blockParameter);
        }

        public Task<BigInteger> NextTokenIdToMintQueryAsync(NextTokenIdToMintFunction nextTokenIdToMintFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<NextTokenIdToMintFunction, BigInteger>(nextTokenIdToMintFunction, blockParameter);
        }

        public Task<BigInteger> NextTokenIdToMintQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<NextTokenIdToMintFunction, BigInteger>(null, blockParameter);
        }

        public Task<bool> OperatorRestrictionQueryAsync(OperatorRestrictionFunction operatorRestrictionFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OperatorRestrictionFunction, bool>(operatorRestrictionFunction, blockParameter);
        }

        public Task<bool> OperatorRestrictionQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OperatorRestrictionFunction, bool>(null, blockParameter);
        }

        public Task<string> OwnerQueryAsync(OwnerFunction ownerFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(ownerFunction, blockParameter);
        }

        public Task<string> OwnerQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(null, blockParameter);
        }

        public Task<string> OwnerOfQueryAsync(OwnerOfFunction ownerOfFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OwnerOfFunction, string>(ownerOfFunction, blockParameter);
        }

        public Task<string> OwnerOfQueryAsync(BigInteger tokenId, BlockParameter blockParameter = null)
        {
            var ownerOfFunction = new OwnerOfFunction();
            ownerOfFunction.TokenId = tokenId;

            return ContractHandler.QueryAsync<OwnerOfFunction, string>(ownerOfFunction, blockParameter);
        }

        public Task<string> PlatformFeeRecipientQueryAsync(PlatformFeeRecipientFunction platformFeeRecipientFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<PlatformFeeRecipientFunction, string>(platformFeeRecipientFunction, blockParameter);
        }

        public Task<string> PlatformFeeRecipientQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<PlatformFeeRecipientFunction, string>(null, blockParameter);
        }

        public Task<string> PrimarySaleRecipientQueryAsync(PrimarySaleRecipientFunction primarySaleRecipientFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<PrimarySaleRecipientFunction, string>(primarySaleRecipientFunction, blockParameter);
        }

        public Task<string> PrimarySaleRecipientQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<PrimarySaleRecipientFunction, string>(null, blockParameter);
        }

        public Task<string> RenounceRoleRequestAsync(RenounceRoleFunction renounceRoleFunction)
        {
            return ContractHandler.SendRequestAsync(renounceRoleFunction);
        }

        public Task<TransactionReceipt> RenounceRoleRequestAndWaitForReceiptAsync(RenounceRoleFunction renounceRoleFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(renounceRoleFunction, cancellationToken);
        }

        public Task<string> RenounceRoleRequestAsync(byte[] role, string account)
        {
            var renounceRoleFunction = new RenounceRoleFunction();
            renounceRoleFunction.Role = role;
            renounceRoleFunction.Account = account;

            return ContractHandler.SendRequestAsync(renounceRoleFunction);
        }

        public Task<TransactionReceipt> RenounceRoleRequestAndWaitForReceiptAsync(byte[] role, string account, CancellationTokenSource cancellationToken = null)
        {
            var renounceRoleFunction = new RenounceRoleFunction();
            renounceRoleFunction.Role = role;
            renounceRoleFunction.Account = account;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(renounceRoleFunction, cancellationToken);
        }

        public Task<string> RevokeRoleRequestAsync(RevokeRoleFunction revokeRoleFunction)
        {
            return ContractHandler.SendRequestAsync(revokeRoleFunction);
        }

        public Task<TransactionReceipt> RevokeRoleRequestAndWaitForReceiptAsync(RevokeRoleFunction revokeRoleFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(revokeRoleFunction, cancellationToken);
        }

        public Task<string> RevokeRoleRequestAsync(byte[] role, string account)
        {
            var revokeRoleFunction = new RevokeRoleFunction();
            revokeRoleFunction.Role = role;
            revokeRoleFunction.Account = account;

            return ContractHandler.SendRequestAsync(revokeRoleFunction);
        }

        public Task<TransactionReceipt> RevokeRoleRequestAndWaitForReceiptAsync(byte[] role, string account, CancellationTokenSource cancellationToken = null)
        {
            var revokeRoleFunction = new RevokeRoleFunction();
            revokeRoleFunction.Role = role;
            revokeRoleFunction.Account = account;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(revokeRoleFunction, cancellationToken);
        }

        public Task<RoyaltyInfoOutputDTO> RoyaltyInfoQueryAsync(RoyaltyInfoFunction royaltyInfoFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<RoyaltyInfoFunction, RoyaltyInfoOutputDTO>(royaltyInfoFunction, blockParameter);
        }

        public Task<RoyaltyInfoOutputDTO> RoyaltyInfoQueryAsync(BigInteger tokenId, BigInteger salePrice, BlockParameter blockParameter = null)
        {
            var royaltyInfoFunction = new RoyaltyInfoFunction();
            royaltyInfoFunction.TokenId = tokenId;
            royaltyInfoFunction.SalePrice = salePrice;

            return ContractHandler.QueryDeserializingToObjectAsync<RoyaltyInfoFunction, RoyaltyInfoOutputDTO>(royaltyInfoFunction, blockParameter);
        }

        public Task<string> SafeTransferFromRequestAsync(SafeTransferFromFunction safeTransferFromFunction)
        {
            return ContractHandler.SendRequestAsync(safeTransferFromFunction);
        }

        public Task<TransactionReceipt> SafeTransferFromRequestAndWaitForReceiptAsync(SafeTransferFromFunction safeTransferFromFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(safeTransferFromFunction, cancellationToken);
        }

        public Task<string> SafeTransferFromRequestAsync(string from, string to, BigInteger tokenId)
        {
            var safeTransferFromFunction = new SafeTransferFromFunction();
            safeTransferFromFunction.From = from;
            safeTransferFromFunction.To = to;
            safeTransferFromFunction.TokenId = tokenId;

            return ContractHandler.SendRequestAsync(safeTransferFromFunction);
        }

        public Task<TransactionReceipt> SafeTransferFromRequestAndWaitForReceiptAsync(string from, string to, BigInteger tokenId, CancellationTokenSource cancellationToken = null)
        {
            var safeTransferFromFunction = new SafeTransferFromFunction();
            safeTransferFromFunction.From = from;
            safeTransferFromFunction.To = to;
            safeTransferFromFunction.TokenId = tokenId;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(safeTransferFromFunction, cancellationToken);
        }

        public Task<string> SafeTransferFromRequestAsync(SafeTransferFrom1Function safeTransferFrom1Function)
        {
            return ContractHandler.SendRequestAsync(safeTransferFrom1Function);
        }

        public Task<TransactionReceipt> SafeTransferFromRequestAndWaitForReceiptAsync(SafeTransferFrom1Function safeTransferFrom1Function, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(safeTransferFrom1Function, cancellationToken);
        }

        public Task<string> SafeTransferFromRequestAsync(string from, string to, BigInteger tokenId, byte[] data)
        {
            var safeTransferFrom1Function = new SafeTransferFrom1Function();
            safeTransferFrom1Function.From = from;
            safeTransferFrom1Function.To = to;
            safeTransferFrom1Function.TokenId = tokenId;
            safeTransferFrom1Function.Data = data;

            return ContractHandler.SendRequestAsync(safeTransferFrom1Function);
        }

        public Task<TransactionReceipt> SafeTransferFromRequestAndWaitForReceiptAsync(string from, string to, BigInteger tokenId, byte[] data, CancellationTokenSource cancellationToken = null)
        {
            var safeTransferFrom1Function = new SafeTransferFrom1Function();
            safeTransferFrom1Function.From = from;
            safeTransferFrom1Function.To = to;
            safeTransferFrom1Function.TokenId = tokenId;
            safeTransferFrom1Function.Data = data;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(safeTransferFrom1Function, cancellationToken);
        }

        public Task<string> SetApprovalForAllRequestAsync(SetApprovalForAllFunction setApprovalForAllFunction)
        {
            return ContractHandler.SendRequestAsync(setApprovalForAllFunction);
        }

        public Task<TransactionReceipt> SetApprovalForAllRequestAndWaitForReceiptAsync(SetApprovalForAllFunction setApprovalForAllFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setApprovalForAllFunction, cancellationToken);
        }

        public Task<string> SetApprovalForAllRequestAsync(string @operator, bool approved)
        {
            var setApprovalForAllFunction = new SetApprovalForAllFunction();
            setApprovalForAllFunction.Operator = @operator;
            setApprovalForAllFunction.Approved = approved;

            return ContractHandler.SendRequestAsync(setApprovalForAllFunction);
        }

        public Task<TransactionReceipt> SetApprovalForAllRequestAndWaitForReceiptAsync(string @operator, bool approved, CancellationTokenSource cancellationToken = null)
        {
            var setApprovalForAllFunction = new SetApprovalForAllFunction();
            setApprovalForAllFunction.Operator = @operator;
            setApprovalForAllFunction.Approved = approved;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setApprovalForAllFunction, cancellationToken);
        }

        public Task<string> SetContractURIRequestAsync(SetContractURIFunction setContractURIFunction)
        {
            return ContractHandler.SendRequestAsync(setContractURIFunction);
        }

        public Task<TransactionReceipt> SetContractURIRequestAndWaitForReceiptAsync(SetContractURIFunction setContractURIFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setContractURIFunction, cancellationToken);
        }

        public Task<string> SetContractURIRequestAsync(string uri)
        {
            var setContractURIFunction = new SetContractURIFunction();
            setContractURIFunction.Uri = uri;

            return ContractHandler.SendRequestAsync(setContractURIFunction);
        }

        public Task<TransactionReceipt> SetContractURIRequestAndWaitForReceiptAsync(string uri, CancellationTokenSource cancellationToken = null)
        {
            var setContractURIFunction = new SetContractURIFunction();
            setContractURIFunction.Uri = uri;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setContractURIFunction, cancellationToken);
        }

        public Task<string> SetDefaultRoyaltyInfoRequestAsync(SetDefaultRoyaltyInfoFunction setDefaultRoyaltyInfoFunction)
        {
            return ContractHandler.SendRequestAsync(setDefaultRoyaltyInfoFunction);
        }

        public Task<TransactionReceipt> SetDefaultRoyaltyInfoRequestAndWaitForReceiptAsync(
            SetDefaultRoyaltyInfoFunction setDefaultRoyaltyInfoFunction,
            CancellationTokenSource cancellationToken = null
        )
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setDefaultRoyaltyInfoFunction, cancellationToken);
        }

        public Task<string> SetDefaultRoyaltyInfoRequestAsync(string royaltyRecipient, BigInteger royaltyBps)
        {
            var setDefaultRoyaltyInfoFunction = new SetDefaultRoyaltyInfoFunction();
            setDefaultRoyaltyInfoFunction.RoyaltyRecipient = royaltyRecipient;
            setDefaultRoyaltyInfoFunction.RoyaltyBps = royaltyBps;

            return ContractHandler.SendRequestAsync(setDefaultRoyaltyInfoFunction);
        }

        public Task<TransactionReceipt> SetDefaultRoyaltyInfoRequestAndWaitForReceiptAsync(string royaltyRecipient, BigInteger royaltyBps, CancellationTokenSource cancellationToken = null)
        {
            var setDefaultRoyaltyInfoFunction = new SetDefaultRoyaltyInfoFunction();
            setDefaultRoyaltyInfoFunction.RoyaltyRecipient = royaltyRecipient;
            setDefaultRoyaltyInfoFunction.RoyaltyBps = royaltyBps;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setDefaultRoyaltyInfoFunction, cancellationToken);
        }

        public Task<string> SetOperatorRestrictionRequestAsync(SetOperatorRestrictionFunction setOperatorRestrictionFunction)
        {
            return ContractHandler.SendRequestAsync(setOperatorRestrictionFunction);
        }

        public Task<TransactionReceipt> SetOperatorRestrictionRequestAndWaitForReceiptAsync(
            SetOperatorRestrictionFunction setOperatorRestrictionFunction,
            CancellationTokenSource cancellationToken = null
        )
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setOperatorRestrictionFunction, cancellationToken);
        }

        public Task<string> SetOperatorRestrictionRequestAsync(bool restriction)
        {
            var setOperatorRestrictionFunction = new SetOperatorRestrictionFunction();
            setOperatorRestrictionFunction.Restriction = restriction;

            return ContractHandler.SendRequestAsync(setOperatorRestrictionFunction);
        }

        public Task<TransactionReceipt> SetOperatorRestrictionRequestAndWaitForReceiptAsync(bool restriction, CancellationTokenSource cancellationToken = null)
        {
            var setOperatorRestrictionFunction = new SetOperatorRestrictionFunction();
            setOperatorRestrictionFunction.Restriction = restriction;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setOperatorRestrictionFunction, cancellationToken);
        }

        public Task<string> SetOwnerRequestAsync(SetOwnerFunction setOwnerFunction)
        {
            return ContractHandler.SendRequestAsync(setOwnerFunction);
        }

        public Task<TransactionReceipt> SetOwnerRequestAndWaitForReceiptAsync(SetOwnerFunction setOwnerFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setOwnerFunction, cancellationToken);
        }

        public Task<string> SetOwnerRequestAsync(string newOwner)
        {
            var setOwnerFunction = new SetOwnerFunction();
            setOwnerFunction.NewOwner = newOwner;

            return ContractHandler.SendRequestAsync(setOwnerFunction);
        }

        public Task<TransactionReceipt> SetOwnerRequestAndWaitForReceiptAsync(string newOwner, CancellationTokenSource cancellationToken = null)
        {
            var setOwnerFunction = new SetOwnerFunction();
            setOwnerFunction.NewOwner = newOwner;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setOwnerFunction, cancellationToken);
        }

        public Task<string> SetPlatformFeeInfoRequestAsync(SetPlatformFeeInfoFunction setPlatformFeeInfoFunction)
        {
            return ContractHandler.SendRequestAsync(setPlatformFeeInfoFunction);
        }

        public Task<TransactionReceipt> SetPlatformFeeInfoRequestAndWaitForReceiptAsync(SetPlatformFeeInfoFunction setPlatformFeeInfoFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setPlatformFeeInfoFunction, cancellationToken);
        }

        public Task<string> SetPlatformFeeInfoRequestAsync(string platformFeeRecipient, BigInteger platformFeeBps)
        {
            var setPlatformFeeInfoFunction = new SetPlatformFeeInfoFunction();
            setPlatformFeeInfoFunction.PlatformFeeRecipient = platformFeeRecipient;
            setPlatformFeeInfoFunction.PlatformFeeBps = platformFeeBps;

            return ContractHandler.SendRequestAsync(setPlatformFeeInfoFunction);
        }

        public Task<TransactionReceipt> SetPlatformFeeInfoRequestAndWaitForReceiptAsync(string platformFeeRecipient, BigInteger platformFeeBps, CancellationTokenSource cancellationToken = null)
        {
            var setPlatformFeeInfoFunction = new SetPlatformFeeInfoFunction();
            setPlatformFeeInfoFunction.PlatformFeeRecipient = platformFeeRecipient;
            setPlatformFeeInfoFunction.PlatformFeeBps = platformFeeBps;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setPlatformFeeInfoFunction, cancellationToken);
        }

        public Task<string> SetPrimarySaleRecipientRequestAsync(SetPrimarySaleRecipientFunction setPrimarySaleRecipientFunction)
        {
            return ContractHandler.SendRequestAsync(setPrimarySaleRecipientFunction);
        }

        public Task<TransactionReceipt> SetPrimarySaleRecipientRequestAndWaitForReceiptAsync(
            SetPrimarySaleRecipientFunction setPrimarySaleRecipientFunction,
            CancellationTokenSource cancellationToken = null
        )
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setPrimarySaleRecipientFunction, cancellationToken);
        }

        public Task<string> SetPrimarySaleRecipientRequestAsync(string saleRecipient)
        {
            var setPrimarySaleRecipientFunction = new SetPrimarySaleRecipientFunction();
            setPrimarySaleRecipientFunction.SaleRecipient = saleRecipient;

            return ContractHandler.SendRequestAsync(setPrimarySaleRecipientFunction);
        }

        public Task<TransactionReceipt> SetPrimarySaleRecipientRequestAndWaitForReceiptAsync(string saleRecipient, CancellationTokenSource cancellationToken = null)
        {
            var setPrimarySaleRecipientFunction = new SetPrimarySaleRecipientFunction();
            setPrimarySaleRecipientFunction.SaleRecipient = saleRecipient;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setPrimarySaleRecipientFunction, cancellationToken);
        }

        public Task<string> SetRoyaltyInfoForTokenRequestAsync(SetRoyaltyInfoForTokenFunction setRoyaltyInfoForTokenFunction)
        {
            return ContractHandler.SendRequestAsync(setRoyaltyInfoForTokenFunction);
        }

        public Task<TransactionReceipt> SetRoyaltyInfoForTokenRequestAndWaitForReceiptAsync(
            SetRoyaltyInfoForTokenFunction setRoyaltyInfoForTokenFunction,
            CancellationTokenSource cancellationToken = null
        )
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setRoyaltyInfoForTokenFunction, cancellationToken);
        }

        public Task<string> SetRoyaltyInfoForTokenRequestAsync(BigInteger tokenId, string recipient, BigInteger bps)
        {
            var setRoyaltyInfoForTokenFunction = new SetRoyaltyInfoForTokenFunction();
            setRoyaltyInfoForTokenFunction.TokenId = tokenId;
            setRoyaltyInfoForTokenFunction.Recipient = recipient;
            setRoyaltyInfoForTokenFunction.Bps = bps;

            return ContractHandler.SendRequestAsync(setRoyaltyInfoForTokenFunction);
        }

        public Task<TransactionReceipt> SetRoyaltyInfoForTokenRequestAndWaitForReceiptAsync(BigInteger tokenId, string recipient, BigInteger bps, CancellationTokenSource cancellationToken = null)
        {
            var setRoyaltyInfoForTokenFunction = new SetRoyaltyInfoForTokenFunction();
            setRoyaltyInfoForTokenFunction.TokenId = tokenId;
            setRoyaltyInfoForTokenFunction.Recipient = recipient;
            setRoyaltyInfoForTokenFunction.Bps = bps;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setRoyaltyInfoForTokenFunction, cancellationToken);
        }

        public Task<bool> SupportsInterfaceQueryAsync(SupportsInterfaceFunction supportsInterfaceFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SupportsInterfaceFunction, bool>(supportsInterfaceFunction, blockParameter);
        }

        public Task<bool> SupportsInterfaceQueryAsync(byte[] interfaceId, BlockParameter blockParameter = null)
        {
            var supportsInterfaceFunction = new SupportsInterfaceFunction();
            supportsInterfaceFunction.InterfaceId = interfaceId;

            return ContractHandler.QueryAsync<SupportsInterfaceFunction, bool>(supportsInterfaceFunction, blockParameter);
        }

        public Task<string> SymbolQueryAsync(SymbolFunction symbolFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SymbolFunction, string>(symbolFunction, blockParameter);
        }

        public Task<string> SymbolQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SymbolFunction, string>(null, blockParameter);
        }

        public Task<BigInteger> TokenByIndexQueryAsync(TokenByIndexFunction tokenByIndexFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TokenByIndexFunction, BigInteger>(tokenByIndexFunction, blockParameter);
        }

        public Task<BigInteger> TokenByIndexQueryAsync(BigInteger index, BlockParameter blockParameter = null)
        {
            var tokenByIndexFunction = new TokenByIndexFunction();
            tokenByIndexFunction.Index = index;

            return ContractHandler.QueryAsync<TokenByIndexFunction, BigInteger>(tokenByIndexFunction, blockParameter);
        }

        public Task<BigInteger> TokenOfOwnerByIndexQueryAsync(TokenOfOwnerByIndexFunction tokenOfOwnerByIndexFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TokenOfOwnerByIndexFunction, BigInteger>(tokenOfOwnerByIndexFunction, blockParameter);
        }

        public Task<BigInteger> TokenOfOwnerByIndexQueryAsync(string owner, BigInteger index, BlockParameter blockParameter = null)
        {
            var tokenOfOwnerByIndexFunction = new TokenOfOwnerByIndexFunction();
            tokenOfOwnerByIndexFunction.Owner = owner;
            tokenOfOwnerByIndexFunction.Index = index;

            return ContractHandler.QueryAsync<TokenOfOwnerByIndexFunction, BigInteger>(tokenOfOwnerByIndexFunction, blockParameter);
        }

        public Task<string> TokenURIQueryAsync(TokenURIFunction tokenURIFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TokenURIFunction, string>(tokenURIFunction, blockParameter);
        }

        public Task<string> TokenURIQueryAsync(BigInteger tokenId, BlockParameter blockParameter = null)
        {
            var tokenURIFunction = new TokenURIFunction();
            tokenURIFunction.TokenId = tokenId;

            return ContractHandler.QueryAsync<TokenURIFunction, string>(tokenURIFunction, blockParameter);
        }

        public Task<BigInteger> TotalSupplyQueryAsync(TotalSupplyFunction totalSupplyFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(totalSupplyFunction, blockParameter);
        }

        public Task<BigInteger> TotalSupplyQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(null, blockParameter);
        }

        public Task<string> TransferFromRequestAsync(TransferFromFunction transferFromFunction)
        {
            return ContractHandler.SendRequestAsync(transferFromFunction);
        }

        public Task<TransactionReceipt> TransferFromRequestAndWaitForReceiptAsync(TransferFromFunction transferFromFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFromFunction, cancellationToken);
        }

        public Task<string> TransferFromRequestAsync(string from, string to, BigInteger tokenId)
        {
            var transferFromFunction = new TransferFromFunction();
            transferFromFunction.From = from;
            transferFromFunction.To = to;
            transferFromFunction.TokenId = tokenId;

            return ContractHandler.SendRequestAsync(transferFromFunction);
        }

        public Task<TransactionReceipt> TransferFromRequestAndWaitForReceiptAsync(string from, string to, BigInteger tokenId, CancellationTokenSource cancellationToken = null)
        {
            var transferFromFunction = new TransferFromFunction();
            transferFromFunction.From = from;
            transferFromFunction.To = to;
            transferFromFunction.TokenId = tokenId;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFromFunction, cancellationToken);
        }

        public Task<VerifyOutputDTO> VerifyQueryAsync(VerifyFunction verifyFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<VerifyFunction, VerifyOutputDTO>(verifyFunction, blockParameter);
        }

        public Task<VerifyOutputDTO> VerifyQueryAsync(MintRequest req, byte[] signature, BlockParameter blockParameter = null)
        {
            var verifyFunction = new VerifyFunction();
            verifyFunction.Req = req;
            verifyFunction.Signature = signature;

            return ContractHandler.QueryDeserializingToObjectAsync<VerifyFunction, VerifyOutputDTO>(verifyFunction, blockParameter);
        }
    }
}
