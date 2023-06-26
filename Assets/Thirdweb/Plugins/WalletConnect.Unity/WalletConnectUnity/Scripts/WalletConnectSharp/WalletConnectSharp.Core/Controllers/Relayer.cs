using Newtonsoft.Json;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Core.Models.Subscriber;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Network.Websocket;
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
    /// The Relayer module handles the interaction with the WalletConnect relayer server.
    /// Each Relayer module uses a Publisher, Subscriber and a JsonRPCProvider.
    /// </summary>
    public class Relayer : IRelayer
    {
        private bool transportExplicityClosed = false;
        
        /// <summary>
        /// The default relay server URL used when no relay URL is given
        /// </summary>
        public static readonly string DEFAULT_RELAY_URL = "wss://relay.walletconnect.com";
        
        /// <summary>
        /// The EventDelegaor this Relayer module is using
        /// </summary>
        public EventDelegator Events { get; }

        /// <summary>
        /// The Name of this Relayer module
        /// </summary>
        public string Name
        {
            get
            {
                return $"{Core.Name}-relayer";
            }
        }

        /// <summary>
        /// The context string this Relayer module is using
        /// </summary>
        public string Context
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        /// The ICore module that is using this Relayer module
        /// </summary>
        public ICore Core { get; }
        
        /// <summary>
        /// The ISubscriber module that this Relayer module is using
        /// </summary>
        public ISubscriber Subscriber { get; }
        
        /// <summary>
        /// The IPublisher module that this Relayer module is using
        /// </summary>
        public IPublisher Publisher { get; }
        
        /// <summary>
        /// The IMessageTracker module that this Relayer module is using
        /// </summary>
        public IMessageTracker Messages { get; }
        
        /// <summary>
        /// The IJsonRpcProvider module that this Relayer module is using
        /// </summary>
        public IJsonRpcProvider Provider { get; private set; }
        
        /// <summary>
        /// Whether this Relayer is connected
        /// </summary>
        public bool Connected 
        {
            get
            {
                return this.Provider.Connection.Connected;
            }
        }

        /// <summary>
        /// Whether this Relayer is currently connecting
        /// </summary>
        public bool Connecting
        {
            get
            {
                return this.Provider.Connection.Connecting;
            }
        }

        public bool TransportExplicitlyClosed
        {
            get
            {
                return transportExplicityClosed;
            }
        }

        private string relayUrl;
        private string projectId;
        private bool initialized;
        private bool reconnecting = false;
        
        /// <summary>
        /// Create a new Relayer with the given RelayerOptions.
        /// </summary>
        /// <param name="opts">The options that must be specified. This includes the ICore module
        /// using this module, the RelayURL (optional) and the project Id</param>
        public Relayer(RelayerOptions opts)
        {
            Core = opts.Core;
            Events = new EventDelegator(this);
            Messages = new MessageTracker(Core);
            Subscriber = new Subscriber(this);
            Publisher = new Publisher(this);

            relayUrl = opts.RelayUrl;
            if (string.IsNullOrWhiteSpace(relayUrl))
            {
                relayUrl = DEFAULT_RELAY_URL;
            }

            projectId = opts.ProjectId;
        }
        
        /// <summary>
        /// Initialize this Relayer module. This will initialize all sub-modules
        /// and connect the backing IJsonRpcProvider.
        /// </summary>
        public async Task Init()
        {
            await CreateProvider();

            await Task.WhenAll(
                Messages.Init(), TransportOpen(), Subscriber.Init()
            );

            RegisterEventListeners();
            
            initialized = true;

#pragma warning disable CS4014
            Task.Run(async () =>
#pragma warning restore CS4014
            {
                await Task.Delay(TimeSpan.FromSeconds(10));

                if (this.Subscriber.Topics.Length == 0)
                {
                    // No topics subscribed to after init, closing transport
                    await this.TransportClose();
                    this.transportExplicityClosed = false;
                }
            });
        }

        protected virtual async Task CreateProvider()
        {
            var auth = await this.Core.Crypto.SignJwt(this.relayUrl);
            Provider = CreateProvider(auth);
            RegisterProviderEventListeners();
        }

        protected virtual IJsonRpcProvider CreateProvider(string auth)
        {
            return new JsonRpcProvider(
                new WebsocketConnection(
                    RelayUrl.FormatRelayRpcUrl(
                        relayUrl,
                        IRelayer.Protocol,
                        IRelayer.Version.ToString(),
                        SDKConstants.SDK_VERSION,
                        projectId,
                        auth
                    )
                )
            );
        }

        protected virtual void RegisterProviderEventListeners()
        {
            Provider.On<string>(ProviderEvents.RawRequestMessage, (sender, @event) =>
            {
                OnProviderPayload(@event.EventData);
            });
            
            Provider.On(ProviderEvents.Connect, () =>
            {
                Events.Trigger(RelayerEvents.Connect, new object());
            });
            
            Provider.On(ProviderEvents.Disconnect, async () =>
            {
                Events.Trigger(RelayerEvents.Disconnect, new object());

                if (this.transportExplicityClosed)
                    return;
                
                // Attempt to reconnect after one second
                await Task.Delay(1000);

                await RestartTransport();
            });

            Provider.On<object>(ProviderEvents.Error, (sender, @event) =>
            {
                Events.Trigger(RelayerEvents.Error, @event.EventData);
            });
        }

        protected virtual void RegisterEventListeners()
        {
            this.Events.ListenFor<object>(RelayerEvents.ConnectionStalled, async (sender, @event) =>
            {
                await this.RestartTransport();
            });
        }

        protected virtual async void OnProviderPayload(string payloadJson)
        {
            var payload = JsonConvert.DeserializeObject<JsonRpcPayload>(payloadJson);
            
            if (payload != null && payload.IsRequest && payload.Method.EndsWith("_subscription"))
            {
                var @event = JsonConvert.DeserializeObject<JsonRpcRequest<JsonRpcSubscriptionParams>>(payloadJson);

                var messageEvent = new MessageEvent()
                {
                    Message = @event.Params.Data.Message,
                    Topic = @event.Params.Data.Topic
                };

                await AcknowledgePayload(payload);
                await OnMessageEvent(messageEvent);
            }
        }

        protected virtual async Task<bool> ShouldIgnoreMessageEvent(MessageEvent messageEvent)
        {
            if (!(await Subscriber.IsSubscribed(messageEvent.Topic))) return true;

            var exists = Messages.Has(messageEvent.Topic, messageEvent.Message);
            return exists;
        }

        protected virtual Task RecordMessageEvent(MessageEvent messageEvent)
        {
            return Messages.Set(messageEvent.Topic, messageEvent.Message);
        }

        protected virtual async Task OnMessageEvent(MessageEvent messageEvent)
        {
            if (await ShouldIgnoreMessageEvent(messageEvent)) return;

            Events.Trigger(RelayerEvents.Message, messageEvent);
            await RecordMessageEvent(messageEvent);
        }

        protected virtual async Task AcknowledgePayload(JsonRpcPayload payload)
        {
            var response = new JsonRpcResponse<bool>()
            {
                Id = payload.Id,
                Result = true
            };
            await Provider.Connection.SendResult(response, this);
        }

        /// <summary>
        /// Publish a message to this Relayer in the given topic (optionally) specifying
        /// PublishOptions.
        /// </summary>
        /// <param name="topic">The topic to publish the message in</param>
        /// <param name="message">The message to publish</param>
        /// <param name="opts">(Optional) Publish options to specify TTL and tag</param>
        public async Task Publish(string topic, string message, PublishOptions opts = null)
        {
            IsInitialized();
            await Publisher.Publish(topic, message, opts);
            await RecordMessageEvent(new MessageEvent()
            {
                Topic = topic,
                Message = message
            });
        }

        /// <summary>
        /// Subscribe to a given topic optionally specifying Subscribe options
        /// </summary>
        /// <param name="topic">The topic to subscribe to</param>
        /// <param name="opts">(Optional) Subscribe options that specify protocol options</param>
        /// <returns></returns>
        public async Task<string> Subscribe(string topic, SubscribeOptions opts = null)
        {
            IsInitialized();
            var ids = this.Subscriber.TopicMap.Get(topic);
            if (ids.Length > 0)
            {
                return ids[0];
            }

            TaskCompletionSource<string> task1 = new TaskCompletionSource<string>();
            this.Subscriber.Once<ActiveSubscription>(Controllers.Subscriber.SubscriberEvents.Created, (sender, @event) =>
            {
                if (@event.EventData.Topic == topic)
                    task1.TrySetResult("");
            });

            return (await Task.WhenAll(
                task1.Task,
                this.Subscriber.Subscribe(topic, opts)
            ))[1];
        }

        /// <summary>
        /// Unsubscribe to a given topic optionally specify unsubscribe options
        /// </summary>
        /// <param name="topic">Tbe topic to unsubscribe to</param>
        /// <param name="opts">(Optional) Unsubscribe options specifying protocol options</param>
        /// <returns></returns>
        public Task Unsubscribe(string topic, UnsubscribeOptions opts = null)
        {
            IsInitialized();
            return Subscriber.Unsubscribe(topic, opts);
        }

        public async Task<TR> Request<T, TR>(IRequestArguments<T> request, object context = null)
        {
            await this.ToEstablishConnection();
            return await this.Provider.Request<T, TR>(request, context);
        }

        public async Task TransportClose()
        {
            transportExplicityClosed = true;
            if (Connected)
            {
                await this.Provider.Disconnect();
                this.Events.Trigger(RelayerEvents.TransportClosed, new object());
            }
        }

        public async Task TransportOpen(string relayUrl = null)
        {
            this.transportExplicityClosed = false;
            if (reconnecting) return;
            this.relayUrl = relayUrl ?? this.relayUrl;
            this.reconnecting = true;
            try
            {
                TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
                if (!this.initialized)
                {
                    task1.SetResult(true);
                }
                else
                {
                    this.Subscriber.Once<object>(Controllers.Subscriber.SubscriberEvents.Resubscribed,
                        (sender, @event) =>
                        {
                            task1.SetResult(true);
                        });
                }

                TaskCompletionSource<bool> task2 = new TaskCompletionSource<bool>();

                void RejectTransportOpen(object sender, GenericEvent<object> @event)
                {
                    task2.TrySetException(new Exception("closeTransport called before connection was established"));
                }

                async void Task2()
                {
                    this.Events.ListenForOnce<object>(RelayerEvents.TransportClosed, RejectTransportOpen);
                    try
                    {
                        await this.Provider.Connect().WithTimeout(TimeSpan.FromSeconds(5), "socket stalled");
                        task2.TrySetResult(true);
                    }
                    finally
                    {
                        this.Events.RemoveListener<object>(RelayerEvents.TransportClosed, RejectTransportOpen);
                    }
                }

                Task2();

                await Task.WhenAll(task1.Task, task2.Task);
            }
            catch (Exception e)
            {
                // TODO Check for system socket hang up message
                if (e.Message != "socket stalled")
                    throw;

                this.Events.Trigger(RelayerEvents.TransportClosed, new object());
            }
            finally
            {
                this.reconnecting = false;
            }
        }

        public async Task RestartTransport(string relayUrl = null)
        {
            if (this.transportExplicityClosed || this.reconnecting) return;
            this.relayUrl = relayUrl ?? this.relayUrl;
            if (this.Connected)
            {
                TaskCompletionSource<bool> task1 = new TaskCompletionSource<bool>();
                this.Provider.Once<bool>(ProviderEvents.Disconnect, (sender, @event) =>
                {
                    task1.SetResult(true);
                });

                await Task.WhenAll(task1.Task, this.TransportClose());
            }

            await CreateProvider();
            await TransportOpen();
        }

        protected virtual void IsInitialized()
        {
            if (!initialized)
            {
                throw WalletConnectException.FromType(ErrorType.NOT_INITIALIZED, Name);
            }
        }

        private async Task ToEstablishConnection()
        {
            if (Connected) return;
            if (Connecting)
            {
                // Check for connection
                while (Connecting)
                {
                    await Task.Delay(20);
                }

                if (!Connected && !Connecting)
                    throw new IOException("Could not establish connection");

                return;
            }

            await this.RestartTransport();
        }
    }
}
