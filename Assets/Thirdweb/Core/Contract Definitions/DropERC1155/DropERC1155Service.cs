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
using Thirdweb.Contracts.DropERC1155.ContractDefinition;

namespace Thirdweb.Contracts.DropERC1155
{
    public partial class DropERC1155Service
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(
            Nethereum.Web3.Web3 web3,
            DropERC1155Deployment dropERC1155Deployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            return web3.Eth.GetContractDeploymentHandler<DropERC1155Deployment>().SendRequestAndWaitForReceiptAsync(dropERC1155Deployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, DropERC1155Deployment dropERC1155Deployment)
        {
            return web3.Eth.GetContractDeploymentHandler<DropERC1155Deployment>().SendRequestAsync(dropERC1155Deployment);
        }

        public static async Task<DropERC1155Service> DeployContractAndGetServiceAsync(
            Nethereum.Web3.Web3 web3,
            DropERC1155Deployment dropERC1155Deployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, dropERC1155Deployment, cancellationTokenSource);
            return new DropERC1155Service(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.Web3 Web3 { get; }

        public ContractHandler ContractHandler { get; }

        public DropERC1155Service(Nethereum.Web3.Web3 web3, string contractAddress)
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

        public Task<BigInteger> BalanceOfQueryAsync(BalanceOfFunction balanceOfFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameter);
        }

        public Task<BigInteger> BalanceOfQueryAsync(string account, BigInteger id, BlockParameter blockParameter = null)
        {
            var balanceOfFunction = new BalanceOfFunction();
            balanceOfFunction.Account = account;
            balanceOfFunction.Id = id;

            return ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameter);
        }

        public Task<List<BigInteger>> BalanceOfBatchQueryAsync(BalanceOfBatchFunction balanceOfBatchFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<BalanceOfBatchFunction, List<BigInteger>>(balanceOfBatchFunction, blockParameter);
        }

        public Task<List<BigInteger>> BalanceOfBatchQueryAsync(List<string> accounts, List<BigInteger> ids, BlockParameter blockParameter = null)
        {
            var balanceOfBatchFunction = new BalanceOfBatchFunction();
            balanceOfBatchFunction.Accounts = accounts;
            balanceOfBatchFunction.Ids = ids;

            return ContractHandler.QueryAsync<BalanceOfBatchFunction, List<BigInteger>>(balanceOfBatchFunction, blockParameter);
        }

        public Task<string> BurnBatchRequestAsync(BurnBatchFunction burnBatchFunction)
        {
            return ContractHandler.SendRequestAsync(burnBatchFunction);
        }

        public Task<TransactionReceipt> BurnBatchRequestAndWaitForReceiptAsync(BurnBatchFunction burnBatchFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(burnBatchFunction, cancellationToken);
        }

        public Task<string> BurnBatchRequestAsync(string account, List<BigInteger> ids, List<BigInteger> values)
        {
            var burnBatchFunction = new BurnBatchFunction();
            burnBatchFunction.Account = account;
            burnBatchFunction.Ids = ids;
            burnBatchFunction.Values = values;

            return ContractHandler.SendRequestAsync(burnBatchFunction);
        }

        public Task<TransactionReceipt> BurnBatchRequestAndWaitForReceiptAsync(string account, List<BigInteger> ids, List<BigInteger> values, CancellationTokenSource cancellationToken = null)
        {
            var burnBatchFunction = new BurnBatchFunction();
            burnBatchFunction.Account = account;
            burnBatchFunction.Ids = ids;
            burnBatchFunction.Values = values;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(burnBatchFunction, cancellationToken);
        }

        public Task<string> ClaimRequestAsync(ClaimFunction claimFunction)
        {
            return ContractHandler.SendRequestAsync(claimFunction);
        }

        public Task<TransactionReceipt> ClaimRequestAndWaitForReceiptAsync(ClaimFunction claimFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(claimFunction, cancellationToken);
        }

        public Task<string> ClaimRequestAsync(string receiver, BigInteger tokenId, BigInteger quantity, string currency, BigInteger pricePerToken, AllowlistProof allowlistProof, byte[] data)
        {
            var claimFunction = new ClaimFunction();
            claimFunction.Receiver = receiver;
            claimFunction.TokenId = tokenId;
            claimFunction.Quantity = quantity;
            claimFunction.Currency = currency;
            claimFunction.PricePerToken = pricePerToken;
            claimFunction.AllowlistProof = allowlistProof;
            claimFunction.Data = data;

            return ContractHandler.SendRequestAsync(claimFunction);
        }

