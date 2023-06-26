using System;
using System.Threading.Tasks;
using WalletConnectSharp.Events.Interfaces;

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// An interface describing a connection to a JSON RPC node
    /// </summary>
    public interface IJsonRpcConnection : IEvents, IDisposable
    {
        /// <summary>
        /// Whether this connection is active and connected
        /// </summary>
        bool Connected { get; }
        
        /// <summary>
        /// Whether this connection is active but still connecting
        /// </summary>
        bool Connecting { get; }
        
        /// <summary>
        /// The current host Url this json rpc connection is using
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Open this connection
        /// </summary>
        /// <returns>A task that is establishing the connection</returns>
        Task Open();
        
        /// <summary>
        /// Open this connection with the provided generic connection parameters. How the connection parameters are used is up
        /// to the implementation.
        /// </summary>
        /// <param name="options">The connection parameters to use for establishing the connection</param>
        /// <typeparam name="T">The type of the connection parameters</typeparam>
        /// <returns>A task that is establishing the connection</returns>
        Task Open<T>(T options);

        /// <summary>
        /// Close this connection
        /// </summary>
        /// <returns></returns>
        Task Close();
        
        /// <summary>
        /// Send a JSON RPC request with the given parameter type T. This function does not return or wait for a
        /// response. Any response SHOULD be triggered by the IEvents interface using the request's ID as the eventId 
        /// </summary>
        /// <param name="requestPayload">The JSON RPC request to send</param>
        /// <param name="context">The current context</param>
        /// <typeparam name="T">The type of the parameter in the JSON RPC request</typeparam>
        /// <returns>A task that is performing the send</returns>
        Task SendRequest<T>(IJsonRpcRequest<T> requestPayload, object context);

        /// <summary>
        /// Send a JSON RPC response to a JSON RPC request with the response type of T. This function does not return
        /// or wait for response. JSON RPC responses do not receive any response and therefore do not trigger any events
        /// </summary>
        /// <param name="responsePayload">The JSON RPC response object to send</param>
        /// <param name="context">The current context</param>
        /// <typeparam name="T">The type of the response inside the JSON RPC response</typeparam>
        /// <returns>A task that is performing the send</returns>
        Task SendResult<T>(IJsonRpcResult<T> responsePayload, object context);
        
        /// <summary>
        /// Send a JSON RPC error. This function does not return or wait for response. JSON RPC errors do not receive
        /// any response and therefore do not trigger any events
        /// </summary>
        /// <param name="errorPayload">The error to send</param>
        /// <param name="context">The current context</param>
        /// <returns>A task that is performing the send</returns>
        Task SendError(IJsonRpcError errorPayload, object context);
    }
}