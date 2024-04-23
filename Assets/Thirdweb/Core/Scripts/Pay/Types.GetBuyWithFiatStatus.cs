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
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("swapType")]
        public string SwapType { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("quote")]
        public OnRampQuote Quote { get; set; }

        [JsonProperty("source")]
        public TransactionDetails Source { get; set; }

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

        [JsonProperty("onRampToken")]
        public Token OnRampToken { get; set; }

        [JsonProperty("toToken")]
        public Token ToToken { get; set; }
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
        PENDING_CRYPTO_SWAP,
        CRYPTO_SWAP_PARTIAL_SUCCESS,
        CRYPTO_SWAP_REVERTED_ON_CHAIN,
        CRYPTO_SWAP_COMPLETED,
        CRYPTO_SWAP_UNKNOWN_ERROR,
        CRYPTO_SWAP_WAITING_BRIDGE
    }
}
