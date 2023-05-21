using System.Text.Json;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace MetaMask.Models
{
    public class MetaMaskTransaction
    {

        [JsonProperty("to")]
        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonProperty("from")]
        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonProperty("value")]
        [JsonPropertyName("value")]
        public string Value { get; set; }

    }
}
