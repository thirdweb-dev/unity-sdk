using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Sign.Interfaces
{
    /// <summary>
    /// An interface that represents functions the Sign client Engine can perform. These
    /// functions exist in both the Engine and in the Sign client. 
    /// </summary>
    public interface IEngineAPI
    {
        /// <summary>
        /// Get all pending session requests as an array
        /// </summary>
        PendingRequestStruct[] PendingSessionRequests { get; }
        
        /// <summary>
        /// Connect (a dApp) with the given ConnectOptions. At a minimum, you must specified a RequiredNamespace. 
        /// </summary>
        /// <param name="options"></param>
        /// <returns>Connection data that includes the session proposal URI as well as a
        /// way to await for a session approval</returns>
        Task<ConnectedData> Connect(ConnectOptions options);

        /// <summary>
        /// Pair (a wallet) with a peer (dApp) using the given uri. The uri must be in the correct
        /// format otherwise an exception will be thrown.
        /// </summary>
        /// <param name="uri">The URI to pair with</param>
        /// <returns>The proposal the connecting peer wants to connect using. You must approve or reject
        /// the proposal</returns>
        Task<ProposalStruct> Pair(string uri);

        /// <summary>
        /// Approve a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// </summary>
        /// <param name="proposalStruct">The proposal to approve</param>
        /// <param name="approvedAddresses">An array of address strings to connect to the session</param>
        /// <returns>Approval data, includes the topic of the session and a way to wait for approval acknowledgement</returns>
        Task<IApprovedData> Approve(ProposalStruct proposalStruct, params string[] approvedAddresses);

        /// <summary>
        /// Approve a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// Use <see cref="ProposalStruct.ApproveProposal(string, ProtocolOptions)"/> to generate an
        /// <see cref="ApproveParams"/> object, or use the alias function <see cref="IEngineAPI.Approve(ProposalStruct, string[])"/>
        /// </summary>
        /// <param name="params">Parameters for the approval. This usually comes from <see cref="ProposalStruct.ApproveProposal(string, ProtocolOptions)"/></param>
        /// <returns>Approval data, includes the topic of the session and a way to wait for approval acknowledgement</returns>
        Task<IApprovedData> Approve(ApproveParams @params);

        /// <summary>
        /// Reject a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// Use <see cref="ProposalStruct.RejectProposal(string)"/> or <see cref="ProposalStruct.RejectProposal(Error)"/>
        /// to generate a <see cref="RejectParams"/> object, or use the alias function <see cref="IEngineAPI.Reject(ProposalStruct, string)"/>
        /// </summary>
        /// <param name="params">The parameters of the rejection</param>
        /// <returns></returns>
        Task Reject(RejectParams @params);

        /// <summary>
        /// Reject a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// </summary>
        /// <param name="proposalStruct">The proposal to reject</param>
        /// <param name="message">A message explaining the reason for the rejection</param>
        Task Reject(ProposalStruct proposalStruct, string message = null);

        /// <summary>
        /// Reject a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// </summary>
        /// <param name="proposalStruct">The proposal to reject</param>
        /// <param name="error">An error explaining the reason for the rejection</param>
        Task Reject(ProposalStruct proposalStruct, Error error);

        /// <summary>
        /// Update a session, adding/removing additional namespaces in the given topic.
        /// </summary>
        /// <param name="topic">The topic to update</param>
        /// <param name="namespaces">The updated namespaces</param>
        /// <returns>A task that returns an interface that can be used to listen for acknowledgement of the updates</returns>
        Task<IAcknowledgement> Update(string topic, Namespaces namespaces);

        /// <summary>
        /// Extend a session in the given topic. 
        /// </summary>
        /// <param name="topic">The topic of the session to extend</param>
        /// <returns>A task that returns an interface that can be used to listen for acknowledgement of the extension</returns>
        Task<IAcknowledgement> Extend(string topic);

        /// <summary>
        /// Send a request to the session in the given topic with the request data T. You may (optionally) specify
        /// a chainId the request should be performed in. This function will await a response of type TR from the session.
        ///
        /// If no response is ever received, then a Timeout exception may be thrown.
        ///
        /// The type T MUST define the RpcMethodAttribute to tell the SDK what JSON RPC method to use for the given
        /// type T.
        /// Either type T or TR MUST define a RpcRequestOptions and RpcResponseOptions attribute to tell the SDK
        /// what options to use for the Request / Response.
        /// </summary>
        /// <param name="topic">The topic of the session to send the request in</param>
        /// <param name="data">The data of the request</param>
        /// <param name="chainId">An (optional) chainId the request should be performed in</param>
        /// <param name="expiry">An override to specify how long this request will live for. If null is given, then expiry will be taken from either T or TR attributed options</param>
        /// <typeparam name="T">The type of the request data. MUST define the RpcMethodAttribute</typeparam>
        /// <typeparam name="TR">The type of the response data.</typeparam>
        /// <returns>The response data as type TR</returns>
        Task<TR> Request<T, TR>(string topic, T data, string chainId = null, long? expiry = null);

        /// <summary>
        /// Send a response to a request to the session in the given topic with the response data TR. This function
        /// can be called directly, however it may be easier to use <see cref="TypedEventHandler{T, TR}.OnResponse"/> event
        /// to handle sending responses to specific requests. 
        /// </summary>
        /// <param name="topic">The topic of the session to respond in</param>
        /// <param name="response">The JSON RPC response to send</param>
        /// <typeparam name="T">The type of the request data</typeparam>
        /// <typeparam name="TR">The type of the response data</typeparam>
        Task Respond<T, TR>(string topic, JsonRpcResponse<TR> response);

        /// <summary>
        /// Emit an event to the session with the given topic with the given <see cref="EventData{T}"/>. You may
        /// optionally specify a chainId to specify where the event occured. 
        /// </summary>
        /// <param name="topic">The topic of the session to emit the event to</param>
        /// <param name="eventData">The event data for the event emitted</param>
        /// <param name="chainId">An (optional) chainId to specify where the event occured</param>
        /// <typeparam name="T">The type of the event data</typeparam>
        Task Emit<T>(string topic, EventData<T> eventData, string chainId = null);

        /// <summary>
        /// Send a ping to the session in the given topic
        /// </summary>
        /// <param name="topic">The topic of the session to send a ping to</param>
        Task Ping(string topic);

        /// <summary>
        /// Disconnect a session in the given topic with an (optional) error reason
        /// </summary>
        /// <param name="topic">The topic of the session to disconnect</param>
        /// <param name="reason">An (optional) error reason for the disconnect</param>
        Task Disconnect(string topic, Error reason = null);

        /// <summary>
        /// Find all sessions that have a namespace that match the given <see cref="RequiredNamespaces"/>
        /// </summary>
        /// <param name="requiredNamespaces">The required namespaces the session must have to be returned</param>
        /// <returns>All sessions that have a namespace that match the given <see cref="RequiredNamespaces"/></returns>
        SessionStruct[] Find(RequiredNamespaces requiredNamespaces);

        void HandleEventMessageType<T>(Func<string, JsonRpcRequest<SessionEvent<T>>, Task> requestCallback,
            Func<string, JsonRpcResponse<bool>, Task> responseCallback);
    }
}
