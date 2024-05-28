using System.Threading;
using System.Threading.Tasks;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectUnity.Modal
{
    internal sealed class ConnectionController
    {
        private readonly ISignClient _client;

        private ConnectOptions _lastConnectOptions;
        private Task<ConnectedData> _connectionTask;


        public ConnectionController(ISignClient signClient)
        {
            _client = signClient;
        }

        public void InitiateConnection(ConnectOptions connectOptions)
        {
            _lastConnectOptions = connectOptions;
            _connectionTask = _client.Connect(connectOptions);
        }

        public async Task<ConnectedData> GetConnectionDataAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_connectionTask == null)
            {
                InitiateConnection(_lastConnectOptions);
            }

            // ReSharper disable once PossibleNullReferenceException
            var result = await _connectionTask;
            return result;
        }
    }
}