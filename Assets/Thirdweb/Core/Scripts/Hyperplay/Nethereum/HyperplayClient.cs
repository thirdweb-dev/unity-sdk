using System;
using System.Linq;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;

namespace Thirdweb.Hyperplay
{
    public class HyperplayClient : ClientBase
    {
        private readonly Hyperplay _hyperplay;

        public HyperplayClient(Hyperplay Hyperplay)
        {
            this._hyperplay = Hyperplay;
        }

        private static readonly Random rng = new();
        private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GenerateRpcId()
        {
            var date = (long)((DateTime.UtcNow - UnixEpoch).TotalMilliseconds) * (10L * 10L * 10L);
            var extra = (long)Math.Floor(rng.NextDouble() * (10.0 * 10.0 * 10.0));
            return date + extra;
        }

        protected override async Task<RpcResponseMessage> SendAsync(RpcRequestMessage message, string route = null)
        {
            message.Id = GenerateRpcId();
            return await _hyperplay.Request(message);
        }

        protected override Task<RpcResponseMessage[]> SendAsync(RpcRequestMessage[] requests)
        {
            return Task.WhenAll(requests.Select(r => SendAsync(r)));
        }
    }
}
