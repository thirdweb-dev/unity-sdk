using System.Net.WebSockets;
using Newtonsoft.Json;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network.Models;
using Websocket.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Network.Websocket
{
    /// <summary>
    /// A JSON RPC connection using Websocket.Client library + EventDelegator
    /// </summary>
    public class WebsocketConnection : IJsonRpcConnection, IModule, IObserver<ResponseMessage>, IObserver<DisconnectionInfo>
    {
        private EventDelegator _delegator;
        private WebsocketClient _socket;
        private string _url;
        private bool _registering;
        private Guid _context;

        /// <summary>
        /// The Open timeout
        /// </summary>
        public TimeSpan OpenTimeout = TimeSpan.FromSeconds(60);

        /// <summary>
        /// The Url to connect to
        /// </summary>
        public string Url
        {
            get
            {
                return _url;
            }
        }

        /// <summary>
        /// The name of this websocket connection module
        /// </summary>
        public string Name
        {
            get
            {
                return "websocket-connection";
            }
        }

        /// <summary>
        /// The context string of this Websocket module
        /// </summary>
        public string Context
        {
            get
            {
                return _context.ToString();
            }
        }

        /// <summary>
        /// The EventDelegator this Websocket connection module is using
        /// </summary>
        public EventDelegator Events
        {
            get
            {
                return _delegator;
            }
        }
        
        /// <summary>
        /// Whether this websocket connection is connected
        /// </summary>
        public bool Connected
        {
            get
            {
                return _socket != null;
            }
        }

        /// <summary>
        /// Whether this websocket connection is currently connecting
        /// </summary>
        public bool Connecting
        {
            get
            {
                return _registering;
            }
        }

        /// <summary>
        /// Create a new websocket connection that will connect to the given URL
        /// </summary>
        /// <param name="url">The URL to connect to</param>
        /// <exception cref="ArgumentException">If the given URL is invalid</exception>
        public WebsocketConnection(string url)
        {
            if (!Validation.IsWsUrl(url))
                throw new ArgumentException("Provided URL is not compatible with WebSocket connection: " + url);
            
            _context = Guid.NewGuid();
            this._url = url;
            _delegator = new EventDelegator(this);
        }

        /// <summary>
        /// Open this connection
        /// </summary>
        public async Task Open()
        {
            await Register(_url);
        }

        /// <summary>
        /// Open this connection using a string url
        /// </summary>
        /// <param name="options">Must be a string url. If any other type, then normal Open() is invoked</param>
        /// <typeparam name="T">The type of the options. Should always be string</typeparam>
        public async Task Open<T>(T options)
        {
            if (typeof(string).IsAssignableFrom(typeof(T)))
            {
                await Register(options as string);
            }

            await Open();
        }

        private async Task<WebsocketClient> Register(string url)
        {
            if (!Validation.IsWsUrl(url))
            {
                throw new ArgumentException("Provided URL is not compatible with WebSocket connection: " + url);
            }

            if (_registering)
            {
                TaskCompletionSource<WebsocketClient> registeringTask =
                    new TaskCompletionSource<WebsocketClient>(TaskCreationOptions.None);
                
                Events.ListenForOnce("register_error",
                    delegate(object sender, GenericEvent<Exception> @event)
                    {
                        registeringTask.SetException(@event.EventData);
                    });
                
                Events.ListenForOnce("open",
                    delegate(object sender, GenericEvent<WebsocketClient> @event)
                    {
                        registeringTask.SetResult(@event.EventData);
                    });

                await registeringTask.Task;

                return registeringTask.Task.Result;
            }

            this._url = url;
            this._registering = true;

            try
            {
                _socket = new WebsocketClient(new Uri(_url));

                await _socket.Start().WithTimeout(OpenTimeout, "Unavailable WS RPC url at " + _url);
                OnOpen(_socket);
                return _socket;
            }
            catch (Exception e)
            {
                Events.Trigger(WebsocketConnectionEvents.RegisterError, e);
                OnClose(new DisconnectionInfo(DisconnectionType.Error, WebSocketCloseStatus.Empty, e.Message, null, e));

                throw;
            }
        }

        private void OnOpen(WebsocketClient socket)
        {
            //socket.MessageReceived.Subscribe(OnPayload);
            socket.MessageReceived.Subscribe(this);
            //socket.DisconnectionHappened.Subscribe(OnDisconnect);
            socket.DisconnectionHappened.Subscribe(this);

            this._socket = socket;
            this._registering = false;
            Events.Trigger(WebsocketConnectionEvents.Open, _socket);
        }

        private void OnDisconnect(DisconnectionInfo obj)
        {
            if (obj.Exception != null)
                Events.Trigger(WebsocketConnectionEvents.Error, obj.Exception);
            
            OnClose(obj);
        }
        
        private void OnClose(DisconnectionInfo obj)
        {
            if (this._socket == null)
                return;
            
            //_socket.Dispose();
            this._socket = null;
            this._registering = false;
            Events.Trigger(WebsocketConnectionEvents.Close, obj);
        }

        private void OnPayload(ResponseMessage obj)
        {
            string json = null;
            switch (obj.MessageType)
            {
                case WebSocketMessageType.Binary:
                    return;
                case WebSocketMessageType.Text:
                    json = obj.Text;
                    break;
                case WebSocketMessageType.Close:
                    return;
            }

            if (string.IsNullOrWhiteSpace(json)) return;
            
            //Console.WriteLine($"[{Name}] Got payload {json}");

            Events.Trigger(WebsocketConnectionEvents.Payload, json);
        }

        /// <summary>
        /// Close this connection
        /// </summary>
        /// <exception cref="IOException">If this connection was already closed</exception>
        public async Task Close()
        {
            if (_socket == null)
                throw new IOException("Connection already closed");

            await _socket.Stop(WebSocketCloseStatus.NormalClosure, "Close Invoked");
            
            OnClose(new DisconnectionInfo(DisconnectionType.Exit, WebSocketCloseStatus.Empty, "Close Invoked", null, null));
        }

        /// <summary>
        /// Send a Json RPC request through this websocket connection, using the given context
        /// </summary>
        /// <param name="requestPayload">The request payload to encode and send</param>
        /// <param name="context">The context to use when sending</param>
        /// <typeparam name="T">The type of the Json RPC request parameter</typeparam>
        public async Task SendRequest<T>(IJsonRpcRequest<T> requestPayload, object context)
        {
            if (_socket == null)
                _socket = await Register(_url);

            try
            {
                _socket.Send(JsonConvert.SerializeObject(requestPayload));
            }
            catch (Exception e)
            {
                OnError<T>(requestPayload, e);
            }
        }

        /// <summary>
        /// Send a Json RPC response through this websocket connection, using the given context
        /// </summary>
        /// <param name="requestPayload">The response payload to encode and send</param>
        /// <param name="context">The context to use when sending</param>
        /// <typeparam name="T">The type of the Json RPC response result</typeparam>
        public async Task SendResult<T>(IJsonRpcResult<T> requestPayload, object context)
        {
            if (_socket == null)
                _socket = await Register(_url);

            try
            {
                _socket.Send(JsonConvert.SerializeObject(requestPayload));
            }
            catch (Exception e)
            {
                OnError<T>(requestPayload, e);
            }
        }

        /// <summary>
        /// Send a Json RPC error response through this websocket connection, using the given context
        /// </summary>
        /// <param name="requestPayload">The error response payload to encode and send</param>
        /// <param name="context">The context to use when sending</param>
        public async Task SendError(IJsonRpcError requestPayload, object context)
        {
            if (_socket == null)
                _socket = await Register(_url);

            try
            {
                _socket.Send(JsonConvert.SerializeObject(requestPayload));
            }
            catch (Exception e)
            {
                OnError<object>(requestPayload, e);
            }
        }

        /// <summary>
        /// Dispose this websocket connection. Will automatically Close this
        /// websocket if still connected.
        /// </summary>
        public async void Dispose()
        {
            if (Connected)
            {
                await Close();
            }
        }

        private string addressNotFoundError = "getaddrinfo ENOTFOUND";
        private string connectionRefusedError = "connect ECONNREFUSED";
        private void OnError<T>(IJsonRpcPayload ogPayload, Exception e)
        {
            var exception = e.Message.Contains(addressNotFoundError) || e.Message.Contains(connectionRefusedError)
                ? new IOException("Unavailable WS RPC url at " + _url) : e;

            var message = exception.Message;
            var payload = new JsonRpcResponse<T>(ogPayload.Id, new Error()
            {
                Code = exception.HResult,
                Data = null,
                Message = message
            }, default(T));

            //Trigger the payload event, converting the new JsonRpcResponse object to JSON string
            Events.Trigger(WebsocketConnectionEvents.Payload, JsonConvert.SerializeObject(payload));
        }

        public void OnCompleted()
        {
            // TODO Dispose?
        }

        public void OnError(Exception error)
        {
            // TODO Log error
        }

        public void OnNext(DisconnectionInfo value)
        {
            OnDisconnect(value);
        }

        public void OnNext(ResponseMessage value)
        {
            OnPayload(value);
        }
    }
}
