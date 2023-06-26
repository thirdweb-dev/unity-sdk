using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Subscriber
{
    public class BatchSubscribeParams
    {
        [JsonProperty("topics")]
        public string[] Topics { get; set; }
    }
}
