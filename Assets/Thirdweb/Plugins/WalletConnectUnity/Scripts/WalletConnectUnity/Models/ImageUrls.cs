using Newtonsoft.Json;
using UnityEngine;

namespace WalletConnectUnity.Models
{
    public class ImageUrls
    {
        [JsonProperty("sm")]
        public string SmallUrl;

        [JsonProperty("md")]
        public string MediumUrl;

        [JsonProperty("lg")]
        public string LargeUrl;

        public Sprite SmallIcon;
        
        public Sprite MediumIcon;
        
        public Sprite LargeIcon;
    }
}