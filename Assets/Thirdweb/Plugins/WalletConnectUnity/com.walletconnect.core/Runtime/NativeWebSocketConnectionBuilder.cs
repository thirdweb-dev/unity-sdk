using System.Threading.Tasks;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Interfaces;

namespace WalletConnectUnity.Core
{
    public class NativeWebSocketConnectionBuilder : IConnectionBuilder
    {
        public Task<IJsonRpcConnection> CreateConnection(string url)
        {
            return Task.FromResult<IJsonRpcConnection>(new WebSocketConnection(url));
        }
    }
}