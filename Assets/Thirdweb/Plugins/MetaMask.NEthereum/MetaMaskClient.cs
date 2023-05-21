using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetaMask.Models;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MetaMask.NEthereum
{
    public class MetaMaskClient : ClientBase
    {
        private MetaMaskWallet _metaMask;

        public MetaMaskClient(MetaMaskWallet metaMask)
        {
            this._metaMask = metaMask;
        }
        
        private static readonly Random rng = new Random();
        private static readonly DateTime UnixEpoch =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GenerateRpcId()
        {
            var date = (long)((DateTime.UtcNow - UnixEpoch).TotalMilliseconds) * (10L * 10L * 10L);
            var extra = (long)Math.Floor(rng.NextDouble() * (10.0 * 10.0 * 10.0));
            return date + extra;
        }

        protected override async Task<RpcResponseMessage> SendAsync(RpcRequestMessage message, string route = null)
        {
            // Regenerate the NEthereum json-rpc id
            var id = GenerateRpcId();
            var mapParameters = message.RawParameters as Dictionary<string, object>;
            var arrayParameters = message.RawParameters as object[];
            var rawParameters = message.RawParameters;

            var rpcRequestMessage = mapParameters != null
                ? new RpcRequestMessage(id, message.Method, mapParameters)
                : arrayParameters != null
                    ? new RpcRequestMessage(id, message.Method, arrayParameters)
                    : new RpcRequestMessage(id, message.Method, rawParameters);
            
            var response = await _metaMask.Request(new MetaMaskEthereumRequest()
            {
                Id = rpcRequestMessage.Id.ToString(),
                Method = rpcRequestMessage.Method,
                Parameters = rpcRequestMessage.RawParameters
            });

            try
            {
                var convertedResponse = JsonConvert.DeserializeObject<JToken>(response.ToString());
                return new RpcResponseMessage(rpcRequestMessage.Id, convertedResponse);
            }
#pragma warning disable CS0168
            catch (JsonReaderException jex)
#pragma warning restore CS0168
            {
                // Sometimes we'll get back a tx hash instead of a response object.
                // For those cases we catch the JSON error and use the hash string directly.
                var stringResponse = response.ToString();
                return new RpcResponseMessage(rpcRequestMessage.Id, stringResponse);
            }
        }

        protected override Task<RpcResponseMessage[]> SendAsync(RpcRequestMessage[] requests)
        {
            return Task.WhenAll(requests.Select(r => SendAsync(r)));
        }
    }
}