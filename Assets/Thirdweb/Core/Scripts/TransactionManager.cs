using System.Threading.Tasks;
using System.Numerics;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using MinimalForwarder = Thirdweb.Contracts.Forwarder.ContractDefinition;
using Newtonsoft.Json;
using Thirdweb.Contracts.Forwarder.ContractDefinition;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using System.Collections.Generic;
using System.Linq;
using Amazon.Runtime.Internal.Util;

#pragma warning disable CS0618

namespace Thirdweb
{
    public static class TransactionManager
    {
        private static bool warned;

        public static async Task<TWResult> ThirdwebRead<TWFunction, TWResult>(ThirdwebSDK sdk, string contractAddress, TWFunction functionMessage)
            where TWFunction : FunctionMessage, new()
        {
            try
            {
                functionMessage.FromAddress = await sdk.Wallet.GetAddress();
            }
            catch (System.Exception)
            {
                if (!warned)
                {
                    ThirdwebDebug.Log("Sending accountless query, make sure a wallet is connected if this was not intended.");
                    warned = true;
                }
            }

            var web3 = Utils.GetWeb3(sdk.Session.ChainId, sdk.Session.Options.clientId, sdk.Session.Options.bundleId);
            var queryHandler = web3.Eth.GetContractQueryHandler<TWFunction>();
            return await queryHandler.QueryAsync<TWResult>(contractAddress, functionMessage);
        }

        public static async Task<TWResult[]> ThirdwebMulticallRead<TWFunction, TWResult>(ThirdwebSDK sdk, string contractAddress, TWFunction[] functionMessages)
            where TWFunction : FunctionMessage, new()
            where TWResult : IFunctionOutputDTO, new()
        {
            var web3 = Utils.GetWeb3(sdk.Session.ChainId, sdk.Session.Options.clientId, sdk.Session.Options.bundleId);
            MultiQueryHandler multiqueryHandler = web3.Eth.GetMultiQueryHandler();
            var calls = new List<MulticallInputOutput<TWFunction, TWResult>>();
            for (int i = 0; i < functionMessages.Length; i++)
            {
                calls.Add(new MulticallInputOutput<TWFunction, TWResult>(functionMessages[i], contractAddress));
            }
            var results = await multiqueryHandler.MultiCallAsync(MultiQueryHandler.DEFAULT_CALLS_PER_REQUEST, calls.ToArray()).ConfigureAwait(false);
            return calls.Select(x => x.Output).ToArray();
        }

        public static async Task<TransactionResult> ThirdwebWrite<TWFunction>(
            ThirdwebSDK sdk,
            string contractAddress,
            TWFunction functionMessage,
            BigInteger? weiValue = null,
            BigInteger? gasOverride = null
        )
            where TWFunction : FunctionMessage, new()
        {
            var receipt = await ThirdwebWriteRawResult(sdk, contractAddress, functionMessage, weiValue, gasOverride);
            return receipt.ToTransactionResult();
        }

        public static async Task<TransactionReceipt> ThirdwebWriteRawResult<TWFunction>(
            ThirdwebSDK sdk,
            string contractAddress,
            TWFunction functionMessage,
            BigInteger? weiValue = null,
            BigInteger? gasOverride = null
        )
            where TWFunction : FunctionMessage, new()
        {
            functionMessage.FromAddress = await sdk.Wallet.GetAddress();
            functionMessage.AmountToSend = weiValue ?? 0;

            if (gasOverride.HasValue)
            {
                functionMessage.Gas = gasOverride.Value;
            }
            else
            {
                try
                {
                    var web3 = Utils.GetWeb3(sdk.Session.ChainId, sdk.Session.Options.clientId, sdk.Session.Options.bundleId);
                    var gasEstimator = web3.Eth.GetContractTransactionHandler<TWFunction>();
                    var gas = await gasEstimator.EstimateGasAsync(contractAddress, functionMessage);
                    functionMessage.Gas = gas.Value < 100000 ? 100000 : gas.Value;
                }
                catch (System.InvalidOperationException e)
                {
                    ThirdwebDebug.LogWarning($"Failed to estimate gas for transaction, proceeding with 100k gas: {e}");
                    functionMessage.Gas = 100000;
                }
            }
            var transactionInput = functionMessage.CreateTransactionInput(contractAddress);
            var tx = new Transaction(sdk, transactionInput);
            var hash = await tx.Send();
            return await Transaction.WaitForTransactionResultRaw(hash, sdk.Session.ChainId);
        }
    }
}
