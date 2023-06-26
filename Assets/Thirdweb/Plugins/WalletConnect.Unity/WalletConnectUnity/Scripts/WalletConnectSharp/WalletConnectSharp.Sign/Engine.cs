using System.Text.RegularExpressions;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Expirer;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Core.Models.Verify;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Interfaces;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Interfaces;
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

namespace WalletConnectSharp.Sign
{
    /// <summary>
    /// The Engine for running the Sign client protocol and code flow.
    /// </summary>
    public partial class Engine : IEnginePrivate, IEngine, IModule, IEvents
    {
        private const long ProposalExpiry = Clock.THIRTY_DAYS;
        private const long SessionExpiry = Clock.SEVEN_DAYS;
        private const int KeyLength = 32;
        
        /// <summary>
        /// The EventDelegator that should be used to listen to (or trigger) events
        /// </summary>
        public EventDelegator Events { get; }

        private bool _initialized = false;
        
        /// <summary>
        /// The <see cref="ISignClient"/> using this Engine
        /// </summary>
        public ISignClient Client { get; }

        private IEnginePrivate PrivateThis => this;

        private ITypedMessageHandler MessageHandler => Client.Core.MessageHandler;

        /// <summary>
        /// The name of this Engine module
        /// </summary>
        public string Name => $"{Client.Name}-engine";

        /// <summary>
        /// The context string for this Engine module
        /// </summary>
        public string Context
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        /// Create a new Engine with the given <see cref="ISignClient"/> module
        /// </summary>
        /// <param name="client">That client that will be using this Engine</param>
        public Engine(ISignClient client)
        {
            this.Client = client;
            Events = new EventDelegator(this);
        }

        /// <summary>
        /// Initialize the Engine. This loads any persistant state and connects to the WalletConnect
        /// relay server 
        /// </summary>
        /// <returns></returns>
        public async Task Init()
        {
            if (!this._initialized)
            {
                await PrivateThis.Cleanup();
                this.RegisterRelayerEvents();
                this.RegisterExpirerEvents();
                this._initialized = true;
            }
        }

        private void RegisterExpirerEvents()
        {
            this.Client.Core.Expirer.On<Expiration>(ExpirerEvents.Expired, ExpiredCallback);
        }

        private void RegisterRelayerEvents()
        {
            // Register all Request Types
            MessageHandler.HandleMessageType<SessionPropose, SessionProposeResponse>(PrivateThis.OnSessionProposeRequest, PrivateThis.OnSessionProposeResponse);
            MessageHandler.HandleMessageType<SessionSettle, bool>(PrivateThis.OnSessionSettleRequest, PrivateThis.OnSessionSettleResponse);
            MessageHandler.HandleMessageType<SessionUpdate, bool>(PrivateThis.OnSessionUpdateRequest, PrivateThis.OnSessionUpdateResponse);
            MessageHandler.HandleMessageType<SessionExtend, bool>(PrivateThis.OnSessionExtendRequest, PrivateThis.OnSessionExtendResponse);
            MessageHandler.HandleMessageType<SessionDelete, bool>(PrivateThis.OnSessionDeleteRequest, null);
            MessageHandler.HandleMessageType<SessionPing, bool>(PrivateThis.OnSessionPingRequest, PrivateThis.OnSessionPingResponse);
        }

        /// <summary>
        /// Get static event handlers for requests / responses for the given type T, TR. This is similar to
        /// <see cref="IEngine.HandleMessageType{T,TR}"/> but uses EventHandler rather than callback functions
        /// </summary>
        /// <typeparam name="T">The request type to trigger the requestCallback for</typeparam>
        /// <typeparam name="TR">The response type to trigger the responseCallback for</typeparam>
        /// <returns>The <see cref="TypedEventHandler{T,TR}"/> managing events for the given types T, TR</returns>
        public TypedEventHandler<T, TR> SessionRequestEvents<T, TR>()
        {
            return SessionRequestEventHandler<T, TR>.GetInstance(Client.Core, PrivateThis);
        }

