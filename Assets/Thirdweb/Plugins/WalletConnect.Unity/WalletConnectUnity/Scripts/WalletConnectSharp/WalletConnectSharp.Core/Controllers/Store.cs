using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Core.Controllers
{
    /// <summary>
    /// A generic Store module that is capable of storing any key / value of types TKey : TValue
    /// </summary>
    /// <typeparam name="TKey">The type of the keys stored</typeparam>
    /// <typeparam name="TValue">The type of the values stored, the value must contain the key</typeparam>
    public class Store<TKey, TValue> : IStore<TKey, TValue> where TValue : IKeyHolder<TKey>
    {
        private bool initialized;
        private Dictionary<TKey, TValue> map = new Dictionary<TKey, TValue>();
        private TValue[] cached = Array.Empty<TValue>();
        
        /// <summary>
        /// The ICore module using this Store module
        /// </summary>
        public ICore Core { get; }
        
        /// <summary>
        /// The StoragePrefix this Store module will prepend to the storage key
        /// </summary>
        public string StoragePrefix { get; }

        /// <summary>
        /// The version of this Store module
        /// </summary>
        public string Version
        {
            get
            {
                return "0.3";
            }
        }
        
        /// <summary>
        /// The Name of this Store module
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// The context string of this Store module
        /// </summary>
        public string Context { get; }

        /// <summary>
        /// The storage key this Store module will store data in
        /// </summary>
        public string StorageKey
        {
            get
            {
                return StoragePrefix + Version + "//" + Name;
            }
        }

        /// <summary>
        /// How many items this Store module is currently holding
        /// </summary>
        public int Length
        {
            get
            {
                return map.Count;
            }
        }

        /// <summary>
        /// An array of TKey of all keys in this Store module
        /// </summary>
        public TKey[] Keys
        {
            get
            {
                return map.Keys.ToArray();
            }
        }

        /// <summary>
        /// An array of TValue of all values in this Store module
        /// </summary>
        public TValue[] Values
        {
            get
            {
                return map.Values.ToArray();
            }
        }

        /// <summary>
        /// Create a new Store module with the given ICore, name, and storagePrefix.
        /// </summary>
        /// <param name="core">The ICore module that is using this Store module</param>
        /// <param name="name">The name of this Store module</param>
        /// <param name="storagePrefix">The storage prefix that should be used in the storage key</param>
        public Store(ICore core, string name, string storagePrefix = null)
        {
            Core = core;

            name = $"{core.Name}-{name}";
            Name = name;
            Context = name;
            
            if (storagePrefix == null)
                StoragePrefix = WalletConnectCore.STORAGE_PREFIX;
            else
                StoragePrefix = storagePrefix;
        }

        /// <summary>
        /// Initialize this Store module. This will load all data from the storage module used
        /// by ICore
        /// </summary>
        public async Task Init()
        {
            if (!initialized)
            {
                await Restore();

                foreach (var value in cached)
                {
                    if (value != null)
                        map.Add(value.Key, value);
                }

                cached = Array.Empty<TValue>();
                initialized = true;
            }
        }

        /// <summary>
        /// Store a given key/value. If the key already exists in this Store, then the
        /// value will be updated
        /// </summary>
        /// <param name="key">The key to store in</param>
        /// <param name="value">The value to store</param>
        /// <returns></returns>
        public Task Set(TKey key, TValue value)
        {
            IsInitialized();

            if (map.ContainsKey(key))
            {
                return Update(key, value);
            }
            map.Add(key, value);
            return Persist();
        }

        /// <summary>
        /// Get the value stored under a given TKey key
        /// </summary>
        /// <param name="key">The key to lookup a value for.</param>
        /// <exception cref="WalletConnectException">Thrown when the given key doesn't exist in this Store</exception>
        /// <returns>Returns the TValue value stored at the given key</returns>
        public TValue Get(TKey key)
        {
            IsInitialized();
            var value = GetData(key);
            return value;
        }

        /// <summary>
        /// Update the given key with the TValue update. Partial updates are supported
        /// using reflection. This means, only non-null values in TValue update will be updated
        /// </summary>
        /// <param name="key">The key to update</param>
        /// <param name="update">The updates to make</param>
        /// <returns></returns>
        public Task Update(TKey key, TValue update)
        {
            IsInitialized();
            
            // Partial updates aren't built into C#
            // However, we can use reflection to sort of
            // get the same thing
            try
            {
                // First, we check if we even have a value to reference
                var previousValue = Get(key);

                // Find all properties the type TKey has
                Type t = typeof(TValue);
                var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

                // Loop through all of them
                foreach (var prop in properties)
                {
                    // Grab the updated value
                    var @value = prop.GetValue(update, null);
                    // If it exists (its not null), then set it
                    if (@value != null)
                    {
                        object test = previousValue;
                        prop.SetValue(test, @value, null);
                        previousValue = (TValue)test;
                    }
                }
                
                // Now, set the update variable to be the new modified 
                // previousValue object
                update = previousValue;
            }
            catch (WalletConnectException e)
            {
                // ignored if no previous value exists
            }

            map.Remove(key);
            map.Add(key, update);

            return Persist();
        }

        /// <summary>
        /// Delete a given key with an ErrorResponse reason
        /// </summary>
        /// <param name="key">The key to delete</param>
        /// <param name="reason">The reason this key was deleted using an ErrorResponse</param>
        /// <returns></returns>
        public Task Delete(TKey key, Error reason)
        {
            IsInitialized();

            if (!map.ContainsKey(key)) return Task.CompletedTask;

            map.Remove(key);

            return Persist();
        }

        public IDictionary<TKey, TValue> ToDictionary()
        {
            IsInitialized();

            return new ReadOnlyDictionary<TKey, TValue>(map);
        }

        protected virtual Task SetDataStore(TValue[] data)
        {
            return Core.Storage.SetItem<TValue[]>(StorageKey, data);
        }

        protected virtual async Task<TValue[]> GetDataStore()
        {
            if (await Core.Storage.HasItem(StorageKey))
                return await Core.Storage.GetItem<TValue[]>(StorageKey);

            return Array.Empty<TValue>();
        }

        protected virtual TValue GetData(TKey key)
        {
            if (!map.ContainsKey(key))
            {
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY, $"{Name}: {key}");
            }

            return map[key];
        }

        protected virtual Task Persist()
        {
            return SetDataStore(Values);
        }

        protected virtual async Task Restore()
        {
            var persisted = await GetDataStore();
            if (persisted == null) return;
            if (persisted.Length == 0) return;
            if (map.Count > 0)
            {
                throw WalletConnectException.FromType(ErrorType.RESTORE_WILL_OVERRIDE, Name);
            }

            cached = persisted;
        }

        protected virtual void IsInitialized()
        {
            if (!initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, Name);
            }
        }
    }
}
