using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Pairing
{
    /// <summary>
    /// A class that represents a new pairing. Includes the pairing topic
    /// and the URI the wallet should use to pair & retrieve the session proposal
    /// </summary>
    public class CreatePairingData
    {
        /// <summary>
        /// The new pairing topic
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        /// <summary>
        /// The URI the wallet should use to pair & retrieve the session proposal
        /// </summary>
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
