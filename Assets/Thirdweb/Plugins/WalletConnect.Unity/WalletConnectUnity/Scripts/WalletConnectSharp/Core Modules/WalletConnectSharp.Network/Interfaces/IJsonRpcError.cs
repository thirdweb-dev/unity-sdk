using Newtonsoft.Json;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// A JSON RPC response that may include an error
    /// </summary>
    public interface IJsonRpcError : IJsonRpcPayload
    {
        /// <summary>
        /// The error for this JSON RPC response or null if no error is present
        /// </summary>
        [JsonProperty("error")]
        Error Error { get; }
    }
}