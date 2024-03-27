using System.Threading.Tasks;

namespace Thirdweb.Pay
{
    public static partial class ThirdwebPay
    {
        /// <summary>
        /// Send a quoted swap transaction.
        /// </summary>
        /// <param name="buyWithCryptoQuote">Swap quote containing the transaction request</param>
        /// <param name="sdk">Optional SDK instance, defaults to ThirdwebManager instance</param>
        /// <returns></returns>
        public static async Task<string> BuyWithCrypto(BuyWithCryptoQuoteResult buyWithCryptoQuote, ThirdwebSDK sdk = null)
        {
            sdk ??= ThirdwebManager.Instance.SDK;

            if (buyWithCryptoQuote.Approval != null)
            {
                ThirdwebDebug.Log("Approving ERC20...");
                var erc20ToApprove = sdk.GetContract(buyWithCryptoQuote.Approval.TokenAddress);
                var approvalRes = await erc20ToApprove.ERC20.SetAllowance(buyWithCryptoQuote.Approval.SpenderAddress, buyWithCryptoQuote.Approval.AmountWei.ToEth());
                ThirdwebDebug.Log($"Approval transaction receipt: {approvalRes}");
            }

            ThirdwebDebug.Log("Sending swap transaction...");
            var hash = await sdk.Wallet.SendRawTransaction(
                new Thirdweb.TransactionRequest()
                {
                    from = buyWithCryptoQuote.TransactionRequest.From,
                    to = buyWithCryptoQuote.TransactionRequest.To,
                    data = buyWithCryptoQuote.TransactionRequest.Data,
                    value = buyWithCryptoQuote.TransactionRequest.Value,
                    gasLimit = buyWithCryptoQuote.TransactionRequest.GasLimit,
                    gasPrice = buyWithCryptoQuote.TransactionRequest.GasPrice,
                }
            );
            ThirdwebDebug.Log($"Swap transaction hash: {hash}");

            return hash;
        }
    }
}
