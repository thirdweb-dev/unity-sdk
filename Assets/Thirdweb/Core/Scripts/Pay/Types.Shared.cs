using System.Numerics;
using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    public class ErrorResponse
    {
        [JsonProperty("error")]
        public ErrorDetails Error { get; set; }
    }

    public class ErrorDetails
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("stack")]
        public string Stack { get; set; }

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }
    }

    public class Token
    {
        [JsonProperty("chainId")]
        public BigInteger ChainId { get; set; }

        [JsonProperty("tokenAddress")]
        public string TokenAddress { get; set; }

        [JsonProperty("decimals")]
        public int Decimals { get; set; }

        [JsonProperty("priceUSDCents")]
        public int PriceUSDCents { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }
    }

    public class Estimated
    {
        [JsonProperty("fromAmountUSDCents")]
        public double FromAmountUSDCents { get; set; }

        [JsonProperty("toAmountMinUSDCents")]
        public double ToAmountMinUSDCents { get; set; }

        [JsonProperty("toAmountUSDCents")]
        public double ToAmountUSDCents { get; set; }

        [JsonProperty("slippageBPS")]
        public int SlippageBPS { get; set; }

        [JsonProperty("feesUSDCents")]
        public double FeesUSDCents { get; set; }

        [JsonProperty("gasCostUSDCents")]
        public double GasCostUSDCents { get; set; }

        [JsonProperty("durationSeconds")]
        public int DurationSeconds { get; set; }
    }

    public class OnRampCurrency
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("amountUnits")]
        public string AmountUnits { get; set; }

        [JsonProperty("decimals")]
        public int Decimals { get; set; }

        [JsonProperty("currencySymbol")]
        public string CurrencySymbol { get; set; }
    }

    public enum SwapType
    {
        SAME_CHAIN,
        CROSS_CHAIN,
        ON_RAMP
    }
}
