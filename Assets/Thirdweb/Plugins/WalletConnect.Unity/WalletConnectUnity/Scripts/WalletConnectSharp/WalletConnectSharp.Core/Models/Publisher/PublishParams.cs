using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Core.Models.Publisher
{
    /// <summary>
    /// A class that holds the parameters of a publish 
    /// </summary>
    public class PublishParams
    {
        /// <summary>
        /// The topic to publish the message to
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        /// <summary>
        /// The message to publish in the set topic
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
        
        /// <summary>
        /// The required PublishOptions to use when publishing
        /// </summary>
        [JsonProperty("opts")]
        public PublishOptions Options { get; set; }
    }
}
