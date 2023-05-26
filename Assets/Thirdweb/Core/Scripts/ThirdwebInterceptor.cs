using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetaMask.Unity;
using Nethereum.JsonRpc.Client;
using Newtonsoft.Json;

namespace Thirdweb
{
    public class ThirdwebInterceptor : RequestInterceptor
    {
        private readonly ThirdwebSession _thirdwebSession;

        public ThirdwebInterceptor(ThirdwebSession thirdwebSession)
        {
            _thirdwebSession = thirdwebSession;
        }

        public override async Task<object> InterceptSendRequestAsync<T>(Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync, RpcRequest request, string route = null)
        {
            if (request.Method == "eth_accounts" && _thirdwebSession.IsConnected)
            {
                return await _thirdwebSession.GetAddress().ConfigureAwait(false);
            }

            return await interceptedSendRequestAsync(request, route).ConfigureAwait(false);
        }

        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<string, string, object[], Task<T>> interceptedSendRequestAsync,
            string method,
            string route = null,
            params object[] paramList
        )
        {
            if (method == "eth_accounts")
            {
                return await _thirdwebSession.GetAddress().ConfigureAwait(false);
            }

            return await interceptedSendRequestAsync(method, route, paramList).ConfigureAwait(false);
        }
    }
}
