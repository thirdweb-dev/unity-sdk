using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectSharp.Common;

namespace WalletConnectSharp.Core.Interfaces
{
    /// <summary>
    /// An alias for a dictionary of string key-value pairs
    /// </summary>
    public sealed class MessageRecord : Dictionary<string, string>
    {
    }

    /// <summary>
    /// An interface that represents the message tracker module. This module
    /// tracks the messages that are sent and received by the SDK.
    /// </summary>
    public interface IMessageTracker : IModule
    {
        /// <summary>
        /// A dictionary of all messages sent / received by the SDK. The key of the
        /// dictionary is the topic the message was sent in
        /// </summary>
        public Dictionary<string, MessageRecord> Messages { get; }

        /// <summary>
        /// Initialize this module, which will load all messages from the storage.
        /// </summary>
        /// <returns></returns>
        public Task Init();

        /// <summary>
        /// Store a new message in a specific topic. The hash of the message will be returned. If the message
        /// was already previously stored then nothing is stored. The hash of the message will always be returned. 
        /// </summary>
        /// <param name="topic">The topic the message was in</param>
        /// <param name="message">The message to store</param>
        /// <returns>The hash of the message stored</returns>
        public Task<string> Set(string topic, string message);

        /// <summary>
        /// Get all messages stored in a specific topic. If the topic does not exist then an empty MessageRecord is returned.
        /// </summary>
        /// <param name="topic">The topic to retrieve messages from</param>
        /// <returns>The MessageRecord of all messages (and their hashes) stored.</returns>
        public Task<MessageRecord> Get(string topic);

        /// <summary>
        /// Whether a given message is stored in a given topic.
        /// </summary>
        /// <param name="topic">The topic to check for the given message in</param>
        /// <param name="message">The message to look for in the given topic</param>
        /// <returns>True if the given message exists in the given topic</returns>
        public bool Has(string topic, string message);

        /// <summary>
        /// Delete all messages from a topic. If the topic does not exist then nothing is deleted.
        /// </summary>
        /// <param name="topic">The topic to delete all messages from</param>
        /// <returns></returns>
        public Task Delete(string topic);
    }
}
