using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models
{
    public class WcSessionRequestRequest : JsonRpcRequest
    {
        [JsonProperty("params")]
        public WcSessionRequestRequestParams[] parameters;

        public WcSessionRequestRequest(ClientMeta clientMeta, string clientId, int chainId = 1)
            : base(WalletConnectStates.SessionRequest)
        {
            this.parameters = new[]
            {
                new WcSessionRequestRequestParams()
                {
                    peerId = clientId,
                    chainId = chainId,
                    peerMeta = clientMeta
                }
            };
        }

        public class WcSessionRequestRequestParams
        {
            public string peerId;
            public ClientMeta peerMeta;
            public int chainId;
        }
    }
}
