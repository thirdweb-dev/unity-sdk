using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace MetaMask.Models
{
    public class MetaMaskAnalyticsInfo
    {

        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("event")]
        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonProperty("communicationLayerPreference")]
        [JsonPropertyName("communicationLayerPreference")]
        public string CommunicationLayerPreference { get; set; }

        [JsonProperty("sdkVersion")]
        [JsonPropertyName("sdkVersion")]
        public string SdkVersion { get; set; }

        [JsonProperty("originatorInfo")]
        [JsonPropertyName("originatorInfo")]
        public MetaMaskOriginatorInfo OriginatorInfo { get; set; }
    }
}
