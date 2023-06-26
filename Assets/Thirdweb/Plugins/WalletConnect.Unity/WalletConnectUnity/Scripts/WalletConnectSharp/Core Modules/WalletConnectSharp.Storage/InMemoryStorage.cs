using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectSharp.Storage
{
    public class InMemoryStorage : IKeyValueStorage
    {
        protected Dictionary<string, object> Entries = new Dictionary<string, object>();
        private bool _initialized = false;

        public virtual Task Init()
        {
            _initialized = true;
            return Task.CompletedTask;
        }

        public virtual Task<string[]> GetKeys()
        {
            IsInitialized();
            return Task.FromResult(Entries.Keys.ToArray());
        }

        public virtual async Task<T[]> GetEntriesOfType<T>()
        {
            IsInitialized();
            return (await GetEntries()).OfType<T>().ToArray();
        }

        public virtual Task<object[]> GetEntries()
        {
            IsInitialized();
            return Task.FromResult(Entries.Values.ToArray());
        }

        public virtual Task<T> GetItem<T>(string key)
        {
            IsInitialized();
            return Task.FromResult(Entries[key] is T ? (T)Entries[key] : default);
        }
        
        public virtual Task SetItem<T>(string key, T value)
        {
            IsInitialized();
            Entries[key] = value;
            return Task.CompletedTask;
        }
        public virtual Task RemoveItem(string key)
        {
            IsInitialized();
            Entries.Remove(key);
            return Task.CompletedTask;
        }

        public virtual Task<bool> HasItem(string key)
        {
            IsInitialized();
            return Task.FromResult(Entries.ContainsKey(key));
        }

        public virtual Task Clear()
        {
            IsInitialized();
            Entries.Clear();
            return Task.CompletedTask;
        }

        protected void IsInitialized()
        {
            if (!_initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, "Storage");
            }
        }
    }
}