        public Task<TransactionReceipt> ClaimRequestAndWaitForReceiptAsync(
            string receiver,
            BigInteger tokenId,
            BigInteger quantity,
            string currency,
            BigInteger pricePerToken,
            AllowlistProof allowlistProof,
            byte[] data,
            CancellationTokenSource cancellationToken = null
        )
        {
            var claimFunction = new ClaimFunction();
            claimFunction.Receiver = receiver;
            claimFunction.TokenId = tokenId;
            claimFunction.Quantity = quantity;
            claimFunction.Currency = currency;
            claimFunction.PricePerToken = pricePerToken;
            claimFunction.AllowlistProof = allowlistProof;
            claimFunction.Data = data;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(claimFunction, cancellationToken);
        }

        public Task<ClaimConditionOutputDTO> ClaimConditionQueryAsync(ClaimConditionFunction claimConditionFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<ClaimConditionFunction, ClaimConditionOutputDTO>(claimConditionFunction, blockParameter);
        }

        public Task<ClaimConditionOutputDTO> ClaimConditionQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var claimConditionFunction = new ClaimConditionFunction();
            claimConditionFunction.ReturnValue1 = returnValue1;

            return ContractHandler.QueryDeserializingToObjectAsync<ClaimConditionFunction, ClaimConditionOutputDTO>(claimConditionFunction, blockParameter);
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

        public Task<BigInteger> GetActiveClaimConditionIdQueryAsync(GetActiveClaimConditionIdFunction getActiveClaimConditionIdFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetActiveClaimConditionIdFunction, BigInteger>(getActiveClaimConditionIdFunction, blockParameter);
        }

        public Task<BigInteger> GetActiveClaimConditionIdQueryAsync(BigInteger tokenId, BlockParameter blockParameter = null)
        {
            var getActiveClaimConditionIdFunction = new GetActiveClaimConditionIdFunction();
            getActiveClaimConditionIdFunction.TokenId = tokenId;

            return ContractHandler.QueryAsync<GetActiveClaimConditionIdFunction, BigInteger>(getActiveClaimConditionIdFunction, blockParameter);
        }

        public Task<BigInteger> GetBaseURICountQueryAsync(GetBaseURICountFunction getBaseURICountFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetBaseURICountFunction, BigInteger>(getBaseURICountFunction, blockParameter);
        }

