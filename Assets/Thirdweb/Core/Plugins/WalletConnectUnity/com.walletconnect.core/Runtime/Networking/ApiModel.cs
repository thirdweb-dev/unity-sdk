using Newtonsoft.Json;

namespace WalletConnectUnity.Core.Networking
{
    public struct UrlQueryParams
    {
        // mandatory
        public string Page { get; set; }

        // mandatory
        public string Entries { get; set; }

        public string Search { get; set; }

        public string Include { get; set; }

        public string Exclude { get; set; }

        public string Platform { get; set; }
    }

    public class GetWalletsResponse
    {
        [JsonProperty("count")] public int Count { get; set; }

        [JsonProperty("data")] public Wallet[] Data { get; set; }
    }

    public class Wallet
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("homepage")] public string Homepage { get; set; }

        [JsonProperty("image_id")] public string ImageId { get; set; }

        [JsonProperty("order")] public int Order { get; set; }

        [JsonProperty("mobile_link")] public string MobileLink { get; set; }

        [JsonProperty("desktop_link")] public string DesktopLink { get; set; }

        [JsonProperty("webapp_link")] public string WebappLink { get; set; }

        [JsonProperty("app_store")] public string AppStore { get; set; }

        [JsonProperty("play_store")] public string PlayStore { get; set; }
    }
}