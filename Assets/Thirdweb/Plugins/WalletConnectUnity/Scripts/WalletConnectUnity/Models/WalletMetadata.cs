using Newtonsoft.Json;

namespace WalletConnectUnity.Models
{
    public class WalletMetadata
    {
        [JsonProperty("shortName")]
        public string ShortName;

        [JsonProperty("colors")]
        public WalletColors Colors;
    }
}