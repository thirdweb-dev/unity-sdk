using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Pairing
{
    /// <summary>
    /// The event that is emitted when any pairing event occurs. Some examples
    /// include
    /// * Pairing Ping
    /// * Pairing Delete
    /// </summary>
    public class PairingEvent
    {
        /// <summary>
        /// The ID of the JSON Rpc request that triggered this session event
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }
        
        /// <summary>
        /// The topic of the session this event took place in
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }
}
