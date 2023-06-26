using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Models.Expirer;
using WalletConnectSharp.Events.Interfaces;
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
    /// The interface for the Expirer module. The Expirer module keeps track of expiration dates and triggers an event when an expiration date
    /// has passed
    /// </summary>
    public interface IExpirer : IModule, IEvents
    {
        /// <summary>
        /// The number of expirations this module is tracking
        /// </summary>
        int Length { get; }
        
        /// <summary>
        /// An array of key values that represents each expiration this module is tracking
        /// </summary>
        string[] Keys { get; }
        
        /// <summary>
        /// An array of expirations this module is tracking
        /// </summary>
        Expiration[] Values { get; }

        /// <summary>
        /// Initialize this module. This will restore all stored expiration from Storage
        /// </summary>
        Task Init();

        /// <summary>
        /// Determine whether this Expirer is tracking an expiration with the given string key (usually a topic). 
        /// </summary>
        /// <param name="key">The key of the expiration to check existence for</param>
        /// <returns>True if the given key is being tracked by this module, false otherwise</returns>
        bool Has(string key);

        /// <summary>
        /// Determine whether this Expirer is tracking an expiration with the given long key (usually an id). 
        /// </summary>
        /// <param name="key">The key of the expiration to check existence for</param>
        /// <returns>True if the given key is being tracked by this module, false otherwise</returns>
        bool Has(long key);

        /// <summary>
        /// Store a new expiration date with the given string key (usually a topic).
        /// This will also start tracking for the expiration date
        /// </summary>
        /// <param name="key">The string key of the expiration to store</param>
        /// <param name="expiry">The expiration date to store</param>
        void Set(string key, long expiry);

        /// <summary>
        /// Store a new expiration date with the given long key (usually a id).
        /// This will also start tracking for the expiration date
        /// </summary>
        /// <param name="key">The long key of the expiration to store</param>
        /// <param name="expiry">The expiration date to store</param>
        void Set(long key, long expiry);

        /// <summary>
        /// Get an expiration date with the given string key (usually a topic)
        /// </summary>
        /// <param name="key">The string key to get the expiration for</param>
        /// <returns>The expiration date</returns>
        Expiration Get(string key);

        /// <summary>
        /// Get an expiration date with the given long key (usually an id)
        /// </summary>
        /// <param name="key">The long key to get the expiration for</param>
        /// <returns>The expiration date</returns>
        Expiration Get(long key);

        /// <summary>
        /// Delete a expiration with the given string key (usually a topic).
        /// </summary>
        /// <param name="key">The string key of the expiration to delete</param>
        Task Delete(string key);

        /// <summary>
        /// Delete a expiration with the given long key (usually a id).
        /// </summary>
        /// <param name="key">The long key of the expiration to delete</param>
        Task Delete(long key);
    }
}
