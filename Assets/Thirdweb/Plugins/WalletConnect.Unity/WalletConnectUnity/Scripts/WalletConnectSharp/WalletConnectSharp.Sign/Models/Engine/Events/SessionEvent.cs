using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine.Events
{
    /// <summary>
    /// The event that is emitted when any session event occurs. Some examples
    /// include
    /// * Session Extended
    /// * Session Ping
    /// * PairingStore Ping
    /// * Session Delete
    /// * PairingStore Delete
    /// * Generic Session Request
    /// * Generic Session Event Emitted
    /// </summary>
    public class SessionEvent
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
