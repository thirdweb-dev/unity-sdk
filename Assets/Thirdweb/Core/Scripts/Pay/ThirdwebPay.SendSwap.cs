using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    public static partial class ThirdwebPay
    {
        public static async Task<string> SendSwap(SwapQuoteResult swapRoute, ThirdwebSDK sdk = null)
        {
            sdk ??= ThirdwebManager.Instance.SDK;

            if (swapRoute.Approval != null)
            {
                ThirdwebDebug.Log("Approving ERC20...");
                var erc20ToApprove = sdk.GetContract(swapRoute.Approval.TokenAddress);
                var approvalRes = await erc20ToApprove.ERC20.SetAllowance(swapRoute.Approval.SpenderAddress, swapRoute.Approval.AmountWei.ToEth());
                ThirdwebDebug.Log($"Approval transaction receipt: {approvalRes}");
            }

            ThirdwebDebug.Log("Sending swap transaction...");
            var sendRes = await sdk.Wallet.SendRawTransaction(
                new Thirdweb.TransactionRequest()
                {
                    from = swapRoute.TransactionRequest.From,
                    to = swapRoute.TransactionRequest.To,
                    data = swapRoute.TransactionRequest.Data,
                    value = swapRoute.TransactionRequest.Value,
                    gasLimit = swapRoute.TransactionRequest.GasLimit,
                    gasPrice = swapRoute.TransactionRequest.GasPrice,
                }
            );
            ThirdwebDebug.Log($"Swap transaction receipt: {sendRes.receipt.transactionHash}");

            return sendRes.receipt.transactionHash;
        }
    }
}
