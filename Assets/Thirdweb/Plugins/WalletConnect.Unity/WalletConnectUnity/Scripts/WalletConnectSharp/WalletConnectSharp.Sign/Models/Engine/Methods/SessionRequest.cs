using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    /// <summary>
    /// A class that represents the request wc_sessionRequest. Used to send a generic JSON RPC request to the
    /// peer in this session.
    /// </summary>
    [RpcMethod("wc_sessionRequest")]
    [RpcRequestOptions(Clock.ONE_DAY, 1108)]
    [RpcResponseOptions(Clock.ONE_DAY, 1109)]
    public class SessionRequest<T> : IWcMethod
    {
        /// <summary>
        /// The chainId this request should be performed in
        /// </summary>
        [JsonProperty("chainId")]
        public string ChainId { get; set; }
        
        /// <summary>
        /// The JSON RPC request to send to the peer
        /// </summary>
        [JsonProperty("request")]
        public JsonRpcRequest<T> Request { get; set; }
    }
}
