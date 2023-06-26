using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    /// <summary>
    /// The data for an Iridium JWT payload 
    /// </summary>
    public class IridiumJWTPayload
    {
        /// <summary>
        /// The iss value
        /// </summary>
        [JsonProperty("iss")]
        public string Iss { get; set; }
        
        /// <summary>
        /// The sub value
        /// </summary>
        [JsonProperty("sub")]
        public string Sub { get; set; }
        
        /// <summary>
        /// The aud value
        /// </summary>
        [JsonProperty("aud")]
        public string Aud { get; set; }
        
        /// <summary>
        /// The iat value
        /// </summary>
        [JsonProperty("iat")]
        public long Iat { get; set; }
        
        /// <summary>
        /// The exp value
        /// </summary>
        [JsonProperty("exp")]
        public long Exp { get; set; }
    }
}
