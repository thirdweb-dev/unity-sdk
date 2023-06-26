using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    /// <summary>
    /// A class representing the options for encoding
    /// </summary>
    public class EncodeOptions
    {
        /// <summary>
        /// The envelope type to use
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; set; }
        
        /// <summary>
        /// The public key that is sending the encoded message
        /// </summary>
        [JsonProperty("senderPublicKey")]
        public string SenderPublicKey { get; set; }
        
        /// <summary>
        /// The public key that is receiving the encoded message
        /// </summary>
        [JsonProperty("receiverPublicKey")]
        public string ReceiverPublicKey { get; set; }
    }
}
