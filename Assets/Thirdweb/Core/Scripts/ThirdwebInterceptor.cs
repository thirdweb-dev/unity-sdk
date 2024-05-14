using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Newtonsoft.Json;
using Thirdweb.Wallets;
using UnityEngine.Scripting;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectUnity.Core;

namespace Thirdweb
{
    public class ThirdwebInterceptor : RequestInterceptor
    {
        private readonly IThirdwebWallet _thirdwebWallet;

        public ThirdwebInterceptor(IThirdwebWallet thirdwebWallet)
        {
            _thirdwebWallet = thirdwebWallet;
        }

        public override async Task<object> InterceptSendRequestAsync<T>(Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync, RpcRequest request, string route = null)
        {
            if (request.Method == "eth_chainId")
            {
                switch (_thirdwebWallet.GetProvider())
                {
                    case WalletProvider.Metamask:
                        return new HexBigInteger((BigInteger)MetaMask.Unity.MetaMaskUnity.Instance.Wallet.ChainId).HexValue;
                    default:
                        break;
                }
            }
            else if (request.Method == "eth_accounts")
            {
                var addy = await _thirdwebWallet.GetAddress();
                return new string[] { addy };
            }
            else if (request.Method == "personal_sign")
            {
                var signerWeb3 = await _thirdwebWallet.GetSignerWeb3();

                switch (_thirdwebWallet.GetProvider())
                {
                    case WalletProvider.WalletConnect:
                        var msg = request.RawParameters[0].ToString();
                        var acc = request.RawParameters[1].ToString();
                        var personalSign = new PersonalSign(msg, acc);
                        return await WalletConnect.Instance.RequestAsync<PersonalSign, string>(personalSign);
                    case WalletProvider.LocalWallet:
                    case WalletProvider.InAppWallet:
                        var message = request.RawParameters[0].ToString();
                        return new EthereumMessageSigner().EncodeUTF8AndSign(message, new EthECKey(_thirdwebWallet.GetLocalAccount().PrivateKey));
                    case WalletProvider.SmartWallet:
                        return await signerWeb3.Client.SendRequestAsync<T>("personal_sign", null, request.RawParameters);
                    default:
                        break;
                }
            }
            else if (request.Method == "eth_signTypedData_v4")
            {
                var signerWeb3 = await _thirdwebWallet.GetSignerWeb3();

                switch (_thirdwebWallet.GetProvider())
                {
                    case WalletProvider.WalletConnect:
                        var data = request.RawParameters[1].ToString();
                        var account = request.RawParameters[0].ToString();
                        var ethSignTypedDataV4 = new EthSignTypedDataV4(account, data);
                        return await WalletConnect.Instance.RequestAsync<EthSignTypedDataV4, string>(ethSignTypedDataV4);
                    case WalletProvider.LocalWallet:
                    case WalletProvider.InAppWallet:
                        throw new Exception("Please use Wallet.SignTypedDataV4 instead.");
                    case WalletProvider.SmartWallet:
                        return await signerWeb3.Client.SendRequestAsync<T>("eth_signTypedData_v4", null, request.RawParameters);
                    default:
                        break;
                }
            }
            else if (request.Method == "eth_sendTransaction")
            {
                switch (_thirdwebWallet.GetProvider())
                {
                    case WalletProvider.WalletConnect:
                        var txParams = JsonConvert.SerializeObject(request.RawParameters[0]);
                        var callInput = JsonConvert.DeserializeObject<TransactionInput>(txParams);
                        var ethSendTransaction = new EthSendTransaction(
                            new Transaction
                            {
                                From = callInput.From,
                                To = callInput.To,
                                Gas = callInput.Gas?.HexValue,
                                GasPrice = callInput.GasPrice?.HexValue,
                                Value = callInput.Value?.HexValue,
                                Data = callInput.Data,
                            }
                        );
                        ThirdwebDebug.Log($"Sending transaction: {JsonConvert.SerializeObject(ethSendTransaction)}");
                        var result = await WalletConnect.Instance.RequestAsync<EthSendTransaction, string>(ethSendTransaction);
                        ThirdwebDebug.Log($"Transaction success! TxHash: {result}");
                        return result;
                    default:
                        break;
                }
            }

            return await interceptedSendRequestAsync(request, route);
        }

        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<string, string, object[], Task<T>> interceptedSendRequestAsync,
            string method,
            string route = null,
            params object[] paramList
        )
        {
            if (method == "eth_chainId")
            {
                switch (_thirdwebWallet.GetProvider())
                {
                    case WalletProvider.Metamask:
                        return new HexBigInteger((BigInteger)MetaMask.Unity.MetaMaskUnity.Instance.Wallet.ChainId).HexValue;
                    default:
                        break;
                }
            }
            else if (method == "eth_accounts")
            {
                var addy = await _thirdwebWallet.GetAddress();
                return new string[] { addy };
            }
            else if (method == "personal_sign")
            {
                var signerWeb3 = await _thirdwebWallet.GetSignerWeb3();

                switch (_thirdwebWallet.GetProvider())
                {
                    case WalletProvider.WalletConnect:
                        var msg = paramList[0].ToString();
                        var acc = paramList[1].ToString();
                        var personalSign = new PersonalSign(msg, acc);
                        return await WalletConnect.Instance.RequestAsync<PersonalSign, string>(personalSign);
                    case WalletProvider.LocalWallet:
                    case WalletProvider.InAppWallet:
                        var message = paramList[0].ToString();
                        return new EthereumMessageSigner().EncodeUTF8AndSign(message, new EthECKey(_thirdwebWallet.GetLocalAccount().PrivateKey));
                    case WalletProvider.SmartWallet:
                        return await signerWeb3.Client.SendRequestAsync<T>("personal_sign", null, paramList);
                    default:
                        break;
                }
            }
            else if (method == "eth_signTypedData_v4")
            {
                var signerWeb3 = await _thirdwebWallet.GetSignerWeb3();

                switch (_thirdwebWallet.GetProvider())
                {
                    case WalletProvider.WalletConnect:
                        var msg = paramList[0].ToString();
                        var acc = paramList[1].ToString();
                        var ethSignTypedDataV4 = new EthSignTypedDataV4(acc, msg);
                        return await WalletConnect.Instance.RequestAsync<EthSignTypedDataV4, string>(ethSignTypedDataV4);
                    case WalletProvider.LocalWallet:
                    case WalletProvider.InAppWallet:
                        throw new Exception("Please use Wallet.SignTypedDataV4 instead.");
                    case WalletProvider.SmartWallet:
                        return await signerWeb3.Client.SendRequestAsync<T>("eth_signTypedData_v4", null, paramList);
                    default:
                        break;
                }
            }
            else if (method == "eth_sendTransaction")
            {
                switch (_thirdwebWallet.GetProvider())
                {
                    case WalletProvider.WalletConnect:
                        var txParams = JsonConvert.SerializeObject(paramList[0]);
                        var callInput = JsonConvert.DeserializeObject<TransactionInput>(txParams);
                        var ethSendTransaction = new EthSendTransaction(
                            new Transaction
                            {
                                From = callInput.From,
                                To = callInput.To,
                                Gas = callInput.Gas?.HexValue,
                                GasPrice = callInput.GasPrice?.HexValue,
                                Value = callInput.Value?.HexValue,
                                Data = callInput.Data,
                            }
                        );
                        ThirdwebDebug.Log($"Sending transaction: {JsonConvert.SerializeObject(ethSendTransaction)}");
                        var result = await WalletConnect.Instance.RequestAsync<EthSendTransaction, string>(ethSendTransaction);
                        ThirdwebDebug.Log($"Transaction success! TxHash: {result}");
                        return result;
                    default:
                        break;
                }
            }

            return await interceptedSendRequestAsync(method, route, paramList);
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

        [RpcMethod("eth_sendTransaction"), RpcRequestOptions(Clock.ONE_MINUTE, 99997)]
        public class EthSendTransaction : List<Transaction>
        {
            public EthSendTransaction(params Transaction[] transactions)
                : base(transactions) { }

            [Preserve]
            public EthSendTransaction() { }
        }

        [RpcMethod("personal_sign")]
        [RpcRequestOptions(Clock.ONE_MINUTE, 99998)]
        public class PersonalSign : List<string>
        {
            public PersonalSign(string hexUtf8, string account)
                : base(new[] { hexUtf8, account }) { }

            [Preserve]
            public PersonalSign() { }
        }

        [RpcMethod("eth_signTypedData_v4")]
        [RpcRequestOptions(Clock.ONE_MINUTE, 99999)]
        public class EthSignTypedDataV4 : List<string>
        {
            public EthSignTypedDataV4(string account, string data)
                : base(new[] { account, data }) { }

            [Preserve]
            public EthSignTypedDataV4() { }
        }
    }
}
