namespace WalletConnectSharp.Core.Interfaces
{
    /// <summary>
    /// An interface that represents a mapping of topics to a list of subscription ids
    /// </summary>
    public interface ISubscriberMap
    {
        /// <summary>
        /// An array of topics in this mapping
        /// </summary>
        public string[] Topics { get; }

        /// <summary>
        /// Add an subscription id to the given topic
        /// </summary>
        /// <param name="topic">The topic to add the subscription id to</param>
        /// <param name="id">The subscription id to add</param>
        public void Set(string topic, string id);

        /// <summary>
        /// Get an array of all subscription ids in a given topic
        /// </summary>
        /// <param name="topic">The topic to get subscription ids for</param>
        /// <returns>An array of subscription ids in a given topic</returns>
        public string[] Get(string topic);

        /// <summary>
        /// Determine whether a subscription id exists in a given topic
        /// </summary>
        /// <param name="topic">The topic to check in</param>
        /// <param name="id">The subscription id to check for</param>
        /// <returns>True if the subscription id is in the topic, false otherwise</returns>
        public bool Exists(string topic, string id);

        /// <summary>
        /// Delete subscription id from a topic. If no subscription id is given,
        /// then all subscription ids in the given topic are removed.
        /// </summary>
        /// <param name="topic">The topic to remove from</param>
        /// <param name="id">The subscription id to remove, if set to null then all ids are removed from the topic</param>
        public void Delete(string topic, string id = null);

        /// <summary>
        /// Clear all entries in the mapping
        /// </summary>
        public void Clear();
    }
}
