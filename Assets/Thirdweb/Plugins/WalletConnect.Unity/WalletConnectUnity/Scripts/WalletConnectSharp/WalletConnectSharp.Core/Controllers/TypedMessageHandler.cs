using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Crypto.Models;
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
    public class TypedMessageHandler : ITypedMessageHandler
    {
        private bool _initialized = false;
        private Dictionary<string, DecodeOptions> _decodeOptionsMap = new Dictionary<string, DecodeOptions>();

        public EventDelegator Events { get; }
        
        public ICore Core { get; }

        /// <summary>
        /// The name of this publisher module
        /// </summary>
        public string Name
        {
            get
            {
                return $"{Core.Name}-typedmessagehandler";
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

        public TypedMessageHandler(ICore core)
        {
            this.Core = core;
            this.Events = new EventDelegator(this);
        }
        
        public Task Init()
        {
            if (!_initialized)
            {
                this.Core.Relayer.On<MessageEvent>(RelayerEvents.Message, RelayerMessageCallback);
            }

            _initialized = true;
            return Task.CompletedTask;
        }
        
        async void RelayerMessageCallback(object sender, GenericEvent<MessageEvent> e)
        {
            var topic = e.EventData.Topic;
            var message = e.EventData.Message;

            var options = DecodeOptionForTopic(topic);

            var payload = await this.Core.Crypto.Decode<JsonRpcPayload>(topic, message, options);
            if (payload.IsRequest)
            {
                Events.Trigger($"request_{payload.Method}", e.EventData);
            }
            else if (payload.IsResponse)
            {
                Events.Trigger($"response_raw", new DecodedMessageEvent()
                {
                    Topic = topic,
                    Message = message,
                    Payload = payload
                });
            }
        }

        /// <summary>
        /// Handle a specific request / response type and call the given callbacks for requests and responses. The
        /// response callback is only triggered when it originates from the request of the same type.
        /// </summary>
        /// <param name="requestCallback">The callback function to invoke when a request is received with the given request type</param>
        /// <param name="responseCallback">The callback function to invoke when a response is received with the given response type</param>
        /// <typeparam name="T">The request type to trigger the requestCallback for</typeparam>
        /// <typeparam name="TR">The response type to trigger the responseCallback for</typeparam>
        public async void HandleMessageType<T, TR>(Func<string, JsonRpcRequest<T>, Task> requestCallback, Func<string, JsonRpcResponse<TR>, Task> responseCallback)
        {
            var method = RpcMethodAttribute.MethodForType<T>();
            var rpcHistory = await this.Core.History.JsonRpcHistoryOfType<T, TR>();
            
            async void RequestCallback(object sender, GenericEvent<MessageEvent> e)
            {
                if (requestCallback == null) return;
                
                var topic = e.EventData.Topic;
                var message = e.EventData.Message;
                
                var options = DecodeOptionForTopic(topic);

                var payload = await this.Core.Crypto.Decode<JsonRpcRequest<T>>(topic, message, options);
                
                (await this.Core.History.JsonRpcHistoryOfType<T, TR>()).Set(topic, payload, null);

                await requestCallback(topic, payload);
            }
            
            async void ResponseCallback(object sender, GenericEvent<MessageEvent> e)
            {
                if (responseCallback == null) return;
                
                var topic = e.EventData.Topic;
                var message = e.EventData.Message;
                
                var options = DecodeOptionForTopic(topic);

                var payload = await this.Core.Crypto.Decode<JsonRpcResponse<TR>>(topic, message, options);

                await (await this.Core.History.JsonRpcHistoryOfType<T, TR>()).Resolve(payload);

                await responseCallback(topic, payload);
            }

            async void InspectResponseRaw(object sender, GenericEvent<DecodedMessageEvent> e)
            {
                var topic = e.EventData.Topic;
                var message = e.EventData.Message;

                var payload = e.EventData.Payload;

                try
                {
                    var record = await rpcHistory.Get(topic, payload.Id);

                    // ignored if we can't find anything in the history
                    if (record == null) return;
                    var resMethod = record.Request.Method;
                    
                    // Trigger the true response event, which will trigger ResponseCallback
                    Events.Trigger($"response_{resMethod}", new MessageEvent()
                    {
                        Topic = topic,
                        Message = message
                    });
                }
                catch(WalletConnectException err)
                {
                    if (err.CodeType != ErrorType.NO_MATCHING_KEY)
                        throw;
                    
                    // ignored if we can't find anything in the history
                }
            }
            
            Events.ListenFor<MessageEvent>($"request_{method}", RequestCallback);
            
            Events.ListenFor<MessageEvent>($"response_{method}", ResponseCallback);
            
            // Handle response_raw in this context
            // This will allow us to examine response_raw in every typed context registered
            Events.ListenFor<DecodedMessageEvent>($"response_raw", InspectResponseRaw);
        }

        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcRequestOptionsAttribute"/> from
        /// either the type T1 or T2. T1 will take priority over T2.
        /// </summary>
        /// <typeparam name="T1">The first type to check for <see cref="RpcRequestOptionsAttribute"/></typeparam>
        /// <typeparam name="T2">The second type to check for <see cref="RpcRequestOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcRequestOptionsAttribute"/>
        /// from either type T1 or T2</returns>
        /// <exception cref="Exception">If no <see cref="RpcOptionsAttribute"/> is found in either type</exception>
        public PublishOptions RpcRequestOptionsFromType<T1, T2>()
        {
            var opts = RpcRequestOptionsForType<T1>();
            if (opts == null)
            {
                opts = RpcRequestOptionsForType<T2>();
                if (opts == null)
                {
                    throw new Exception($"No RpcRequestOptions attribute found in either {typeof(T1).FullName} or {typeof(T2).FullName}!");
                }
            }

            return opts;
        }

        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcRequestOptionsAttribute"/> from
        /// the given type T
        /// </summary>
        /// <typeparam name="T">The type to check for <see cref="RpcRequestOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcRequestOptionsAttribute"/>
        /// from the given type T</returns>
        /// <exception cref="Exception">If no <see cref="RpcOptionsAttribute"/> is found in the type T</exception>
        public PublishOptions RpcRequestOptionsForType<T>()
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(RpcRequestOptionsAttribute), true);
            if (attributes.Length > 1)
                throw new Exception($"Type {typeof(T).FullName} has multiple RpcRequestOptions attributes!");
            if (attributes.Length == 0)
                return null;

            var opts = attributes.Cast<RpcRequestOptionsAttribute>().First();

            return new PublishOptions()
            {
                Tag = opts.Tag,
                TTL = opts.TTL
            };
        }

        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcResponseOptionsAttribute"/> from
        /// either the type T1 or T2. T1 will take priority over T2.
        /// </summary>
        /// <typeparam name="T1">The first type to check for <see cref="RpcResponseOptionsAttribute"/></typeparam>
        /// <typeparam name="T2">The second type to check for <see cref="RpcResponseOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcResponseOptionsAttribute"/>
        /// from either type T1 or T2</returns>
        /// <exception cref="Exception">If no <see cref="RpcResponseOptionsAttribute"/> is found in either type</exception>
        public PublishOptions RpcResponseOptionsFromTypes<T1, T2>()
        {
            var opts = RpcResponseOptionsForType<T1>();
            if (opts != null)
            {
                return opts;
            }

            opts = RpcResponseOptionsForType<T2>();
            if (opts == null)
            {
                throw new Exception($"No RpcResponseOptions attribute found in either {typeof(T1).FullName} or {typeof(T2).FullName}!");
            }

            return opts;
        }
        
        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcResponseOptionsAttribute"/> from
        /// the given type T
        /// </summary>
        /// <typeparam name="T">The type to check for <see cref="RpcResponseOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcResponseOptionsAttribute"/>
        /// from the given type T</returns>
        /// <exception cref="Exception">If no <see cref="RpcResponseOptionsAttribute"/> is found in the type T</exception>
        public PublishOptions RpcResponseOptionsForType<T>()
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(RpcResponseOptionsAttribute), true);
            if (attributes.Length > 1)
                throw new Exception($"Type {typeof(T).FullName} has multiple RpcResponseOptions attributes!");
            if (attributes.Length == 0)
                return null;

            var opts = attributes.Cast<RpcResponseOptionsAttribute>().First();

            return new PublishOptions()
            {
                Tag = opts.Tag,
                TTL = opts.TTL
            };
        }

        public void SetDecodeOptionsForTopic(DecodeOptions options, string topic)
        {
            _decodeOptionsMap.Add(topic, options);
        }

        public DecodeOptions DecodeOptionForTopic(string topic)
        {
            if (_decodeOptionsMap.ContainsKey(topic))
                return _decodeOptionsMap[topic];
            return null;
        }

        /// <summary>
        /// Send a typed request message with the given request / response type pair T, TR to the given topic
        /// </summary>
        /// <param name="topic">The topic to send the request in</param>
        /// <param name="parameters">The typed request message to send</param>
        /// <param name="expiry">An override to specify how long this request will live for. If null is given, then expiry will be taken from either T or TR attributed options</param>
        /// <typeparam name="T">The request type</typeparam>
        /// <typeparam name="TR">The response type</typeparam>
        /// <returns>The id of the request sent</returns>
        public async Task<long> SendRequest<T, TR>(string topic, T parameters, long? expiry = null, EncodeOptions options = null)
        {
            var method = RpcMethodAttribute.MethodForType<T>();

            var payload = new JsonRpcRequest<T>(method, parameters);

            var message = await this.Core.Crypto.Encode(topic, payload, options);

            var opts = RpcRequestOptionsFromType<T, TR>();

            if (expiry != null)
            {
                opts.TTL = (long)expiry;
            }
            
            (await this.Core.History.JsonRpcHistoryOfType<T, TR>()).Set(topic, payload, null);

            // await is intentionally omitted here because of a possible race condition
            // where a response is received before the publish call is resolved
#pragma warning disable CS4014
            this.Core.Relayer.Publish(topic, message, opts);
#pragma warning restore CS4014

            return payload.Id;
        }

        /// <summary>
        /// Send a typed response message with the given request / response type pair T, TR to the given topic
        /// </summary>
        /// <param name="id">The id of the request to respond to</param>
        /// <param name="topic">The topic to send the response in</param>
        /// <param name="result">The typed response message to send</param>
        /// <typeparam name="T">The request type</typeparam>
        /// <typeparam name="TR">The response type</typeparam>
        public async Task SendResult<T, TR>(long id, string topic, TR result, EncodeOptions options = null)
        {
            var payload = new JsonRpcResponse<TR>(id, null, result);
            var message = await this.Core.Crypto.Encode(topic, payload, options);
            var opts = RpcResponseOptionsFromTypes<T, TR>();
            await this.Core.Relayer.Publish(topic, message, opts);
            await (await this.Core.History.JsonRpcHistoryOfType<T, TR>()).Resolve(payload);
        }

        /// <summary>
        /// Send an error response message with the given request / response type pair T, TR to the given topic
        /// </summary>
        /// <param name="id">The id of the request to respond to</param>
        /// <param name="topic">The topic to send the response in</param>
        /// <param name="error">The error response to send</param>
        /// <typeparam name="T">The request type</typeparam>
        /// <typeparam name="TR">The response type</typeparam>
        public async Task SendError<T, TR>(long id, string topic, Error error, EncodeOptions options = null)
        {
            var payload = new JsonRpcResponse<TR>(id, error, default);
            var message = await this.Core.Crypto.Encode(topic, payload, options);
            var opts = RpcResponseOptionsFromTypes<T, TR>();
            await this.Core.Relayer.Publish(topic, message, opts);
            await (await this.Core.History.JsonRpcHistoryOfType<T, TR>()).Resolve(payload);
        }
    }
}
