using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Ethereum
{
    public class EthGenericRequest<T> : JsonRpcRequest
    {
        [JsonProperty("params", Order = 4)]
        private readonly T[] _parameters;

        [JsonIgnore]
        public T[] Parameters => _parameters;

        public EthGenericRequest(string jsonRpcMethodName, params T[] data)
            : base(jsonRpcMethodName)
        {
            _parameters = data;
        }
    }
}
