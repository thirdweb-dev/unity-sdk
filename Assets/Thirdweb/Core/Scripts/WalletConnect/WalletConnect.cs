using System.Threading.Tasks;
using Nethereum.JsonRpc.Client.RpcMessages;
using Newtonsoft.Json;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Network.Models;
using System.Collections.Generic;
using WalletConnectSharp.Common.Utils;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexTypes;
using UnityEngine;
using Thirdweb.Redcode.Awaiting;

namespace Thirdweb.WalletConnect
{
    public class WalletConnect
    {
        public string[] Accounts { get; private set; }
        public string ChainId { get; private set; }
        public WalletConnectSignClient Client { get; private set; }
        public string Topic { get; private set; }

        public WalletConnect(string[] Accounts, string chainId, WalletConnectSignClient client)
        {
            this.Accounts = Accounts;
            this.ChainId = chainId;
            this.Client = client;
            this.Topic = Client.Session.Get(Client.Session.Keys[0]).Topic;
        }

        internal async Task<RpcResponseMessage> Request(RpcRequestMessage message)
        {
            ThirdwebDebug.Log($"WalletConnect Request: {JsonConvert.SerializeObject(message)}");
            OpenWallet();
            switch (message.Method)
            {
                case "eth_sendTransaction":
                    var ethSendTransactionReqParams = JsonConvert.DeserializeObject<TransactionInput[]>(JsonConvert.SerializeObject(message.RawParameters));
                    var ethSendTransactionReq = new EthSendTransaction(
                        new Transaction()
                        {
                            From = ethSendTransactionReqParams[0].From,
                            To = ethSendTransactionReqParams[0].To,
                            Value = ethSendTransactionReqParams[0].Value?.HexValue ?? "0x",
                            Gas = ethSendTransactionReqParams[0].Gas?.HexValue ?? null,
                            GasPrice = ethSendTransactionReqParams[0].GasPrice?.HexValue ?? null,
                            Data = ethSendTransactionReqParams[0].Data ?? "0x"
                        }
                    );
                    var ethSendTransactionRes = await Client.Request<EthSendTransaction, string>(Topic, ethSendTransactionReq, ChainId);
                    return new RpcResponseMessage(message.Id, ethSendTransactionRes);
                case "personal_sign":
                    var personalSignReqParams = JsonConvert.DeserializeObject<string[]>(JsonConvert.SerializeObject(message.RawParameters));
                    var personalSignReq = new PersonalSign(new string[] { personalSignReqParams[0], personalSignReqParams[1] });
                    var personalSignRes = await Client.Request<PersonalSign, string>(Topic, personalSignReq, ChainId);
                    return new RpcResponseMessage(message.Id, personalSignRes);
                case "eth_signTypedData_v4":
                    var ethSignTypedDataReqParams = JsonConvert.DeserializeObject<string[]>(JsonConvert.SerializeObject(message.RawParameters));
                    var ethSignTypedDataReq = new EthSignTypedDataV4(new string[] { ethSignTypedDataReqParams[0], ethSignTypedDataReqParams[1] });
                    var ethSignTypedDataRes = await Client.Request<EthSignTypedDataV4, string>(Topic, ethSignTypedDataReq, ChainId);
                    return new RpcResponseMessage(message.Id, ethSignTypedDataRes);
                case "wallet_switchEthereumChain":
                    var walletSwitchEthereumChainReqParams = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(message.RawParameters));
                    var walletSwitchEthereumChainReq = new WalletSwitchEthereumChain(new object[] { walletSwitchEthereumChainReqParams[0] });
                    ThirdwebDebug.Log($"WalletSwitchEthereumChain: {JsonConvert.SerializeObject(walletSwitchEthereumChainReq)}");
                    var walletSwitchEthereumChainRes = await Client.Request<WalletSwitchEthereumChain, object>(Topic, walletSwitchEthereumChainReq, ChainId);
                    ThirdwebChain newChain = JsonConvert.DeserializeObject<ThirdwebChain>(JsonConvert.SerializeObject(walletSwitchEthereumChainReqParams[0]));
                    ChainId = ChainId.Substring(0, ChainId.IndexOf(":") + 1) + new HexBigInteger(newChain.chainId).Value;
                    return new RpcResponseMessage(message.Id, JsonConvert.SerializeObject(walletSwitchEthereumChainRes));
                case "wallet_addEthereumChain":
                    var walletAddEthereumChainReqParams = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(message.RawParameters));
                    var walletAddEthereumChainReq = new WalletAddEthereumChain(new object[] { walletAddEthereumChainReqParams[0] });
                    var walletAddEthereumChainRes = await Client.Request<WalletAddEthereumChain, object>(Topic, walletAddEthereumChainReq, ChainId);
                    return new RpcResponseMessage(message.Id, JsonConvert.SerializeObject(walletAddEthereumChainRes));
                default:
                    throw new System.Exception($"Method {message.Method} not implemented");
            }
        }

        private async void OpenWallet()
        {
            await new WaitForSecondsRealtime(0.5f);
            if (Application.isMobilePlatform && !Application.isEditor)
                UnityEngine.Application.OpenURL("wc://");
        }

        internal async Task Disconnect()
        {
            await Client.Disconnect(
                "User disconnected",
                new Error()
                {
                    Code = 0,
                    Message = "User disconnected",
                    Data = null
                }
            );
        }
    }

    public class Transaction
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("gas", NullValueHandling = NullValueHandling.Ignore)]
        public string Gas { get; set; }

        [JsonProperty("gasPrice", NullValueHandling = NullValueHandling.Ignore)]
        public string GasPrice { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; } = "0x";
    }

    [RpcMethod("eth_sendTransaction"), RpcRequestOptions(Clock.ONE_MINUTE, 99999)]
    public class EthSendTransaction : List<Transaction>
    {
        public EthSendTransaction(params Transaction[] transactions)
            : base(transactions) { }
    }

    [RpcMethod("personal_sign"), RpcRequestOptions(Clock.ONE_MINUTE, 99999)]
    public class PersonalSign : List<string>
    {
        public PersonalSign(params string[] personalSignParams)
            : base(personalSignParams) { }
    }

    [RpcMethod("eth_signTypedData_v4"), RpcRequestOptions(Clock.ONE_MINUTE, 99999)]
    public class EthSignTypedDataV4 : List<string>
    {
        public EthSignTypedDataV4(params string[] ethSignTypedDataParams)
            : base(ethSignTypedDataParams) { }
    }

    [RpcMethod("wallet_switchEthereumChain"), RpcRequestOptions(Clock.ONE_MINUTE, 99999)]
    public class WalletSwitchEthereumChain : List<object>
    {
        public WalletSwitchEthereumChain(params object[] walletSwitchEthereumChainParams)
            : base(walletSwitchEthereumChainParams) { }
    }

    [RpcMethod("wallet_addEthereumChain"), RpcRequestOptions(Clock.ONE_MINUTE, 99999)]
    public class WalletAddEthereumChain : List<object>
    {
        public WalletAddEthereumChain(params object[] walletAddEthereumChainParams)
            : base(walletAddEthereumChainParams) { }
    }
}
