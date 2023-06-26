using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    /// <summary>
    /// A class that represents the request wc_sessionSettle. Used to approve and settle a proposed session.
    /// </summary>
    [RpcMethod("wc_sessionSettle")]
    [RpcRequestOptions(Clock.FIVE_MINUTES, 1102)]
    [RpcResponseOptions(Clock.FIVE_MINUTES, 1103)]
    public class SessionSettle : IWcMethod
    {
        /// <summary>
        /// The protocol options that should be used in this session
        /// </summary>
        [JsonProperty("relay")]
        public ProtocolOptions Relay { get; set; }
        
        /// <summary>
        /// All namespaces that are enabled in this session
        /// </summary>
        [JsonProperty("namespaces")]
        public Namespaces Namespaces { get; set; }
        
        /// <summary>
        /// When this session will expire
        /// </summary>
        [JsonProperty("expiry")]
        public long Expiry { get; set; }
        
        /// <summary>
        /// The controlling <see cref="Participant"/> in this session. In most cases, this is the dApp.
        /// </summary>
        [JsonProperty("controller")]
        public Participant Controller { get; set; }
    }
}
