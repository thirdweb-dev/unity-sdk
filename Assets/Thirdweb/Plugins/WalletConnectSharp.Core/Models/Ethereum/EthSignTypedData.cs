using Newtonsoft.Json;
using WalletConnectSharp.Core.Models.Ethereum.Types;

namespace WalletConnectSharp.Core.Models.Ethereum
{
    public sealed class EthSignTypedData<T> : EthGenericRequest<string>
    {
        public EthSignTypedData(string address, T data, EIP712Domain domain)
            : base(ValidJsonRpcRequestMethods.EthSignTypedData, address, JsonConvert.SerializeObject(new EvmTypedData<T>(data, domain))) { }
    }
}
