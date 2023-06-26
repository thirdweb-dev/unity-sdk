using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Models.History;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Interfaces
{
    /// <summary>
    /// A module that stores Json RPC request/response history data for a given Request type (T) and Response type (TR).
    /// Each request / response is stored in a JsonRpcRecord of type T, TR
    /// </summary>
    /// <typeparam name="T">The JSON RPC Request type</typeparam>
    /// <typeparam name="TR">The JSON RPC Response type</typeparam>
    public interface IJsonRpcHistory<T, TR> : IModule, IEvents
    {
        /// <summary>
        /// A mapping of Json RPC Records to their corresponding Json RPC id
        /// </summary>
        IReadOnlyDictionary<long, JsonRpcRecord<T, TR>> Records { get; }
        
        /// <summary>
        /// The number of history records stored
        /// </summary>
        int Size { get; }
        
        /// <summary>
        /// An array of all JsonRpcRecord ids
        /// </summary>
        long[] Keys { get; }
        
        /// <summary>
        /// An array of all JsonRpcRecords, each record contains a request / response
        /// </summary>
        JsonRpcRecord<T, TR>[] Values { get; }
        
        /// <summary>
        /// An array of all pending requests. A request is pending when it has no response
        /// </summary>
        RequestEvent<T>[] Pending { get; }

        /// <summary>
        /// Initialize this JsonRpcFactory. This will restore all history records from storage
        /// </summary>
        /// <returns></returns>
        Task Init();

        /// <summary>
        /// Set a new request in the given topic on the given chainId. This will add the request to the
        /// history as pending. To add a response to this request, use the Resolve method
        /// </summary>
        /// <param name="topic">The topic to record this request in</param>
        /// <param name="request">The request to record</param>
        /// <param name="chainId">The chainId this request came from</param>
        void Set(string topic, IJsonRpcRequest<T> request, string chainId);

        /// <summary>
        /// Get a request that has previously been set with a given topic and id.
        /// </summary>
        /// <param name="topic">The topic of the request was made in</param>
        /// <param name="id">The id of the request to get</param>
        /// <returns>The recorded request record</returns>
        Task<JsonRpcRecord<T, TR>> Get(string topic, long id);

        /// <summary>
        /// Resolve a request that has previously been set using a specific response. The id and topic of the response
        /// will be used to determine which request to resolve. If the request is not found, then nothing happens.
        /// </summary>
        /// <param name="response">The response to resolve. The id and topic of the response
        /// will be used to determine which request to resolve.</param>
        /// <returns></returns>
        Task Resolve(IJsonRpcResult<TR> response);

        /// <summary>
        /// Delete a request record with a given topic and id (optional). If the request is not found, then nothing happens.
        /// </summary>
        /// <param name="topic">The topic the request was made in</param>
        /// <param name="id">The id of the request. If no id is given then all requests in the given topic are deleted.</param>
        void Delete(string topic, long? id = null);

        /// <summary>
        /// Check if a request with a given topic and id exists.
        /// </summary>
        /// <param name="topic">The topic the request was made in</param>
        /// <param name="id">The id of the request</param>
        /// <returns>True if the request with the given topic and id exists, false otherwise</returns>
        Task<bool> Exists(string topic, long id);
    }
}
