using System.Collections.Generic;
using UnityEngine.Scripting;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;

namespace WalletConnectUnity.Core.Evm
{
    [RpcMethod("personal_sign")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99998)]
    public class PersonalSign : List<string>
    {
        public PersonalSign(string hexUtf8, string account) : base(new[] { hexUtf8, account })
        {
        }

        [Preserve]
        public PersonalSign()
        {
        }
    }

    [RpcMethod("eth_sendTransaction")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99997)]
    public class EthSendTransaction : List<Transaction>
    {
        public EthSendTransaction(params Transaction[] transactions) : base(transactions)
        {
        }

        [Preserve]
        public EthSendTransaction()
        {
        }
    }

    [RpcMethod("wallet_switchEthereumChain")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99990)]
    public class WalletSwitchEthereumChain : List<object>
    {
        public WalletSwitchEthereumChain(string chainId) : base(new[] { new { chainId } })
        {
        }

        [Preserve]
        public WalletSwitchEthereumChain()
        {
        }
    }

    [RpcMethod("wallet_addEthereumChain")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99990)]
    public class WalletAddEthereumChain : List<object>
    {
        public WalletAddEthereumChain(EthereumChain chain) : base(new[] { chain })
        {
        }

        [Preserve]
        public WalletAddEthereumChain()
        {
        }
    }

    [RpcMethod("eth_signTypedData_v4")]
    [RpcRequestOptions(Clock.ONE_MINUTE, 99999)]
    public class EthSignTypedDataV4 : List<string>
    {
        public EthSignTypedDataV4(string account, string data) : base(new[] { account, data })
        {
        }

        [Preserve]
        public EthSignTypedDataV4()
        {
        }
    }
}