using Newtonsoft.Json;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;

namespace WalletConnectSharp.Sign.Models.Engine.Methods
{
    /// <summary>
    /// A class that represents the request wc_sessionUpdate. Used to update the <see cref="Namespaces"/> enabled
    /// in this session
    /// </summary>
    [RpcMethod("wc_sessionUpdate")]
    [RpcRequestOptions(Clock.ONE_DAY, 1104)]
    [RpcResponseOptions(Clock.ONE_DAY, 1105)]
    public class SessionUpdate : IWcMethod
    {
        /// <summary>
        /// The updated namespaces that are enabled for this session
        /// </summary>
        [JsonProperty("namespaces")]
        public Namespaces Namespaces { get; set; }
    }
}
