using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Crypto.Interfaces;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectSharp.Crypto
{
    /// <summary>
    /// A module that handles the storage of key/value pairs. 
    /// </summary>
    public class KeyChain : IKeyChain
    {
        private Dictionary<string, string> _keyChain = new Dictionary<string, string>();
        
        /// <summary>
        /// The backing IKeyValueStorage module being used to store the key/pairs
        /// </summary>
        public IKeyValueStorage Storage { get; private set; }
        
        /// <summary>
        /// A read-only dictionary of all keypairs
        /// </summary>
        public IReadOnlyDictionary<string, string> Keychain => new ReadOnlyDictionary<string, string>(_keyChain);
        
        /// <summary>
        /// The name of this module, always "keychain"
        /// </summary>
        public string Name
        {
            get
            {
                return "keychain";
            }
        }

        /// <summary>
        /// The context string for this keychain
        /// </summary>
        public string Context
        {
            get
            {
                //TODO Set to logger context
                return "walletconnectsharp";
            }
        }

        /// <summary>
        /// The version of this keychain module
        /// </summary>
        public string Version
        {
            get
            {
                return "0.3";
            }
        }

        /// <summary>
        /// The storage key that is used to store the keychain in the given IKeyValueStorage
        /// </summary>
        public string StorageKey => this._storagePrefix + this.Version + "//" + this.Name;

        private bool _initialized = false;
        private readonly string _storagePrefix = Constants.CORE_STORAGE_PREFIX;

        /// <summary>
        /// Create a new keychain using the given IKeyValueStorage module as the
        /// primary storage of all keypairs
        /// </summary>
        /// <param name="storage">The storage module to use to save/load keypairs from</param>
        public KeyChain(IKeyValueStorage storage)
        {
            this.Storage = storage;
        }

        /// <summary>
        /// Initialize the KeyChain, this will load the keychain into memory from the storage 
        /// </summary>
        public async Task Init()
        {
            if (!this._initialized)
            {
                var keyChain = await GetKeyChain();
                if (keyChain != null)
                {
                    this._keyChain = keyChain;
                }

                this._initialized = true;
            }
        }

        /// <summary>
        /// Check if a given tag exists in this KeyChain. This task is asynchronous but completes instantly.
        /// Async support is built in for future implementations which may use a cloud keystore
        /// </summary>
        /// <param name="tag">The tag to check for existence</param>
        /// <returns>True if the tag exists, false otherwise</returns>
        public Task<bool> Has(string tag)
        {
            this.IsInitialized();
            return Task.FromResult(this._keyChain.ContainsKey(tag));
        }

        /// <summary>
        /// Set a key with the given tag. The private key can only be retrieved using the tag
        /// given
        /// </summary>
        /// <param name="tag">The tag to save with the key given</param>
        /// <param name="key">The key to set with the given tag</param>
        public async Task Set(string tag, string key)
        {
            this.IsInitialized();
            if (await Has(tag))
            {
                this._keyChain[tag] = key;
            }
            else
            {
                this._keyChain.Add(tag, key);
            }

            await SaveKeyChain();
        }

        /// <summary>
        /// Get a saved key with the given tag. If no tag exists, then a WalletConnectException will
        /// be thrown.
        /// </summary>
        /// <param name="tag">The tag of the key to retrieve</param>
        /// <returns>The key with the given tag</returns>
        /// <exception cref="WalletConnectException">Thrown if the given tag does not match any key</exception>
        public async Task<string> Get(string tag)
        {
            this.IsInitialized();

            if (!await Has(tag))
            {
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY, new {tag});
            }

            return this._keyChain[tag];
        }

        /// <summary>
        /// Delete a key with the given tag. If no tag exists, then a WalletConnectException will
        /// be thrown.
        /// </summary>
        /// <param name="tag">The tag of the key to delete</param>
        /// <exception cref="WalletConnectException">Thrown if the given tag does not match any key</exception>
        public async Task Delete(string tag)
        {
            this.IsInitialized();
            
            if (!await Has(tag))
            {
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY, new {tag});
            }

            _keyChain.Remove(tag);

            await this.SaveKeyChain();
        }

        private void IsInitialized()
        {
            if (!this._initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, new {Name});
            }
        }
        
        private async Task<Dictionary<string, string>> GetKeyChain()
        {
            var hasKey = await Storage.HasItem(StorageKey);
            if (!hasKey)
            {
                await Storage.SetItem(StorageKey, new Dictionary<string, string>());
            }
            return await Storage.GetItem<Dictionary<string, string>>(StorageKey);
        }

        private async Task SaveKeyChain()
        {
            await Storage.SetItem(StorageKey, this._keyChain);
        }
    }
}
