using System.Threading.Tasks;
using System.Numerics;
using Nethereum.Contracts;
using UnityEngine;

namespace Thirdweb
{
    public static class TransactionManager
    {
        private static bool warned;

        public static async Task<TWResult> ThirdwebRead<TWFunction, TWResult>(string contractAddress, TWFunction functionMessage)
            where TWFunction : FunctionMessage, new()
        {
            try
            {
                functionMessage.FromAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            }
            catch (System.Exception)
            {
                if (!warned)
                {
                    Debug.LogWarning("Sending accountless query, make sure a wallet is connected if this was not intended.");
                    warned = true;
                }
            }
            var queryHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetContractQueryHandler<TWFunction>();
            return await queryHandler.QueryAsync<TWResult>(contractAddress, functionMessage);
        }

        public static async Task<TransactionResult> ThirdwebWrite<TWFunction>(string contractAddress, TWFunction functionMessage, BigInteger? weiValue = null)
            where TWFunction : FunctionMessage, new()
        {
            functionMessage.FromAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            functionMessage.AmountToSend = weiValue ?? 0;

            var transactionHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetContractTransactionHandler<TWFunction>();
            var gas = await transactionHandler.EstimateGasAsync(contractAddress, functionMessage);
            functionMessage.Gas = gas.Value < 100000 ? 100000 : gas;
            var receipt = await transactionHandler.SendRequestAndWaitForReceiptAsync(contractAddress, functionMessage);
            return receipt.ToTransactionResult();
        }
    }
}
