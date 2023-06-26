using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    /// <summary>
    /// The header data for an Iridium JWT header
    /// </summary>
    public class IridiumJWTHeader
    {
        /// <summary>
        /// The default header to use
        /// </summary>
        public static readonly IridiumJWTHeader DEFAULT = new IridiumJWTHeader()
        {
            Alg = "EdDSA",
            Typ = "JWT"
        };
        
        /// <summary>
        /// The encoding algorithm to use
        /// </summary>
        [JsonProperty("alg")]
        public string Alg { get; set; }
        
        /// <summary>
        /// The encoding type to use
        /// </summary>
        [JsonProperty("typ")]
        public string Typ { get; set; }
    }
}
