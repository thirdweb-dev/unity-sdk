using Newtonsoft.Json;

namespace WalletConnectSharp.Crypto.Models
{
    
    public class IridiumJWTDecoded : IridiumJWTSigned
    {
        [JsonProperty("data")]
        public byte[] Data { get; set; }
    }
}
