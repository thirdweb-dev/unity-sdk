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
using Thirdweb.Contracts.Pack.ContractDefinition;

namespace Thirdweb.Contracts.Pack
{
    public partial class PackService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, PackDeployment packDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<PackDeployment>().SendRequestAndWaitForReceiptAsync(packDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, PackDeployment packDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<PackDeployment>().SendRequestAsync(packDeployment);
        }

        public static async Task<PackService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, PackDeployment packDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, packDeployment, cancellationTokenSource);
            return new PackService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.Web3 Web3 { get; }

        public ContractHandler ContractHandler { get; }

        public PackService(Nethereum.Web3.Web3 web3, string contractAddress)
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

        public Task<string> AddPackContentsRequestAsync(AddPackContentsFunction addPackContentsFunction)
        {
            return ContractHandler.SendRequestAsync(addPackContentsFunction);
        }

        public Task<TransactionReceipt> AddPackContentsRequestAndWaitForReceiptAsync(AddPackContentsFunction addPackContentsFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(addPackContentsFunction, cancellationToken);
        }

        public Task<string> AddPackContentsRequestAsync(BigInteger packId, List<Token> contents, List<BigInteger> numOfRewardUnits, string recipient)
        {
            var addPackContentsFunction = new AddPackContentsFunction();
            addPackContentsFunction.PackId = packId;
            addPackContentsFunction.Contents = contents;
            addPackContentsFunction.NumOfRewardUnits = numOfRewardUnits;
            addPackContentsFunction.Recipient = recipient;

            return ContractHandler.SendRequestAsync(addPackContentsFunction);
        }

        public Task<TransactionReceipt> AddPackContentsRequestAndWaitForReceiptAsync(
            BigInteger packId,
            List<Token> contents,
            List<BigInteger> numOfRewardUnits,
            string recipient,
            CancellationTokenSource cancellationToken = null
        )
        {
            var addPackContentsFunction = new AddPackContentsFunction();
            addPackContentsFunction.PackId = packId;
            addPackContentsFunction.Contents = contents;
            addPackContentsFunction.NumOfRewardUnits = numOfRewardUnits;
            addPackContentsFunction.Recipient = recipient;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(addPackContentsFunction, cancellationToken);
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

        public Task<bool> CanUpdatePackQueryAsync(CanUpdatePackFunction canUpdatePackFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<CanUpdatePackFunction, bool>(canUpdatePackFunction, blockParameter);
        }

        public Task<bool> CanUpdatePackQueryAsync(BigInteger returnValue1, BlockParameter blockParameter = null)
        {
            var canUpdatePackFunction = new CanUpdatePackFunction();
            canUpdatePackFunction.ReturnValue1 = returnValue1;

            return ContractHandler.QueryAsync<CanUpdatePackFunction, bool>(canUpdatePackFunction, blockParameter);
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

        public Task<string> CreatePackRequestAsync(CreatePackFunction createPackFunction)
        {
            return ContractHandler.SendRequestAsync(createPackFunction);
        }

        public Task<TransactionReceipt> CreatePackRequestAndWaitForReceiptAsync(CreatePackFunction createPackFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(createPackFunction, cancellationToken);
        }

        public Task<string> CreatePackRequestAsync(
            List<Token> contents,
            List<BigInteger> numOfRewardUnits,
            string packUri,
            BigInteger openStartTimestamp,
            BigInteger amountDistributedPerOpen,
            string recipient
        )
        {
            var createPackFunction = new CreatePackFunction();
            createPackFunction.Contents = contents;
            createPackFunction.NumOfRewardUnits = numOfRewardUnits;
            createPackFunction.PackUri = packUri;
            createPackFunction.OpenStartTimestamp = openStartTimestamp;
            createPackFunction.AmountDistributedPerOpen = amountDistributedPerOpen;
            createPackFunction.Recipient = recipient;

            return ContractHandler.SendRequestAsync(createPackFunction);
        }

        public Task<TransactionReceipt> CreatePackRequestAndWaitForReceiptAsync(
            List<Token> contents,
            List<BigInteger> numOfRewardUnits,
            string packUri,
            BigInteger openStartTimestamp,
            BigInteger amountDistributedPerOpen,
            string recipient,
            CancellationTokenSource cancellationToken = null
        )
        {
            var createPackFunction = new CreatePackFunction();
            createPackFunction.Contents = contents;
            createPackFunction.NumOfRewardUnits = numOfRewardUnits;
            createPackFunction.PackUri = packUri;
            createPackFunction.OpenStartTimestamp = openStartTimestamp;
            createPackFunction.AmountDistributedPerOpen = amountDistributedPerOpen;
            createPackFunction.Recipient = recipient;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(createPackFunction, cancellationToken);
        }

        public Task<GetDefaultRoyaltyInfoOutputDTO> GetDefaultRoyaltyInfoQueryAsync(GetDefaultRoyaltyInfoFunction getDefaultRoyaltyInfoFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetDefaultRoyaltyInfoFunction, GetDefaultRoyaltyInfoOutputDTO>(getDefaultRoyaltyInfoFunction, blockParameter);
        }

        public Task<GetDefaultRoyaltyInfoOutputDTO> GetDefaultRoyaltyInfoQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetDefaultRoyaltyInfoFunction, GetDefaultRoyaltyInfoOutputDTO>(null, blockParameter);
        }

        public Task<GetPackContentsOutputDTO> GetPackContentsQueryAsync(GetPackContentsFunction getPackContentsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetPackContentsFunction, GetPackContentsOutputDTO>(getPackContentsFunction, blockParameter);
        }

        public Task<GetPackContentsOutputDTO> GetPackContentsQueryAsync(BigInteger packId, BlockParameter blockParameter = null)
        {
            var getPackContentsFunction = new GetPackContentsFunction();
            getPackContentsFunction.PackId = packId;

            return ContractHandler.QueryDeserializingToObjectAsync<GetPackContentsFunction, GetPackContentsOutputDTO>(getPackContentsFunction, blockParameter);
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

        public Task<BigInteger> GetTokenCountOfBundleQueryAsync(GetTokenCountOfBundleFunction getTokenCountOfBundleFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetTokenCountOfBundleFunction, BigInteger>(getTokenCountOfBundleFunction, blockParameter);
        }

        public Task<BigInteger> GetTokenCountOfBundleQueryAsync(BigInteger bundleId, BlockParameter blockParameter = null)
        {
            var getTokenCountOfBundleFunction = new GetTokenCountOfBundleFunction();
            getTokenCountOfBundleFunction.BundleId = bundleId;

            return ContractHandler.QueryAsync<GetTokenCountOfBundleFunction, BigInteger>(getTokenCountOfBundleFunction, blockParameter);
        }

        public Task<GetTokenOfBundleOutputDTO> GetTokenOfBundleQueryAsync(GetTokenOfBundleFunction getTokenOfBundleFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetTokenOfBundleFunction, GetTokenOfBundleOutputDTO>(getTokenOfBundleFunction, blockParameter);
        }

        public Task<GetTokenOfBundleOutputDTO> GetTokenOfBundleQueryAsync(BigInteger bundleId, BigInteger index, BlockParameter blockParameter = null)
        {
            var getTokenOfBundleFunction = new GetTokenOfBundleFunction();
            getTokenOfBundleFunction.BundleId = bundleId;
            getTokenOfBundleFunction.Index = index;

            return ContractHandler.QueryDeserializingToObjectAsync<GetTokenOfBundleFunction, GetTokenOfBundleOutputDTO>(getTokenOfBundleFunction, blockParameter);
        }

        public Task<string> GetUriOfBundleQueryAsync(GetUriOfBundleFunction getUriOfBundleFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetUriOfBundleFunction, string>(getUriOfBundleFunction, blockParameter);
        }

        public Task<string> GetUriOfBundleQueryAsync(BigInteger bundleId, BlockParameter blockParameter = null)
        {
            var getUriOfBundleFunction = new GetUriOfBundleFunction();
            getUriOfBundleFunction.BundleId = bundleId;

            return ContractHandler.QueryAsync<GetUriOfBundleFunction, string>(getUriOfBundleFunction, blockParameter);
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

        public Task<string> InitializeRequestAsync(string defaultAdmin, string name, string symbol, string contractURI, List<string> trustedForwarders, string royaltyRecipient, BigInteger royaltyBps)
        {
            var initializeFunction = new InitializeFunction();
            initializeFunction.DefaultAdmin = defaultAdmin;
            initializeFunction.Name = name;
            initializeFunction.Symbol = symbol;
            initializeFunction.ContractURI = contractURI;
            initializeFunction.TrustedForwarders = trustedForwarders;
            initializeFunction.RoyaltyRecipient = royaltyRecipient;
            initializeFunction.RoyaltyBps = royaltyBps;

            return ContractHandler.SendRequestAsync(initializeFunction);
        }

        public Task<TransactionReceipt> InitializeRequestAndWaitForReceiptAsync(
            string defaultAdmin,
            string name,
            string symbol,
            string contractURI,
            List<string> trustedForwarders,
            string royaltyRecipient,
            BigInteger royaltyBps,
            CancellationTokenSource cancellationToken = null
        )
        {
            var initializeFunction = new InitializeFunction();
            initializeFunction.DefaultAdmin = defaultAdmin;
            initializeFunction.Name = name;
            initializeFunction.Symbol = symbol;
            initializeFunction.ContractURI = contractURI;
            initializeFunction.TrustedForwarders = trustedForwarders;
            initializeFunction.RoyaltyRecipient = royaltyRecipient;
            initializeFunction.RoyaltyBps = royaltyBps;

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

        public Task<string> OnERC1155BatchReceivedRequestAsync(OnERC1155BatchReceivedFunction onERC1155BatchReceivedFunction)
        {
            return ContractHandler.SendRequestAsync(onERC1155BatchReceivedFunction);
        }

        public Task<TransactionReceipt> OnERC1155BatchReceivedRequestAndWaitForReceiptAsync(
            OnERC1155BatchReceivedFunction onERC1155BatchReceivedFunction,
            CancellationTokenSource cancellationToken = null
        )
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(onERC1155BatchReceivedFunction, cancellationToken);
        }

        public Task<string> OnERC1155BatchReceivedRequestAsync(string returnValue1, string returnValue2, List<BigInteger> returnValue3, List<BigInteger> returnValue4, byte[] returnValue5)
        {
            var onERC1155BatchReceivedFunction = new OnERC1155BatchReceivedFunction();
            onERC1155BatchReceivedFunction.ReturnValue1 = returnValue1;
            onERC1155BatchReceivedFunction.ReturnValue2 = returnValue2;
            onERC1155BatchReceivedFunction.ReturnValue3 = returnValue3;
            onERC1155BatchReceivedFunction.ReturnValue4 = returnValue4;
            onERC1155BatchReceivedFunction.ReturnValue5 = returnValue5;

            return ContractHandler.SendRequestAsync(onERC1155BatchReceivedFunction);
        }

        public Task<TransactionReceipt> OnERC1155BatchReceivedRequestAndWaitForReceiptAsync(
            string returnValue1,
            string returnValue2,
            List<BigInteger> returnValue3,
            List<BigInteger> returnValue4,
            byte[] returnValue5,
            CancellationTokenSource cancellationToken = null
        )
        {
            var onERC1155BatchReceivedFunction = new OnERC1155BatchReceivedFunction();
            onERC1155BatchReceivedFunction.ReturnValue1 = returnValue1;
            onERC1155BatchReceivedFunction.ReturnValue2 = returnValue2;
            onERC1155BatchReceivedFunction.ReturnValue3 = returnValue3;
            onERC1155BatchReceivedFunction.ReturnValue4 = returnValue4;
            onERC1155BatchReceivedFunction.ReturnValue5 = returnValue5;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(onERC1155BatchReceivedFunction, cancellationToken);
        }

        public Task<string> OnERC1155ReceivedRequestAsync(OnERC1155ReceivedFunction onERC1155ReceivedFunction)
        {
            return ContractHandler.SendRequestAsync(onERC1155ReceivedFunction);
        }

        public Task<TransactionReceipt> OnERC1155ReceivedRequestAndWaitForReceiptAsync(OnERC1155ReceivedFunction onERC1155ReceivedFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(onERC1155ReceivedFunction, cancellationToken);
        }

        public Task<string> OnERC1155ReceivedRequestAsync(string returnValue1, string returnValue2, BigInteger returnValue3, BigInteger returnValue4, byte[] returnValue5)
        {
            var onERC1155ReceivedFunction = new OnERC1155ReceivedFunction();
            onERC1155ReceivedFunction.ReturnValue1 = returnValue1;
            onERC1155ReceivedFunction.ReturnValue2 = returnValue2;
            onERC1155ReceivedFunction.ReturnValue3 = returnValue3;
            onERC1155ReceivedFunction.ReturnValue4 = returnValue4;
            onERC1155ReceivedFunction.ReturnValue5 = returnValue5;

            return ContractHandler.SendRequestAsync(onERC1155ReceivedFunction);
        }

        public Task<TransactionReceipt> OnERC1155ReceivedRequestAndWaitForReceiptAsync(
            string returnValue1,
            string returnValue2,
            BigInteger returnValue3,
            BigInteger returnValue4,
            byte[] returnValue5,
            CancellationTokenSource cancellationToken = null
        )
        {
            var onERC1155ReceivedFunction = new OnERC1155ReceivedFunction();
            onERC1155ReceivedFunction.ReturnValue1 = returnValue1;
            onERC1155ReceivedFunction.ReturnValue2 = returnValue2;
            onERC1155ReceivedFunction.ReturnValue3 = returnValue3;
            onERC1155ReceivedFunction.ReturnValue4 = returnValue4;
            onERC1155ReceivedFunction.ReturnValue5 = returnValue5;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(onERC1155ReceivedFunction, cancellationToken);
        }

        public Task<string> OnERC721ReceivedRequestAsync(OnERC721ReceivedFunction onERC721ReceivedFunction)
        {
            return ContractHandler.SendRequestAsync(onERC721ReceivedFunction);
        }

        public Task<TransactionReceipt> OnERC721ReceivedRequestAndWaitForReceiptAsync(OnERC721ReceivedFunction onERC721ReceivedFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(onERC721ReceivedFunction, cancellationToken);
        }

        public Task<string> OnERC721ReceivedRequestAsync(string returnValue1, string returnValue2, BigInteger returnValue3, byte[] returnValue4)
        {
            var onERC721ReceivedFunction = new OnERC721ReceivedFunction();
            onERC721ReceivedFunction.ReturnValue1 = returnValue1;
            onERC721ReceivedFunction.ReturnValue2 = returnValue2;
            onERC721ReceivedFunction.ReturnValue3 = returnValue3;
            onERC721ReceivedFunction.ReturnValue4 = returnValue4;

            return ContractHandler.SendRequestAsync(onERC721ReceivedFunction);
        }

        public Task<TransactionReceipt> OnERC721ReceivedRequestAndWaitForReceiptAsync(
            string returnValue1,
            string returnValue2,
            BigInteger returnValue3,
            byte[] returnValue4,
            CancellationTokenSource cancellationToken = null
        )
        {
            var onERC721ReceivedFunction = new OnERC721ReceivedFunction();
            onERC721ReceivedFunction.ReturnValue1 = returnValue1;
            onERC721ReceivedFunction.ReturnValue2 = returnValue2;
            onERC721ReceivedFunction.ReturnValue3 = returnValue3;
            onERC721ReceivedFunction.ReturnValue4 = returnValue4;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(onERC721ReceivedFunction, cancellationToken);
        }

        public Task<string> OpenPackRequestAsync(OpenPackFunction openPackFunction)
        {
            return ContractHandler.SendRequestAsync(openPackFunction);
        }

        public Task<TransactionReceipt> OpenPackRequestAndWaitForReceiptAsync(OpenPackFunction openPackFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(openPackFunction, cancellationToken);
        }

        public Task<string> OpenPackRequestAsync(BigInteger packId, BigInteger amountToOpen)
        {
            var openPackFunction = new OpenPackFunction();
            openPackFunction.PackId = packId;
            openPackFunction.AmountToOpen = amountToOpen;

            return ContractHandler.SendRequestAsync(openPackFunction);
        }

        public Task<TransactionReceipt> OpenPackRequestAndWaitForReceiptAsync(BigInteger packId, BigInteger amountToOpen, CancellationTokenSource cancellationToken = null)
        {
            var openPackFunction = new OpenPackFunction();
            openPackFunction.PackId = packId;
            openPackFunction.AmountToOpen = amountToOpen;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(openPackFunction, cancellationToken);
        }

        public Task<string> OwnerQueryAsync(OwnerFunction ownerFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(ownerFunction, blockParameter);
        }

        public Task<string> OwnerQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(null, blockParameter);
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
    }
}
