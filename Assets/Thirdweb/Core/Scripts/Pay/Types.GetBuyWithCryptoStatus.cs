using System;
using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    public class SwapStatusResponse
    {
        [JsonProperty("result")]
        public BuyWithCryptoStatusResult Result { get; set; }
    }

    public class BuyWithCryptoStatusResult
    {
        [JsonProperty("quote")]
        public SwapQuote Quote { get; set; }

        [JsonProperty("swapType")]
        public string SwapType { get; set; }

        [JsonProperty("source")]
        public TransactionDetails Source { get; set; }

        [JsonProperty("destination")]
        public TransactionDetails Destination { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("subStatus")]
        public string SubStatus { get; set; }

        [JsonProperty("fromAddress")]
        public string FromAddress { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("failureMessage")]
        public string FailureMessage { get; set; }

        [JsonProperty("bridge")]
        public string Bridge { get; set; }
    }

    public class TransactionDetails
    {
        [JsonProperty("transactionHash")]
        public string TransactionHash { get; set; }

        [JsonProperty("token")]
        public Token Token { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("amountWei")]
        public string AmountWei { get; set; }

        [JsonProperty("amountUSDCents")]
        public double AmountUSDCents { get; set; }

        [JsonProperty("completedAt")]
        public DateTime CompletedAt { get; set; }

        [JsonProperty("explorerLink")]
        public string ExplorerLink { get; set; }
    }

    public class SwapQuote
    {
        [JsonProperty("fromToken")]
        public Token FromToken { get; set; }

        [JsonProperty("toToken")]
        public Token ToToken { get; set; }

        [JsonProperty("fromAmountWei")]
        public string FromAmountWei { get; set; }

        [JsonProperty("fromAmount")]
        public string FromAmount { get; set; }

        [JsonProperty("toAmountWei")]
        public string ToAmountWei { get; set; }

        [JsonProperty("toAmount")]
        public string ToAmount { get; set; }

        [JsonProperty("toAmountMin")]
        public string ToAmountMin { get; set; }

        [JsonProperty("toAmountMinWei")]
        public string ToAmountMinWei { get; set; }

        [JsonProperty("estimated")]
        public Estimated Estimated { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public enum SwapStatus
    {
        NOT_FOUND,
        NONE,
        PENDING,
        FAILED,
        COMPLETED
    }

    public enum SwapSubStatus
    {
        NONE,
        WAITING_BRIDGE,
        REVERTED_ON_CHAIN,
        SUCCESS,
        PARTIAL_SUCCESS,
        UNKNOWN_ERROR
    }
}
