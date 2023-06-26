using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Core.Controllers
{
    /// <summary>
    /// The MessageTracker module acts as a data store
    /// that stores all hashed messages that are sent to a given topic
    /// </summary>
    public class MessageTracker : IMessageTracker
    {
        /// <summary>
        /// The current version of this MessageTracker module
        /// </summary>
        public static readonly string Version = "0.3";
        
        /// <summary>
        /// The name of this MessageTracker module
        /// </summary>
        public string Name
        {
            get
            {
                return $"{_core.Name}-messages";
            }
        }

        /// <summary>
        /// The context string for this MessageTracker module
        /// </summary>
        public string Context
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        /// The storage key this module will store data in
        /// </summary>
        public string StorageKey
        {
            get
            {
                return WalletConnectCore.STORAGE_PREFIX + Version + "//" + Name;
            }
        }

        private bool initialized;
        private ICore _core;
        
        /// <summary>
        /// A mapping of MessageRecords by a topic string key. Each MessageRecord
        /// stores a list of hashed messages sent in the topic string key
        /// </summary>
        public Dictionary<string, MessageRecord> Messages { get; private set; }

        /// <summary>
        /// Create a new MessageTracker module
        /// </summary>
        /// <param name="core">The ICore instance that will use this module</param>
        public MessageTracker(ICore core)
        {
            this._core = core;
        }
        
        /// <summary>
        /// Initializes this MessageTracker module. This will load all
        /// previous MessageRecords from storage.
        /// </summary>
        public async Task Init()
        {
            if (!initialized)
            {
                var messages = await GetRelayerMessages();

                if (messages != null)
                {
                    Messages = messages;
                }

                initialized = true;
            }
        }

        /// <summary>
        /// Set the message from a topic and store it
        /// </summary>
        /// <param name="topic">The topic to store the message in</param>
        /// <param name="message">The message to hash and store</param>
        /// <returns>The hashed message that was stored</returns>
        public async Task<string> Set(string topic, string message)
        {
            IsInitialized();

            var hash = HashUtils.HashMessage(message);
            
            MessageRecord messages;
            if (Messages.ContainsKey(topic))
                messages = Messages[topic];
            else
            {
                messages = new MessageRecord();
                Messages.Add(topic, messages);
            }

            if (messages.ContainsKey(hash))
                return hash;
            
            messages.Add(hash, message);
            await Persist();
            return hash;
        }

        /// <summary>
        /// Get all hashed messages stored in a given topic
        /// </summary>
        /// <param name="topic">The topic to get hashed messages for</param>
        /// <returns>All hashed messages stored in the given topic</returns>
        public Task<MessageRecord> Get(string topic)
        {
            IsInitialized();

            return Task.FromResult(Messages.ContainsKey(topic) ? Messages[topic] : new MessageRecord());
        }

        /// <summary>
        /// Determine whether a given message has been set before in a given
        /// topic
        /// </summary>
        /// <param name="topic">The topic to look in</param>
        /// <param name="message">The message to hash and find</param>
        /// <returns>Returns true if the hashed message has been set in the topic</returns>
        public bool Has(string topic, string message)
        {
            IsInitialized();

            if (!Messages.ContainsKey(topic)) return false;
            
            var hash = HashUtils.HashMessage(message);

            return Messages[topic].ContainsKey(hash);

        }

        /// <summary>
        /// Delete a topic and all set hashed messages
        /// </summary>
        /// <param name="topic">The topic to delete</param>
        public async Task Delete(string topic)
        {
            IsInitialized();

            Messages.Remove(topic);

            await Persist();
        }

        private async Task SetRelayerMessages(Dictionary<string, MessageRecord> messages)
        {
            await _core.Storage.SetItem(StorageKey, messages);
        }

        private async Task<Dictionary<string, MessageRecord>> GetRelayerMessages()
        {
            if (await _core.Storage.HasItem(StorageKey))
                return await _core.Storage.GetItem<Dictionary<string, MessageRecord>>(StorageKey);

            return new Dictionary<string, MessageRecord>();
        }

        private Task Persist()
        {
            return SetRelayerMessages(Messages);
        }

        private void IsInitialized()
        {
            if (!initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, this.Name);
            }
        }
    }
}
