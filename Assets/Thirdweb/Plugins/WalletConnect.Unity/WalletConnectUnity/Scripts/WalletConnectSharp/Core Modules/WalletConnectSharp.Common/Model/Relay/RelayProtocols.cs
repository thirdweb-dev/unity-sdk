using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WalletConnectSharp.Common.Model.Relay
{
    /// <summary>
    /// A class that defines the different RPC methods for a
    /// given pub/sub protocol
    /// </summary>
    public abstract class RelayProtocols
    {
        /// <summary>
        /// The default protocol as a string
        /// </summary>
        public static readonly string Default = "irn";
        
        /// <summary>
        /// The Waku protocol definitions
        /// </summary>
        public static RelayProtocols Waku = new WakuRelayProtocol();
        
        /// <summary>
        /// The Irn protocol definitions
        /// </summary>
        public static RelayProtocols Irn = new IrnRelayProtocol();
        
        /// <summary>
        /// The Iridium protocol definitions
        /// </summary>
        public static RelayProtocols Iridium = new IridiumRelayProtocol();
        
        private static Dictionary<string, RelayProtocols> _protocols = new Dictionary<string, RelayProtocols>()
        {
            { "waku", Waku },
            { "irn", Irn },
            { "iridium", Iridium },
        };

        /// <summary>
        /// A mapping of protocol names => Protocol Definitions
        /// </summary>
        public static IReadOnlyDictionary<string, RelayProtocols> Protocols => _protocols;

        /// <summary>
        /// Get protocol definitions by the protocol's name
        /// </summary>
        /// <param name="protocol">The protocol name to get definitions for</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The protocol doesn't exist</exception>
        public static RelayProtocols GetRelayProtocol(string protocol)
        {
            if (Protocols.ContainsKey(protocol))
                return Protocols[protocol];

            throw new ArgumentException("Relay Protocol not supported: " + protocol);
        }
        
        /// <summary>
        /// The Publish action RPC method name
        /// </summary>
        [JsonProperty("publish")]
        public abstract string Publish { get; }
        
        public abstract string BatchPublish { get; }
        
        /// <summary>
        /// The Subscribe action RPC method name
        /// </summary>
        [JsonProperty("subscribe")]
        public abstract string Subscribe { get; }
        
        public abstract string BatchSubscribe { get; }
        
        /// <summary>
        /// The Subscription action RPC method name
        /// </summary>
        [JsonProperty("subscription")]
        public abstract string Subscription { get; }
        
        /// <summary>
        /// The Unsubscribe action RPC method name
        /// </summary>
        [JsonProperty("unsubscribe")]
        public abstract string Unsubscribe { get; }
        
        public abstract string BatchUnsubscribe { get; }

        /// <summary>
        /// A class that defines all RelayProtocol definitions for the
        /// Waku protocol
        /// </summary>
        public class WakuRelayProtocol : RelayProtocols
        {
            public override string Publish
            {
                get
                {
                    return "waku_publish";
                }
            }

            public override string BatchPublish
            {
                get
                {
                    return "waku_batchPublish";
                }
            }

            public override string Subscribe
            {
                get
                {
                    return "waku_subscribe";
                }
            }

            public override string BatchSubscribe
            {
                get
                {
                    return "waku_batchSubscribe";
                }
            }

            public override string Subscription
            {
                get
                {
                    return "waku_subscription";
                }
            }

            public override string Unsubscribe
            {
                get
                {
                    return "waku_unsubscribe";
                }
            }

            public override string BatchUnsubscribe
            {
                get
                {
                    return "waku_batchUnsubscribe";
                }
            }
        }

        /// <summary>
        /// A class that defines all RelayProtocol definitions for the
        /// Irn protocol
        /// </summary>
        public class IrnRelayProtocol : RelayProtocols
        {
            public override string Publish
            {
                get
                {
                    return "irn_publish";
                }
            }

            public override string BatchPublish
            {
                get
                {
                    return "irn_batchPublish";
                }
            }

            public override string Subscribe
            {
                get
                {
                    return "irn_subscribe";
                }
            }

            public override string BatchSubscribe
            {
                get
                {
                    return "irn_batchSubscribe";
                }
            }

            public override string Subscription
            {
                get
                {
                    return "irn_subscription";
                }
            }

            public override string Unsubscribe
            {
                get
                {
                    return "irn_unsubscribe";
                }
            }

            public override string BatchUnsubscribe
            {
                get
                {
                    return "irn_batchUnsubscribe";
                }
            }
        }

        /// <summary>
        /// A class that defines all RelayProtocol definitions for the
        /// Iridium protocol
        /// </summary>
        public class IridiumRelayProtocol : RelayProtocols
        {
            public override string Publish
            {
                get
                {
                    return "iridium_publish";
                }
            }

            public override string BatchPublish
            {
                get
                {
                    return "iridium_batchPublish";
                }
            }

            public override string Subscribe
            {
                get
                {
                    return "iridium_subscribe";
                }
            }

            public override string BatchSubscribe
            {
                get
                {
                    return "iridium_batchSubscribe";
                }
            }

            public override string Subscription
            {
                get
                {
                    return "iridium_subscription";
                }
            }

            public override string Unsubscribe
            {
                get
                {
                    return "iridium_unsubscribe";
                }
            }

            public override string BatchUnsubscribe
            {
                get
                {
                    return "iridium_batchUnsubscribe";
                }
            }
        }

        public static RelayProtocols DefaultProtocol => GetRelayProtocol(Default);
    }
}
