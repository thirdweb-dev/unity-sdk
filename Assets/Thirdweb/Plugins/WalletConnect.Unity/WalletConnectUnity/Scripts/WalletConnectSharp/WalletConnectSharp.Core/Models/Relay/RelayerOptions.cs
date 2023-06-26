using Newtonsoft.Json;
using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Core.Models.Relay
{
    /// <summary>
    /// The options for configuring the Relayer module
    /// </summary>
    public class RelayerOptions
    {
        /// <summary>
        /// The ICore instance the Relayer should use. An ICore module is required as the Relayer
        /// module requires the core modules to function properly
        /// </summary>
        [JsonProperty("core")]
        public ICore Core { get; set; }

        /// <summary>
        /// The URL of the Relay server to connect to. This should not include any auth information, the Relayer module
        /// will construct it's own auth token using the project ID specified
        /// </summary>
        [JsonProperty("relayUrl")]
        public string RelayUrl { get; set; }
        
        /// <summary>
        /// The project ID to use for Relay authentication
        /// </summary>
        [JsonProperty("projectId")]
        public string ProjectId { get; set; }
    }
}
