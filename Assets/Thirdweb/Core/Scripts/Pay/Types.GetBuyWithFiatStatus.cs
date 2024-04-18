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

        [JsonProperty("failureMessage")]
        public string FailureMessage { get; set; }
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
        PENDING_CRYPTO_SWAP
    }
}
