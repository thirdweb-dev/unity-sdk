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
using Thirdweb.Contracts.EntryPoint.ContractDefinition;

namespace Thirdweb.Contracts.EntryPoint
{
    public partial class EntryPointService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(
            Nethereum.Web3.Web3 web3,
            EntryPointDeployment entryPointDeployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            return web3.Eth.GetContractDeploymentHandler<EntryPointDeployment>().SendRequestAndWaitForReceiptAsync(entryPointDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, EntryPointDeployment entryPointDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<EntryPointDeployment>().SendRequestAsync(entryPointDeployment);
        }

        public static async Task<EntryPointService> DeployContractAndGetServiceAsync(
            Nethereum.Web3.Web3 web3,
            EntryPointDeployment entryPointDeployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, entryPointDeployment, cancellationTokenSource);
            return new EntryPointService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.IWeb3 Web3 { get; }

        public ContractHandler ContractHandler { get; }

        public EntryPointService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public EntryPointService(Nethereum.Web3.IWeb3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<BigInteger> SigValidationFailedQueryAsync(SigValidationFailedFunction sigValidationFailedFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SigValidationFailedFunction, BigInteger>(sigValidationFailedFunction, blockParameter);
        }

        public Task<BigInteger> SigValidationFailedQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SigValidationFailedFunction, BigInteger>(null, blockParameter);
        }

        public Task<string> AddStakeRequestAsync(AddStakeFunction addStakeFunction)
        {
            return ContractHandler.SendRequestAsync(addStakeFunction);
        }

        public Task<TransactionReceipt> AddStakeRequestAndWaitForReceiptAsync(AddStakeFunction addStakeFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(addStakeFunction, cancellationToken);
        }

        public Task<string> AddStakeRequestAsync(uint unstakeDelaySec)
        {
            var addStakeFunction = new AddStakeFunction();
            addStakeFunction.UnstakeDelaySec = unstakeDelaySec;

            return ContractHandler.SendRequestAsync(addStakeFunction);
        }

        public Task<TransactionReceipt> AddStakeRequestAndWaitForReceiptAsync(uint unstakeDelaySec, CancellationTokenSource cancellationToken = null)
        {
            var addStakeFunction = new AddStakeFunction();
            addStakeFunction.UnstakeDelaySec = unstakeDelaySec;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(addStakeFunction, cancellationToken);
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

        public Task<string> DepositToRequestAsync(DepositToFunction depositToFunction)
        {
            return ContractHandler.SendRequestAsync(depositToFunction);
        }

        public Task<TransactionReceipt> DepositToRequestAndWaitForReceiptAsync(DepositToFunction depositToFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(depositToFunction, cancellationToken);
        }

        public Task<string> DepositToRequestAsync(string account)
        {
            var depositToFunction = new DepositToFunction();
            depositToFunction.Account = account;

            return ContractHandler.SendRequestAsync(depositToFunction);
        }

        public Task<TransactionReceipt> DepositToRequestAndWaitForReceiptAsync(string account, CancellationTokenSource cancellationToken = null)
        {
            var depositToFunction = new DepositToFunction();
            depositToFunction.Account = account;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(depositToFunction, cancellationToken);
        }

        public Task<DepositsOutputDTO> DepositsQueryAsync(DepositsFunction depositsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<DepositsFunction, DepositsOutputDTO>(depositsFunction, blockParameter);
        }

        public Task<DepositsOutputDTO> DepositsQueryAsync(string returnValue1, BlockParameter blockParameter = null)
        {
            var depositsFunction = new DepositsFunction();
            depositsFunction.ReturnValue1 = returnValue1;

            return ContractHandler.QueryDeserializingToObjectAsync<DepositsFunction, DepositsOutputDTO>(depositsFunction, blockParameter);
        }

        public Task<GetDepositInfoOutputDTO> GetDepositInfoQueryAsync(GetDepositInfoFunction getDepositInfoFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetDepositInfoFunction, GetDepositInfoOutputDTO>(getDepositInfoFunction, blockParameter);
        }

        public Task<GetDepositInfoOutputDTO> GetDepositInfoQueryAsync(string account, BlockParameter blockParameter = null)
        {
            var getDepositInfoFunction = new GetDepositInfoFunction();
            getDepositInfoFunction.Account = account;

            return ContractHandler.QueryDeserializingToObjectAsync<GetDepositInfoFunction, GetDepositInfoOutputDTO>(getDepositInfoFunction, blockParameter);
        }

        public Task<BigInteger> GetNonceQueryAsync(GetNonceFunction getNonceFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetNonceFunction, BigInteger>(getNonceFunction, blockParameter);
        }

        public Task<BigInteger> GetNonceQueryAsync(string sender, BigInteger key, BlockParameter blockParameter = null)
        {
            var getNonceFunction = new GetNonceFunction();
            getNonceFunction.Sender = sender;
            getNonceFunction.Key = key;

            return ContractHandler.QueryAsync<GetNonceFunction, BigInteger>(getNonceFunction, blockParameter);
        }

        public Task<string> GetSenderAddressRequestAsync(GetSenderAddressFunction getSenderAddressFunction)
        {
            return ContractHandler.SendRequestAsync(getSenderAddressFunction);
        }

        public Task<TransactionReceipt> GetSenderAddressRequestAndWaitForReceiptAsync(GetSenderAddressFunction getSenderAddressFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(getSenderAddressFunction, cancellationToken);
        }

        public Task<string> GetSenderAddressRequestAsync(byte[] initCode)
        {
            var getSenderAddressFunction = new GetSenderAddressFunction();
            getSenderAddressFunction.InitCode = initCode;

            return ContractHandler.SendRequestAsync(getSenderAddressFunction);
        }

        public Task<TransactionReceipt> GetSenderAddressRequestAndWaitForReceiptAsync(byte[] initCode, CancellationTokenSource cancellationToken = null)
        {
            var getSenderAddressFunction = new GetSenderAddressFunction();
            getSenderAddressFunction.InitCode = initCode;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(getSenderAddressFunction, cancellationToken);
        }

        public Task<byte[]> GetUserOpHashQueryAsync(GetUserOpHashFunction getUserOpHashFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetUserOpHashFunction, byte[]>(getUserOpHashFunction, blockParameter);
        }

        public Task<byte[]> GetUserOpHashQueryAsync(UserOperation userOp, BlockParameter blockParameter = null)
        {
            var getUserOpHashFunction = new GetUserOpHashFunction();
            getUserOpHashFunction.UserOp = userOp;

            return ContractHandler.QueryAsync<GetUserOpHashFunction, byte[]>(getUserOpHashFunction, blockParameter);
        }

        public Task<string> HandleAggregatedOpsRequestAsync(HandleAggregatedOpsFunction handleAggregatedOpsFunction)
        {
            return ContractHandler.SendRequestAsync(handleAggregatedOpsFunction);
        }

        public Task<TransactionReceipt> HandleAggregatedOpsRequestAndWaitForReceiptAsync(HandleAggregatedOpsFunction handleAggregatedOpsFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(handleAggregatedOpsFunction, cancellationToken);
        }

        public Task<string> HandleAggregatedOpsRequestAsync(List<UserOpsPerAggregator> opsPerAggregator, string beneficiary)
        {
            var handleAggregatedOpsFunction = new HandleAggregatedOpsFunction();
            handleAggregatedOpsFunction.OpsPerAggregator = opsPerAggregator;
            handleAggregatedOpsFunction.Beneficiary = beneficiary;

            return ContractHandler.SendRequestAsync(handleAggregatedOpsFunction);
        }

        public Task<TransactionReceipt> HandleAggregatedOpsRequestAndWaitForReceiptAsync(
            List<UserOpsPerAggregator> opsPerAggregator,
            string beneficiary,
            CancellationTokenSource cancellationToken = null
        )
        {
            var handleAggregatedOpsFunction = new HandleAggregatedOpsFunction();
            handleAggregatedOpsFunction.OpsPerAggregator = opsPerAggregator;
            handleAggregatedOpsFunction.Beneficiary = beneficiary;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(handleAggregatedOpsFunction, cancellationToken);
        }

        public Task<string> HandleOpsRequestAsync(HandleOpsFunction handleOpsFunction)
        {
            return ContractHandler.SendRequestAsync(handleOpsFunction);
        }

        public Task<TransactionReceipt> HandleOpsRequestAndWaitForReceiptAsync(HandleOpsFunction handleOpsFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(handleOpsFunction, cancellationToken);
        }

        public Task<string> HandleOpsRequestAsync(List<UserOperation> ops, string beneficiary)
        {
            var handleOpsFunction = new HandleOpsFunction();
            handleOpsFunction.Ops = ops;
            handleOpsFunction.Beneficiary = beneficiary;

            return ContractHandler.SendRequestAsync(handleOpsFunction);
        }

        public Task<TransactionReceipt> HandleOpsRequestAndWaitForReceiptAsync(List<UserOperation> ops, string beneficiary, CancellationTokenSource cancellationToken = null)
        {
            var handleOpsFunction = new HandleOpsFunction();
            handleOpsFunction.Ops = ops;
            handleOpsFunction.Beneficiary = beneficiary;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(handleOpsFunction, cancellationToken);
        }

        public Task<string> IncrementNonceRequestAsync(IncrementNonceFunction incrementNonceFunction)
        {
            return ContractHandler.SendRequestAsync(incrementNonceFunction);
        }

        public Task<TransactionReceipt> IncrementNonceRequestAndWaitForReceiptAsync(IncrementNonceFunction incrementNonceFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(incrementNonceFunction, cancellationToken);
        }

        public Task<string> IncrementNonceRequestAsync(BigInteger key)
        {
            var incrementNonceFunction = new IncrementNonceFunction();
            incrementNonceFunction.Key = key;

            return ContractHandler.SendRequestAsync(incrementNonceFunction);
        }

        public Task<TransactionReceipt> IncrementNonceRequestAndWaitForReceiptAsync(BigInteger key, CancellationTokenSource cancellationToken = null)
        {
            var incrementNonceFunction = new IncrementNonceFunction();
            incrementNonceFunction.Key = key;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(incrementNonceFunction, cancellationToken);
        }

        public Task<string> InnerHandleOpRequestAsync(InnerHandleOpFunction innerHandleOpFunction)
        {
            return ContractHandler.SendRequestAsync(innerHandleOpFunction);
        }

        public Task<TransactionReceipt> InnerHandleOpRequestAndWaitForReceiptAsync(InnerHandleOpFunction innerHandleOpFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(innerHandleOpFunction, cancellationToken);
        }

        public Task<string> InnerHandleOpRequestAsync(byte[] callData, UserOpInfo opInfo, byte[] context)
        {
            var innerHandleOpFunction = new InnerHandleOpFunction();
            innerHandleOpFunction.CallData = callData;
            innerHandleOpFunction.OpInfo = opInfo;
            innerHandleOpFunction.Context = context;

            return ContractHandler.SendRequestAsync(innerHandleOpFunction);
        }

        public Task<TransactionReceipt> InnerHandleOpRequestAndWaitForReceiptAsync(byte[] callData, UserOpInfo opInfo, byte[] context, CancellationTokenSource cancellationToken = null)
        {
            var innerHandleOpFunction = new InnerHandleOpFunction();
            innerHandleOpFunction.CallData = callData;
            innerHandleOpFunction.OpInfo = opInfo;
            innerHandleOpFunction.Context = context;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(innerHandleOpFunction, cancellationToken);
        }

        public Task<BigInteger> NonceSequenceNumberQueryAsync(NonceSequenceNumberFunction nonceSequenceNumberFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<NonceSequenceNumberFunction, BigInteger>(nonceSequenceNumberFunction, blockParameter);
        }

        public Task<BigInteger> NonceSequenceNumberQueryAsync(string returnValue1, BigInteger returnValue2, BlockParameter blockParameter = null)
        {
            var nonceSequenceNumberFunction = new NonceSequenceNumberFunction();
            nonceSequenceNumberFunction.ReturnValue1 = returnValue1;
            nonceSequenceNumberFunction.ReturnValue2 = returnValue2;

            return ContractHandler.QueryAsync<NonceSequenceNumberFunction, BigInteger>(nonceSequenceNumberFunction, blockParameter);
        }

        public Task<string> SimulateHandleOpRequestAsync(SimulateHandleOpFunction simulateHandleOpFunction)
        {
            return ContractHandler.SendRequestAsync(simulateHandleOpFunction);
        }

        public Task<TransactionReceipt> SimulateHandleOpRequestAndWaitForReceiptAsync(SimulateHandleOpFunction simulateHandleOpFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(simulateHandleOpFunction, cancellationToken);
        }

        public Task<string> SimulateHandleOpRequestAsync(UserOperation op, string target, byte[] targetCallData)
        {
            var simulateHandleOpFunction = new SimulateHandleOpFunction();
            simulateHandleOpFunction.Op = op;
            simulateHandleOpFunction.Target = target;
            simulateHandleOpFunction.TargetCallData = targetCallData;

            return ContractHandler.SendRequestAsync(simulateHandleOpFunction);
        }

        public Task<TransactionReceipt> SimulateHandleOpRequestAndWaitForReceiptAsync(UserOperation op, string target, byte[] targetCallData, CancellationTokenSource cancellationToken = null)
        {
            var simulateHandleOpFunction = new SimulateHandleOpFunction();
            simulateHandleOpFunction.Op = op;
            simulateHandleOpFunction.Target = target;
            simulateHandleOpFunction.TargetCallData = targetCallData;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(simulateHandleOpFunction, cancellationToken);
        }

        public Task<string> SimulateValidationRequestAsync(SimulateValidationFunction simulateValidationFunction)
        {
            return ContractHandler.SendRequestAsync(simulateValidationFunction);
        }

        public Task<TransactionReceipt> SimulateValidationRequestAndWaitForReceiptAsync(SimulateValidationFunction simulateValidationFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(simulateValidationFunction, cancellationToken);
        }

        public Task<string> SimulateValidationRequestAsync(UserOperation userOp)
        {
            var simulateValidationFunction = new SimulateValidationFunction();
            simulateValidationFunction.UserOp = userOp;

            return ContractHandler.SendRequestAsync(simulateValidationFunction);
        }

        public Task<TransactionReceipt> SimulateValidationRequestAndWaitForReceiptAsync(UserOperation userOp, CancellationTokenSource cancellationToken = null)
        {
            var simulateValidationFunction = new SimulateValidationFunction();
            simulateValidationFunction.UserOp = userOp;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(simulateValidationFunction, cancellationToken);
        }

        public Task<string> UnlockStakeRequestAsync(UnlockStakeFunction unlockStakeFunction)
        {
            return ContractHandler.SendRequestAsync(unlockStakeFunction);
        }

        public Task<string> UnlockStakeRequestAsync()
        {
            return ContractHandler.SendRequestAsync<UnlockStakeFunction>();
        }

        public Task<TransactionReceipt> UnlockStakeRequestAndWaitForReceiptAsync(UnlockStakeFunction unlockStakeFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(unlockStakeFunction, cancellationToken);
        }

        public Task<TransactionReceipt> UnlockStakeRequestAndWaitForReceiptAsync(CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync<UnlockStakeFunction>(null, cancellationToken);
        }

        public Task<string> WithdrawStakeRequestAsync(WithdrawStakeFunction withdrawStakeFunction)
        {
            return ContractHandler.SendRequestAsync(withdrawStakeFunction);
        }

        public Task<TransactionReceipt> WithdrawStakeRequestAndWaitForReceiptAsync(WithdrawStakeFunction withdrawStakeFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(withdrawStakeFunction, cancellationToken);
        }

        public Task<string> WithdrawStakeRequestAsync(string withdrawAddress)
        {
            var withdrawStakeFunction = new WithdrawStakeFunction();
            withdrawStakeFunction.WithdrawAddress = withdrawAddress;

            return ContractHandler.SendRequestAsync(withdrawStakeFunction);
        }

        public Task<TransactionReceipt> WithdrawStakeRequestAndWaitForReceiptAsync(string withdrawAddress, CancellationTokenSource cancellationToken = null)
        {
            var withdrawStakeFunction = new WithdrawStakeFunction();
            withdrawStakeFunction.WithdrawAddress = withdrawAddress;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(withdrawStakeFunction, cancellationToken);
        }

        public Task<string> WithdrawToRequestAsync(WithdrawToFunction withdrawToFunction)
        {
            return ContractHandler.SendRequestAsync(withdrawToFunction);
        }

        public Task<TransactionReceipt> WithdrawToRequestAndWaitForReceiptAsync(WithdrawToFunction withdrawToFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(withdrawToFunction, cancellationToken);
        }

        public Task<string> WithdrawToRequestAsync(string withdrawAddress, BigInteger withdrawAmount)
        {
            var withdrawToFunction = new WithdrawToFunction();
            withdrawToFunction.WithdrawAddress = withdrawAddress;
            withdrawToFunction.WithdrawAmount = withdrawAmount;

            return ContractHandler.SendRequestAsync(withdrawToFunction);
        }

        public Task<TransactionReceipt> WithdrawToRequestAndWaitForReceiptAsync(string withdrawAddress, BigInteger withdrawAmount, CancellationTokenSource cancellationToken = null)
        {
            var withdrawToFunction = new WithdrawToFunction();
            withdrawToFunction.WithdrawAddress = withdrawAddress;
            withdrawToFunction.WithdrawAmount = withdrawAmount;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(withdrawToFunction, cancellationToken);
        }
    }
}
