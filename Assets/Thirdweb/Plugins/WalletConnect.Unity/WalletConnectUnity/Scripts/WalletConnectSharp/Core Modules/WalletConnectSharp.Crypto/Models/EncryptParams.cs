using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    /// <summary>
    /// A class representing the encrypt parameters
    /// </summary>
    public class EncryptParams
    {
        /// <summary>
        /// The message to encrypt
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
        
        /// <summary>
        /// The Sym key to use for encrypting
        /// </summary>
        [JsonProperty("symKey")]
        public string SymKey { get; set; }
        
        /// <summary>
        /// The envelope type to use when encrypting
        /// </summary>
        [JsonProperty("type")]
        public int Type { get; set; }
        
        /// <summary>
        /// The IV to use for the encryption
        /// </summary>
        [JsonProperty("iv")]
        public string Iv { get; set; }
        
        /// <summary>
        /// The public key of the sender of this encrypted message
        /// </summary>
        [JsonProperty("senderPublicKey")]
        public string SenderPublicKey { get; set; }
    }
}
