using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models
{
    public class WCSessionUpdate : JsonRpcRequest
    {
        [JsonProperty("params")]
        public WCSessionData[] parameters;

        public WCSessionUpdate(WCSessionData data)
            : base(WalletConnectStates.SessionUpdate)
        {
            this.parameters = new[] { data };
        }
    }
}
