using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    /// <summary>
    /// Represents a signed Iridium JWT token
    /// </summary>
    public class IridiumJWTSigned : IridiumJWTData
    {
        [JsonProperty("signature")]
        public byte[] Signature { get; set; }
    }
}
