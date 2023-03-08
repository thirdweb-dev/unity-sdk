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
using Thirdweb.Contracts.TokenERC20.ContractDefinition;

namespace Thirdweb.Contracts.TokenERC20
{
    public partial class TokenERC20Service
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(
            Nethereum.Web3.Web3 web3,
            TokenERC20Deployment tokenERC20Deployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            return web3.Eth.GetContractDeploymentHandler<TokenERC20Deployment>().SendRequestAndWaitForReceiptAsync(tokenERC20Deployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, TokenERC20Deployment tokenERC20Deployment)
        {
            return web3.Eth.GetContractDeploymentHandler<TokenERC20Deployment>().SendRequestAsync(tokenERC20Deployment);
        }

        public static async Task<TokenERC20Service> DeployContractAndGetServiceAsync(
            Nethereum.Web3.Web3 web3,
            TokenERC20Deployment tokenERC20Deployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, tokenERC20Deployment, cancellationTokenSource);
            return new TokenERC20Service(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.Web3 Web3 { get; }

        public ContractHandler ContractHandler { get; }

        public TokenERC20Service(Nethereum.Web3.Web3 web3, string contractAddress)
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

        public Task<byte[]> DOMAIN_SEPARATORQueryAsync(DOMAIN_SEPARATORFunction dOMAIN_SEPARATORFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DOMAIN_SEPARATORFunction, byte[]>(dOMAIN_SEPARATORFunction, blockParameter);
        }

        public Task<byte[]> DOMAIN_SEPARATORQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DOMAIN_SEPARATORFunction, byte[]>(null, blockParameter);
        }

        public Task<BigInteger> AllowanceQueryAsync(AllowanceFunction allowanceFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AllowanceFunction, BigInteger>(allowanceFunction, blockParameter);
        }

        public Task<BigInteger> AllowanceQueryAsync(string owner, string spender, BlockParameter blockParameter = null)
        {
            var allowanceFunction = new AllowanceFunction();
            allowanceFunction.Owner = owner;
            allowanceFunction.Spender = spender;

            return ContractHandler.QueryAsync<AllowanceFunction, BigInteger>(allowanceFunction, blockParameter);
        }

        public Task<string> ApproveRequestAsync(ApproveFunction approveFunction)
        {
            return ContractHandler.SendRequestAsync(approveFunction);
        }

        public Task<TransactionReceipt> ApproveRequestAndWaitForReceiptAsync(ApproveFunction approveFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationToken);
        }

        public Task<string> ApproveRequestAsync(string spender, BigInteger amount)
        {
            var approveFunction = new ApproveFunction();
            approveFunction.Spender = spender;
            approveFunction.Amount = amount;

            return ContractHandler.SendRequestAsync(approveFunction);
        }

