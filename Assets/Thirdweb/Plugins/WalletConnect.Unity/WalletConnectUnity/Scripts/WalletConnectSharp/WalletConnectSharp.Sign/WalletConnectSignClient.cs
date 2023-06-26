using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Controllers;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Crypto;
using WalletConnectSharp.Events;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Controllers;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using WalletConnectSharp.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Sign
{
    /// <summary>
    /// The main entry point to the SDK. Create a new instance of this class
    /// using the static <see cref="Init"/> function. You will first need to
    /// create <see cref="SignClientOptions"/>
    /// </summary>
    public class WalletConnectSignClient : ISignClient
    {
        /// <summary>
        /// The protocol ALL Sign Client will use as a protocol string
        /// </summary>
        public static readonly string PROTOCOL = "wc";
        
        /// <summary>
        /// The protocol version ALL Sign Client use
        /// </summary>
        public static readonly int VERSION = 2;
        
        /// <summary>
        /// The base context string ALL Sign Client use
        /// </summary>
        public static readonly string CONTEXTPOSTFIX = "client";

        /// <summary>
        /// The storage key prefix this Sign Client will use when storing data 
        /// </summary>
        public static readonly string StoragePrefix = $"{PROTOCOL}@{VERSION}:{CONTEXTPOSTFIX}";

        /// <summary>
        /// The name of this Sign Client module
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The context string for this Sign Client module
        /// </summary>
        public string Context { get; }

        /// <summary>
        /// The <see cref="EventDelegator"/> this Sign Client module will use
        /// to trigger events. Listen to Client events using this.
        /// </summary>
        public EventDelegator Events { get; }
        
        /// <summary>
        /// The Metadata for this instance of the Sign Client module
        /// </summary>
        public Metadata Metadata { get; }
        
        /// <summary>
        /// The <see cref="ICore"/> module this Sign Client module is using
        /// </summary>
        public ICore Core { get; }
        
        /// <summary>
        /// The <see cref="IEngine"/> module this Sign Client module is using. Used to do all
        /// protocol activities behind the scenes, should not be used directly.
        /// </summary>
        public IEngine Engine { get; }
        
        /// <summary>
        /// The <see cref="IPairingStore"/> module this Sign Client module is using. Used for storing pairing data
        /// </summary>
        public IPairingStore PairingStore { get; }
        
        /// <summary>
        /// The <see cref="ISession"/> module this Sign Client module is using. Used for storing session data
        /// </summary>
        public ISession Session { get; }
        
        /// <summary>
        /// The <see cref="IProposal"/> module this Sign Client module is using. Used for storing proposal data
        /// </summary>
        public IProposal Proposal { get; }

        public IPendingRequests PendingRequests { get; }

        /// <summary>
        /// The <see cref="SignClientOptions"/> this Sign Client was initialized with. 
        /// </summary>
        public SignClientOptions Options { get; }

        /// <summary>
        /// The protocol this Sign Client is using as a protocol string
        /// </summary>
        public string Protocol
        {
            get
            {
                return PROTOCOL;
            }
        }

        /// <summary>
        /// The protocol version this Sign Client is using
        /// </summary>
        public int Version
        {
            get
            {
                return VERSION;
            }
        }
        
        
        public PendingRequestStruct[] PendingSessionRequests
        {
            get
            {
                return Engine.PendingSessionRequests;
            }
        }

        /// <summary>
        /// Create a new <see cref="WalletConnectSignClient"/> instance with the given <see cref="SignClientOptions"/>
        /// and initialize it.
        /// </summary>
        /// <param name="options">The options to initialize the new <see cref="WalletConnectSignClient"/> with</param>
        /// <returns>A new and fully initialized <see cref="WalletConnectSignClient"/></returns>
        public static async Task<WalletConnectSignClient> Init(SignClientOptions options)
        {
            var client = new WalletConnectSignClient(options);
            await client.Initialize();

            return client;
        }

        private WalletConnectSignClient(SignClientOptions options)
        {
            if (options == null || options.Metadata == null)
                throw new ArgumentException("The Metadata field must be set in the SignClientOptions object");
            else
                Metadata = options.Metadata;
            
            Options = options;

            if (string.IsNullOrWhiteSpace(options.Name))
            {
                if (!string.IsNullOrWhiteSpace(options.Metadata.Name))
                    options.Name = $"{Metadata.Name}-{CONTEXTPOSTFIX}";
                else
                    throw new ArgumentException("The Name field in Metadata must be set");
            }
            
            Name = options.Name;

            if (string.IsNullOrWhiteSpace(options.BaseContext))
                Context = $"{Metadata.Name}-{CONTEXTPOSTFIX}";
            else
                Context = options.BaseContext;

            // Setup storage
            if (options.Storage == null)
            {
                var storage = new FileSystemStorage();
                options.Storage = storage;

                // If keychain is also not set, use the same storage instance
                options.KeyChain ??= new KeyChain(storage);
            }

            if (options.Core != null)
                Core = options.Core;
            else
                Core = new WalletConnectCore(options);

            Events = new EventDelegator(this);

            PendingRequests = new PendingRequests(Core);
            PairingStore = new PairingStore(Core);
            Session = new Session(Core);
            Proposal = new Proposal(Core);
            Engine = new Engine(this);
        }

        /// <summary>
        /// Connect (a dApp) with the given ConnectOptions. At a minimum, you must specified a RequiredNamespace. 
        /// </summary>
        /// <param name="options"></param>
        /// <returns>Connection data that includes the session proposal URI as well as a
        /// way to await for a session approval</returns>
        public Task<ConnectedData> Connect(ConnectOptions options)
        {
            return Engine.Connect(options);
        }

        /// <summary>
        /// Pair (a wallet) with a peer (dApp) using the given uri. The uri must be in the correct
        /// format otherwise an exception will be thrown.
        /// </summary>
        /// <param name="uri">The URI to pair with</param>
        /// <returns>The proposal the connecting peer wants to connect using. You must approve or reject
        /// the proposal</returns>
        public Task<ProposalStruct> Pair(string uri)
        {
            return Engine.Pair(uri);
        }

        /// <summary>
        /// Approve a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// Use <see cref="ProposalStruct.ApproveProposal(string[], ProtocolOptions)"/> to generate an
        /// <see cref="ApproveParams"/> object, or use the alias function <see cref="IEngineAPI.Approve(ProposalStruct, string[])"/>
        /// </summary>
        /// <param name="params">Parameters for the approval. This usually comes from <see cref="ProposalStruct.ApproveProposal(string, ProtocolOptions)"/></param>
        /// <returns>Approval data, includes the topic of the session and a way to wait for approval acknowledgement</returns>
        public Task<IApprovedData> Approve(ApproveParams @params)
        {
            return Engine.Approve(@params);
        }

        /// <summary>
        /// Approve a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// </summary>
        /// <param name="proposalStruct">The proposal to approve</param>
        /// <param name="approvedAddresses">An array of address strings to connect to the session</param>
        /// <returns>Approval data, includes the topic of the session and a way to wait for approval acknowledgement</returns>
        public Task<IApprovedData> Approve(ProposalStruct proposalStruct, params string[] approvedAddresses)
        {
            return Approve(proposalStruct.ApproveProposal(approvedAddresses));
        }

        /// <summary>
        /// Reject a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// Use <see cref="ProposalStruct.RejectProposal(string)"/> or <see cref="ProposalStruct.RejectProposal(Error)"/>
        /// to generate a <see cref="RejectParams"/> object, or use the alias function <see cref="IEngineAPI.Reject(ProposalStruct, string)"/>
        /// </summary>
        /// <param name="params">The parameters of the rejection</param>
        /// <returns></returns>
        public Task Reject(RejectParams @params)
        {
            return Engine.Reject(@params);
        }
        
        /// <summary>
        /// Reject a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// </summary>
        /// <param name="proposalStruct">The proposal to reject</param>
        /// <param name="message">A message explaining the reason for the rejection</param>
        public Task Reject(ProposalStruct proposalStruct, string message = null)
        {
            if (proposalStruct.Id == null)
                throw new ArgumentException("No proposal Id given");

            if (message == null)
                message = "Proposal denied by remote host";

            return Reject(proposalStruct, new Error()
            {
                Message = message,
                Code = (long) ErrorType.USER_DISCONNECTED,
            });
        }
        
        /// <summary>
        /// Reject a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// </summary>
        /// <param name="proposalStruct">The proposal to reject</param>
        /// <param name="error">An error explaining the reason for the rejection</param>
        public Task Reject(ProposalStruct proposalStruct, Error error)
        {
            if (proposalStruct.Id == null)
                throw new ArgumentException("No proposal Id given");

            var rejectParams = new RejectParams()
            {
                Id = (long) proposalStruct.Id,
                Reason = error
            };
            
            return Reject(rejectParams);
        }

        /// <summary>
        /// Update a session, adding/removing additional namespaces in the given topic.
        /// </summary>
        /// <param name="topic">The topic to update</param>
        /// <param name="namespaces">The updated namespaces</param>
        /// <returns>A task that returns an interface that can be used to listen for acknowledgement of the updates</returns>
        public Task<IAcknowledgement> Update(string topic, Namespaces namespaces)
        {
            return Engine.Update(topic, namespaces);
        }

        /// <summary>
        /// Extend a session in the given topic. 
        /// </summary>
        /// <param name="topic">The topic of the session to extend</param>
        /// <returns>A task that returns an interface that can be used to listen for acknowledgement of the extension</returns>
        public Task<IAcknowledgement> Extend(string topic)
        {
            return Engine.Extend(topic);
        }

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
        /// <typeparam name="T">The type of the request data. MUST define the RpcMethodAttribute</typeparam>
        /// <typeparam name="TR">The type of the response data.</typeparam>
        /// <returns>The response data as type TR</returns>
        public Task<TR> Request<T, TR>(string topic, T data, string chainId = null, long? expiry = null)
        {
            return Engine.Request<T, TR>(topic, data, chainId, expiry);
        }

        /// <summary>
        /// Send a response to a request to the session in the given topic with the response data TR. This function
        /// can be called directly, however it may be easier to use <see cref="TypedEventHandler{T, TR}.OnResponse"/> event
        /// to handle sending responses to specific requests. 
        /// </summary>
        /// <param name="topic">The topic of the session to respond in</param>
        /// <param name="response">The JSON RPC response to send</param>
        /// <typeparam name="T">The type of the request data</typeparam>
        /// <typeparam name="TR">The type of the response data</typeparam>
        public Task Respond<T, TR>(string topic, JsonRpcResponse<TR> response)
        {
            return Engine.Respond<T, TR>(topic, response);
        }

        /// <summary>
        /// Emit an event to the session with the given topic with the given <see cref="EventData{T}"/>. You may
        /// optionally specify a chainId to specify where the event occured. 
        /// </summary>
        /// <param name="topic">The topic of the session to emit the event to</param>
        /// <param name="eventData">The event data for the event emitted</param>
        /// <param name="chainId">An (optional) chainId to specify where the event occured</param>
        /// <typeparam name="T">The type of the event data</typeparam>
        public Task Emit<T>(string topic, EventData<T> eventData, string chainId = null)
        {
            return Engine.Emit(topic, eventData, chainId);
        }

        /// <summary>
        /// Send a ping to the session in the given topic
        /// </summary>
        /// <param name="topic">The topic of the session to send a ping to</param>
        public Task Ping(string topic)
        {
            return Engine.Ping(topic);
        }

        /// <summary>
        /// Disconnect a session in the given topic with an (optional) error reason
        /// </summary>
        /// <param name="topic">The topic of the session to disconnect</param>
        /// <param name="reason">An (optional) error reason for the disconnect</param>
        public Task Disconnect(string topic, Error reason)
        {
            return Engine.Disconnect(topic, reason);
        }

        /// <summary>
        /// Find all sessions that have a namespace that match the given <see cref="RequiredNamespaces"/>
        /// </summary>
        /// <param name="requiredNamespaces">The required namespaces the session must have to be returned</param>
        /// <returns>All sessions that have a namespace that match the given <see cref="RequiredNamespaces"/></returns>
        public SessionStruct[] Find(RequiredNamespaces requiredNamespaces)
        {
            return Engine.Find(requiredNamespaces);
        }

        public void HandleEventMessageType<T>(Func<string, JsonRpcRequest<SessionEvent<T>>, Task> requestCallback, Func<string, JsonRpcResponse<bool>, Task> responseCallback)
        {
            this.Engine.HandleEventMessageType<T>(requestCallback, responseCallback);
        }

        private async Task Initialize()
        {
            await this.Core.Start();
            await PendingRequests.Init();
            await PairingStore.Init();
            await Session.Init();
            await Proposal.Init();
            await Engine.Init();
        }
    }
}
