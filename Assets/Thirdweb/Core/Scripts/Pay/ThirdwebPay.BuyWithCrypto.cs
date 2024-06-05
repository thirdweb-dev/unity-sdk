using System.Numerics;
using System.Threading.Tasks;

namespace Thirdweb.Pay
{
    public partial class ThirdwebPay
    {
        /// <summary>
        /// Send a quoted swap transaction.
        /// </summary>
        /// <param name="buyWithCryptoQuote">Swap quote containing the transaction request</param>
        /// <param name="sdk">Optional SDK instance, defaults to ThirdwebManager instance</param>
        /// <returns></returns>
        public async Task<string> BuyWithCrypto(BuyWithCryptoQuoteResult buyWithCryptoQuote)
        {
            if (buyWithCryptoQuote.Approval != null)
            {
                ThirdwebDebug.Log("Approving ERC20...");
                var erc20ToApprove = _sdk.GetContract(buyWithCryptoQuote.Approval.TokenAddress);
                var currentAllowance = await erc20ToApprove.ERC20.Allowance(buyWithCryptoQuote.Approval.SpenderAddress);
                if (BigInteger.Parse(currentAllowance.value) >= BigInteger.Parse(buyWithCryptoQuote.Approval.AmountWei))
                {
                    ThirdwebDebug.Log("Already approved");
                }
                else
                {
                    var approvalRes = await erc20ToApprove.ERC20.SetAllowance(buyWithCryptoQuote.Approval.SpenderAddress, buyWithCryptoQuote.Approval.AmountWei.ToEth());
                    ThirdwebDebug.Log($"Approval transaction receipt: {approvalRes}");
                }
            }

            ThirdwebDebug.Log("Sending swap transaction...");
            var hash = await _sdk.Wallet.SendRawTransaction(
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
