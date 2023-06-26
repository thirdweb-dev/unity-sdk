using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Models.Pairing;
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
    /// The interface for a module that handles pairing two peers and storing related data
    /// </summary>
    public interface IPairing : IModule, IEvents
    {
        /// <summary>
        /// Get the <see cref="IStore{TKey,TValue}"/> module that is handling the storage of
        /// <see cref="PairingStruct"/> 
        /// </summary>
        IPairingStore Store { get; }
        
        /// <summary>
        /// Get all active and inactive pairings
        /// </summary>
        PairingStruct[] Pairings { get; }
        
        /// <summary>
        /// The <see cref="ICore"/> module using this module instance
        /// </summary>
        ICore Core { get; }

        /// <summary>
        /// Initialize this pairing module. This will restore all active / inactive pairings
        /// from storage
        /// </summary>
        Task Init();

        /// <summary>
        /// Pair with a peer using the given uri. The uri must be in the correct
        /// format otherwise an exception will be thrown. You may (optionally) pair
        /// without activating the pairing. By default the pairing will be activated before
        /// it is returned
        /// </summary>
        /// <param name="uri">The URI to pair with</param>
        /// <returns>The pairing data that can be used to pair with the peer</returns>
        Task<PairingStruct> Pair(string uri, bool activatePairing = true);

        /// <summary>
        /// Create a new pairing at the given pairing topic
        /// </summary>
        /// <returns>A new instance of <see cref="CreatePairingData"/> that includes the pairing topic and
        /// uri</returns>
        Task<CreatePairingData> Create();

        /// <summary>
        /// Activate a previously created pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to activate</param>
        Task Activate(string topic);

        /// <summary>
        /// Subscribe to method requests
        /// </summary>
        /// <param name="methods">The methods to register and subscribe</param>
        Task Register(string[] methods);

        /// <summary>
        /// Update the expiration of an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to update</param>
        /// <param name="expiration">The new expiration date as a unix timestamp (seconds)</param>
        /// <returns></returns>
        Task UpdateExpiry(string topic, long expiration);

        /// <summary>
        /// Update the metadata of an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to update</param>
        /// <param name="metadata">The new metadata</param>
        Task UpdateMetadata(string topic, Metadata metadata);

        /// <summary>
        /// Ping an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to ping</param>
        Task Ping(string topic);

        /// <summary>
        /// Disconnect an existing pairing at the given topic
        /// </summary>
        /// <param name="topic">The topic of the pairing to disconnect</param>
        Task Disconnect(string topic);

        /// <summary>
        /// Parse a session proposal URI and return all information in the URI in a
        /// new <see cref="UriParameters"/> object
        /// </summary>
        /// <param name="uri">The uri to parse</param>
        /// <returns>A new <see cref="UriParameters"/> object that contains all data
        /// parsed from the given uri</returns>
        UriParameters ParseUri(string uri);
    }
}
