using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.HostWallet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Thirdweb;

namespace WalletConnectUnity.Nethereum
{
    public class WalletConnectInterceptor : RequestInterceptor
    {
        private readonly WalletConnectService _walletConnectService;

        private readonly HashSet<string> _signMethods =
            new()
            {
                ApiMethods.eth_sendTransaction.ToString(),
                ApiMethods.personal_sign.ToString(),
                ApiMethods.eth_signTypedData_v4.ToString(),
                ApiMethods.wallet_switchEthereumChain.ToString(),
                ApiMethods.wallet_addEthereumChain.ToString()
            };

        public WalletConnectInterceptor(WalletConnectService walletConnectService)
        {
            _walletConnectService = walletConnectService;
        }

        public override async Task<object> InterceptSendRequestAsync<T>(Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync, RpcRequest request, string route = null)
        {
            ThirdwebDebug.Log($"[WalletConnectInterceptor] InterceptSendRequestAsync: {request.Method}");
            if (!_signMethods.Contains(request.Method))
            {
                return await base.InterceptSendRequestAsync(interceptedSendRequestAsync, request, route).ConfigureAwait(false);
            }

            if (!_walletConnectService.IsWalletConnected)
                throw new InvalidOperationException("[WalletConnectInterceptor] Wallet is not connected");

            if (_walletConnectService.IsMethodSupported(request.Method))
            {
                if (request.Method == ApiMethods.eth_sendTransaction.ToString())
                {
                    return await _walletConnectService.SendTransactionAsync((TransactionInput)request.RawParameters[0]);
                }

                if (request.Method == ApiMethods.personal_sign.ToString())
                {
                    return await _walletConnectService.PersonalSignAsync((string)request.RawParameters[0]);
                }

                if (request.Method == ApiMethods.eth_signTypedData_v4.ToString())
                {
                    return await _walletConnectService.EthSignTypedDataV4Async((string)request.RawParameters[1]);
                }

                if (request.Method == ApiMethods.wallet_switchEthereumChain.ToString())
                {
                    ThirdwebDebug.Log($"[WalletConnectInterceptor] wallet_switchEthereumChain");
                    var param = JsonConvert.DeserializeObject<ThirdwebChain>(JsonConvert.SerializeObject(request.RawParameters[0]));
                    ThirdwebDebug.Log($"[WalletConnectInterceptor] chainId: {param.chainId}");
                    var res = await _walletConnectService.WalletSwitchEthereumChainAsync(new SwitchEthereumChainParameter() { ChainId = new HexBigInteger(param.chainId), });
                    ThirdwebDebug.Log($"[WalletConnectInterceptor] wallet_switchEthereumChain res: {res}");
                    return res;
                }
                if (request.Method == ApiMethods.wallet_addEthereumChain.ToString())
                {
                    var param = JsonConvert.DeserializeObject<ThirdwebChainData>(JsonConvert.SerializeObject(request.RawParameters[0]));
                    return await _walletConnectService.WalletAddEthereumChainAsync(
                        new AddEthereumChainParameter()
                        {
                            ChainId = new HexBigInteger(param.chainId),
                            BlockExplorerUrls = param.blockExplorerUrls.ToList(),
                            ChainName = param.chainName,
                            IconUrls = param.iconUrls.ToList(),
                            NativeCurrency = new NativeCurrency()
                            {
                                Name = param.nativeCurrency.name,
                                Symbol = param.nativeCurrency.symbol,
                                Decimals = (uint)param.nativeCurrency.decimals
                            },
                            RpcUrls = param.rpcUrls.ToList()
                        }
                    );
                }

                throw new NotImplementedException();
            }

            return await base.InterceptSendRequestAsync(interceptedSendRequestAsync, request, route).ConfigureAwait(false);
        }

        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<string, string, object[], Task<T>> interceptedSendRequestAsync,
            string method,
            string route = null,
            params object[] paramList
        )
        {
            ThirdwebDebug.Log($"[WalletConnectInterceptor] InterceptSendRequestAsync: {method}");

            if (!_signMethods.Contains(method))
            {
                return await base.InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList).ConfigureAwait(false);
            }

            if (!_walletConnectService.IsWalletConnected)
                throw new InvalidOperationException("[WalletConnectInterceptor] Wallet is not connected");

            if (_walletConnectService.IsMethodSupported(method))
            {
                if (method == ApiMethods.eth_sendTransaction.ToString())
                {
                    return await _walletConnectService.SendTransactionAsync((TransactionInput)paramList[0]);
                }

                if (method == ApiMethods.personal_sign.ToString())
                {
                    return await _walletConnectService.PersonalSignAsync((string)paramList[0]);
                }

                if (method == ApiMethods.eth_signTypedData_v4.ToString())
                {
                    return await _walletConnectService.EthSignTypedDataV4Async((string)paramList[1]);
                }

                if (method == ApiMethods.wallet_switchEthereumChain.ToString())
                {
                    var param = JsonConvert.DeserializeObject<ThirdwebChain>(JsonConvert.SerializeObject(paramList[0]));
                    return await _walletConnectService.WalletSwitchEthereumChainAsync(new SwitchEthereumChainParameter() { ChainId = new HexBigInteger(param.chainId), });
                }

                if (method == ApiMethods.wallet_addEthereumChain.ToString())
                {
                    var param = JsonConvert.DeserializeObject<ThirdwebChainData>(JsonConvert.SerializeObject(paramList[0]));
                    return await _walletConnectService.WalletAddEthereumChainAsync(
                        new AddEthereumChainParameter()
                        {
                            ChainId = new HexBigInteger(param.chainId),
                            BlockExplorerUrls = param.blockExplorerUrls.ToList(),
                            ChainName = param.chainName,
                            IconUrls = param.iconUrls.ToList(),
                            NativeCurrency = new NativeCurrency()
                            {
                                Name = param.nativeCurrency.name,
                                Symbol = param.nativeCurrency.symbol,
                                Decimals = (uint)param.nativeCurrency.decimals
                            },
                            RpcUrls = param.rpcUrls.ToList()
                        }
                    );
                }

                throw new NotImplementedException();
            }

            return await base.InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList).ConfigureAwait(false);
        }
    }
}
