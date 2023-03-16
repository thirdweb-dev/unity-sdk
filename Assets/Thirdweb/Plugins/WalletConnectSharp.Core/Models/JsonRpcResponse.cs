using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models
{
    public class JsonRpcResponse : JsonRpcPayload, IEventSource
    {
        [JsonProperty]
        private JsonRpcError error;

        [JsonIgnore]
        public JsonRpcError Error
        {
            get { return error; }
        }

        [JsonIgnore]
        public bool IsError
        {
            get { return error != null; }
        }

        public class JsonRpcError
        {
            [JsonProperty]
            private int? code;

            [JsonProperty]
            private string message;

            [JsonIgnore]
            public int? Code
            {
                get { return code; }
            }

            [JsonIgnore]
            public string Message
            {
                get { return message; }
            }
        }

        [JsonIgnore]
        public string Event
        {
            get { return "response:" + ID; }
        }
    }
}
