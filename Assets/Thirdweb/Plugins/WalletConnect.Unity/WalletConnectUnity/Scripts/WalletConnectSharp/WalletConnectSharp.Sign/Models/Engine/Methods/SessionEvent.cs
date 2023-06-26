using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models.Engine.Events;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    /// <summary>
    /// A class that represents the request wc_sessionEvent. Used to emit a generic
    /// event
    /// </summary>
    [RpcMethod("wc_sessionEvent")]
    [RpcRequestOptions(Clock.FIVE_MINUTES, 1110)]
    [RpcResponseOptions(Clock.FIVE_MINUTES, 1111)]
    public class SessionEvent<T> : IWcMethod
    {
        /// <summary>
        /// The chainId this event took place in
        /// </summary>
        [JsonProperty("chainId")]
        public string ChainId { get; set; }
        
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        /// <summary>
        /// The event data
        /// </summary>
        [JsonProperty("event")]
        public EventData<T> Event { get; set; }
    }
}
