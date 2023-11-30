using Newtonsoft.Json;

namespace WalletConnectUnity.Models
{
    public class WalletInjection
    {
        [JsonProperty("namespace")]
        public string Namespace;

        [JsonProperty("injected_id")]
        public string InjectedId;
    }
}