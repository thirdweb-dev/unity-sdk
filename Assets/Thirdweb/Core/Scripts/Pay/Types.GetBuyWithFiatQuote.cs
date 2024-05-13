using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    public class BuyWithFiatQuoteParams
    {
        [JsonProperty("fromCurrencySymbol")]
        public string FromCurrencySymbol { get; set; }

        [JsonProperty("fromAmount")]
        public string FromAmount { get; set; }

        [JsonProperty("fromAmountUnits")]
        public string FromAmountUnits { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("toChainId")]
        public string ToChainId { get; set; }

        [JsonProperty("toTokenAddress")]
        public string ToTokenAddress { get; set; }

        [JsonProperty("toAmount")]
        public string ToAmount { get; set; }

        [JsonProperty("toAmountWei")]
        public string ToAmountWei { get; set; }

        [JsonProperty("maxSlippageBPS")]
        public double? MaxSlippageBPS { get; set; }

        [JsonProperty("isTestMode")]
        public bool IsTestMode { get; set; }

        public BuyWithFiatQuoteParams(
            string fromCurrencySymbol,
            string toAddress,
            string toChainId,
            string toTokenAddress,
            string fromAmount = null,
            string fromAmountUnits = null,
            string toAmount = null,
            string toAmountWei = null,
            double? maxSlippageBPS = null,
            bool isTestMode = false
        )
        {
            FromCurrencySymbol = fromCurrencySymbol;
            FromAmount = fromAmount;
            FromAmountUnits = fromAmountUnits;
            ToAddress = toAddress;
            ToChainId = toChainId;
            ToTokenAddress = toTokenAddress;
            ToAmount = toAmount;
            ToAmountWei = toAmountWei;
            MaxSlippageBPS = maxSlippageBPS;
            IsTestMode = isTestMode;
        }
    }

    public class BuyWithFiatQuoteResult
    {
        [JsonProperty("intentId")]
        public string IntentId { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("fromCurrency")]
        public OnRampCurrency FromCurrency { get; set; }

        [JsonProperty("fromCurrencyWithFees")]
        public OnRampCurrency FromCurrencyWithFees { get; set; }

        [JsonProperty("onRampToken")]
        public OnRampToken OnRampToken { get; set; }

        [JsonProperty("toToken")]
        public Token ToToken { get; set; }

        [JsonProperty("estimatedToAmountMinWei")]
        public string EstimatedToAmountMinWei { get; set; }

        [JsonProperty("estimatedToAmountMin")]
        public string EstimatedToAmountMin { get; set; }

        [JsonProperty("processingFees")]
        public List<OnRampFees> ProcessingFees { get; set; }

        [JsonProperty("estimatedDurationSeconds")]
        public string EstimatedDurationSeconds { get; set; }

        [JsonProperty("maxSlippageBPS")]
        public double MaxSlippageBPS { get; set; }

        [JsonProperty("onRampLink")]
        public string OnRampLink { get; set; }
    }

    public class OnRampToken
    {
        [JsonProperty("token")]
        public Token Token { get; set; }

        [JsonProperty("amountWei")]
        public string AmountWei { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("amountUSDCents")]
        public double AmountUSDCents { get; set; }
    }

    public class OnRampFees
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("amountUnits")]
        public string AmountUnits { get; set; }

        [JsonProperty("decimals")]
        public int Decimals { get; set; }

        [JsonProperty("currencySymbol")]
        public string CurrencySymbol { get; set; }

        [JsonProperty("feeType")]
        public string FeeType { get; set; }
    }

    public class GetFiatQuoteResponse
    {
        [JsonProperty("result")]
        public BuyWithFiatQuoteResult Result { get; set; }
    }
}
