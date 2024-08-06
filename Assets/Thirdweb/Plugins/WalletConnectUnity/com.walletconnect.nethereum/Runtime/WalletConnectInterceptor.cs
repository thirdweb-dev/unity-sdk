using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.HostWallet;

namespace WalletConnectUnity.Nethereum
{
    public class WalletConnectInterceptor : RequestInterceptor
    {
        private readonly WalletConnectService _walletConnectService;

        private readonly HashSet<string> _signMethods = new()
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

        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync,
            RpcRequest request,
            string route = null)
        {
            if (!_signMethods.Contains(request.Method))
            {
                return await base
                    .InterceptSendRequestAsync(interceptedSendRequestAsync, request, route)
                    .ConfigureAwait(false);
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
                    // If parameter has only one element, it's a json data.
                    // Otherwise, expect the data to be at index 1
                    var dataIndex = request.RawParameters.Length > 1 ? 1 : 0;
                    
                    return await _walletConnectService.EthSignTypedDataV4Async((string)request.RawParameters[dataIndex]);
                }

                if (request.Method == ApiMethods.wallet_switchEthereumChain.ToString())
                {
                    return await _walletConnectService.WalletSwitchEthereumChainAsync((SwitchEthereumChainParameter)request.RawParameters[0]);
                }

                if (request.Method == ApiMethods.wallet_addEthereumChain.ToString())
                {
                    return await _walletConnectService.WalletAddEthereumChainAsync((AddEthereumChainParameter)request.RawParameters[0]);
                }

                throw new NotImplementedException();
            }

            return await base
                .InterceptSendRequestAsync(interceptedSendRequestAsync, request, route)
                .ConfigureAwait(false);
        }

        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<string, string, object[], Task<T>> interceptedSendRequestAsync,
            string method,
            string route = null,
            params object[] paramList)
        {
            if (!_signMethods.Contains(method))
            {
                return await base
                    .InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList)
                    .ConfigureAwait(false);
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
                    return await _walletConnectService.EthSignTypedDataV4Async((string)paramList[0]);
                }

                if (method == ApiMethods.wallet_switchEthereumChain.ToString())
                {
                    return await _walletConnectService.WalletSwitchEthereumChainAsync((SwitchEthereumChainParameter)paramList[0]);
                }

                if (method == ApiMethods.wallet_addEthereumChain.ToString())
                {
                    return await _walletConnectService.WalletAddEthereumChainAsync((AddEthereumChainParameter)paramList[0]);
                }

                throw new NotImplementedException();
            }

            return await base
                .InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList)
                .ConfigureAwait(false);
        }
    }
}