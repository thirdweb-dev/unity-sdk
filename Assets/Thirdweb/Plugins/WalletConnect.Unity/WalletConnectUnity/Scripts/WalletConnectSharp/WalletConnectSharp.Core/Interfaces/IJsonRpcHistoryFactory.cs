using System.Threading.Tasks;

namespace WalletConnectSharp.Core.Interfaces
{
    /// <summary>
    /// The interface for a factory that creates/tracks instances of <see cref="IJsonRpcHistory{T,TR}"/> for the given
    /// types T and TR. This is used to track the history of requests and responses for a given session. This factory
    /// is NOT a singleton, rather each ICore instance should have its own instance of this factory. The
    /// <see cref="IJsonRpcHistory{T,TR}"/> instance the factory returns must act as a singleton for the given
    /// ICore context. The factory implementation must be context-aware, meaning that it must be able to separate different
    /// singleton instances of <see cref="IJsonRpcHistory{T,TR}"/> for different ICore instances.
    /// </summary>
    public interface IJsonRpcHistoryFactory
    {
        /// <summary>
        /// The ICore instance this factory is for
        /// </summary>
        ICore Core { get; }
        
        /// <summary>
        /// Get the singleton instance of <see cref="IJsonRpcHistory{T,TR}"/> for the given types T and TR.
        /// </summary>
        /// <typeparam name="T">The request type of the history to keep track</typeparam>
        /// <typeparam name="TR">The response type of the history to keep track</typeparam>
        /// <returns>A new or existing <see cref="IJsonRpcHistory{T,TR}"/> singleton instance for the given type T and TR.
        /// If no singleton instance exists in the current ICore context, then a new instance will be created.</returns>
        Task<IJsonRpcHistory<T, TR>> JsonRpcHistoryOfType<T, TR>();
    }
}
