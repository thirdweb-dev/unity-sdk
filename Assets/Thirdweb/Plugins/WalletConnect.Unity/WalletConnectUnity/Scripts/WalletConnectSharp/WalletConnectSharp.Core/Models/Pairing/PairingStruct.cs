using Newtonsoft.Json;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Relay;

namespace WalletConnectSharp.Core.Models.Pairing
{
    /// <summary>
    /// A struct that stores pairing data, including the topic the pairing took place, whether
    /// the pairing is active or not, when the pairing expires and who the pairing was with
    /// </summary>
    public struct PairingStruct : IKeyHolder<string>
    {
        /// <summary>
        /// The topic the pairing took place in
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }

        /// <summary>
        /// This is the key field, mapped to the Topic. Implemented for <see cref="IKeyHolder{TKey}"/>
        /// so this struct can be stored using <see cref="IStore{TKey,TValue}"/>
        /// </summary>
        [JsonIgnore]
        public string Key
        {
            get
            {
                return Topic;
            }
        }
        
        /// <summary>
        /// When this pairing expires
        /// </summary>
        [JsonProperty("expiry")]
        public long? Expiry { get; set; }
        
        /// <summary>
        /// Relay protocol options for this pairing
        /// </summary>
        [JsonProperty("relay")]
        public ProtocolOptions Relay { get; set; }
        
        /// <summary>
        /// Whether this pairing is active or not
        /// </summary>
        [JsonProperty("active")]
        public bool? Active { get; set; }
        
        /// <summary>
        /// The metadata of the peer this pairing is with
        /// </summary>
        [JsonProperty("peerMetadata")]
        public Metadata PeerMetadata { get; set; }
    }
}
