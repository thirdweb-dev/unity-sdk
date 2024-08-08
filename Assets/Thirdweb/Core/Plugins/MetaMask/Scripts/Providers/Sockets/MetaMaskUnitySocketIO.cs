#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using MetaMask.SocketIOClient;
using MetaMask.SocketIOClient.Transport;
using MetaMask.Sockets;
using UnityEngine.Networking;

namespace MetaMask.Providers.Sockets
{

    public class MetaMaskUnitySocketIO : IMetaMaskSocketWrapper
    {

        /// <summary>Raised when the connection to the server is established.</summary>
        public event EventHandler Connected;

        /// <summary>Raised when the socket has been disconnected.</summary>
        public event EventHandler Disconnected;

        /// <summary>The socket.</summary>
        protected SocketIOUnity socket;

        public SocketIO Socket => this.socket;

        /// <summary>Creates a new MetaMaskUnitySocketIO instance.</summary>
        public MetaMaskUnitySocketIO()
        {
        }

        public async Task<(string, bool, string)> SendWebRequest(string url, string data, Dictionary<string, string> headers)
        {
            using (var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST, new DownloadHandlerBuffer(),
                       new UploadHandlerRaw(Encoding.UTF8.GetBytes(data))))
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        uwr.SetRequestHeader(header.Key, header.Value);
                    }
                }

                await uwr.SendWebRequest();

                return (uwr.downloadHandler.text, uwr.result == UnityWebRequest.Result.Success, uwr.error);
            }
        }

        /// <summary>Initializes the socket.</summary>0
        /// <param name="url">The URL of the socket.</param>
        /// <param name="options">The options for the socket.</param>
        public void Initialize(string url, MetaMaskSocketOptions options)
        {
            var socketOptions = new SocketIOOptions();
            socketOptions.ExtraHeaders = options.ExtraHeaders;
            socketOptions.Transport = TransportProtocol.WebSocket;
            socketOptions.AutoUpgrade = true;

            this.socket = new SocketIOUnity(url, socketOptions);

            this.socket.OnConnected += OnSocketConnected;
            this.socket.OnDisconnected += OnSocketDisconnected;
        }

        private void OnSocketDisconnected(object sender, string e)
        {
            Debug.Log(e);
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnSocketConnected(object sender, EventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        /// <summary>Connects to the server.</summary>
        /// <returns>A task that represents the asynchronous connect operation.</returns>
        public Task ConnectAsync()
        {
            this.socket.Connect();
            return Task.CompletedTask;
        }

        /// <summary>Disconnects the socket.</summary>
        public Task DisconnectAsync()
        {
            if (this.socket != null)
                this.socket.Disconnect();
            return Task.CompletedTask;
        }

        /// <summary>Disposes of the socket.</summary>
        public void Dispose()
        {
            if (this.socket != null)
                this.socket.Dispose();
        }

        /// <summary>Emit an event to the server.</summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="data">The data to send with the event.</param>
        public void Emit(string eventName, params object[] data)
        {
            if (this.socket != null)
                this.socket.Emit(eventName, data);
        }

        public async void EmitOrError(string eventName, Action<string> onError, params object[] data)
        {
            if (this.socket != null)
                await this.socket.EmitAsync(eventName, (response) =>
                {
                    var error = response.GetValue<string>(0);
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        onError(error);
                    }
                }, data);
        }
        
        public async void EmitWithResultOrError<T>(string eventName, Action<T> onResult, Action<string> onError, params object[] data)
        {
            if (this.socket != null)
            {
                await this.socket.EmitAsync(eventName, (response) =>
                {
                    var error = response.GetValue<string>(0);
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        onError(error);
                    }
                    else
                    {
                        var result = response.GetValue<T>(1);
                        onResult(result);
                    }
                }, data);
            }
        }

        /// <summary>Registers a callback for the specified event.</summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="callback">The callback to register.</param>
        public void On(string eventName, Action<string> callback)
        {
            this.socket.On(eventName, response =>
            {
                callback(response.ToString());
            });
        }

        /// <summary>Removes the specified callback from the list of callbacks for the specified event.</summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="callback">The callback to remove.</param>
        public void Off(string eventName, Action<string> callback = null)
        {
            this.socket.Off(eventName);
        }
    }
}
#endif