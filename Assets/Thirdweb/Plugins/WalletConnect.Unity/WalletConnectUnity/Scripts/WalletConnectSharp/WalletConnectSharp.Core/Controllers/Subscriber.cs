using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Heartbeat;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Core.Models.Subscriber;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
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
    /// This module handles both subscribing to events as well as keeping track
    /// of active and pending subscriptions. It will also resubscribe to topics if
    /// the backing Relayer connection disconnects
    /// </summary>
    public class Subscriber : ISubscriber
    {
        /// <summary>
        /// Constants that define eventIds for the Subscriber events
        /// </summary>
        public static class SubscriberEvents
        {
            public static readonly string Created = "subscription_created";
            public static readonly string Deleted = "subscription_deleted";
            public static readonly string Expired = "subscription_expired";
            public static readonly string Disabled = "subscription_disabled";
            public static readonly string Sync = "subscription_sync";
            public static readonly string Resubscribed = "subscription_resubscribed";
        }
        
        /// <summary>
        /// The EventDelegator this module is using
        /// </summary>
        public EventDelegator Events { get; }

        /// <summary>
        /// The name of this Subscriber
        /// </summary>
        public string Name
        {
            get
            {
                return $"{_relayer.Name}-subscription";
            }
        }

        /// <summary>
        /// The context string for this module
        /// </summary>
        public string Context
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        /// The version of this module
        /// </summary>
        public string Version
        {
            get
            {
                return "0.3";
            }
        }

        private Dictionary<string, ActiveSubscription> _subscriptions = new Dictionary<string, ActiveSubscription>();
        private Dictionary<string, PendingSubscription> pending = new Dictionary<string, PendingSubscription>();
        private TaskCompletionSource<bool> restartTask = null;

        public bool RestartInProgress
        {
            get
            {
                return restartTask != null && !restartTask.Task.IsCompleted;
            }
        }

        /// <summary>
        /// A dictionary of active subscriptions where the key is the id of the Subscription
        /// </summary>
        public IReadOnlyDictionary<string, ActiveSubscription> Subscriptions
        {
            get
            {
                return _subscriptions;
            }
        }

        private TopicMap _topicMap = new TopicMap();

        /// <summary>
        /// A subscription mapping of Topics => Subscription ids
        /// </summary>
        public ISubscriberMap TopicMap
        {
            get
            {
                return _topicMap;
            }
        }

        /// <summary>
        /// The number of active subscriptions 
        /// </summary>
        public int Length
        {
            get
            {
                return _subscriptions.Count;
            }
        }

        /// <summary>
        /// An array of active subscription Ids
        /// </summary>
        public string[] Ids
        {
            get
            {
                return _subscriptions.Keys.ToArray();
            }
        }

        /// <summary>
        /// An array of active Subscriptions
        /// </summary>
        public ActiveSubscription[] Values
        {
            get
            {
                return _subscriptions.Values.ToArray();
            }
        }

        /// <summary>
        /// An array of topics that are currently subscribed
        /// </summary>
        public string[] Topics
        {
            get
            {
                return _topicMap.Topics;
            }
        }

        /// <summary>
        /// The Storage key this module is using to store subscriptions
        /// </summary>
        public string StorageKey
        {
            get
            {
                return WalletConnectCore.STORAGE_PREFIX + Version + "//" + Name;
            }
        }

        private IRelayer _relayer;
        private bool initialized;
        private string clientId;
        private ActiveSubscription[] cached = Array.Empty<ActiveSubscription>();

        /// <summary>
        /// Create a new Subscriber module using a backing Relayer
        /// </summary>
        /// <param name="relayer">The relayer to use to subscribe to topics</param>
        public Subscriber(IRelayer relayer)
        {
            _relayer = relayer;
            
            Events = new EventDelegator(this);
        }
        
        /// <summary>
        /// Initialize this Subscriber, which will restore + resubscribe to all active subscriptions found
        /// in storage
        /// </summary>
        public async Task Init()
        {
            if (!initialized)
            {
                await Restart();
                RegisterEventListeners();
                OnEnabled();
                this.clientId = await this._relayer.Core.Crypto.GetClientId();
            }
        }

        private async Task Restart()
        {
            this.restartTask = new TaskCompletionSource<bool>();
            await Restore();
            await Reset();
            this.restartTask.SetResult(true);
        }

        protected virtual void RegisterEventListeners()
        {
            _relayer.Core.HeartBeat.On<object>(HeartbeatEvents.Pulse, (sender, @event) =>
            {
                CheckPending();
            });
            
            _relayer.Provider.On<object>(ProviderEvents.Connect, (sender, @event) =>
            {
                OnConnect();
            });
            
            _relayer.Provider.On<object>(ProviderEvents.Disconnect, (sender, @event) =>
            {
                OnDisconnect();
            });
            
            this.On<object>(SubscriberEvents.Created, AsyncPersist);

            this.On<object>(SubscriberEvents.Deleted, AsyncPersist);
        }

        protected virtual async void AsyncPersist(object sender, GenericEvent<object> @event)
        {
            await Persist();
        }
        
        
        protected virtual async Task Persist()
        {
            await SetRelayerSubscriptions(Values);
            Events.Trigger(SubscriberEvents.Sync, new object());
        }

        protected virtual async Task<ActiveSubscription[]> GetRelayerSubscriptions()
        {
            if (await _relayer.Core.Storage.HasItem(StorageKey))
                return await _relayer.Core.Storage.GetItem<ActiveSubscription[]>(StorageKey);

            return Array.Empty<ActiveSubscription>();
        }

        protected virtual async Task SetRelayerSubscriptions(ActiveSubscription[] subscriptions)
        {
            await _relayer.Core.Storage.SetItem(StorageKey, subscriptions);
        }

        protected virtual async Task Restore()
        {
            var persisted = await GetRelayerSubscriptions();

            if (persisted.Length == 0) return;

            if (Subscriptions.Count > 0)
            {
                throw WalletConnectException.FromType(ErrorType.RESTORE_WILL_OVERRIDE, Name);
            }

            cached = persisted;
        }

        protected virtual async void CheckPending()
        {
            if (_relayer.TransportExplicitlyClosed)
                return;


            await BatchSubscribe(pending.Values.ToArray());
        }

        protected virtual async Task Reset()
        {
            if (cached.Length > 0)
            {
                var batches = cached.Batch(500);
                foreach (var batch in batches)
                {
                    await this.BatchSubscribe(batch.ToArray());
                }
            }
            
            this.Events.Trigger(SubscriberEvents.Resubscribed, new object());
        }

        protected virtual async Task Resubscribe(ActiveSubscription subscription)
        {
            if (!Ids.Contains(subscription.Id))
            {
                var @params = new PendingSubscription()
                {
                    Relay = subscription.Relay,
                    Topic = subscription.Topic
                };

                if (pending.ContainsKey(@params.Topic))
                    pending.Remove(@params.Topic);
                
                pending.Add(@params.Topic, @params);

                var id = await RpcSubscribe(@params.Topic, @params.Relay);
                OnResubscribe(id, @params);
            }
        }

        protected virtual async Task<string> RpcSubscribe(string topic, ProtocolOptions relay)
        {
            var api = RelayProtocols.GetRelayProtocol(relay.Protocol);
            var request = new RequestArguments<JsonRpcSubscriberParams>()
            {
                Method = api.Subscribe,
                Params = new JsonRpcSubscriberParams()
                {
                    Topic = topic
                }
            };
            
            var subscribe = _relayer.Request<JsonRpcSubscriberParams, string>(request);
            await subscribe.WithTimeout(20000);

            return HashUtils.HashMessage(topic + this.clientId);
        }

        protected virtual Task RpcUnsubscribe(string topic, string id, ProtocolOptions relay)
        {
            var api = RelayProtocols.GetRelayProtocol(relay.Protocol);
            var request = new RequestArguments<JsonRpcUnsubscribeParams>()
            {
                Method = api.Unsubscribe,
                Params = new JsonRpcUnsubscribeParams()
                {
                    Id = id,
                    Topic = topic
                }
            };

            return _relayer.Request<JsonRpcUnsubscribeParams, object>(request);
        }

        protected virtual void OnEnabled()
        {
            cached = Array.Empty<ActiveSubscription>();
            initialized = true;
        }

        protected virtual void OnDisconnect()
        {
            OnDisable();
        }

        protected virtual void OnDisable()
        {
            cached = Values;
            _subscriptions.Clear();
            _topicMap.Clear();
            initialized = false;
        }

        protected virtual async void OnConnect()
        {
            if (RestartInProgress) return;
            
            await Restart();
            OnEnabled();
        }

        private Task RestartToComplete()
        {
            if (!RestartInProgress) return Task.CompletedTask;

            return restartTask.Task;
        }

        protected virtual void OnSubscribe(string id, PendingSubscription @params)
        {
            SetSubscription(id, new ActiveSubscription()
            {
                Id = id,
                Relay = @params.Relay,
                Topic = @params.Topic
            });

            pending.Remove(@params.Topic);
        }

        protected virtual void OnResubscribe(string id, PendingSubscription @params)
        {
            AddSubscription(id, new ActiveSubscription()
            {
                Id = id,
                Relay = @params.Relay,
                Topic = @params.Topic
            });

            pending.Remove(@params.Topic);
        }

        protected virtual async Task OnUnsubscribe(string topic, string id, Error reason)
        {
            // TODO Figure out how to do this
            //Events.RemoveListener(id);

            if (HasSubscription(id, topic))
            {
                DeleteSubscription(id, reason);
            }
            
            await _relayer.Messages.Delete(topic);
        }

        protected virtual void SetSubscription(string id, ActiveSubscription subscription)
        {
            if (_subscriptions.ContainsKey(id)) return;
            
            AddSubscription(id, subscription);
        }

        protected virtual void AddSubscription(string id, ActiveSubscription subscription)
        {
            if (_subscriptions.ContainsKey(id))
                _subscriptions.Remove(id);
            
            _subscriptions.Add(id, subscription);
            _topicMap.Set(subscription.Topic, id);
            Events.Trigger(SubscriberEvents.Created, subscription);
        }

        protected virtual Task UnsubscribeByTopic(string topic, UnsubscribeOptions opts = null)
        {
            if (opts == null)
            {
                opts = new UnsubscribeOptions()
                {
                    Relay = new ProtocolOptions()
                    {
                        Protocol = RelayProtocols.Default
                    }
                };
            }

            var ids = TopicMap.Get(topic);

            return Task.WhenAll(
                ids.Select(id => UnsubscribeById(topic, id, opts))
            );
        }

        protected virtual void DeleteSubscription(string id, Error reason)
        {
            var subscription = GetSubscription(id);
            _subscriptions.Remove(id);
            _topicMap.Delete(subscription.Topic, id);
            Events.Trigger(SubscriberEvents.Deleted, new DeletedSubscription()
            {
                Id = id,
                Reason = reason,
                Relay = subscription.Relay,
                Topic = subscription.Topic
            });
        }

        protected virtual async Task UnsubscribeById(string topic, string id, UnsubscribeOptions opts)
        {
            if (opts == null)
            {
                opts = new UnsubscribeOptions()
                {
                    Id = id,
                    Relay = new ProtocolOptions()
                    {
                        Protocol = RelayProtocols.Default
                    }
                };
            }

            await RpcUnsubscribe(topic, id, opts.Relay);
            Error reason = null;
            await OnUnsubscribe(topic, id, reason);
        }

        protected virtual ActiveSubscription GetSubscription(string id)
        {
            if (!_subscriptions.ContainsKey(id))
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY, Name + ": " + id);

            return _subscriptions[id];
        }

        protected virtual bool HasSubscription(string id, string topic)
        {
            var result = false;
            try
            {
                var subscriptions = GetSubscription(id);
                result = subscriptions.Topic == topic;
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        }

        protected virtual void IsInitialized()
        {
            if (!initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, Name);
            }
        }

        /// <summary>
        /// Subscribe to a new topic with (optional) SubscribeOptions
        /// </summary>
        /// <param name="topic">The topic to subscribe to</param>
        /// <param name="opts">Options to determine the protocol to use for subscribing</param>
        /// <returns>The subscription id</returns>
        public async Task<string> Subscribe(string topic, SubscribeOptions opts = null)
        {
            await RestartToComplete();
            
            if (opts == null)
            {
                opts = new SubscribeOptions()
                {
                    Relay = new ProtocolOptions()
                    {
                        Protocol = RelayProtocols.Default
                    }
                };
            }
            
            IsInitialized();

            var @params = new PendingSubscription()
            {
                Relay = opts.Relay,
                Topic = topic
            };
            
            pending.Add(topic, @params);
            var id = await RpcSubscribe(topic, @params.Relay);
            OnSubscribe(id, @params);
            return id;
        }

        /// <summary>
        /// Unsubscribe to a given topic with optional UnsubscribeOptions
        /// </summary>
        /// <param name="topic">The topic to unsubscribe from</param>
        /// <param name="opts">The options to specify the subscription id as well as protocol options</param>
        public async Task Unsubscribe(string topic, UnsubscribeOptions opts = null)
        {
            await RestartToComplete();
            
            IsInitialized();

            if (opts != null && !string.IsNullOrWhiteSpace(opts.Id))
            {
                await UnsubscribeById(topic, opts.Id, opts);
            }
            else
            {
                await UnsubscribeByTopic(topic, opts);
            }
        }

        /// <summary>
        /// Determines whether the given topic is subscribed or not
        /// </summary>
        /// <param name="topic">The topic to check</param>
        /// <returns>Return true if the topic is subscribed, false otherwise</returns>
        public Task<bool> IsSubscribed(string topic)
        {
            if (Topics.Contains(topic)) return Task.FromResult(true);

            return Task.Run(async delegate()
            {
                var startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

                while (true)
                {
                    if (!pending.ContainsKey(topic) && Topics.Contains(topic))
                    {
                        return true;
                    }

                    var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                    if (currentTime - startTime >= 5)
                    {
                        return false;
                    }

                    await Task.Delay(20);
                }
            });
        }

        protected virtual async Task<string[]> RpcBatchSubscribe(string[] topics, ProtocolOptions relay)
        {
            if (topics.Length == 0) return Array.Empty<string>();

            var api = RelayProtocols.GetRelayProtocol(relay.Protocol);
            var request = new RequestArguments<BatchSubscribeParams>()
            {
                Method = api.BatchSubscribe,
                Params = new BatchSubscribeParams() { Topics = topics }
            };
            try
            {
                return await this._relayer.Request<BatchSubscribeParams, string[]>(request)
                    .WithTimeout(TimeSpan.FromSeconds(10));
            }
            catch (Exception e)
            {
                this._relayer.Events.Trigger(RelayerEvents.ConnectionStalled, new object());
                throw;
            }
        }

        protected virtual async Task BatchSubscribe(PendingSubscription[] subscriptions)
        {
            if (subscriptions.Length == 0) return;
            var topics = subscriptions.Select(s => s.Topic).ToArray();
            var relay = subscriptions[0].Relay;
            var result = await this.RpcBatchSubscribe(topics, relay);
            OnBatchSubscribe(result.Select((r, i) => new ActiveSubscription()
            {
                Id = r,
                Relay = relay,
                Topic = topics[i]
            }).ToArray());
        }

        private void OnBatchSubscribe(ActiveSubscription[] subscriptions)
        {
            if (subscriptions.Length == 0) return;
            foreach (var sub in subscriptions)
            {
                SetSubscription(sub.Id, sub);
                this.pending.Remove(sub.Topic);
            }
        }
    }
}
