using Newtonsoft.Json;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnectSharp.Sign.Models.Engine.Events
{
    /// <summary>
    /// The event data emitted when a session event is triggered
    /// </summary>
    /// <typeparam name="T">The type of the event data in the session event1</typeparam>
    public class EmitEvent<T> : SessionEvent
    {
        /// <summary>
        /// The wc_sessionEvent request that triggered this event
        /// </summary>
        [JsonProperty("params")]
        public SessionEvent<T> Params { get; set; }
    }
}
