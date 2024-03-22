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
using Thirdweb.Contracts.Forwarder.ContractDefinition;

namespace Thirdweb.Contracts.Forwarder
{
    public partial class ForwarderService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(
            Nethereum.Web3.Web3 web3,
            ForwarderDeployment forwarderDeployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            return web3.Eth.GetContractDeploymentHandler<ForwarderDeployment>().SendRequestAndWaitForReceiptAsync(forwarderDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, ForwarderDeployment forwarderDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<ForwarderDeployment>().SendRequestAsync(forwarderDeployment);
        }

        public static async Task<ForwarderService> DeployContractAndGetServiceAsync(
            Nethereum.Web3.Web3 web3,
            ForwarderDeployment forwarderDeployment,
            CancellationTokenSource cancellationTokenSource = null
        )
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, forwarderDeployment, cancellationTokenSource);
            return new ForwarderService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.Web3 Web3 { get; }

        public ContractHandler ContractHandler { get; }

        public ForwarderService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<string> ExecuteRequestAsync(ExecuteFunction executeFunction)
        {
            return ContractHandler.SendRequestAsync(executeFunction);
        }

        public Task<TransactionReceipt> ExecuteRequestAndWaitForReceiptAsync(ExecuteFunction executeFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(executeFunction, cancellationToken);
        }

        public Task<string> ExecuteRequestAsync(ForwardRequest req, byte[] signature)
        {
            var executeFunction = new ExecuteFunction();
            executeFunction.Req = req;
            executeFunction.Signature = signature;

            return ContractHandler.SendRequestAsync(executeFunction);
        }

        public Task<TransactionReceipt> ExecuteRequestAndWaitForReceiptAsync(ForwardRequest req, byte[] signature, CancellationTokenSource cancellationToken = null)
        {
            var executeFunction = new ExecuteFunction();
            executeFunction.Req = req;
            executeFunction.Signature = signature;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(executeFunction, cancellationToken);
        }

        public Task<BigInteger> GetNonceQueryAsync(GetNonceFunction getNonceFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetNonceFunction, BigInteger>(getNonceFunction, blockParameter);
        }

        public Task<BigInteger> GetNonceQueryAsync(string from, BlockParameter blockParameter = null)
        {
            var getNonceFunction = new GetNonceFunction();
            getNonceFunction.From = from;

            return ContractHandler.QueryAsync<GetNonceFunction, BigInteger>(getNonceFunction, blockParameter);
        }

        public Task<bool> VerifyQueryAsync(VerifyFunction verifyFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<VerifyFunction, bool>(verifyFunction, blockParameter);
        }

        public Task<bool> VerifyQueryAsync(ForwardRequest req, byte[] signature, BlockParameter blockParameter = null)
        {
            var verifyFunction = new VerifyFunction();
            verifyFunction.Req = req;
            verifyFunction.Signature = signature;

            return ContractHandler.QueryAsync<VerifyFunction, bool>(verifyFunction, blockParameter);
        }
    }
}