        /// <summary>
        /// An alias for <see cref="HandleMessageType{T,TR}"/> where T is <see cref="SessionRequest{T}"/> and
        /// TR is unchanged
        /// </summary>
        /// <param name="requestCallback">The callback function to invoke when a request is received with the given request type</param>
        /// <param name="responseCallback">The callback function to invoke when a response is received with the given response type</param>
        /// <typeparam name="T">The request type to trigger the requestCallback for. Will be wrapped in <see cref="SessionRequest{T}"/></typeparam>
        /// <typeparam name="TR">The response type to trigger the responseCallback for</typeparam>
        public void HandleSessionRequestMessageType<T, TR>(Func<string, JsonRpcRequest<SessionRequest<T>>, Task> requestCallback, Func<string, JsonRpcResponse<TR>, Task> responseCallback)
        {
            Client.Core.MessageHandler.HandleMessageType(requestCallback, responseCallback);
        }
        
        /// <summary>
        /// An alias for <see cref="HandleMessageType{T,TR}"/> where T is <see cref="SessionEvent{T}"/> and
        /// TR is unchanged
        /// </summary>
        /// <param name="requestCallback">The callback function to invoke when a request is received with the given request type</param>
        /// <param name="responseCallback">The callback function to invoke when a response is received with the given response type</param>
        /// <typeparam name="T">The request type to trigger the requestCallback for. Will be wrapped in <see cref="SessionEvent{T}"/></typeparam>
        /// <typeparam name="TR">The response type to trigger the responseCallback for</typeparam>
        public void HandleEventMessageType<T>(Func<string, JsonRpcRequest<SessionEvent<T>>, Task> requestCallback, Func<string, JsonRpcResponse<bool>, Task> responseCallback)
        {
            Client.Core.MessageHandler.HandleMessageType(requestCallback, responseCallback);
        }

        /// <summary>
        /// Parse a session proposal URI and return all information in the URI in a
        /// new <see cref="UriParameters"/> object
        /// </summary>
        /// <param name="uri">The uri to parse</param>
        /// <returns>A new <see cref="UriParameters"/> object that contains all data
        /// parsed from the given uri</returns>
        public UriParameters ParseUri(string uri)
        {
            var pathStart = uri.IndexOf(":", StringComparison.Ordinal);
            int? pathEnd = uri.IndexOf("?", StringComparison.Ordinal) != -1 ? uri.IndexOf("?", StringComparison.Ordinal) : (int?)null;
            var protocol = uri.Substring(0, pathStart);

            string path;
            if (pathEnd != null) path = uri.Substring(pathStart + 1, (int)pathEnd - (pathStart + 1));
            else path = uri.Substring(pathStart + 1);

            var requiredValues = path.Split("@");
            string queryString = pathEnd != null ? uri.Substring((int)pathEnd) : "";
            var queryParams = Regex.Matches(queryString, "([^?=&]+)(=([^&]*))?").Cast<Match>()
                .ToDictionary(x => x.Groups[1].Value, x => x.Groups[3].Value);

            var result = new UriParameters()
            {
                Protocol = protocol,
                Topic = requiredValues[0],
                Version = int.Parse(requiredValues[1]),
                SymKey = queryParams["symKey"],
                Relay = new ProtocolOptions()
                {
                    Protocol = queryParams["relay-protocol"],
                    Data = queryParams.ContainsKey("relay-data") ? queryParams["relay-data"] : null,
                }
            };

            return result;
        }

        /// <summary>
        /// Get all pending session requests
        /// </summary>
        public PendingRequestStruct[] PendingSessionRequests
        {
            get
            {
                this.IsInitialized();
                return this.Client.PendingRequests.Values;
            }
        }

