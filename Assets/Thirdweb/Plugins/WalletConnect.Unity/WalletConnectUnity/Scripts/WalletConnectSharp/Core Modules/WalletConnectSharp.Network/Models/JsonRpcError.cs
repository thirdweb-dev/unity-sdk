using Newtonsoft.Json;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// A Json RPC response containing an error
    /// </summary>
    public class JsonRpcError : IJsonRpcError
    {
        /// <summary>
        /// The id field
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
        /// The error field 
        /// </summary>
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public Error Error { get; set; }

        /// <summary>
        /// Create a blank JSON rpc error response
        /// </summary>
        public JsonRpcError()
        {
        }

        /// <summary>
        /// Create a JSON rpc error response with the given id and ErrorResponse
        /// </summary>
        /// <param name="id">The id of the response</param>
        /// <param name="error">The error value</param>
        public JsonRpcError(long id, Error error)
        {
            Id = id;
            Error = error;
        }
    }
}
