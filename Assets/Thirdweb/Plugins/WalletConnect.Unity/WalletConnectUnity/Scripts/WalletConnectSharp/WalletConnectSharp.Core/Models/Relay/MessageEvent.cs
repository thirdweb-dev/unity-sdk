using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Relay
{
    /// <summary>
    /// A class that represents a message that has been sent / received inside
    /// a specific topic
    /// </summary>
    public class MessageEvent
    {
        /// <summary>
        /// The topic the message was sent / received in
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        /// <summary>
        /// The message that was sent / received
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
