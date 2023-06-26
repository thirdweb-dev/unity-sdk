using System;
using System.Collections.Generic;
using System.Linq;
using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Core.Controllers
{
    /// <summary>
    /// A mapping of topics to a list of subscription ids
    /// </summary>
    public class TopicMap : ISubscriberMap
    {
        private Dictionary<string, List<string>> topicMap = new Dictionary<string, List<string>>();

        /// <summary>
        /// An array of topics in this mapping
        /// </summary>
        public string[] Topics => topicMap.Keys.ToArray();

        /// <summary>
        /// Add an subscription id to the given topic
        /// </summary>
        /// <param name="topic">The topic to add the subscription id to</param>
        /// <param name="id">The subscription id to add</param>
        public void Set(string topic, string id)
        {
            if (Exists(topic, id)) return;
            
            if (!topicMap.ContainsKey(topic))
                topicMap.Add(topic, new List<string>());

            var ids = topicMap[topic];
            ids.Add(id);
        }

        /// <summary>
        /// Get an array of all subscription ids in a given topic
        /// </summary>
        /// <param name="topic">The topic to get subscription ids for</param>
        /// <returns>An array of subscription ids in a given topic</returns>
        public string[] Get(string topic)
        {
            if (!topicMap.ContainsKey(topic))
                return Array.Empty<string>();

            return topicMap[topic].ToArray();
        }

        /// <summary>
        /// Determine whether a subscription id exists in a given topic
        /// </summary>
        /// <param name="topic">The topic to check in</param>
        /// <param name="id">The subscription id to check for</param>
        /// <returns>True if the subscription id is in the topic, false otherwise</returns>
        public bool Exists(string topic, string id)
        {
            var ids = Get(topic);
            return ids.Contains(id);
        }

        /// <summary>
        /// Delete subscription id from a topic. If no subscription id is given,
        /// then all subscription ids in the given topic are removed.
        /// </summary>
        /// <param name="topic">The topic to remove from</param>
        /// <param name="id">The subscription id to remove, if set to null then all ids are removed from the topic</param>
        public void Delete(string topic, string id = null)
        {
            if (!topicMap.ContainsKey(topic))
                return;

            if (id == null)
            {
                topicMap.Remove(topic);
                return;
            }

            if (!Exists(topic, id))
                return;

            var ids = topicMap[topic];
            ids.Remove(id);

            if (ids.Count == 0)
                topicMap.Remove(topic);
        }

        /// <summary>
        /// Clear all entries in this TopicMap
        /// </summary>
        public void Clear()
        {
            topicMap.Clear();
        }
    }
}
