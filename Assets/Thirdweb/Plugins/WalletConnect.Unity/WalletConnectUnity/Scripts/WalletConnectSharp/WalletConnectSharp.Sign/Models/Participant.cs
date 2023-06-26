using Newtonsoft.Json;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Pairing;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// A class that represents a participant. This store the public key and additional metadata
    /// about the participant
    /// </summary>
    public class Participant
    {
        /// <summary>
        /// The public key of this participant, encoded as a hex string
        /// </summary>
        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }
        
        /// <summary>
        /// The metadata for this participant
        /// </summary>
        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }
    }
}