        public Task<TransactionReceipt> ApproveRequestAndWaitForReceiptAsync(string spender, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var approveFunction = new ApproveFunction();
            approveFunction.Spender = spender;
            approveFunction.Amount = amount;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationToken);
        }

        public Task<BigInteger> BalanceOfQueryAsync(BalanceOfFunction balanceOfFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameter);
        }

        public Task<BigInteger> BalanceOfQueryAsync(string account, BlockParameter blockParameter = null)
        {
            var balanceOfFunction = new BalanceOfFunction();
            balanceOfFunction.Account = account;

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

        public Task<string> BurnRequestAsync(BigInteger amount)
        {
            var burnFunction = new BurnFunction();
            burnFunction.Amount = amount;

            return ContractHandler.SendRequestAsync(burnFunction);
        }

        public Task<TransactionReceipt> BurnRequestAndWaitForReceiptAsync(BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var burnFunction = new BurnFunction();
            burnFunction.Amount = amount;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(burnFunction, cancellationToken);
        }

        public Task<string> BurnFromRequestAsync(BurnFromFunction burnFromFunction)
        {
            return ContractHandler.SendRequestAsync(burnFromFunction);
        }

        public Task<TransactionReceipt> BurnFromRequestAndWaitForReceiptAsync(BurnFromFunction burnFromFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(burnFromFunction, cancellationToken);
        }

        public Task<string> BurnFromRequestAsync(string account, BigInteger amount)
        {
            var burnFromFunction = new BurnFromFunction();
            burnFromFunction.Account = account;
            burnFromFunction.Amount = amount;

            return ContractHandler.SendRequestAsync(burnFromFunction);
        }

        public Task<TransactionReceipt> BurnFromRequestAndWaitForReceiptAsync(string account, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var burnFromFunction = new BurnFromFunction();
            burnFromFunction.Account = account;
            burnFromFunction.Amount = amount;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(burnFromFunction, cancellationToken);
        }

        public Task<CheckpointsOutputDTO> CheckpointsQueryAsync(CheckpointsFunction checkpointsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<CheckpointsFunction, CheckpointsOutputDTO>(checkpointsFunction, blockParameter);
        }

        public Task<CheckpointsOutputDTO> CheckpointsQueryAsync(string account, uint pos, BlockParameter blockParameter = null)
        {
            var checkpointsFunction = new CheckpointsFunction();
            checkpointsFunction.Account = account;
            checkpointsFunction.Pos = pos;

            return ContractHandler.QueryDeserializingToObjectAsync<CheckpointsFunction, CheckpointsOutputDTO>(checkpointsFunction, blockParameter);
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

        public Task<byte> DecimalsQueryAsync(DecimalsFunction decimalsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DecimalsFunction, byte>(decimalsFunction, blockParameter);
        }

        public Task<byte> DecimalsQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DecimalsFunction, byte>(null, blockParameter);
        }

        public Task<string> DecreaseAllowanceRequestAsync(DecreaseAllowanceFunction decreaseAllowanceFunction)
        {
            return ContractHandler.SendRequestAsync(decreaseAllowanceFunction);
        }

        public Task<TransactionReceipt> DecreaseAllowanceRequestAndWaitForReceiptAsync(DecreaseAllowanceFunction decreaseAllowanceFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(decreaseAllowanceFunction, cancellationToken);
        }

        public Task<string> DecreaseAllowanceRequestAsync(string spender, BigInteger subtractedValue)
        {
            var decreaseAllowanceFunction = new DecreaseAllowanceFunction();
            decreaseAllowanceFunction.Spender = spender;
            decreaseAllowanceFunction.SubtractedValue = subtractedValue;

            return ContractHandler.SendRequestAsync(decreaseAllowanceFunction);
        }

        public Task<TransactionReceipt> DecreaseAllowanceRequestAndWaitForReceiptAsync(string spender, BigInteger subtractedValue, CancellationTokenSource cancellationToken = null)
        {
            var decreaseAllowanceFunction = new DecreaseAllowanceFunction();
            decreaseAllowanceFunction.Spender = spender;
            decreaseAllowanceFunction.SubtractedValue = subtractedValue;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(decreaseAllowanceFunction, cancellationToken);
        }

        public Task<string> DelegateRequestAsync(DelegateFunction @delegateFunction)
        {
            return ContractHandler.SendRequestAsync(@delegateFunction);
        }

        public Task<TransactionReceipt> DelegateRequestAndWaitForReceiptAsync(DelegateFunction @delegateFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(@delegateFunction, cancellationToken);
        }

        public Task<string> DelegateRequestAsync(string delegatee)
        {
            var @delegateFunction = new DelegateFunction();
            @delegateFunction.Delegatee = delegatee;

            return ContractHandler.SendRequestAsync(@delegateFunction);
        }

        public Task<TransactionReceipt> DelegateRequestAndWaitForReceiptAsync(string delegatee, CancellationTokenSource cancellationToken = null)
        {
            var @delegateFunction = new DelegateFunction();
            @delegateFunction.Delegatee = delegatee;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(@delegateFunction, cancellationToken);
        }

        public Task<string> DelegateBySigRequestAsync(DelegateBySigFunction delegateBySigFunction)
        {
            return ContractHandler.SendRequestAsync(delegateBySigFunction);
        }

        public Task<TransactionReceipt> DelegateBySigRequestAndWaitForReceiptAsync(DelegateBySigFunction delegateBySigFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(delegateBySigFunction, cancellationToken);
        }

        public Task<string> DelegateBySigRequestAsync(string delegatee, BigInteger nonce, BigInteger expiry, byte v, byte[] r, byte[] s)
        {
            var delegateBySigFunction = new DelegateBySigFunction();
            delegateBySigFunction.Delegatee = delegatee;
            delegateBySigFunction.Nonce = nonce;
            delegateBySigFunction.Expiry = expiry;
            delegateBySigFunction.V = v;
            delegateBySigFunction.R = r;
            delegateBySigFunction.S = s;

            return ContractHandler.SendRequestAsync(delegateBySigFunction);
        }

        public Task<TransactionReceipt> DelegateBySigRequestAndWaitForReceiptAsync(
            string delegatee,
            BigInteger nonce,
            BigInteger expiry,
            byte v,
            byte[] r,
            byte[] s,
            CancellationTokenSource cancellationToken = null
        )
        {
            var delegateBySigFunction = new DelegateBySigFunction();
            delegateBySigFunction.Delegatee = delegatee;
            delegateBySigFunction.Nonce = nonce;
            delegateBySigFunction.Expiry = expiry;
            delegateBySigFunction.V = v;
            delegateBySigFunction.R = r;
            delegateBySigFunction.S = s;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(delegateBySigFunction, cancellationToken);
        }

        public Task<string> DelegatesQueryAsync(DelegatesFunction delegatesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DelegatesFunction, string>(delegatesFunction, blockParameter);
        }

        public Task<string> DelegatesQueryAsync(string account, BlockParameter blockParameter = null)
        {
            var delegatesFunction = new DelegatesFunction();
            delegatesFunction.Account = account;

            return ContractHandler.QueryAsync<DelegatesFunction, string>(delegatesFunction, blockParameter);
        }

        public Task<BigInteger> GetPastTotalSupplyQueryAsync(GetPastTotalSupplyFunction getPastTotalSupplyFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetPastTotalSupplyFunction, BigInteger>(getPastTotalSupplyFunction, blockParameter);
        }

        public Task<BigInteger> GetPastTotalSupplyQueryAsync(BigInteger blockNumber, BlockParameter blockParameter = null)
        {
            var getPastTotalSupplyFunction = new GetPastTotalSupplyFunction();
            getPastTotalSupplyFunction.BlockNumber = blockNumber;

            return ContractHandler.QueryAsync<GetPastTotalSupplyFunction, BigInteger>(getPastTotalSupplyFunction, blockParameter);
        }

        public Task<BigInteger> GetPastVotesQueryAsync(GetPastVotesFunction getPastVotesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetPastVotesFunction, BigInteger>(getPastVotesFunction, blockParameter);
        }

        public Task<BigInteger> GetPastVotesQueryAsync(string account, BigInteger blockNumber, BlockParameter blockParameter = null)
        {
            var getPastVotesFunction = new GetPastVotesFunction();
            getPastVotesFunction.Account = account;
            getPastVotesFunction.BlockNumber = blockNumber;

            return ContractHandler.QueryAsync<GetPastVotesFunction, BigInteger>(getPastVotesFunction, blockParameter);
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

        public Task<BigInteger> GetVotesQueryAsync(GetVotesFunction getVotesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetVotesFunction, BigInteger>(getVotesFunction, blockParameter);
        }

        public Task<BigInteger> GetVotesQueryAsync(string account, BlockParameter blockParameter = null)
        {
            var getVotesFunction = new GetVotesFunction();
            getVotesFunction.Account = account;

            return ContractHandler.QueryAsync<GetVotesFunction, BigInteger>(getVotesFunction, blockParameter);
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

        public Task<string> IncreaseAllowanceRequestAsync(IncreaseAllowanceFunction increaseAllowanceFunction)
        {
            return ContractHandler.SendRequestAsync(increaseAllowanceFunction);
        }

        public Task<TransactionReceipt> IncreaseAllowanceRequestAndWaitForReceiptAsync(IncreaseAllowanceFunction increaseAllowanceFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(increaseAllowanceFunction, cancellationToken);
        }

        public Task<string> IncreaseAllowanceRequestAsync(string spender, BigInteger addedValue)
        {
            var increaseAllowanceFunction = new IncreaseAllowanceFunction();
            increaseAllowanceFunction.Spender = spender;
            increaseAllowanceFunction.AddedValue = addedValue;

            return ContractHandler.SendRequestAsync(increaseAllowanceFunction);
        }

        public Task<TransactionReceipt> IncreaseAllowanceRequestAndWaitForReceiptAsync(string spender, BigInteger addedValue, CancellationTokenSource cancellationToken = null)
        {
            var increaseAllowanceFunction = new IncreaseAllowanceFunction();
            increaseAllowanceFunction.Spender = spender;
            increaseAllowanceFunction.AddedValue = addedValue;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(increaseAllowanceFunction, cancellationToken);
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
            string primarySaleRecipient,
            string platformFeeRecipient,
            BigInteger platformFeeBps
        )
        {
            var initializeFunction = new InitializeFunction();
            initializeFunction.DefaultAdmin = defaultAdmin;
            initializeFunction.Name = name;
            initializeFunction.Symbol = symbol;
            initializeFunction.ContractURI = contractURI;
            initializeFunction.TrustedForwarders = trustedForwarders;
            initializeFunction.PrimarySaleRecipient = primarySaleRecipient;
            initializeFunction.PlatformFeeRecipient = platformFeeRecipient;
            initializeFunction.PlatformFeeBps = platformFeeBps;

            return ContractHandler.SendRequestAsync(initializeFunction);
        }

        public Task<TransactionReceipt> InitializeRequestAndWaitForReceiptAsync(
            string defaultAdmin,
            string name,
            string symbol,
            string contractURI,
            List<string> trustedForwarders,
            string primarySaleRecipient,
            string platformFeeRecipient,
            BigInteger platformFeeBps,
            CancellationTokenSource cancellationToken = null
        )
        {
            var initializeFunction = new InitializeFunction();
            initializeFunction.DefaultAdmin = defaultAdmin;
            initializeFunction.Name = name;
            initializeFunction.Symbol = symbol;
            initializeFunction.ContractURI = contractURI;
            initializeFunction.TrustedForwarders = trustedForwarders;
            initializeFunction.PrimarySaleRecipient = primarySaleRecipient;
            initializeFunction.PlatformFeeRecipient = platformFeeRecipient;
            initializeFunction.PlatformFeeBps = platformFeeBps;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(initializeFunction, cancellationToken);
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

        public Task<string> MintToRequestAsync(string to, BigInteger amount)
        {
            var mintToFunction = new MintToFunction();
            mintToFunction.To = to;
            mintToFunction.Amount = amount;

            return ContractHandler.SendRequestAsync(mintToFunction);
        }

        public Task<TransactionReceipt> MintToRequestAndWaitForReceiptAsync(string to, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var mintToFunction = new MintToFunction();
            mintToFunction.To = to;
            mintToFunction.Amount = amount;

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

        public Task<BigInteger> NoncesQueryAsync(NoncesFunction noncesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<NoncesFunction, BigInteger>(noncesFunction, blockParameter);
        }

        public Task<BigInteger> NoncesQueryAsync(string owner, BlockParameter blockParameter = null)
        {
            var noncesFunction = new NoncesFunction();
            noncesFunction.Owner = owner;

            return ContractHandler.QueryAsync<NoncesFunction, BigInteger>(noncesFunction, blockParameter);
        }

        public Task<uint> NumCheckpointsQueryAsync(NumCheckpointsFunction numCheckpointsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<NumCheckpointsFunction, uint>(numCheckpointsFunction, blockParameter);
        }

        public Task<uint> NumCheckpointsQueryAsync(string account, BlockParameter blockParameter = null)
        {
            var numCheckpointsFunction = new NumCheckpointsFunction();
            numCheckpointsFunction.Account = account;

            return ContractHandler.QueryAsync<NumCheckpointsFunction, uint>(numCheckpointsFunction, blockParameter);
        }

        public Task<string> PermitRequestAsync(PermitFunction permitFunction)
        {
            return ContractHandler.SendRequestAsync(permitFunction);
        }

        public Task<TransactionReceipt> PermitRequestAndWaitForReceiptAsync(PermitFunction permitFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(permitFunction, cancellationToken);
        }

        public Task<string> PermitRequestAsync(string owner, string spender, BigInteger value, BigInteger deadline, byte v, byte[] r, byte[] s)
        {
            var permitFunction = new PermitFunction();
            permitFunction.Owner = owner;
            permitFunction.Spender = spender;
            permitFunction.Value = value;
            permitFunction.Deadline = deadline;
            permitFunction.V = v;
            permitFunction.R = r;
            permitFunction.S = s;

            return ContractHandler.SendRequestAsync(permitFunction);
        }

        public Task<TransactionReceipt> PermitRequestAndWaitForReceiptAsync(
            string owner,
            string spender,
            BigInteger value,
            BigInteger deadline,
            byte v,
            byte[] r,
            byte[] s,
            CancellationTokenSource cancellationToken = null
        )
        {
            var permitFunction = new PermitFunction();
            permitFunction.Owner = owner;
            permitFunction.Spender = spender;
            permitFunction.Value = value;
            permitFunction.Deadline = deadline;
            permitFunction.V = v;
            permitFunction.R = r;
            permitFunction.S = s;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(permitFunction, cancellationToken);
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

        public Task<BigInteger> TotalSupplyQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(null, blockParameter);
        }

        public Task<string> TransferRequestAsync(TransferFunction transferFunction)
        {
            return ContractHandler.SendRequestAsync(transferFunction);
        }

        public Task<TransactionReceipt> TransferRequestAndWaitForReceiptAsync(TransferFunction transferFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFunction, cancellationToken);
        }

        public Task<string> TransferRequestAsync(string to, BigInteger amount)
        {
            var transferFunction = new TransferFunction();
            transferFunction.To = to;
            transferFunction.Amount = amount;

            return ContractHandler.SendRequestAsync(transferFunction);
        }

        public Task<TransactionReceipt> TransferRequestAndWaitForReceiptAsync(string to, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var transferFunction = new TransferFunction();
            transferFunction.To = to;
            transferFunction.Amount = amount;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFunction, cancellationToken);
        }

        public Task<string> TransferFromRequestAsync(TransferFromFunction transferFromFunction)
        {
            return ContractHandler.SendRequestAsync(transferFromFunction);
        }

        public Task<TransactionReceipt> TransferFromRequestAndWaitForReceiptAsync(TransferFromFunction transferFromFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(transferFromFunction, cancellationToken);
        }

        public Task<string> TransferFromRequestAsync(string from, string to, BigInteger amount)
        {
            var transferFromFunction = new TransferFromFunction();
            transferFromFunction.From = from;
            transferFromFunction.To = to;
            transferFromFunction.Amount = amount;

            return ContractHandler.SendRequestAsync(transferFromFunction);
        }

        public Task<TransactionReceipt> TransferFromRequestAndWaitForReceiptAsync(string from, string to, BigInteger amount, CancellationTokenSource cancellationToken = null)
        {
            var transferFromFunction = new TransferFromFunction();
            transferFromFunction.From = from;
            transferFromFunction.To = to;
            transferFromFunction.Amount = amount;

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