        /// <summary>
        /// Connect (a dApp) with the given ConnectOptions. At a minimum, you must specified a RequiredNamespace. 
        /// </summary>
        /// <param name="options"></param>
        /// <returns>Connection data that includes the session proposal URI as well as a
        /// way to await for a session approval</returns>
        public async Task<ConnectedData> Connect(ConnectOptions options)
        {
            this.IsInitialized();
            await PrivateThis.IsValidConnect(options);
            var requiredNamespaces = options.RequiredNamespaces;
            var relays = options.Relays;
            var topic = options.PairingTopic;
            string uri = "";
            var active = false;

            if (!string.IsNullOrEmpty(topic))
            {
                var pairing = this.Client.Core.Pairing.Store.Get(topic);
                if (pairing.Active != null)
                    active = pairing.Active.Value;
            }

            if (string.IsNullOrEmpty(topic) || !active)
            {
                var CreatePairing = await this.Client.Core.Pairing.Create();
                topic = CreatePairing.Topic;
                uri = CreatePairing.Uri;
            }

            var publicKey = await this.Client.Core.Crypto.GenerateKeyPair();
            var proposal = new SessionPropose()
            {
                RequiredNamespaces = requiredNamespaces,
                Relays = relays != null
                    ? new[] { relays }
                    : new[]
                    {
                        new ProtocolOptions()
                        {
                            Protocol = RelayProtocols.Default
                        }
                    },
                Proposer = new Participant()
                {
                    PublicKey = publicKey,
                    Metadata = this.Client.Metadata
                }
            };

            TaskCompletionSource<SessionStruct> approvalTask = new TaskCompletionSource<SessionStruct>();
            this.Events.ListenForOnce<SessionStruct>("session_connect", async (sender, e) =>
            {
                if (approvalTask.Task.IsCompleted)
                    return;
                
                var session = e.EventData;
                session.Self.PublicKey = publicKey;
                
                // var completeSession = session with { RequiredNamespaces = requiredNamespaces };
                var completeSession = new SessionStruct()
                {
                    Acknowledged = session.Acknowledged,
                    Controller = session.Controller,
                    Expiry = session.Expiry,
                    Namespaces = session.Namespaces,
                    Peer = session.Peer,
                    Relay = session.Relay,
                    RequiredNamespaces = requiredNamespaces,
                    Self = session.Self,
                    Topic = session.Topic
                };
                
                await PrivateThis.SetExpiry(session.Topic, session.Expiry.Value);
                await Client.Session.Set(session.Topic, completeSession);
                
                if (!string.IsNullOrWhiteSpace(topic))
                {
                    await this.Client.Core.Pairing.UpdateMetadata(topic, session.Peer.Metadata);
                }
                approvalTask.SetResult(completeSession);
            });
            
            this.Events.ListenForOnce<JsonRpcResponse<SessionProposeResponse>>("session_connect", (sender, e) =>
            {
                if (approvalTask.Task.IsCompleted)
                    return;
                if (e.EventData.IsError)
                {
                    approvalTask.SetException(e.EventData.Error.ToException());
                }
            });

            if (string.IsNullOrWhiteSpace(topic))
            {
                throw WalletConnectException.FromType(ErrorType.NO_MATCHING_KEY, $"connect() pairing topic: {topic}");
            }

            var id = await MessageHandler.SendRequest<SessionPropose, SessionProposeResponse>(topic, proposal);
            var expiry = Clock.CalculateExpiry(Clock.FIVE_MINUTES);

            await PrivateThis.SetProposal(id, new ProposalStruct()
            {
                Expiry = expiry,
                Id = id,
                Proposer = proposal.Proposer,
                Relays = proposal.Relays,
                RequiredNamespaces = proposal.RequiredNamespaces,
                OptionalNamespaces = proposal.OptionalNamespaces,
                SessionProperties = proposal.SessionProperties,
            });

            return new ConnectedData()
            {
                Uri = uri,
                Approval = approvalTask.Task
            };
        }

        /// <summary>
        /// Pair (a wallet) with a peer (dApp) using the given uri. The uri must be in the correct
        /// format otherwise an exception will be thrown.
        /// </summary>
        /// <param name="uri">The URI to pair with</param>
        /// <returns>The proposal the connecting peer wants to connect using. You must approve or reject
        /// the proposal</returns>
        public async Task<ProposalStruct> Pair(string uri)
        {
            this.IsInitialized();
            var pairing = await this.Client.Core.Pairing.Pair(uri);
            var topic = pairing.Topic;

            TaskCompletionSource<ProposalStruct> sessionProposeTask = new TaskCompletionSource<ProposalStruct>();
            
            Client.Once(EngineEvents.SessionProposal,
                delegate(object sender, GenericEvent<SessionProposalEvent> @event)
                {
                    var proposal = @event.EventData.Proposal;
                    if (topic != proposal.PairingTopic)
                        return;

                    if (@event.EventData.VerifiedContext.Validation == Validation.Invalid)
                        sessionProposeTask.SetException(new Exception(
                            $"Could not validate, invalid validation status {@event.EventData.VerifiedContext.Validation} for origin {@event.EventData.VerifiedContext.Origin}"));
                    else
                        sessionProposeTask.SetResult(proposal);
                });

            return await sessionProposeTask.Task;
        }

