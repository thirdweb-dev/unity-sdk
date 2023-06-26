using WalletConnectSharp.Common;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Crypto.Models;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Network.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Core.Interfaces
{
    /// <summary>
    /// An interface for a module that handles both typed message sending (requests, responses and errors) and handles
    /// typed message listening (requests and responses)
    /// </summary>
    public interface ITypedMessageHandler : IModule, IEvents
    {
        /// <summary>
        /// The <see cref="IRelayer"/> this module is using to send / listen for messages
        /// </summary>
        ICore Core { get; }
        
        /// <summary>
        /// Initialize this TypedMessageHandler module
        /// </summary>
        Task Init();
        
        /// <summary>
        /// Handle a specific request / response type and call the given callbacks for requests and responses. The
        /// response callback is only triggered when it originates from the request of the same type.
        /// </summary>
        /// <param name="requestCallback">The callback function to invoke when a request is received with the given request type</param>
        /// <param name="responseCallback">The callback function to invoke when a response is received with the given response type</param>
        /// <typeparam name="T">The request type to trigger the requestCallback for</typeparam>
        /// <typeparam name="TR">The response type to trigger the responseCallback for</typeparam>
        void HandleMessageType<T, TR>(Func<string, JsonRpcRequest<T>, Task> requestCallback,
            Func<string, JsonRpcResponse<TR>, Task> responseCallback);

        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcRequestOptionsAttribute"/> from
        /// either the type T1 or T2. T1 will take priority over T2.
        /// </summary>
        /// <typeparam name="T1">The first type to check for <see cref="RpcRequestOptionsAttribute"/></typeparam>
        /// <typeparam name="T2">The second type to check for <see cref="RpcRequestOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcRequestOptionsAttribute"/>
        /// from either type T1 or T2</returns>
        /// <exception cref="Exception">If no <see cref="RpcOptionsAttribute"/> is found in either type</exception>
        PublishOptions RpcRequestOptionsFromType<T1, T2>();

        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcRequestOptionsAttribute"/> from
        /// the given type T
        /// </summary>
        /// <typeparam name="T">The type to check for <see cref="RpcRequestOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcRequestOptionsAttribute"/>
        /// from the given type T</returns>
        /// <exception cref="Exception">If no <see cref="RpcOptionsAttribute"/> is found in the type T</exception>
        PublishOptions RpcRequestOptionsForType<T>();

        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcResponseOptionsAttribute"/> from
        /// either the type T1 or T2. T1 will take priority over T2.
        /// </summary>
        /// <typeparam name="T1">The first type to check for <see cref="RpcResponseOptionsAttribute"/></typeparam>
        /// <typeparam name="T2">The second type to check for <see cref="RpcResponseOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcResponseOptionsAttribute"/>
        /// from either type T1 or T2</returns>
        /// <exception cref="Exception">If no <see cref="RpcResponseOptionsAttribute"/> is found in either type</exception>
        PublishOptions RpcResponseOptionsFromTypes<T1, T2>();

        /// <summary>
        /// Build <see cref="PublishOptions"/> from an <see cref="RpcResponseOptionsAttribute"/> from
        /// the given type T
        /// </summary>
        /// <typeparam name="T">The type to check for <see cref="RpcResponseOptionsAttribute"/></typeparam>
        /// <returns><see cref="PublishOptions"/> constructed from the values found in the <see cref="RpcResponseOptionsAttribute"/>
        /// from the given type T</returns>
        /// <exception cref="Exception">If no <see cref="RpcResponseOptionsAttribute"/> is found in the type T</exception>
        PublishOptions RpcResponseOptionsForType<T>();

        /// <summary>
        /// Set the decode options that should be used whenever a message in the given
        /// topic is received. By default, no decode options are used when a message is received.
        /// Use this function to set decode options for Type1 messages inside a specific topic
        /// </summary>
        /// <param name="options">The decode options to use for all messages received in a specific topic</param>
        /// <param name="topic">The topic to set the given decode options for</param>
        void SetDecodeOptionsForTopic(DecodeOptions options, string topic);

        /// <summary>
        /// Get the decode options for the given topic, all messages received in the given topic
        /// will be decoded using these decode options. If no decode options are set for the given
        /// topic, then null is returned.
        /// </summary>
        /// <param name="topic">The topic to get decode options for</param>
        /// <returns>The decode options set for the given topic. If no decode options
        /// are set for the given topic, then null is returned.</returns>
        DecodeOptions DecodeOptionForTopic(string topic);

        /// <summary>
        /// Send a typed request message with the given request / response type pair T, TR to the given topic
        /// </summary>
        /// <param name="topic">The topic to send the request in</param>
        /// <param name="parameters">The typed request message to send</param>
        /// <param name="expiry">An override to specify how long this request will live for. If null is given, then expiry will be taken from either T or TR attributed options</param>
        /// <param name="options">(optional) Crypto Encoding options</param>
        /// <typeparam name="T">The request type</typeparam>
        /// <typeparam name="TR">The response type</typeparam>
        /// <returns>The id of the request sent</returns>
        Task<long> SendRequest<T, TR>(string topic, T parameters, long? expiry = null, EncodeOptions options = null);

        /// <summary>
        /// Send a typed response message with the given request / response type pair T, TR to the given topic
        /// </summary>
        /// <param name="id">The id of the request to respond to</param>
        /// <param name="topic">The topic to send the response in</param>
        /// <param name="result">The typed response message to send</param>
        /// <param name="options">(optional) Crypto Encoding options</param>
        /// <typeparam name="T">The request type</typeparam>
        /// <typeparam name="TR">The response type</typeparam>
        Task SendResult<T, TR>(long id, string topic, TR result, EncodeOptions options = null);

        /// <summary>
        /// Send an error response message with the given request / response type pair T, TR to the given topic
        /// </summary>
        /// <param name="id">The id of the request to respond to</param>
        /// <param name="topic">The topic to send the response in</param>
        /// <param name="error">The error response to send</param>
        /// <param name="options">(optional) Crypto Encoding options</param>
        /// <typeparam name="T">The request type</typeparam>
        /// <typeparam name="TR">The response type</typeparam>
        Task SendError<T, TR>(long id, string topic, Error error, EncodeOptions options = null);
    }
}
