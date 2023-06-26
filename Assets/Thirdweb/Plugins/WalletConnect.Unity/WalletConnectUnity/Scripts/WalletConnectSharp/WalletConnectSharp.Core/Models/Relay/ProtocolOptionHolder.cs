using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Relay
{
    /// <summary>
    /// An abstract class that simply holds ProtocolOptions under the Relay property
    /// </summary>
    public abstract class ProtocolOptionHolder
    {
        /// <summary>
        /// The relay protocol options to use for this event
        /// </summary>
        [JsonProperty("relay")]
        public ProtocolOptions Relay { get; set; }
    }
}
