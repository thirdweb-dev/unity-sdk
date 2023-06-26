using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    /// <summary>
    /// Represents a subscription that's pending
    /// </summary>
    public class PendingSubscription : SubscribeOptions
    {
        /// <summary>
        /// The topic that will be subscribed to
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }
}
