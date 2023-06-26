using System.Threading.Tasks;

namespace WalletConnectSharp.Storage.Interfaces
{
    /// <summary>
    /// The storage module handles the storing by key value pairing. All of the functions are asynchronous 
    /// </summary>
    public interface IKeyValueStorage
    {
        /// <summary>
        /// Initialize the storage. This should load any data or prepare any connection required by the
        /// storage module
        /// </summary>
        /// <returns></returns>
        Task Init();
        
        /// <summary>
        /// The GetKeys functions returns al the keys that are currently stored.
        /// </summary>
        /// <returns> Returns all currently stored keys</returns>
        Task<string[]> GetKeys();
        /// <summary>
        /// The GetEntriesOfType function returns all entries of a specified type, T currently stored. 
        /// </summary>
        /// <typeparam name="T"> T is the type</typeparam>
        /// <returns> Returns all currently stored entries of a specified type.</returns>
        
        Task<T[]> GetEntriesOfType<T>();
        /// <summary>
        /// The GetEntries returns all entries currently stored.
        /// </summary>
        /// <returns> Returns all entries </returns>
        Task<object[]> GetEntries();
        /// <summary>
        /// The GetItem function returns the stored value pair based on the specified key and type.
        /// </summary>
        /// <param name="key"> The key of the value</param>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <returns>Returns the stored value pair based on the specified key and type</returns>
        Task<T> GetItem<T>(string key);
        /// <summary>
        /// The SetItem function stores the  value based on the specified key and type.
        /// </summary>
        /// <param name="key"> The key to store the value with</param>
        /// <param name="value">The value to store</param>
        /// <typeparam name="T">The type</typeparam>
        /// <returns></returns>
        Task SetItem<T>(string key, T value);
        /// <summary>
        /// The RemoveItem function deletes the value stored based off of the specified key.
        /// </summary>
        /// <param name="key">The key to delete the stored value pairing.</param>
        /// <returns></returns>
        Task RemoveItem(string key);
        /// <summary>
        /// The HasItem function checks to see if a key exists.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if it exists, otherwise false</returns>
        Task<bool> HasItem(string key);

        /// <summary>
        /// Clear all entries in this storage. WARNING: This will delete all data!
        /// </summary>
        /// <returns></returns>
        Task Clear();
    }
}