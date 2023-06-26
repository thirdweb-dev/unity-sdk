using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Eth {

public class EthCall
{
    [JsonProperty("to")]
    public string To { get; set; }
    
    [JsonProperty("data")]
    public string Data { get; set; }
}
}
