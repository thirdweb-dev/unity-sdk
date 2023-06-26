using Newtonsoft.Json;

namespace WalletConnectSharp.Sign.Models.Engine.Events
{
    /// <summary>
    /// The event to emit to a session, containing the event data T. 
    /// </summary>
    /// <typeparam name="T">The type of data inside this event</typeparam>
    public class EventData<T>
    {
        /// <summary>
        /// The name of the event
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The event data associated with this event
        /// </summary>
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
