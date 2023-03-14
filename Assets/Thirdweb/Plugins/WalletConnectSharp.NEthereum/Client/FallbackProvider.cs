using System;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using WalletConnectSharp.Core.Extensions;
using WalletConnectSharp.Core.Models;

namespace WalletConnectSharp.NEthereum.Client
{
    public class FallbackProvider : IClient
    {
        public static readonly string[] ValidMethods = JsonRpcRequest.SigningMethods.ToStringArray();

        private readonly IClient _fallback;
        private readonly IClient _signer;

        public FallbackProvider(IClient primary, IClient fallback)
        {
            _signer = primary;
            _fallback = fallback;
        }

        public Task<RpcRequestResponseBatch> SendBatchRequestAsync(RpcRequestResponseBatch rpcRequestResponseBatch)
        {
            // TODO: ISSUE #55: Implement SendBatchRequestAsync
            throw new NotImplementedException();
        }

        public Task SendRequestAsync(RpcRequest request, string route = null)
        {
            return ValidMethods.Contains(request.Method) ? _signer.SendRequestAsync(request, route) : _fallback.SendRequestAsync(request, route);
        }

        public Task SendRequestAsync(string method, string route = null, params object[] paramList)
        {
            return ValidMethods.Contains(method) ? _signer.SendRequestAsync(method, route, paramList) : _fallback.SendRequestAsync(method, route, paramList);
        }

        public RequestInterceptor OverridingRequestInterceptor { get; set; }

        public Task<T> SendRequestAsync<T>(RpcRequest request, string route = null)
        {
            if (request.Method == ValidJsonRpcRequestMethods.EthFeeHistory)
            {
                //convert a null 3rd parameter to an empty array
                if (request.RawParameters[2] == null)
                {
                    request.RawParameters[2] = Array.Empty<object>();
                }
            }

            if (request.Method == ValidJsonRpcRequestMethods.EthSendTransaction)
            {
                //Convert a null from address to the current session's address
                if (request.RawParameters[0] is TransactionInput)
                {
                    var input = request.RawParameters[0] as TransactionInput;

                    if (input.From == null)
                    {
                        if (_signer is WalletConnectClient wcClient)
                        {
                            input.From = wcClient.Session.Accounts[0];
                            request.RawParameters[0] = input;
                        }
                    }
                }
            }

            return ValidMethods.Contains(request.Method) ? _signer.SendRequestAsync<T>(request, route) : _fallback.SendRequestAsync<T>(request, route);
        }

        public Task<T> SendRequestAsync<T>(string method, string route = null, params object[] paramList)
        {
            return ValidMethods.Contains(method) ? _signer.SendRequestAsync<T>(method, route, paramList) : _fallback.SendRequestAsync<T>(method, route, paramList);
        }
    }
}
