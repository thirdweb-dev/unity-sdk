using Newtonsoft.Json;

namespace WalletConnectUnity.Models
{
    public class WalletLink
    {
        [JsonProperty("native")]
        public string NativeProtocol;

        [JsonProperty("universal")]
        public string UniversalUrl;
    }
}