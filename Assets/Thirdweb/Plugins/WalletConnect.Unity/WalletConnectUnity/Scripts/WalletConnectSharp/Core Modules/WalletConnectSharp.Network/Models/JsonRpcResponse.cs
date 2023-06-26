using Newtonsoft.Json;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// Represents a full JSON RPC response with the given result type of T
    /// </summary>
    /// <typeparam name="T">The type of the result property for this JSON RPC response</typeparam>
    public class JsonRpcResponse<T> : IJsonRpcResult<T>
    {
        /// <summary>
        /// The id of this Json rpc response, should match the original request
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }
        
        /// <summary>
        /// The jsonrpc field
        /// </summary>
        [JsonProperty("jsonrpc")]
        public string JsonRPC
        {
            get
            {
                return "2.0";
            }
        }
        
        /// <summary>
        /// The error field for this response, if one is present
        /// </summary>
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public Error Error { get; set; }
        
        /// <summary>
        /// The result field for this response, if one is present
        /// </summary>
        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public T Result { get; set; }

        /// <summary>
        /// Whether or not this response is an error response
        /// </summary>
        [JsonIgnore]
        public bool IsError
        {
            get
            {
                return Error != null;
            }
        }

        /// <summary>
        /// Create a blank Json rpc response
        /// </summary>
        public JsonRpcResponse()
        {
        }

        /// <summary>
        /// Create a new Json rpc response with the given id, error and result
        /// parameters.
        /// </summary>
        /// <param name="id">The id of this json response</param>
        /// <param name="error">The error of this json response, if one is present</param>
        /// <param name="result">The result of this json response, if one is present</param>
        public JsonRpcResponse(long id, Error error, T result)
        {
            Id = id;
            Error = error;
            Result = result;
        }
    }
}
