using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    /// <summary>
    /// Represents an active subscription with the given subscription id
    /// </summary>
    public class ActiveSubscription : PendingSubscription
    {
        /// <summary>
        /// The id of the subscription
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
