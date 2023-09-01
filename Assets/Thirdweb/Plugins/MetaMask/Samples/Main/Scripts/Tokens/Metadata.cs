using Newtonsoft.Json;

namespace MetaMask.Unity.Samples
{
    public class Metadata
    {
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("image")]
        public string Image { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("external_url")]
        public string ExternalUrl { get; set; }
    }
}