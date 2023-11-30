using Newtonsoft.Json;

namespace WalletConnectUnity.Models
{
    public class WalletStandard
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("url")]
        public string Url;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("standard_id")]
        public string StandardId;

        [JsonProperty("standard_prefix")]
        public string StandardPrefix;
    }
}