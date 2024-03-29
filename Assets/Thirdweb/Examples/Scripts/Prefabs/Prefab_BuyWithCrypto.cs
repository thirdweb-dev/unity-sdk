using UnityEngine;
using Thirdweb;
using Thirdweb.Pay;
using Newtonsoft.Json;

public class Prefab_BuyWithCrypto : MonoBehaviour
{
    private BuyWithCryptoQuoteResult _quote;
    private string _txHash;

    public async void GetQuote()
    {
        string connectedAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();

        _quote = null;

        var swapQuoteParams = new BuyWithCryptoQuoteParams(
            fromAddress: connectedAddress,
            fromChainId: 137,
            fromTokenAddress: Utils.NativeTokenAddress,
            toTokenAddress: "0x0d500b1d8e8ef31e21c99d1db9a6444d3adf1270",
            toAmount: "2"
        );

        _quote = await ThirdwebPay.GetBuyWithCryptoQuote(swapQuoteParams);
        ThirdwebDebug.Log($"Quote: {JsonConvert.SerializeObject(_quote, Formatting.Indented)}");
    }

    public async void Buy()
    {
        if (_quote == null)
        {
            ThirdwebDebug.Log("Quote is null. Please get a quote first.");
            return;
        }

        _txHash = null;

        try
        {
            _txHash = await ThirdwebPay.BuyWithCrypto(_quote);
            ThirdwebDebug.Log($"Transaction hash: {_txHash}");
        }
        catch (System.Exception e)
        {
            ThirdwebDebug.Log($"Error: {e.Message}");
        }
    }

    public async void GetStatus()
    {
        if (string.IsNullOrEmpty(_txHash))
        {
            ThirdwebDebug.Log("Transaction hash is empty. Please buy first.");
            return;
        }

        var status = await ThirdwebPay.GetBuyWithCryptoStatus(_txHash);
        if (status.Status == SwapStatus.FAILED.ToString())
            ThirdwebDebug.LogWarning($"Failed! Reason: {status.FailureMessage}");

        ThirdwebDebug.Log($"Status: {JsonConvert.SerializeObject(status, Formatting.Indented)}");
    }

    public async void GetSwapHistory()
    {
        string connectedAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
        var history = await ThirdwebPay.GetBuyWithCryptoHistory(connectedAddress, 0, 1);
        ThirdwebDebug.Log($"History: {JsonConvert.SerializeObject(history, Formatting.Indented)}");

        var historyNext = await ThirdwebPay.GetBuyWithCryptoHistory(connectedAddress, 1, 10, history.NextCursor);
        ThirdwebDebug.Log($"History Next: {JsonConvert.SerializeObject(historyNext, Formatting.Indented)}");
    }
}
