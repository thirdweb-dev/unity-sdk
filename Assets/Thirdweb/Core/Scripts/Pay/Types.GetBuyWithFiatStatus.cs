using System;
using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    public class OnRampStatusResponse
    {
        [JsonProperty("result")]
        public BuyWithFiatStatusResult Result { get; set; }
    }

    public class BuyWithFiatStatusResult
    {
        [JsonProperty("intentId")]
        public string IntentId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("quote")]
        public OnRampQuote Quote { get; set; }

        [JsonProperty("source")]
        public TransactionDetails Source { get; set; }

        [JsonProperty("destination")]
        public TransactionDetails Destination { get; set; }

        [JsonProperty("failureMessage")]
        public string FailureMessage { get; set; }
    }

    public class OnRampQuote
    {
        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("estimatedOnRampAmountWei")]
        public string EstimatedOnRampAmountWei { get; set; }

        [JsonProperty("estimatedOnRampAmount")]
        public string EstimatedOnRampAmount { get; set; }

        [JsonProperty("estimatedToTokenAmount")]
        public string EstimatedToTokenAmount { get; set; }

        [JsonProperty("estimatedToTokenAmountWei")]
        public string EstimatedToTokenAmountWei { get; set; }

        [JsonProperty("fromCurrency")]
        public OnRampCurrency FromCurrency { get; set; }

        [JsonProperty("fromCurrencyWithFees")]
        public OnRampCurrency FromCurrencyWithFees { get; set; }

        [JsonProperty("onRampToken")]
        public Token OnRampToken { get; set; }

        [JsonProperty("toToken")]
        public Token ToToken { get; set; }

        [JsonProperty("estimatedDurationSeconds")]
        public long EstimatedDurationSeconds { get; set; }
    }

    public enum OnRampStatus
    {
        NONE,
        PENDING_PAYMENT,
        PAYMENT_FAILED,
        PENDING_ON_RAMP_TRANSFER,
        ON_RAMP_TRANSFER_IN_PROGRESS,
        ON_RAMP_TRANSFER_COMPLETED,
        ON_RAMP_TRANSFER_FAILED,
        CRYPTO_SWAP_REQUIRED,
        CRYPTO_SWAP_COMPLETED,
        CRYPTO_SWAP_FALLBACK,
        CRYPTO_SWAP_IN_PROGRESS,
        CRYPTO_SWAP_FAILED,
    }
}
