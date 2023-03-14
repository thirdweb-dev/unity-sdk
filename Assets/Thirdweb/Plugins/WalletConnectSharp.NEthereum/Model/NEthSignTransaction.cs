using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Models;

namespace WalletConnectSharp.NEthereum.Model
{
    public class NEthSignTransaction : JsonRpcRequest
    {
        [JsonProperty("params")]
        private TransactionInput[] _parameters;

        [JsonIgnore]
        public TransactionInput[] Parameters => _parameters;

        public NEthSignTransaction(params TransactionInput[] transactionDatas)
            : base(ValidJsonRpcRequestMethods.EthSignTransaction)
        {
            this._parameters = transactionDatas;
        }
    }
}
