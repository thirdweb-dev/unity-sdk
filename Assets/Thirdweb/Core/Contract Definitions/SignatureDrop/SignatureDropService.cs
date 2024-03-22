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
using Thirdweb.Contracts.SignatureDrop.ContractDefinition;

namespace Thirdweb.Contracts.SignatureDrop
{
    public partial class SignatureDropService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(
            Nethereum.Web3.Web3 web3,
            SignatureDropDeployment signatureDropDeployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            return web3.Eth.GetContractDeploymentHandler<SignatureDropDeployment>().SendRequestAndWaitForReceiptAsync(signatureDropDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, SignatureDropDeployment signatureDropDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<SignatureDropDeployment>().SendRequestAsync(signatureDropDeployment);
        }

        public static async Task<SignatureDropService> DeployContractAndGetServiceAsync(
            Nethereum.Web3.Web3 web3,
            SignatureDropDeployment signatureDropDeployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, signatureDropDeployment, cancellationTokenSource);
            return new SignatureDropService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.IWeb3 Web3 { get; }

        public ContractHandler ContractHandler { get; }

        public SignatureDropService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public SignatureDropService(Nethereum.Web3.IWeb3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<byte[]> DefaultAdminRoleQueryAsync(DefaultAdminRoleFunction defaultAdminRoleFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DefaultAdminRoleFunction, byte[]>(defaultAdminRoleFunction, blockParameter);
        }

        public Task<byte[]> DefaultAdminRoleQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DefaultAdminRoleFunction, byte[]>(null, blockParameter);
        }

        public Task<string> ApproveRequestAsync(ApproveFunction approveFunction)
        {
            return ContractHandler.SendRequestAsync(approveFunction);
        }

        public Task<TransactionReceipt> ApproveRequestAndWaitForReceiptAsync(ApproveFunction approveFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationToken);
        }

        public Task<string> ApproveRequestAsync(string to, BigInteger tokenId)
        {
            var approveFunction = new ApproveFunction();
            approveFunction.To = to;
            approveFunction.TokenId = tokenId;

            return ContractHandler.SendRequestAsync(approveFunction);
        }

        public Task<TransactionReceipt> ApproveRequestAndWaitForReceiptAsync(string to, BigInteger tokenId, CancellationTokenSource cancellationToken = null)
        {
            var approveFunction = new ApproveFunction();
            approveFunction.To = to;
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

        public Task<string> ClaimRequestAsync(ClaimFunction claimFunction)
        {
            return ContractHandler.SendRequestAsync(claimFunction);
        }

        public Task<TransactionReceipt> ClaimRequestAndWaitForReceiptAsync(ClaimFunction claimFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(claimFunction, cancellationToken);
        }

        public Task<string> ClaimRequestAsync(string receiver, BigInteger quantity, string currency, BigInteger pricePerToken, AllowlistProof allowlistProof, byte[] data)
        {
            var claimFunction = new ClaimFunction();
            claimFunction.Receiver = receiver;
            claimFunction.Quantity = quantity;
            claimFunction.Currency = currency;
            claimFunction.PricePerToken = pricePerToken;
            claimFunction.AllowlistProof = allowlistProof;
            claimFunction.Data = data;

            return ContractHandler.SendRequestAsync(claimFunction);
        }

        public Task<TransactionReceipt> ClaimRequestAndWaitForReceiptAsync(
            string receiver,
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

        public Task<ClaimConditionOutputDTO> ClaimConditionQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<ClaimConditionFunction, ClaimConditionOutputDTO>(null, blockParameter);
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

        public Task<byte[]> EncryptDecryptQueryAsync(EncryptDecryptFunction encryptDecryptFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<EncryptDecryptFunction, byte[]>(encryptDecryptFunction, blockParameter);
        }

        public Task<byte[]> EncryptDecryptQueryAsync(byte[] data, byte[] key, BlockParameter blockParameter = null)
        {
            var encryptDecryptFunction = new EncryptDecryptFunction();
            encryptDecryptFunction.Data = data;
            encryptDecryptFunction.Key = key;

            return ContractHandler.QueryAsync<EncryptDecryptFunction, byte[]>(encryptDecryptFunction, blockParameter);
        }

        public Task<byte[]> EncryptedDataQueryAsync(EncryptedDataFunction encryptedDataFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<EncryptedDataFunction, byte[]>(encryptedDataFunction, blockParameter);
        }

        public Task<byte[]> EncryptedDataQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var encryptedDataFunction = new EncryptedDataFunction();
            encryptedDataFunction.ReturnValue1 = returnValue1;

            return ContractHandler.QueryAsync<EncryptedDataFunction, byte[]>(encryptedDataFunction, blockParameter);
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

        public Task<GetDefaultRoyaltyInfoOutputDTO> GetDefaultRoyaltyInfoQueryAsync(GetDefaultRoyaltyInfoFunction getDefaultRoyaltyInfoFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetDefaultRoyaltyInfoFunction, GetDefaultRoyaltyInfoOutputDTO>(getDefaultRoyaltyInfoFunction, blockParameter);
        }

        public Task<GetDefaultRoyaltyInfoOutputDTO> GetDefaultRoyaltyInfoQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetDefaultRoyaltyInfoFunction, GetDefaultRoyaltyInfoOutputDTO>(null, blockParameter);
        }

        public Task<GetFlatPlatformFeeInfoOutputDTO> GetFlatPlatformFeeInfoQueryAsync(GetFlatPlatformFeeInfoFunction getFlatPlatformFeeInfoFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetFlatPlatformFeeInfoFunction, GetFlatPlatformFeeInfoOutputDTO>(getFlatPlatformFeeInfoFunction, blockParameter);
        }

        public Task<GetFlatPlatformFeeInfoOutputDTO> GetFlatPlatformFeeInfoQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetFlatPlatformFeeInfoFunction, GetFlatPlatformFeeInfoOutputDTO>(null, blockParameter);
        }

