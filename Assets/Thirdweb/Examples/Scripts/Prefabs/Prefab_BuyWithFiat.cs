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

        var fiatQuoteParams = new BuyWithFiatQuoteParams(fromCurrencySymbol: "USD", toAddress: connectedAddress, toChainId: "137", toTokenAddress: Utils.NativeTokenAddress, toAmount: "20");

        _quote = await ThirdwebPay.GetBuyWithFiatQuote(fiatQuoteParams);
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
            _intentId = ThirdwebPay.BuyWithFiat(_quote);
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

        var status = await ThirdwebPay.GetBuyWithFiatStatus(_intentId);
        if (status.Status == OnRampStatus.PAYMENT_FAILED.ToString() || status.Status == OnRampStatus.ON_RAMP_TRANSFER_FAILED.ToString())
            ThirdwebDebug.LogWarning($"Failed! Reason: {status.FailureMessage}");

        ThirdwebDebug.Log($"Status: {JsonConvert.SerializeObject(status, Formatting.Indented)}");

        if (status.Status == OnRampStatus.PENDING_CRYPTO_SWAP.ToString())
            ThirdwebDebug.Log("OnRamp transfer completed. You may now use this intent id to trigger a BuyWithCrypto transaction and get to your destination token: " + _intentId);
    }

    public async void GetBuyHistory()
    {
        string connectedAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
        var history = await ThirdwebPay.GetBuyHistory(connectedAddress, 0, 10);
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
        var currencies = await ThirdwebPay.GetBuyWithFiatCurrencies();
        ThirdwebDebug.Log($"Supported Currencies: {JsonConvert.SerializeObject(currencies, Formatting.Indented)}");
    }
}
