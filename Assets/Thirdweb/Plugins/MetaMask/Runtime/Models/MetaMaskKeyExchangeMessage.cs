using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace MetaMask.Models
{

    /// <summary>
    /// The MetaMask key exchange message.
    /// </summary>
    public class MetaMaskKeyExchangeMessage
    {

        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonProperty("pubkey")]
        [JsonPropertyName("pubkey")]
        public string PublicKey { get; set; }

        public MetaMaskKeyExchangeMessage(string type)
        {
            Type = type;
        }

        public MetaMaskKeyExchangeMessage(string type, string publicKey)
        {
            Type = type;
            PublicKey = publicKey;
        }

    }
}
