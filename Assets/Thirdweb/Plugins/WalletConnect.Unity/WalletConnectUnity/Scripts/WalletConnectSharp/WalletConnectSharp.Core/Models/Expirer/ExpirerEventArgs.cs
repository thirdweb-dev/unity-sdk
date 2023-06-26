using Newtonsoft.Json;

namespace WalletConnectSharp.Core.Models.Expirer
{
    /// <summary>
    /// The event args that is passed to all <see cref="IExpirer"/> events triggered
    /// </summary>
    public class ExpirerEventArgs
    {
        /// <summary>
        /// The target this expiration is for
        /// </summary>
        [JsonProperty("target")]
        public string Target { get; set; }
        
        /// <summary>
        /// The expiration data for this event
        /// </summary>
        [JsonProperty("expiration")]
        public Expiration Expiration { get; set; }
    }
}