        /// <summary>
        /// Approve a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// Use <see cref="ProposalStruct.ApproveProposal(string, ProtocolOptions)"/> to generate an
        /// <see cref="ApproveParams"/> object, or use the alias function <see cref="IEngineAPI.Approve(ProposalStruct, string[])"/>
        /// </summary>
        /// <param name="@params">Parameters for the approval. This usually comes from <see cref="ProposalStruct.ApproveProposal(string, ProtocolOptions)"/></param>
        /// <returns>Approval data, includes the topic of the session and a way to wait for approval acknowledgement</returns>
        public async Task<IApprovedData> Approve(ApproveParams @params)
        {
            IsInitialized();
            await PrivateThis.IsValidApprove(@params);
            var id = @params.Id;
            var relayProtocol = @params.RelayProtocol;
            var namespaces = @params.Namespaces;
            var proposal = this.Client.Proposal.Get(id);
            var pairingTopic = proposal.PairingTopic;
            var proposer = proposal.Proposer;
            var requiredNamespaces = proposal.RequiredNamespaces;
            var optionalNamespaces = proposal.OptionalNamespaces;

            var selfPublicKey = await this.Client.Core.Crypto.GenerateKeyPair();
            var peerPublicKey = proposer.PublicKey;
            var sessionTopic = await this.Client.Core.Crypto.GenerateSharedKey(
                selfPublicKey,
                peerPublicKey
            );

            var sessionSettle = new SessionSettle()
            {
                Relay = new ProtocolOptions()
                {
                    Protocol = relayProtocol != null ? relayProtocol : "irn"
                },
                Namespaces = namespaces,
                Controller = new Participant()
                {
                    PublicKey = selfPublicKey,
                    Metadata = this.Client.Metadata
                },
                Expiry = Clock.CalculateExpiry(SessionExpiry)
            };

            await this.Client.Core.Relayer.Subscribe(sessionTopic);
            var requestId = await MessageHandler.SendRequest<SessionSettle, bool>(sessionTopic, sessionSettle);

            var acknowledgedTask = new TaskCompletionSource<SessionStruct>();
            
            this.Events.ListenForOnce<JsonRpcResponse<bool>>($"session_approve{requestId}", (sender, e) =>
            {
                if (e.EventData.IsError)
                    acknowledgedTask.SetException(e.EventData.Error.ToException());
                else
                    acknowledgedTask.SetResult(this.Client.Session.Get(sessionTopic));
            });

            var session = new SessionStruct()
            {
                Topic = sessionTopic,
                Acknowledged = false,
                Self = sessionSettle.Controller,
                Peer = proposer,
                Controller = selfPublicKey,
                Expiry = sessionSettle.Expiry,
                Namespaces = sessionSettle.Namespaces,
                Relay = sessionSettle.Relay,
                RequiredNamespaces = requiredNamespaces,
            };

            await this.Client.Session.Set(sessionTopic, session);
            await PrivateThis.SetExpiry(sessionTopic, Clock.CalculateExpiry(SessionExpiry));
            if (!string.IsNullOrWhiteSpace(pairingTopic))
                await this.Client.Core.Pairing.UpdateMetadata(pairingTopic, session.Peer.Metadata);

            if (!string.IsNullOrWhiteSpace(pairingTopic) && id > 0)
            {
                await MessageHandler.SendResult<SessionPropose, SessionProposeResponse>(id, pairingTopic,
                    new SessionProposeResponse()
                    {
                        Relay = new ProtocolOptions()
                        {
                            Protocol = relayProtocol != null ? relayProtocol : "irn"
                        },
                        ResponderPublicKey = selfPublicKey
                    });
                await this.Client.Proposal.Delete(id, Error.FromErrorType(ErrorType.USER_DISCONNECTED));
                await this.Client.Core.Pairing.Activate(pairingTopic);
            }
            
            return IApprovedData.FromTask(sessionTopic, acknowledgedTask.Task);
        }

        /// <summary>
        /// Reject a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// Use <see cref="ProposalStruct.RejectProposal(string)"/> or <see cref="ProposalStruct.RejectProposal(Error)"/>
        /// to generate a <see cref="RejectParams"/> object, or use the alias function <see cref="IEngineAPI.Reject(ProposalStruct, string)"/>
        /// </summary>
        /// <param name="params">The parameters of the rejection</param>
        /// <returns></returns>
        public async Task Reject(RejectParams @params)
        {
            IsInitialized();
            await PrivateThis.IsValidReject(@params);
            var id = @params.Id;
            var reason = @params.Reason;
            var proposal = this.Client.Proposal.Get(id);
            var pairingTopic = proposal.PairingTopic;

            if (!string.IsNullOrWhiteSpace(pairingTopic))
            {
                await MessageHandler.SendError<SessionPropose, SessionProposeResponse>(id, pairingTopic, reason);
                await this.Client.Proposal.Delete(id, Error.FromErrorType(ErrorType.USER_DISCONNECTED));
            }
        }

