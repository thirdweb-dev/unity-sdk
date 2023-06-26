using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Relay
{
    /// <summary>
    /// A class that represents options when publishing messages
    /// </summary>
    public class PublishOptions : ProtocolOptionHolder
    {
        /// <summary>
        /// Time To Live value for the message being published. 
        /// </summary>
        [JsonProperty("ttl")]
        public long TTL { get; set; }

        /// <summary>
        /// A Tag for the message
        /// </summary>
        [JsonProperty("tag")]
        public long Tag { get; set; }
    }
}
