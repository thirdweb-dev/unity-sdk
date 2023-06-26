using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectSharp.Crypto.Interfaces
{
    /// <summary>
    /// A module that represents a keychain
    /// </summary>
    public interface IKeyChain : IModule
    {
        /// <summary>
        /// A read-only dictionary of all keypairs
        /// </summary>
        IReadOnlyDictionary<string, string> Keychain { get; }
        
        /// <summary>
        /// The backing IKeyValueStorage module being used to store the key/pairs
        /// </summary>
        IKeyValueStorage Storage { get; }

        /// <summary>
        /// Initialize the KeyChain, this will load the keychain into memory from the storage 
        /// </summary>
        Task Init();

        /// <summary>
        /// Check if a given tag exists in this KeyChain. This task is asynchronous but completes instantly.
        /// Async support is built in for future implementations which may use a cloud keystore
        /// </summary>
        /// <param name="tag">The tag to check for existence</param>
        /// <returns>True if the tag exists, false otherwise</returns>
        Task<bool> Has(string tag);

        /// <summary>
        /// Set a key with the given tag. The private key can only be retrieved using the tag
        /// given
        /// </summary>
        /// <param name="tag">The tag to save with the key given</param>
        /// <param name="key">The key to set with the given tag</param>
        Task Set(string tag, string key);

        /// <summary>
        /// Get a saved key with the given tag. If no tag exists, then a WalletConnectException will
        /// be thrown.
        /// </summary>
        /// <param name="tag">The tag of the key to retrieve</param>
        /// <returns>The key with the given tag</returns>
        /// <exception cref="WalletConnectException">Thrown if the given tag does not match any key</exception>
        Task<string> Get(string tag);

        /// <summary>
        /// Delete a key with the given tag. If no tag exists, then a WalletConnectException will
        /// be thrown.
        /// </summary>
        /// <param name="tag">The tag of the key to delete</param>
        /// <exception cref="WalletConnectException">Thrown if the given tag does not match any key</exception>
        Task Delete(string tag);
    }
}
