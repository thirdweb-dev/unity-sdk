using Newtonsoft.Json;

namespace WalletConnectUnity.Models
{
    public class WalletApp
    {
        [JsonProperty("browser")]
        public string BrowserUrl;

        [JsonProperty("ios")]
        public string IosUrl;

        [JsonProperty("android")]
        public string AndroidUrl;

        [JsonProperty("mac")]
        public string MacUrl;

        [JsonProperty("windows")]
        public string WindowsUrl;

        [JsonProperty("linux")]
        public string LinuxUrl;

        [JsonProperty("chrome")]
        public string ChromeUrl;

        [JsonProperty("firefox")]
        public string FirefoxUrl;

        [JsonProperty("safari")]
        public string SafariUrl;

        [JsonProperty("edge")]
        public string EdgeUrl;

        [JsonProperty("opera")]
        public string OperaUrl;
    }
}