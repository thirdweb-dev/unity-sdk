using System.Threading.Tasks;
using WalletConnectSharp.Events.Interfaces;

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// Represents a base interface for JsonRpcProvider
    /// </summary>
    public interface IBaseJsonRpcProvider : IEvents
    {
        /// <summary>
        /// Gets the current IJsonRpcConnection this provider is using
        /// </summary>
        IJsonRpcConnection Connection { get; }

        /// <summary>
        /// Connect this provider using already defined connection parameters.
        /// </summary>
        /// <returns>A task that is establishing the connection</returns>
        Task Connect();
        
        /// <summary>
        /// Disconnect this provider directly
        /// </summary>
        /// <returns>A task that is ending the connection</returns>
        Task Disconnect();
        
        /// <summary>
        /// Send a Json RPC request with a parameter field of type T, and decode a response with the type of TR.
        /// </summary>
        /// <param name="request">The json rpc request to send</param>
        /// <param name="context">The current context</param>
        /// <typeparam name="T">The type of the parameter field in the json rpc request</typeparam>
        /// <typeparam name="TR">The type of the parameter field in the json rpc response</typeparam>
        /// <returns>The decoded response for the request</returns>
        Task<TR> Request<T, TR>(IRequestArguments<T> request, object context = null);
    }
}
