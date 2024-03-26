using System.Threading.Tasks;

namespace Thirdweb.Pay
{
    public static partial class ThirdwebPay
    {
        /// <summary>
        /// Send a quoted swap transaction.
        /// </summary>
        /// <param name="swapQuote">Swap route containing the transaction request</param>
        /// <param name="sdk">Optional SDK instance, defaults to ThirdwebManager instance</param>
        /// <returns></returns>
        public static async Task<string> SendSwap(SwapQuoteResult swapQuote, ThirdwebSDK sdk = null)
        {
            sdk ??= ThirdwebManager.Instance.SDK;

            if (swapQuote.Approval != null)
            {
                ThirdwebDebug.Log("Approving ERC20...");
                var erc20ToApprove = sdk.GetContract(swapQuote.Approval.TokenAddress);
                var approvalRes = await erc20ToApprove.ERC20.SetAllowance(swapQuote.Approval.SpenderAddress, swapQuote.Approval.AmountWei.ToEth());
                ThirdwebDebug.Log($"Approval transaction receipt: {approvalRes}");
            }

            ThirdwebDebug.Log("Sending swap transaction...");
            var hash = await sdk.Wallet.SendRawTransaction(
                new Thirdweb.TransactionRequest()
                {
                    from = swapQuote.TransactionRequest.From,
                    to = swapQuote.TransactionRequest.To,
                    data = swapQuote.TransactionRequest.Data,
                    value = swapQuote.TransactionRequest.Value,
                    gasLimit = swapQuote.TransactionRequest.GasLimit,
                    gasPrice = swapQuote.TransactionRequest.GasPrice,
                }
            );
            ThirdwebDebug.Log($"Swap transaction hash: {hash}");

            return hash;
        }
    }
}
