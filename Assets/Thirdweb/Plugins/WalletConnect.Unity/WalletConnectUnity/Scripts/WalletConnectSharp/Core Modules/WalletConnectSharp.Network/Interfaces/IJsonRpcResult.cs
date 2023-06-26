using Newtonsoft.Json;

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// An interface that represents a JSON RPC response to a JSON RPC request with the given result
    /// type of T. This interface also includes an error field if the JSON RPC response is an error
    /// </summary>
    /// <typeparam name="T">The type of the response field in this JSON RPC response</typeparam>
    public interface IJsonRpcResult<T> : IJsonRpcError
    {
        /// <summary>
        /// The result data of the response to the request
        /// </summary>
        [JsonProperty("result")]
        T Result { get; }
    }
}