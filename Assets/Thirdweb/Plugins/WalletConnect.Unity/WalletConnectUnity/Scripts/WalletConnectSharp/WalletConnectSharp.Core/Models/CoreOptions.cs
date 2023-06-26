using Newtonsoft.Json;
using WalletConnectSharp.Crypto.Interfaces;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectSharp.Core.Models
{
    /// <summary>
    /// Options used to configure the Core module. 
    /// </summary>
    public class CoreOptions
    {
        /// <summary>
        /// The Project ID to use to authenticate with the relay server
        /// </summary>
        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        /// <summary>
        /// The name that this Core module will show itself as
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The URL of the relay server to connect to. This should not include any auth info
        /// </summary>
        [JsonProperty("relayUrl")]
        public string RelayUrl { get; set; }
        
        /// <summary>
        /// The base context string to use for module isolation. If null or empty, then the string
        /// "{Name}-client" will be used
        /// </summary>
        [JsonProperty("context")]
        public string BaseContext { get; set; }
        
        /// <summary>
        /// The <see cref="IKeyValueStorage"/> module to use for storage. This module will be used by most Core modules
        /// for storing data and by the default <see cref="IKeyChain"/> module (if no <see cref="IKeyChain"/> module is provided).
        /// If this is set to null, then the default <see cref="FileSystemStorage"/> 
        /// </summary>
        [JsonProperty("storage")]
        public IKeyValueStorage Storage { get; set; }

        /// <summary>
        /// The <see cref="IKeyChain"/> module to use for the <see cref="ICrypto"/> module.
        /// If set to null, then the default <see cref="KeyChain"/> module will be used with the provided Storage module.
        /// </summary>
        [JsonProperty("keychain")]
        public IKeyChain KeyChain { get; set; }
        
    }
}
