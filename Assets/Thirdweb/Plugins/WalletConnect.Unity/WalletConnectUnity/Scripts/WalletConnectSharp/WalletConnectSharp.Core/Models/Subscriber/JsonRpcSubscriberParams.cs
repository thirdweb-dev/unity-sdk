using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    /// <summary>
    /// The parameters for a JSON-RPC Subscribe method call
    /// </summary>
    public class JsonRpcSubscriberParams
    {
        /// <summary>
        /// The topic to subscribe to
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }
    }
}
