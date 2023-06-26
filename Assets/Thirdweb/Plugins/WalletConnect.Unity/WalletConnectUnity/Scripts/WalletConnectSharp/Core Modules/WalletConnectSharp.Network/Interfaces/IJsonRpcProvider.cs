using System;
using System.Threading.Tasks;

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// The interface that represents a JSON RPC provider
    /// </summary>
    public interface IJsonRpcProvider : IBaseJsonRpcProvider
    {
        /// <summary>
        /// Connect this provider to the given URL
        /// </summary>
        /// <param name="connection">The URL to connect to</param>
        /// <returns>A task that is establishing the connection</returns>
        Task Connect(string connection);

        /// <summary>
        /// Connect this provider using the providing connection object
        /// </summary>
        /// <param name="connection">The connection object this provider should use</param>
        /// <returns>A task that is establishing the connection</returns>
        Task Connect(IJsonRpcConnection connection);
    }
}
