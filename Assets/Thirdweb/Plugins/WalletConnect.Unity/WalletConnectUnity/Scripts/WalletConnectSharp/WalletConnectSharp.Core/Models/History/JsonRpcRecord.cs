using Newtonsoft.Json;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Models.History
{
    /// <summary>
    /// A class representing a single JSON RPC history record containing the Id, Topic, Request, Response and ChainId.
    /// If no Response is set, then the record hasn't been resolved yet
    /// </summary>
    /// <typeparam name="T">The type of the request parameter</typeparam>
    /// <typeparam name="R">The type of the response parameter</typeparam>
    public class JsonRpcRecord<T, R>
    {
        /// <summary>
        /// The id of the JSON RPC request
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }
        
        /// <summary>
        /// The topic the request was sent in
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }
        
        /// <summary>
        /// The request data for this JSON RPC record
        /// </summary>
        [JsonProperty("request")]
        public IRequestArguments<T> Request { get; set; }
        
        /// <summary>
        /// The chainId this request is intended for
        /// </summary>
        [JsonProperty("chainId")]
        public string ChainId { get; set; }

        /// <summary>
        /// The response data for this JSON RPC record. If no Response data is set, then this request is
        /// still pending
        /// </summary>
        [JsonProperty("response")]
        public IJsonRpcResult<R> Response;

        /// <summary>
        /// This constructor is required for the JSON deserializer to be able
        /// to identify concrete classes to use when deserializing the interface properties.
        /// </summary>
        public JsonRpcRecord(IJsonRpcRequest<T> request)
        {
            Request = request;
        }
    }
}
