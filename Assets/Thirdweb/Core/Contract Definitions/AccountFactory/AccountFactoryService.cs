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
using Thirdweb.Contracts.AccountFactory.ContractDefinition;

namespace Thirdweb.Contracts.AccountFactory
{
    public partial class AccountFactoryService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(
            Nethereum.Web3.Web3 web3,
            AccountFactoryDeployment accountFactoryDeployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            return web3.Eth.GetContractDeploymentHandler<AccountFactoryDeployment>().SendRequestAndWaitForReceiptAsync(accountFactoryDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, AccountFactoryDeployment accountFactoryDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<AccountFactoryDeployment>().SendRequestAsync(accountFactoryDeployment);
        }

        public static async Task<AccountFactoryService> DeployContractAndGetServiceAsync(
            Nethereum.Web3.Web3 web3,
            AccountFactoryDeployment accountFactoryDeployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, accountFactoryDeployment, cancellationTokenSource);
            return new AccountFactoryService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.IWeb3 Web3 { get; }

        public ContractHandler ContractHandler { get; }

        public AccountFactoryService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public AccountFactoryService(Nethereum.Web3.IWeb3 web3, string contractAddress)
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

        public Task<string> AccountImplementationQueryAsync(AccountImplementationFunction accountImplementationFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AccountImplementationFunction, string>(accountImplementationFunction, blockParameter);
        }

        public Task<string> AccountImplementationQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AccountImplementationFunction, string>(null, blockParameter);
        }

        public Task<string> ContractURIQueryAsync(ContractURIFunction contractURIFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractURIFunction, string>(contractURIFunction, blockParameter);
        }

        public Task<string> ContractURIQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContractURIFunction, string>(null, blockParameter);
        }

        public Task<string> CreateAccountRequestAsync(CreateAccountFunction createAccountFunction)
        {
            return ContractHandler.SendRequestAsync(createAccountFunction);
        }

        public Task<TransactionReceipt> CreateAccountRequestAndWaitForReceiptAsync(CreateAccountFunction createAccountFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(createAccountFunction, cancellationToken);
        }

        public Task<string> CreateAccountRequestAsync(string admin, byte[] data)
        {
            var createAccountFunction = new CreateAccountFunction();
            createAccountFunction.Admin = admin;
            createAccountFunction.Data = data;

            return ContractHandler.SendRequestAsync(createAccountFunction);
        }

        public Task<TransactionReceipt> CreateAccountRequestAndWaitForReceiptAsync(string admin, byte[] data, CancellationTokenSource cancellationToken = null)
        {
            var createAccountFunction = new CreateAccountFunction();
            createAccountFunction.Admin = admin;
            createAccountFunction.Data = data;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(createAccountFunction, cancellationToken);
        }

        public Task<List<string>> GetAccountsOfSignerQueryAsync(GetAccountsOfSignerFunction getAccountsOfSignerFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetAccountsOfSignerFunction, List<string>>(getAccountsOfSignerFunction, blockParameter);
        }

        public Task<List<string>> GetAccountsOfSignerQueryAsync(string signer, BlockParameter blockParameter = null)
        {
            var getAccountsOfSignerFunction = new GetAccountsOfSignerFunction();
            getAccountsOfSignerFunction.Signer = signer;

            return ContractHandler.QueryAsync<GetAccountsOfSignerFunction, List<string>>(getAccountsOfSignerFunction, blockParameter);
        }

        public Task<string> GetAddressQueryAsync(GetAddressFunction getAddressFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetAddressFunction, string>(getAddressFunction, blockParameter);
        }

        public Task<string> GetAddressQueryAsync(string adminSigner, byte[] data, BlockParameter blockParameter = null)
        {
            var getAddressFunction = new GetAddressFunction();
            getAddressFunction.AdminSigner = adminSigner;
            getAddressFunction.Data = data;

            return ContractHandler.QueryAsync<GetAddressFunction, string>(getAddressFunction, blockParameter);
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

        public Task<List<string>> GetSignersOfAccountQueryAsync(GetSignersOfAccountFunction getSignersOfAccountFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetSignersOfAccountFunction, List<string>>(getSignersOfAccountFunction, blockParameter);
        }

        public Task<List<string>> GetSignersOfAccountQueryAsync(string account, BlockParameter blockParameter = null)
        {
            var getSignersOfAccountFunction = new GetSignersOfAccountFunction();
            getSignersOfAccountFunction.Account = account;

            return ContractHandler.QueryAsync<GetSignersOfAccountFunction, List<string>>(getSignersOfAccountFunction, blockParameter);
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

        public Task<string> OnSignerAddedRequestAsync(OnSignerAddedFunction onSignerAddedFunction)
        {
            return ContractHandler.SendRequestAsync(onSignerAddedFunction);
        }

        public Task<TransactionReceipt> OnSignerAddedRequestAndWaitForReceiptAsync(OnSignerAddedFunction onSignerAddedFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(onSignerAddedFunction, cancellationToken);
        }

        public Task<string> OnSignerAddedRequestAsync(string signer)
        {
            var onSignerAddedFunction = new OnSignerAddedFunction();
            onSignerAddedFunction.Signer = signer;

            return ContractHandler.SendRequestAsync(onSignerAddedFunction);
        }

        public Task<TransactionReceipt> OnSignerAddedRequestAndWaitForReceiptAsync(string signer, CancellationTokenSource cancellationToken = null)
        {
            var onSignerAddedFunction = new OnSignerAddedFunction();
            onSignerAddedFunction.Signer = signer;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(onSignerAddedFunction, cancellationToken);
        }

        public Task<string> OnSignerRemovedRequestAsync(OnSignerRemovedFunction onSignerRemovedFunction)
        {
            return ContractHandler.SendRequestAsync(onSignerRemovedFunction);
        }

        public Task<TransactionReceipt> OnSignerRemovedRequestAndWaitForReceiptAsync(OnSignerRemovedFunction onSignerRemovedFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(onSignerRemovedFunction, cancellationToken);
        }

        public Task<string> OnSignerRemovedRequestAsync(string signer)
        {
            var onSignerRemovedFunction = new OnSignerRemovedFunction();
            onSignerRemovedFunction.Signer = signer;

            return ContractHandler.SendRequestAsync(onSignerRemovedFunction);
        }

        public Task<TransactionReceipt> OnSignerRemovedRequestAndWaitForReceiptAsync(string signer, CancellationTokenSource cancellationToken = null)
        {
            var onSignerRemovedFunction = new OnSignerRemovedFunction();
            onSignerRemovedFunction.Signer = signer;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(onSignerRemovedFunction, cancellationToken);
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
    }
}
