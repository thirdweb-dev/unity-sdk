using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.Core.Models
{
    public class JsonRpcPayload : IEventSource
    {
        [JsonProperty(Order = 1)]
        protected long id;

        [JsonProperty(Order = 2)]
        protected string jsonrpc = "2.0";

        [JsonIgnore]
        public long ID => id;

        [JsonIgnore]
        public string JsonRPC => jsonrpc;

        [JsonExtensionData]
        private IDictionary<string, JToken> _extraStuff;

        /// <summary>
        /// Whether this payload represents a request
        /// </summary>
        [JsonIgnore]
        public bool IsRequest
        {
            get { return _extraStuff.ContainsKey("method"); }
        }

        /// <summary>
        /// Whether this payload represents a response
        /// </summary>
        [JsonIgnore]
        public bool IsResponse
        {
            get { return _extraStuff.ContainsKey("result"); }
        }

        /// <summary>
        /// Whether this payload represents an error
        /// </summary>
        [JsonIgnore]
        public bool IsError
        {
            get { return _extraStuff.ContainsKey("error"); }
        }

        [JsonIgnore]
        public string Event
        {
            get
            {
                if (IsRequest)
                    return _extraStuff["method"].ToObject<string>();
                if (IsResponse)
                    return $"response:{id}";

                return null;
            }
        }
    }
}
