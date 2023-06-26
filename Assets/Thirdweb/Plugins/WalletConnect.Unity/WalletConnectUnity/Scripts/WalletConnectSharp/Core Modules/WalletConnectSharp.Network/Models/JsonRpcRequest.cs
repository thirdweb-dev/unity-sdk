using Newtonsoft.Json;
using WalletConnectSharp.Events.Utils;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// Represents a full JSON RPC request with the given parameter type T
    /// </summary>
    /// <typeparam name="T">The parameter type for this JSON RPC request</typeparam>
    public class JsonRpcRequest<T> : IJsonRpcRequest<T>
    {
        /// <summary>
        /// The method of this Json rpc request
        /// </summary>
        public string Method { get; set; }
        
        /// <summary>
        /// The parameters of this Json rpc request
        /// </summary>
        public T Params { get; set; }
        
        /// <summary>
        /// The id of this Json rpc request
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The jsonrpc field
        /// </summary>
        public string JsonRPC
        {
            get
            {
                return "2.0";
            }
        }

        /// <summary>
        /// Create a blank Json rpc request
        /// </summary>
        public JsonRpcRequest()
        {
        }

        /// <summary>
        /// Create a Json rpc request with the given method and parameter. You may
        /// optionally provide an id, if no id is given then a random id is generated.
        /// </summary>
        /// <param name="method">The method of this new Json rpc request</param>
        /// <param name="param">The parameter of this new Json rpc request</param>
        /// <param name="id">The id of this new Json rpc request</param>
        public JsonRpcRequest(string method, T param, long? id = null)
        {
            if (id == null)
            {
                id = RpcPayloadId.Generate();
            }

            this.Method = method;
            this.Params = param;
            this.Id = (long)id;
        }
    }
}
