using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace MetaMask.Models
{

    public class MetaMaskEthereumRequest
    {

        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("method")]
        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        [JsonPropertyName("params")]
        public object Parameters { get; set; }

    }
}