        /// <summary>
        /// Update a session, adding/removing additional namespaces in the given topic.
        /// </summary>
        /// <param name="topic">The topic to update</param>
        /// <param name="namespaces">The updated namespaces</param>
        /// <returns>A task that returns an interface that can be used to listen for acknowledgement of the updates</returns>
        public async Task<IAcknowledgement> Update(string topic, Namespaces namespaces)
        {
            IsInitialized();
            await PrivateThis.IsValidUpdate(topic, namespaces);
            var id = await MessageHandler.SendRequest<SessionUpdate, bool>(topic, new SessionUpdate()
            {
                Namespaces = namespaces
            });

            TaskCompletionSource<bool> acknowledgedTask = new TaskCompletionSource<bool>();
            this.Events.ListenForOnce<JsonRpcResponse<bool>>($"session_update{id}", (sender, e) =>
            {
                if (e.EventData.IsError)
                    acknowledgedTask.SetException(e.EventData.Error.ToException());
                else
                    acknowledgedTask.SetResult(e.EventData.Result);
            });

            await this.Client.Session.Update(topic, new SessionStruct()
            {
                Namespaces = namespaces
            });
            
            return IAcknowledgement.FromTask(acknowledgedTask.Task);
        }

        /// <summary>
        /// Extend a session in the given topic. 
        /// </summary>
        /// <param name="topic">The topic of the session to extend</param>
        /// <returns>A task that returns an interface that can be used to listen for acknowledgement of the extension</returns>
        public async Task<IAcknowledgement> Extend(string topic)
        {
            IsInitialized();
            await PrivateThis.IsValidExtend(topic);
            var id = await MessageHandler.SendRequest<SessionExtend, bool>(topic, new SessionExtend());
            
            TaskCompletionSource<bool> acknowledgedTask = new TaskCompletionSource<bool>();
            this.Events.ListenForOnce<JsonRpcResponse<bool>>($"session_extend{id}", (sender, e) =>
            {
                if (e.EventData.IsError)
                    acknowledgedTask.SetException(e.EventData.Error.ToException());
                else
                    acknowledgedTask.SetResult(e.EventData.Result);
            });

            await PrivateThis.SetExpiry(topic, Clock.CalculateExpiry(SessionExpiry));
            
            return IAcknowledgement.FromTask(acknowledgedTask.Task);
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
        public async Task<TR> Request<T, TR>(string topic, T data, string chainId = null, long? expiry = null)
        {
            await IsValidSessionTopic(topic);

            var method = RpcMethodAttribute.MethodForType<T>();

            string defaultChainId;
            if (string.IsNullOrWhiteSpace(chainId))
            {
                var sessionData = Client.Session.Get(topic);
                var firstRequiredNamespace = sessionData.RequiredNamespaces.Keys.ToArray()[0];
                defaultChainId = sessionData.RequiredNamespaces[firstRequiredNamespace].Chains[0];
            }
            else
            {
                defaultChainId = chainId;
            }

            var request = new JsonRpcRequest<T>(method, data);
            
            IsInitialized();
            await PrivateThis.IsValidRequest(topic, request, defaultChainId);
            long[] id = new long[1];
            
            var taskSource = new TaskCompletionSource<TR>();
            
            SessionRequestEvents<T, TR>()
                .FilterResponses((e) => e.Topic == topic && e.Response.Id == id[0])
                .OnResponse += args =>
            {
                if (args.Response.IsError)
                    taskSource.SetException(args.Response.Error.ToException());
                else
                    taskSource.SetResult(args.Response.Result);

                return Task.CompletedTask;
            };

            id[0] = await MessageHandler.SendRequest<SessionRequest<T>, TR>(topic, new SessionRequest<T>()
            {
                ChainId = defaultChainId,
                Request = request
            });
            

            return await taskSource.Task;
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
        public async Task Respond<T, TR>(string topic, JsonRpcResponse<TR> response)
        {
            IsInitialized();
            await PrivateThis.IsValidRespond(topic, response);
            var id = response.Id;
            if (response.IsError)
            {
                await MessageHandler.SendError<T, TR>(id, topic, response.Error);
            }
            else
            {
                await MessageHandler.SendResult<T, TR>(id, topic, response.Result);
            }

            await PrivateThis.DeletePendingSessionRequest(id, new Error() { Code = 0, Message = "fulfilled" });
        }

        /// <summary>
        /// Emit an event to the session with the given topic with the given <see cref="EventData{T}"/>. You may
        /// optionally specify a chainId to specify where the event occured. 
        /// </summary>
        /// <param name="topic">The topic of the session to emit the event to</param>
        /// <param name="eventData">The event data for the event emitted</param>
        /// <param name="chainId">An (optional) chainId to specify where the event occured</param>
        /// <typeparam name="T">The type of the event data</typeparam>
        public async Task Emit<T>(string topic, EventData<T> @event, string chainId = null)
        {
            IsInitialized();
            await MessageHandler.SendRequest<SessionEvent<T>, object>(topic, new SessionEvent<T>()
            {
                ChainId = chainId,
                Event = @event,
                Topic = topic,
            });
        }

        /// <summary>
        /// Send a ping to the session in the given topic
        /// </summary>
        /// <param name="topic">The topic of the session to send a ping to</param>
        public async Task Ping(string topic)
        {
            IsInitialized();
            await PrivateThis.IsValidPing(topic);
            
            if (this.Client.Session.Keys.Contains(topic))
            {
                var id = await MessageHandler.SendRequest<SessionPing, bool>(topic, new SessionPing());
                var done = new TaskCompletionSource<bool>();
                this.Events.ListenForOnce<JsonRpcResponse<bool>>($"session_ping{id}", (sender, e) =>
                {
                    if (e.EventData.IsError)
                        done.SetException(e.EventData.Error.ToException());
                    else
                        done.SetResult(e.EventData.Result);
                });
                await done.Task;
            } 
            else if (this.Client.Core.Pairing.Store.Keys.Contains(topic))
            {
                await this.Client.Core.Pairing.Ping(topic);
            }
        }

        /// <summary>
        /// Disconnect a session in the given topic with an (optional) error reason
        /// </summary>
        /// <param name="topic">The topic of the session to disconnect</param>
        /// <param name="reason">An (optional) error reason for the disconnect</param>
        public async Task Disconnect(string topic, Error reason)
        {
            IsInitialized();
            var error = reason ?? Error.FromErrorType(ErrorType.USER_DISCONNECTED);
            await PrivateThis.IsValidDisconnect(topic, error);
            
            if (this.Client.Session.Keys.Contains(topic))
            {
                await MessageHandler.SendRequest<SessionDelete, bool>(topic, new SessionDelete()
                {
                    Code = error.Code,
                    Message = error.Message,
                    Data = error.Data
                });
                await PrivateThis.DeleteSession(topic);
            } 
            else if (this.Client.Core.Pairing.Store.Keys.Contains(topic))
            {
                await this.Client.Core.Pairing.Disconnect(topic);
            }
        }

        /// <summary>
        /// Find all sessions that have a namespace that match the given <see cref="RequiredNamespaces"/>
        /// </summary>
        /// <param name="requiredNamespaces">The required namespaces the session must have to be returned</param>
        /// <returns>All sessions that have a namespace that match the given <see cref="RequiredNamespaces"/></returns>
        public SessionStruct[] Find(RequiredNamespaces requiredNamespaces)
        {
            IsInitialized();
            return this.Client.Session.Values.Where(s => IsSessionCompatible(s, requiredNamespaces)).ToArray();
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
        /// </summary>
        /// <param name="proposalStruct">The proposal to reject</param>
        /// <param name="message">A message explaining the reason for the rejection</param>
        public Task Reject(ProposalStruct proposalStruct, string message = null)
        {
            return Reject(proposalStruct.RejectProposal(message));
        }

        /// <summary>
        /// Reject a proposal that was recently paired. If the given proposal was not from a recent pairing,
        /// or the proposal has expired, then an Exception will be thrown.
        /// </summary>
        /// <param name="proposalStruct">The proposal to reject</param>
        /// <param name="error">An error explaining the reason for the rejection</param>
        public Task Reject(ProposalStruct proposalStruct, Error error)
        {
            return Reject(proposalStruct.RejectProposal(error));
        }
    }
}
