using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Relay
{
    /// <summary>
    /// Options for unsubscribing to a subscription with the given id
    /// </summary>
    public class UnsubscribeOptions : ProtocolOptionHolder
    {
        /// <summary>
        /// The id of the subscription to unsubscribe from
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
