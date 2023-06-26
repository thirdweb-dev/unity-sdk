using Newtonsoft.Json;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Model.Errors;
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

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// A full implementation of the IJsonRpcProvider interface using the EventDelegator
    /// </summary>
    public class JsonRpcProvider : IJsonRpcProvider, IModule
    {
        private IJsonRpcConnection _connection;
        private EventDelegator _delegator;
        private bool _hasRegisteredEventListeners;
        private Guid _context;
        private bool _connectingStarted;
        private TaskCompletionSource<bool> Connecting = new TaskCompletionSource<bool>();
        private long _lastId;

        /// <summary>
        /// Whether the provider is currently connecting or not
        /// </summary>
        public bool IsConnecting
        {
            get
            {
                return _connectingStarted && !Connecting.Task.IsCompleted;
            }
        }
        
        /// <summary>
        /// The current Connection for this provider
        /// </summary>
        public IJsonRpcConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        /// <summary>
        /// The name of this provider module
        /// </summary>
        public string Name
        {
            get
            {
                return "json-rpc-provider";
            }
        }

        /// <summary>
        /// The context string of this provider module
        /// </summary>
        public string Context
        {
            get
            {
                //TODO Get context from logger
                return _context.ToString();
            }
        }

        /// <summary>
        /// The EventDelegator this provider is using for events
        /// </summary>
        public EventDelegator Events
        {
            get
            {
                return _delegator;
            }
        }

        /// <summary>
        /// Create a new JsonRpcProvider with the given connection
        /// </summary>
        /// <param name="connection">The IJsonRpcConnection to use</param>
        public JsonRpcProvider(IJsonRpcConnection connection)
        {
            _context = Guid.NewGuid();
            this._delegator = new EventDelegator(this);
            this._connection = connection;
            if (this._connection.Connected)
            {
                RegisterEventListeners();
            }
        }

        /// <summary>
        /// Connect this provider using the given connection string
        /// </summary>
        /// <param name="connection">The connection string to use to connect, usually a URI</param>
        public async Task Connect(string connection)
        {
            if (this._connection.Connected)
            {
                await this._connection.Close();
            }
            
            // Reset connecting task
            Connecting = new TaskCompletionSource<bool>();
            _connectingStarted = true;

            await this._connection.Open(connection);
            FinalizeConnection(this._connection);
        }

        /// <summary>
        /// Connect this provider with the given IJsonRpcConnection connection
        /// </summary>
        /// <param name="connection">The connection object to use to connect</param>
        public async Task Connect(IJsonRpcConnection connection)
        {
            if (this._connection == connection && connection.Connected) return;
            if (this._connection.Connected)
            {
                await this._connection.Close();
            }
            
            // Reset connecting task
            Connecting = new TaskCompletionSource<bool>();
            _connectingStarted = true;

            await connection.Open();

            FinalizeConnection(connection);
        }
        
        private void FinalizeConnection(IJsonRpcConnection connection)
        {
            this._connection = connection;
            RegisterEventListeners();
            Events.Trigger(ProviderEvents.Connect, connection);
            Connecting.SetResult(true);
            _connectingStarted = false;
        }

        /// <summary>
        /// Connect this provider using the backing IJsonRpcConnection that was set in the
        /// constructor
        /// </summary>
        public async Task Connect()
        {
            if (_connection == null)
                throw new Exception("No connection is set");
            await Connect(_connection);
        }

        /// <summary>
        /// Disconnect this provider
        /// </summary>
        public async Task Disconnect()
        {
            await _connection.Close();
            // Reset connecting task
            Connecting = new TaskCompletionSource<bool>();
        }

        /// <summary>
        /// Send a request and wait for a response. The response is returned as a task and can
        /// be awaited
        /// </summary>
        /// <param name="requestArgs">The request arguments to send</param>
        /// <param name="context">The context to use during sending</param>
        /// <typeparam name="T">The type of request arguments</typeparam>
        /// <typeparam name="TR">The type of the response</typeparam>
        /// <returns>A Task that will resolve when a response is received</returns>
        public async Task<TR> Request<T, TR>(IRequestArguments<T> requestArgs, object context = null)
        {
            if (IsConnecting)
                await Connecting.Task;
            else if (!_connectingStarted && !_connection.Connected)
            {
                await Connect(_connection);
            }

            long? id = null;
            if (requestArgs is IJsonRpcRequest<T>)
            {
                id = ((IJsonRpcRequest<T>)requestArgs).Id;
                if (id == 0)
                    id = null; // An id of 0 is null
            }
            var request = new JsonRpcRequest<T>(requestArgs.Method, requestArgs.Params, id);

            TaskCompletionSource<TR> requestTask = new TaskCompletionSource<TR>(TaskCreationOptions.None);
            
            Events.ListenForAndDeserialize<JsonRpcResponse<TR>>(request.Id.ToString(),
                delegate(object sender, GenericEvent<JsonRpcResponse<TR>> @event)
                {
                    if (requestTask.Task.IsCompleted)
                        return;
                    
                    var result = @event.EventData;
                    
                    //Console.WriteLine($"[{Name}] Got response {JsonConvert.SerializeObject(result)}");
                    
                    if (result.Error != null)
                    {
                        requestTask.SetException(new IOException(result.Error.Message));
                    }
                    else
                    {
                        requestTask.SetResult(result.Result);
                    }
                });
            
            Events.ListenFor(request.Id.ToString(), delegate(object sender, GenericEvent<WalletConnectException> @event)
            {
                if (requestTask.Task.IsCompleted)
                    return;
                
                var exception = @event.EventData;
                //Console.WriteLine($"[{Name}] Got Response Error {exception}");
                if (exception != null)
                {
                    requestTask.SetException(exception);
                }
            });

            _lastId = request.Id;
            
            //Console.WriteLine($"[{Name}] Sending request {request.Method} with data {JsonConvert.SerializeObject(request)}");
            await _connection.SendRequest(request, context);

            await requestTask.Task;

            return requestTask.Task.Result;
        }

        protected void RegisterEventListeners()
        {
            if (_hasRegisteredEventListeners) return;
            
            _connection.On<string>("payload", OnPayload);
            _connection.On<object>("close", OnConnectionDisconnected);
            _connection.On<Exception>("error", OnConnectionError);
            _hasRegisteredEventListeners = true;
        }

        private void OnConnectionError(object sender, GenericEvent<Exception> e)
        {
            Events.Trigger(ProviderEvents.Error, e.EventData);
        }

        private void OnConnectionDisconnected(object sender, GenericEvent<object> e)
        {
            Events.TriggerType(ProviderEvents.Disconnect, e.EventData, e.EventData.GetType());
        }

        private void OnPayload(object sender, GenericEvent<string> e)
        {
            var json = e.EventData;

            var payload = JsonConvert.DeserializeObject<JsonRpcPayload>(json);

            if (payload == null)
            {
                throw new IOException("Invalid payload: " + json);
            }

            if (payload.Id == 0)
                payload.Id = _lastId;
            
            Events.Trigger(ProviderEvents.Payload, payload);

            if (payload.IsRequest)
            {
                Events.Trigger(ProviderEvents.RawRequestMessage, json);
            }
            else
            {
                if (payload.IsError)
                {
                    var errorPayload = JsonConvert.DeserializeObject<JsonRpcError>(json);
                    Events.Trigger(payload.Id.ToString(), errorPayload.Error.ToException());
                }
                else
                {
                    Events.Trigger(payload.Id.ToString(), json);
                }
            }
        }
    }
}