        public Task<GetPlatformFeeInfoOutputDTO> GetPlatformFeeInfoQueryAsync(GetPlatformFeeInfoFunction getPlatformFeeInfoFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetPlatformFeeInfoFunction, GetPlatformFeeInfoOutputDTO>(getPlatformFeeInfoFunction, blockParameter);
        }

        public Task<GetPlatformFeeInfoOutputDTO> GetPlatformFeeInfoQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetPlatformFeeInfoFunction, GetPlatformFeeInfoOutputDTO>(null, blockParameter);
        }

        public Task<byte> GetPlatformFeeTypeQueryAsync(GetPlatformFeeTypeFunction getPlatformFeeTypeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetPlatformFeeTypeFunction, byte>(getPlatformFeeTypeFunction, blockParameter);
        }

        public Task<byte> GetPlatformFeeTypeQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetPlatformFeeTypeFunction, byte>(null, blockParameter);
        }

        public Task<string> GetRevealURIQueryAsync(GetRevealURIFunction getRevealURIFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRevealURIFunction, string>(getRevealURIFunction, blockParameter);
        }

        public Task<string> GetRevealURIQueryAsync(BigInteger batchId, byte[] key, BlockParameter blockParameter = null)
        {
            var getRevealURIFunction = new GetRevealURIFunction();
            getRevealURIFunction.BatchId = batchId;
            getRevealURIFunction.Key = key;

            return ContractHandler.QueryAsync<GetRevealURIFunction, string>(getRevealURIFunction, blockParameter);
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

        public Task<BigInteger> GetSupplyClaimedByWalletQueryAsync(string claimer, BlockParameter blockParameter = null)
        {
            var getSupplyClaimedByWalletFunction = new GetSupplyClaimedByWalletFunction();
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

        public Task<bool> IsApprovedForAllQueryAsync(string owner, string @operator, BlockParameter blockParameter = null)
        {
            var isApprovedForAllFunction = new IsApprovedForAllFunction();
            isApprovedForAllFunction.Owner = owner;
            isApprovedForAllFunction.Operator = @operator;

            return ContractHandler.QueryAsync<IsApprovedForAllFunction, bool>(isApprovedForAllFunction, blockParameter);
        }

        public Task<bool> IsEncryptedBatchQueryAsync(IsEncryptedBatchFunction isEncryptedBatchFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<IsEncryptedBatchFunction, bool>(isEncryptedBatchFunction, blockParameter);
        }

        public Task<bool> IsEncryptedBatchQueryAsync(BigInteger batchId, BlockParameter blockParameter = null)
        {
            var isEncryptedBatchFunction = new IsEncryptedBatchFunction();
            isEncryptedBatchFunction.BatchId = batchId;

            return ContractHandler.QueryAsync<IsEncryptedBatchFunction, bool>(isEncryptedBatchFunction, blockParameter);
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

        public Task<string> RevealRequestAsync(RevealFunction revealFunction)
        {
            return ContractHandler.SendRequestAsync(revealFunction);
        }

        public Task<TransactionReceipt> RevealRequestAndWaitForReceiptAsync(RevealFunction revealFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(revealFunction, cancellationToken);
        }

        public Task<string> RevealRequestAsync(BigInteger index, byte[] key)
        {
            var revealFunction = new RevealFunction();
            revealFunction.Index = index;
            revealFunction.Key = key;

            return ContractHandler.SendRequestAsync(revealFunction);
        }

        public Task<TransactionReceipt> RevealRequestAndWaitForReceiptAsync(BigInteger index, byte[] key, CancellationTokenSource cancellationToken = null)
        {
            var revealFunction = new RevealFunction();
            revealFunction.Index = index;
            revealFunction.Key = key;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(revealFunction, cancellationToken);
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

        public Task<string> SetClaimConditionsRequestAsync(SetClaimConditionsFunction setClaimConditionsFunction)
        {
            return ContractHandler.SendRequestAsync(setClaimConditionsFunction);
        }

        public Task<TransactionReceipt> SetClaimConditionsRequestAndWaitForReceiptAsync(SetClaimConditionsFunction setClaimConditionsFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setClaimConditionsFunction, cancellationToken);
        }

        public Task<string> SetClaimConditionsRequestAsync(ClaimCondition condition, bool resetClaimEligibility)
        {
            var setClaimConditionsFunction = new SetClaimConditionsFunction();
            setClaimConditionsFunction.Condition = condition;
            setClaimConditionsFunction.ResetClaimEligibility = resetClaimEligibility;

            return ContractHandler.SendRequestAsync(setClaimConditionsFunction);
        }

        public Task<TransactionReceipt> SetClaimConditionsRequestAndWaitForReceiptAsync(ClaimCondition condition, bool resetClaimEligibility, CancellationTokenSource cancellationToken = null)
        {
            var setClaimConditionsFunction = new SetClaimConditionsFunction();
            setClaimConditionsFunction.Condition = condition;
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

        public Task<string> SetFlatPlatformFeeInfoRequestAsync(SetFlatPlatformFeeInfoFunction setFlatPlatformFeeInfoFunction)
        {
            return ContractHandler.SendRequestAsync(setFlatPlatformFeeInfoFunction);
        }

        public Task<TransactionReceipt> SetFlatPlatformFeeInfoRequestAndWaitForReceiptAsync(
            SetFlatPlatformFeeInfoFunction setFlatPlatformFeeInfoFunction,
            CancellationTokenSource cancellationToken = null
        )
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setFlatPlatformFeeInfoFunction, cancellationToken);
        }

        public Task<string> SetFlatPlatformFeeInfoRequestAsync(string platformFeeRecipient, BigInteger flatFee)
        {
            var setFlatPlatformFeeInfoFunction = new SetFlatPlatformFeeInfoFunction();
            setFlatPlatformFeeInfoFunction.PlatformFeeRecipient = platformFeeRecipient;
            setFlatPlatformFeeInfoFunction.FlatFee = flatFee;

            return ContractHandler.SendRequestAsync(setFlatPlatformFeeInfoFunction);
        }

        public Task<TransactionReceipt> SetFlatPlatformFeeInfoRequestAndWaitForReceiptAsync(string platformFeeRecipient, BigInteger flatFee, CancellationTokenSource cancellationToken = null)
        {
            var setFlatPlatformFeeInfoFunction = new SetFlatPlatformFeeInfoFunction();
            setFlatPlatformFeeInfoFunction.PlatformFeeRecipient = platformFeeRecipient;
            setFlatPlatformFeeInfoFunction.FlatFee = flatFee;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setFlatPlatformFeeInfoFunction, cancellationToken);
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

        public Task<string> SetPlatformFeeTypeRequestAsync(SetPlatformFeeTypeFunction setPlatformFeeTypeFunction)
        {
            return ContractHandler.SendRequestAsync(setPlatformFeeTypeFunction);
        }

        public Task<TransactionReceipt> SetPlatformFeeTypeRequestAndWaitForReceiptAsync(SetPlatformFeeTypeFunction setPlatformFeeTypeFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setPlatformFeeTypeFunction, cancellationToken);
        }

        public Task<string> SetPlatformFeeTypeRequestAsync(byte feeType)
        {
            var setPlatformFeeTypeFunction = new SetPlatformFeeTypeFunction();
            setPlatformFeeTypeFunction.FeeType = feeType;

            return ContractHandler.SendRequestAsync(setPlatformFeeTypeFunction);
        }

        public Task<TransactionReceipt> SetPlatformFeeTypeRequestAndWaitForReceiptAsync(byte feeType, CancellationTokenSource cancellationToken = null)
        {
            var setPlatformFeeTypeFunction = new SetPlatformFeeTypeFunction();
            setPlatformFeeTypeFunction.FeeType = feeType;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setPlatformFeeTypeFunction, cancellationToken);
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

        public Task<BigInteger> TotalMintedQueryAsync(TotalMintedFunction totalMintedFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TotalMintedFunction, BigInteger>(totalMintedFunction, blockParameter);
        }

        public Task<BigInteger> TotalMintedQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TotalMintedFunction, BigInteger>(null, blockParameter);
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

        public Task<bool> VerifyClaimQueryAsync(VerifyClaimFunction verifyClaimFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<VerifyClaimFunction, bool>(verifyClaimFunction, blockParameter);
        }

        public Task<bool> VerifyClaimQueryAsync(string claimer, BigInteger quantity, string currency, BigInteger pricePerToken, AllowlistProof allowlistProof, BlockParameter blockParameter = null)
        {
            var verifyClaimFunction = new VerifyClaimFunction();
            verifyClaimFunction.Claimer = claimer;
            verifyClaimFunction.Quantity = quantity;
            verifyClaimFunction.Currency = currency;
            verifyClaimFunction.PricePerToken = pricePerToken;
            verifyClaimFunction.AllowlistProof = allowlistProof;

            return ContractHandler.QueryAsync<VerifyClaimFunction, bool>(verifyClaimFunction, blockParameter);
        }
    }
}
