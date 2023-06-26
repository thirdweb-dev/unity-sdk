using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Core.Models.Pairing
{
    /// <summary>
    /// A class that holds parameters from a parsed session proposal URI. This can be
    /// retrieved from <see cref="IEngine.ParseUri(string)"/>
    /// </summary>
    public class UriParameters
    {
        /// <summary>
        /// The protocol being used for this session (as a protocol string)
        /// </summary>
        [JsonProperty("protocol")]
        public string Protocol { get; set; }
        
        /// <summary>
        /// The protocol version being used for this session
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; set; }
        
        /// <summary>
        /// The pairing topic that should be used to retrieve the session proposal
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        /// <summary>
        /// The sym key used to encrypt the session proposal
        /// </summary>
        [JsonProperty("symKey")]
        public string SymKey { get; set; }
        
        /// <summary>
        /// Any protocol options that should be used when pairing / approving the session
        /// </summary>
        [JsonProperty("relay")]
        public ProtocolOptions Relay { get; set; }
    }
}
