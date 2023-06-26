using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Heartbeat;
using WalletConnectSharp.Core.Models.Publisher;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Events;
using WalletConnectSharp.Network.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Core.Controllers
{
    /// <summary>
    /// The publisher module is responsible for publishing messages asynchronously. 
    /// </summary>
    public class Publisher : IPublisher
    {
        /// <summary>
        /// The EventDelegator this publisher module is using
        /// </summary>
        public EventDelegator Events { get; }
        
        /// <summary>
        /// The Relayer this publisher module uses to publish messages
        /// </summary>
        public IRelayer Relayer { get; }

        /// <summary>
        /// The name of this publisher module
        /// </summary>
        public string Name
        {
            get
            {
                return $"{Relayer.Name}-publisher";
            }
        }

        /// <summary>
        /// The context string this publisher module is using
        /// </summary>
        public string Context
        {
            get
            {
                return Name;
            }
        }

        protected Dictionary<string, PublishParams> queue = new Dictionary<string, PublishParams>();

        /// <summary>
        /// Create a new Publisher that uses the given IRelayer to publish messages to
        /// </summary>
        /// <param name="relayer">The IRelayer to publish messages to</param>
        public Publisher(IRelayer relayer)
        {
            Relayer = relayer;
            Events = new EventDelegator(this);
            
            RegisterEventListeners();
        }

        private void RegisterEventListeners()
        {
            Relayer.Core.HeartBeat.On<object>(HeartbeatEvents.Pulse, (_, __) => CheckQueue());
        }

        private async void CheckQueue()
        {
            // Unroll here so we don't deal with "collection modified" errors
            var keys = queue.Keys.ToArray();
            
            foreach (var key in keys)
            {
                if (!queue.ContainsKey(key)) continue;
                var @params = queue[key];
                
                var hash = HashUtils.HashMessage(@params.Message);
                await RpcPublish(@params.Topic, @params.Message, @params.Options.TTL, @params.Options.Tag, @params.Options.Relay);
                OnPublish(hash);
            }
        }

        private void OnPublish(string hash)
        {
            this.queue.Remove(hash);
        }

        protected Task RpcPublish(string topic, string message, long ttl, long tag, ProtocolOptions relay)
        {
            var api = RelayProtocols.GetRelayProtocol(relay.Protocol);
            var request = new RequestArguments<RelayPublishRequest>()
            {
                Method = api.Publish,
                Params = new RelayPublishRequest()
                {
                    Message = message,
                    Topic = topic,
                    TTL = ttl,
                    Tag = tag
                }
            };

            return this.Relayer.Request<RelayPublishRequest, object>(request, this);
        }

        /// <summary>
        /// Publish a messages to the given topic, optionally specifying publish options.
        /// </summary>
        /// <param name="topic">The topic to send the message in</param>
        /// <param name="message">The message to send</param>
        /// <param name="opts">(Optional) publish options specifying TTL and tag</param>
        public async Task Publish(string topic, string message, PublishOptions opts = null)
        {
            if (opts == null)
            {
                opts = new PublishOptions()
                {
                    Relay = new ProtocolOptions()
                    {
                        Protocol = RelayProtocols.Default
                    },
                    Tag = 0,
                    TTL = Clock.SIX_HOURS,
                };
            }
            else
            {
                if (opts.Relay == null)
                {
                    opts.Relay = new ProtocolOptions()
                    {
                        Protocol = RelayProtocols.Default
                    };
                }
            }

            var @params = new PublishParams()
            {
                Message = message,
                Options = opts,
                Topic = topic
            };

            var hash = HashUtils.HashMessage(message);
            queue.Add(hash, @params);
            try
            {
                await RpcPublish(topic, message, @params.Options.TTL, @params.Options.Tag, @params.Options.Relay)
                    .WithTimeout(TimeSpan.FromSeconds(10));
                this.Relayer.Events.Trigger(RelayerEvents.Publish, @params);
                OnPublish(hash);
            }
            catch (Exception e)
            {
                this.Relayer.Events.Trigger<object>(RelayerEvents.ConnectionStalled, new object());
                return;
            }
        }
    }
}
