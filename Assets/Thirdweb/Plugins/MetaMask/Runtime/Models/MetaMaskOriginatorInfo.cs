using System.Text.Json.Serialization;

using Newtonsoft.Json;
using UnityEngine;

namespace MetaMask.Models
{
    public class MetaMaskOriginatorInfo
    {

        [JsonProperty("title")]
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonProperty("platform")]
        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonProperty("apiVersion")]
        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; } = Application.version;

    }
}
