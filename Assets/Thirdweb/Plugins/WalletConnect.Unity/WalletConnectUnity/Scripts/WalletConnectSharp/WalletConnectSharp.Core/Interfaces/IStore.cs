using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Network.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Core.Interfaces
{
    /// <summary>
    /// An interface representing a generic Store module that is capable of storing any key / value of types TKey : TValue
    /// </summary>
    /// <typeparam name="TKey">The type of the keys stored</typeparam>
    /// <typeparam name="TValue">The type of the values stored, the value must contain the key</typeparam>
    public interface IStore<TKey, TValue> : IModule where TValue : IKeyHolder<TKey>
    {
        /// <summary>
        /// How many items this Store module is currently holding
        /// </summary>
        public int Length { get; }
        
        /// <summary>
        /// An array of TKey of all keys in this Store module
        /// </summary>
        public TKey[] Keys { get; }
        
        /// <summary>
        /// An array of TValue of all values in this Store module
        /// </summary>
        public TValue[] Values { get; }

        /// <summary>
        /// Initialize this Store module. This will load all data from the storage module used
        /// by ICore
        /// </summary>
        public Task Init();

        /// <summary>
        /// Store a given key/value. If the key already exists in this Store, then the
        /// value will be updated
        /// </summary>
        /// <param name="key">The key to store in</param>
        /// <param name="value">The value to store</param>
        /// <returns></returns>
        public Task Set(TKey key, TValue value);

        /// <summary>
        /// Get the value stored under a given TKey key
        /// </summary>
        /// <param name="key">The key to lookup a value for.</param>
        /// <exception cref="WalletConnectException">Thrown when the given key doesn't exist in this Store</exception>
        /// <returns>Returns the TValue value stored at the given key</returns>
        public TValue Get(TKey key);

        /// <summary>
        /// Update the given key with the TValue update. Partial updates are supported
        /// using reflection. This means, only non-null values in TValue update will be updated
        /// </summary>
        /// <param name="key">The key to update</param>
        /// <param name="update">The updates to make</param>
        /// <returns></returns>
        public Task Update(TKey key, TValue update);

        /// <summary>
        /// Delete a given key with an ErrorResponse reason
        /// </summary>
        /// <param name="key">The key to delete</param>
        /// <param name="reason">The reason this key was deleted using an ErrorResponse</param>
        /// <returns></returns>
        public Task Delete(TKey key, Error reason);

        public IDictionary<TKey, TValue> ToDictionary();
    }
}
