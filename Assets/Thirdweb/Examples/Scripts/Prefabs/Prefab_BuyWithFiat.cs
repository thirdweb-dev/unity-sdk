using UnityEngine;
using Thirdweb;
using Thirdweb.Pay;
using Newtonsoft.Json;
using System.Linq;

public class Prefab_BuyWithFiat : MonoBehaviour
{
    private BuyWithFiatQuoteResult _quote;
    private string _intentId;

    public async void GetQuote()
    {
        string connectedAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();

        _quote = null;

        var fiatQuoteParams = new BuyWithFiatQuoteParams(
            fromCurrencySymbol: "USD",
            toAddress: connectedAddress,
            toChainId: "137",
            toTokenAddress: Utils.NativeTokenAddress,
            toAmount: "20",
            isTestMode: true
        );

        _quote = await ThirdwebManager.Instance.SDK.Pay.GetBuyWithFiatQuote(fiatQuoteParams);
        ThirdwebDebug.Log($"Quote: {JsonConvert.SerializeObject(_quote, Formatting.Indented)}");
    }

    public void Buy()
    {
        if (_quote == null)
        {
            ThirdwebDebug.Log("Get a quote first.");
            return;
        }

        try
        {
            _intentId = ThirdwebManager.Instance.SDK.Pay.BuyWithFiat(_quote);
            ThirdwebDebug.Log($"Intent ID: {_intentId}");
        }
        catch (System.Exception e)
        {
            ThirdwebDebug.Log($"Error: {e.Message}");
        }
    }

    public async void GetStatus()
    {
        if (string.IsNullOrEmpty(_intentId))
        {
            ThirdwebDebug.Log("Intent ID is empty. Please buy first.");
            return;
        }

        var status = await ThirdwebManager.Instance.SDK.Pay.GetBuyWithFiatStatus(_intentId);

        if (status.Status == OnRampStatus.PAYMENT_FAILED.ToString() || status.Status == OnRampStatus.ON_RAMP_TRANSFER_FAILED.ToString() || status.Status == OnRampStatus.CRYPTO_SWAP_FAILED.ToString())
        {
            ThirdwebDebug.LogWarning($"Failed! Reason: {status.FailureMessage}");
        }
        else if (status.Status == OnRampStatus.CRYPTO_SWAP_FALLBACK.ToString())
        {
            ThirdwebDebug.LogWarning($"Fallback! Two step process failed and user received fallback funds on the destination chain.");
        }
        else if (status.Status == OnRampStatus.CRYPTO_SWAP_REQUIRED.ToString())
        {
            ThirdwebDebug.Log("OnRamp transfer completed. You may now use this intent id to trigger a BuyWithCrypto transaction and get to your destination token: " + _intentId);

            // This is only necessary when you can't get to the destination token directly from the onramp
            // Example of how to trigger a BuyWithCrypto transaction using the intent id of the onramp with the newly received funds

            // var swapQuoteParams = new BuyWithCryptoQuoteParams(
            //     fromAddress: status.ToAddress,
            //     fromChainId: status.Quote.OnRampToken.ChainId,
            //     fromTokenAddress: status.Quote.OnRampToken.TokenAddress,
            //     toTokenAddress: status.Quote.ToToken.TokenAddress,
            //     toAmount: status.Quote.EstimatedToTokenAmount,
            //     intentId: _intentId
            // );

            // var quote = await ThirdwebManager.Instance.SDK.Pay.GetBuyWithCryptoQuote(swapQuoteParams);

            // See Prefab_BuyWithCrypto.cs for the rest of the process
        }

        ThirdwebDebug.Log($"Status: {JsonConvert.SerializeObject(status, Formatting.Indented)}");
    }

    public async void GetBuyHistory()
    {
        string connectedAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
        var history = await ThirdwebManager.Instance.SDK.Pay.GetBuyHistory(connectedAddress, 0, 10);
        ThirdwebDebug.Log($"Full History: {JsonConvert.SerializeObject(history, Formatting.Indented)}");

        var latestBuyWithFiatStatus = history.Page.FirstOrDefault(h => h.BuyWithFiatStatus != null)?.BuyWithFiatStatus;
        if (latestBuyWithFiatStatus != null)
            ThirdwebDebug.Log($"Latest Buy With Fiat Status: {JsonConvert.SerializeObject(latestBuyWithFiatStatus, Formatting.Indented)}");
        else
            ThirdwebDebug.Log("No Buy With Fiat Status found.");
    }

    [ContextMenu("Get Supported Currencies")]
    public async void GetSupportedCurrencies()
    {
        var currencies = await ThirdwebManager.Instance.SDK.Pay.GetBuyWithFiatCurrencies();
        ThirdwebDebug.Log($"Supported Currencies: {JsonConvert.SerializeObject(currencies, Formatting.Indented)}");
    }
}
