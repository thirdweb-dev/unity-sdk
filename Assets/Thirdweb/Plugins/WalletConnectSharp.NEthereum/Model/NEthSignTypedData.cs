using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.EIP712;
using Nethereum.Signer.EIP712;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Core.Models.Ethereum.Types;

namespace WalletConnectSharp.NEthereum.Model
{
    public sealed class NEthSignTypedData<T, TDomain> : EthGenericRequest<string>
        where TDomain : IDomain
    {
        public NEthSignTypedData(string address, T data, TypedData<TDomain> typedData)
            : base(ValidJsonRpcRequestMethods.EthSignTypedData, address, typedData.ToJson(data)) { }
    }
}