        public Task<BigInteger> GetBaseURICountQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetBaseURICountFunction, BigInteger>(null, blockParameter);
        }

        public Task<BigInteger> GetBatchIdAtIndexQueryAsync(GetBatchIdAtIndexFunction getBatchIdAtIndexFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetBatchIdAtIndexFunction, BigInteger>(getBatchIdAtIndexFunction, blockParameter);
        }

        public Task<BigInteger> GetBatchIdAtIndexQueryAsync(BigInteger index, BlockParameter blockParameter = null)
        {
            var getBatchIdAtIndexFunction = new GetBatchIdAtIndexFunction();
            getBatchIdAtIndexFunction.Index = index;

            return ContractHandler.QueryAsync<GetBatchIdAtIndexFunction, BigInteger>(getBatchIdAtIndexFunction, blockParameter);
        }

        public Task<GetClaimConditionByIdOutputDTO> GetClaimConditionByIdQueryAsync(GetClaimConditionByIdFunction getClaimConditionByIdFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetClaimConditionByIdFunction, GetClaimConditionByIdOutputDTO>(getClaimConditionByIdFunction, blockParameter);
        }

        public Task<GetClaimConditionByIdOutputDTO> GetClaimConditionByIdQueryAsync(BigInteger tokenId, BigInteger conditionId, BlockParameter blockParameter = null)
        {
            var getClaimConditionByIdFunction = new GetClaimConditionByIdFunction();
            getClaimConditionByIdFunction.TokenId = tokenId;
            getClaimConditionByIdFunction.ConditionId = conditionId;

            return ContractHandler.QueryDeserializingToObjectAsync<GetClaimConditionByIdFunction, GetClaimConditionByIdOutputDTO>(getClaimConditionByIdFunction, blockParameter);
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

        public Task<BigInteger> GetSupplyClaimedByWalletQueryAsync(GetSupplyClaimedByWalletFunction getSupplyClaimedByWalletFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetSupplyClaimedByWalletFunction, BigInteger>(getSupplyClaimedByWalletFunction, blockParameter);
        }

        public Task<BigInteger> GetSupplyClaimedByWalletQueryAsync(BigInteger tokenId, BigInteger conditionId, string claimer, BlockParameter blockParameter = null)
        {
            var getSupplyClaimedByWalletFunction = new GetSupplyClaimedByWalletFunction();
            getSupplyClaimedByWalletFunction.TokenId = tokenId;
            getSupplyClaimedByWalletFunction.ConditionId = conditionId;
            getSupplyClaimedByWalletFunction.Claimer = claimer;

            return ContractHandler.QueryAsync<GetSupplyClaimedByWalletFunction, BigInteger>(getSupplyClaimedByWalletFunction, blockParameter);
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

        public Task<bool> HasRoleWithSwitchQueryAsync(HasRoleWithSwitchFunction hasRoleWithSwitchFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<HasRoleWithSwitchFunction, bool>(hasRoleWithSwitchFunction, blockParameter);
        }

        public Task<bool> HasRoleWithSwitchQueryAsync(byte[] role, string account, BlockParameter blockParameter = null)
        {
            var hasRoleWithSwitchFunction = new HasRoleWithSwitchFunction();
            hasRoleWithSwitchFunction.Role = role;
            hasRoleWithSwitchFunction.Account = account;

            return ContractHandler.QueryAsync<HasRoleWithSwitchFunction, bool>(hasRoleWithSwitchFunction, blockParameter);
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

        public Task<bool> IsApprovedForAllQueryAsync(string account, string @operator, BlockParameter blockParameter = null)
        {
            var isApprovedForAllFunction = new IsApprovedForAllFunction();
            isApprovedForAllFunction.Account = account;
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

        public Task<string> LazyMintRequestAsync(LazyMintFunction lazyMintFunction)
        {
            return ContractHandler.SendRequestAsync(lazyMintFunction);
        }

        public Task<TransactionReceipt> LazyMintRequestAndWaitForReceiptAsync(LazyMintFunction lazyMintFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(lazyMintFunction, cancellationToken);
        }

        public Task<string> LazyMintRequestAsync(BigInteger amount, string baseURIForTokens, byte[] data)
        {
            var lazyMintFunction = new LazyMintFunction();
            lazyMintFunction.Amount = amount;
            lazyMintFunction.BaseURIForTokens = baseURIForTokens;
            lazyMintFunction.Data = data;

            return ContractHandler.SendRequestAsync(lazyMintFunction);
        }

        public Task<TransactionReceipt> LazyMintRequestAndWaitForReceiptAsync(BigInteger amount, string baseURIForTokens, byte[] data, CancellationTokenSource cancellationToken = null)
        {
            var lazyMintFunction = new LazyMintFunction();
            lazyMintFunction.Amount = amount;
            lazyMintFunction.BaseURIForTokens = baseURIForTokens;
            lazyMintFunction.Data = data;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(lazyMintFunction, cancellationToken);
        }

        public Task<BigInteger> MaxTotalSupplyQueryAsync(MaxTotalSupplyFunction maxTotalSupplyFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<MaxTotalSupplyFunction, BigInteger>(maxTotalSupplyFunction, blockParameter);
        }

        public Task<BigInteger> MaxTotalSupplyQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var maxTotalSupplyFunction = new MaxTotalSupplyFunction();
            maxTotalSupplyFunction.ReturnValue1 = returnValue1;

            return ContractHandler.QueryAsync<MaxTotalSupplyFunction, BigInteger>(maxTotalSupplyFunction, blockParameter);
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

        public Task<string> SafeBatchTransferFromRequestAsync(SafeBatchTransferFromFunction safeBatchTransferFromFunction)
        {
            return ContractHandler.SendRequestAsync(safeBatchTransferFromFunction);
        }

        public Task<TransactionReceipt> SafeBatchTransferFromRequestAndWaitForReceiptAsync(
            SafeBatchTransferFromFunction safeBatchTransferFromFunction,
            CancellationTokenSource cancellationToken = null
        )
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(safeBatchTransferFromFunction, cancellationToken);
        }

        public Task<string> SafeBatchTransferFromRequestAsync(string from, string to, List<BigInteger> ids, List<BigInteger> amounts, byte[] data)
        {
            var safeBatchTransferFromFunction = new SafeBatchTransferFromFunction();
            safeBatchTransferFromFunction.From = from;
            safeBatchTransferFromFunction.To = to;
            safeBatchTransferFromFunction.Ids = ids;
            safeBatchTransferFromFunction.Amounts = amounts;
            safeBatchTransferFromFunction.Data = data;

            return ContractHandler.SendRequestAsync(safeBatchTransferFromFunction);
        }

        public Task<TransactionReceipt> SafeBatchTransferFromRequestAndWaitForReceiptAsync(
            string from,
            string to,
            List<BigInteger> ids,
            List<BigInteger> amounts,
            byte[] data,
            CancellationTokenSource cancellationToken = null
        )
        {
            var safeBatchTransferFromFunction = new SafeBatchTransferFromFunction();
            safeBatchTransferFromFunction.From = from;
            safeBatchTransferFromFunction.To = to;
            safeBatchTransferFromFunction.Ids = ids;
            safeBatchTransferFromFunction.Amounts = amounts;
            safeBatchTransferFromFunction.Data = data;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(safeBatchTransferFromFunction, cancellationToken);
        }

        public Task<string> SafeTransferFromRequestAsync(SafeTransferFromFunction safeTransferFromFunction)
        {
            return ContractHandler.SendRequestAsync(safeTransferFromFunction);
        }

        public Task<TransactionReceipt> SafeTransferFromRequestAndWaitForReceiptAsync(SafeTransferFromFunction safeTransferFromFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(safeTransferFromFunction, cancellationToken);
        }

        public Task<string> SafeTransferFromRequestAsync(string from, string to, BigInteger id, BigInteger amount, byte[] data)
        {
            var safeTransferFromFunction = new SafeTransferFromFunction();
            safeTransferFromFunction.From = from;
            safeTransferFromFunction.To = to;
            safeTransferFromFunction.Id = id;
            safeTransferFromFunction.Amount = amount;
            safeTransferFromFunction.Data = data;

            return ContractHandler.SendRequestAsync(safeTransferFromFunction);
        }

        public Task<TransactionReceipt> SafeTransferFromRequestAndWaitForReceiptAsync(
            string from,
            string to,
            BigInteger id,
            BigInteger amount,
            byte[] data,
            CancellationTokenSource cancellationToken = null
        )
        {
            var safeTransferFromFunction = new SafeTransferFromFunction();
            safeTransferFromFunction.From = from;
            safeTransferFromFunction.To = to;
            safeTransferFromFunction.Id = id;
            safeTransferFromFunction.Amount = amount;
            safeTransferFromFunction.Data = data;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(safeTransferFromFunction, cancellationToken);
        }

        public Task<string> SaleRecipientQueryAsync(SaleRecipientFunction saleRecipientFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SaleRecipientFunction, string>(saleRecipientFunction, blockParameter);
        }

        public Task<string> SaleRecipientQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var saleRecipientFunction = new SaleRecipientFunction();
            saleRecipientFunction.ReturnValue1 = returnValue1;

            return ContractHandler.QueryAsync<SaleRecipientFunction, string>(saleRecipientFunction, blockParameter);
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

        public Task<string> SetClaimConditionsRequestAsync(SetClaimConditionsFunction setClaimConditionsFunction)
        {
            return ContractHandler.SendRequestAsync(setClaimConditionsFunction);
        }

        public Task<TransactionReceipt> SetClaimConditionsRequestAndWaitForReceiptAsync(SetClaimConditionsFunction setClaimConditionsFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setClaimConditionsFunction, cancellationToken);
        }

        public Task<string> SetClaimConditionsRequestAsync(BigInteger tokenId, List<ClaimCondition> conditions, bool resetClaimEligibility)
        {
            var setClaimConditionsFunction = new SetClaimConditionsFunction();
            setClaimConditionsFunction.TokenId = tokenId;
            setClaimConditionsFunction.Conditions = conditions;
            setClaimConditionsFunction.ResetClaimEligibility = resetClaimEligibility;

            return ContractHandler.SendRequestAsync(setClaimConditionsFunction);
        }

        public Task<TransactionReceipt> SetClaimConditionsRequestAndWaitForReceiptAsync(
            BigInteger tokenId,
            List<ClaimCondition> conditions,
            bool resetClaimEligibility,
            CancellationTokenSource cancellationToken = null
        )
        {
            var setClaimConditionsFunction = new SetClaimConditionsFunction();
            setClaimConditionsFunction.TokenId = tokenId;
            setClaimConditionsFunction.Conditions = conditions;
            setClaimConditionsFunction.ResetClaimEligibility = resetClaimEligibility;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setClaimConditionsFunction, cancellationToken);
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

        public Task<string> SetMaxTotalSupplyRequestAsync(SetMaxTotalSupplyFunction setMaxTotalSupplyFunction)
        {
            return ContractHandler.SendRequestAsync(setMaxTotalSupplyFunction);
        }

        public Task<TransactionReceipt> SetMaxTotalSupplyRequestAndWaitForReceiptAsync(SetMaxTotalSupplyFunction setMaxTotalSupplyFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setMaxTotalSupplyFunction, cancellationToken);
        }

        public Task<string> SetMaxTotalSupplyRequestAsync(BigInteger tokenId, BigInteger maxTotalSupply)
        {
            var setMaxTotalSupplyFunction = new SetMaxTotalSupplyFunction();
            setMaxTotalSupplyFunction.TokenId = tokenId;
            setMaxTotalSupplyFunction.MaxTotalSupply = maxTotalSupply;

            return ContractHandler.SendRequestAsync(setMaxTotalSupplyFunction);
        }

        public Task<TransactionReceipt> SetMaxTotalSupplyRequestAndWaitForReceiptAsync(BigInteger tokenId, BigInteger maxTotalSupply, CancellationTokenSource cancellationToken = null)
        {
            var setMaxTotalSupplyFunction = new SetMaxTotalSupplyFunction();
            setMaxTotalSupplyFunction.TokenId = tokenId;
            setMaxTotalSupplyFunction.MaxTotalSupply = maxTotalSupply;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setMaxTotalSupplyFunction, cancellationToken);
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

        public Task<string> SetSaleRecipientForTokenRequestAsync(SetSaleRecipientForTokenFunction setSaleRecipientForTokenFunction)
        {
            return ContractHandler.SendRequestAsync(setSaleRecipientForTokenFunction);
        }

        public Task<TransactionReceipt> SetSaleRecipientForTokenRequestAndWaitForReceiptAsync(
            SetSaleRecipientForTokenFunction setSaleRecipientForTokenFunction,
            CancellationTokenSource cancellationToken = null
        )
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setSaleRecipientForTokenFunction, cancellationToken);
        }

        public Task<string> SetSaleRecipientForTokenRequestAsync(BigInteger tokenId, string saleRecipient)
        {
            var setSaleRecipientForTokenFunction = new SetSaleRecipientForTokenFunction();
            setSaleRecipientForTokenFunction.TokenId = tokenId;
            setSaleRecipientForTokenFunction.SaleRecipient = saleRecipient;

            return ContractHandler.SendRequestAsync(setSaleRecipientForTokenFunction);
        }

        public Task<TransactionReceipt> SetSaleRecipientForTokenRequestAndWaitForReceiptAsync(BigInteger tokenId, string saleRecipient, CancellationTokenSource cancellationToken = null)
        {
            var setSaleRecipientForTokenFunction = new SetSaleRecipientForTokenFunction();
            setSaleRecipientForTokenFunction.TokenId = tokenId;
            setSaleRecipientForTokenFunction.SaleRecipient = saleRecipient;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setSaleRecipientForTokenFunction, cancellationToken);
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

        public Task<BigInteger> TotalSupplyQueryAsync(TotalSupplyFunction totalSupplyFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(totalSupplyFunction, blockParameter);
        }

        public Task<BigInteger> TotalSupplyQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var totalSupplyFunction = new TotalSupplyFunction();
            totalSupplyFunction.ReturnValue1 = returnValue1;

            return ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(totalSupplyFunction, blockParameter);
        }

        public Task<string> UriQueryAsync(UriFunction uriFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<UriFunction, string>(uriFunction, blockParameter);
        }

        public Task<string> UriQueryAsync(BigInteger tokenId, BlockParameter blockParameter = null)
        {
            var uriFunction = new UriFunction();
            uriFunction.TokenId = tokenId;

            return ContractHandler.QueryAsync<UriFunction, string>(uriFunction, blockParameter);
        }

        public Task<bool> VerifyClaimQueryAsync(VerifyClaimFunction verifyClaimFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<VerifyClaimFunction, bool>(verifyClaimFunction, blockParameter);
        }

        public Task<bool> VerifyClaimQueryAsync(
            BigInteger conditionId,
            string claimer,
            BigInteger tokenId,
            BigInteger quantity,
            string currency,
            BigInteger pricePerToken,
            AllowlistProof allowlistProof,
            BlockParameter blockParameter = null
        )
        {
            var verifyClaimFunction = new VerifyClaimFunction();
            verifyClaimFunction.ConditionId = conditionId;
            verifyClaimFunction.Claimer = claimer;
            verifyClaimFunction.TokenId = tokenId;
            verifyClaimFunction.Quantity = quantity;
            verifyClaimFunction.Currency = currency;
            verifyClaimFunction.PricePerToken = pricePerToken;
            verifyClaimFunction.AllowlistProof = allowlistProof;

            return ContractHandler.QueryAsync<VerifyClaimFunction, bool>(verifyClaimFunction, blockParameter);
        }
    }
}
