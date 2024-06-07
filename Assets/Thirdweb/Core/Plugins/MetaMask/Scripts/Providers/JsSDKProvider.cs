#if UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using evm.net.Models;
using MetaMask.Cryptography;
using MetaMask.Models;
using MetaMask.Providers;
using MetaMask.Scripts.Utilities;
using MetaMask.SocketIOClient;
using MetaMask.Transports;
using Newtonsoft.Json;
using UnityEngine;

namespace MetaMask.Unity.Providers
{
    public class JsSDKProvider : BaseProvider
    {
        private IAppConfig _appConfig;
        private IMetaMaskSDK _unitySdk;
        
        [DllImport("__Internal")]
        public static extern bool _InitMetaMaskJS(string dappName, string dappUrl, string dappIcon, 
            string infuraAPIKey, string rpcMapJson, 
            string walletCallback, string providerCallback, string errorCallback, string providerEventCallback, bool doJsConnect, bool isDebug);

        [DllImport("__Internal")]
        public static extern bool _SendMetaMaskJS(string id, string method, string jsonData, string responseCallback, string errorCallback);

        [DllImport("__Internal")]
        public static extern bool _TerminateMetaMaskJS();
        
        [DllImport("__Internal")]
        public static extern bool _DisconnectMetaMaskJS();
        
        [DllImport("__Internal")]
        public static extern bool _HasMetaMaskJSSession();

        public JsSDKProvider(IMetaMaskSDK sdk)
        {
            // We need to init this service, because MetaMaskUnity uses it for
            // event propagation... maybe move it out of SocketIO ?
            UnityThread.initUnityThread();
            this._unitySdk = sdk;
            this._appConfig = sdk.Config;
            ConnectionMode = TransportMode.External;
        }
        
        public override void SendMessage(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            
            // Always assume it's a MetaMaskEthereumRequest
            var request = JsonConvert.DeserializeObject<MetaMaskEthereumRequest>(json);

            var decodedCallback = JSCallback.Using<JsonRpcPayload>(payload =>
            {
                var eventArgs = new JsonRpcEventArgs(payload, JsonConvert.SerializeObject(payload));
                EthereumRequestReceived?.Invoke(this, eventArgs);
            });

            var errorCallback = JSCallback.Using<GenericError>(error =>
            {
                var jsonRpcErrpr = JsonConvert.DeserializeObject<JsonRpcPayload>(
                    JsonConvert.SerializeObject(new JsonRpcError()
                    {
                        Error = error,
                        Id = request.Id,
                        JsonRpc = "2.0"

                    }));

                var eventArgs = new JsonRpcEventArgs(jsonRpcErrpr, JsonConvert.SerializeObject(jsonRpcErrpr));
                EthereumRequestReceived?.Invoke(this, eventArgs);
            });
            
            _SendMetaMaskJS(request.Id, request.Method, json, decodedCallback, errorCallback);
        }
        
        public override void LoadOrCreateSession(IAppConfig appConfig, IEciesProvider eciesProvider)
        {
            // Only save the session data, but nothing else
            this.Session = new MetaMaskSession(eciesProvider, new MetaMaskSessionData(appConfig));
        }

        public override void ClearSession()
        {
            _TerminateMetaMaskJS();
        }

        #region NO OP Provider Functions
        public override void SaveSession()
        {
            // NO OP, JS SDK already saves our session
        }

        public override void ReloadNewSession()
        {
            // NO OP, JS SDK init will handle this
        }
        #endregion

        public override void Disconnect()
        {
            _DisconnectMetaMaskJS();
        }

        public override void Connect(bool extendedInitAllowed = false)
        {
            var providerCallback = JSCallback.Using(ConnectCallback);
            var walletCallback = JSCallback.Using(OnWalletAuthorized);
            var errorCallback = JSCallback.Using(OnWalletUnauthorized);
            var eventCallback = JSCallback.UsingJson(OnEthereumEvent);

            var rpcMap = this._unitySdk.RpcUrl.ToDictionary(
                c => $"0x{c.ChainId:x8}",
                c => c.RpcUrl
            );

            if (string.IsNullOrWhiteSpace(_appConfig.AppUrl))
            {
                throw new Exception("AppUrl cannot be null or empty. Please set in Tools > MetaMask > Setup Window");
            }
            
            if (string.IsNullOrWhiteSpace(_appConfig.AppName))
            {
                throw new Exception("AppName cannot be null or empty. Please set in Tools > MetaMask > Setup Window");
            }
            
            _InitMetaMaskJS(_appConfig.AppName, _appConfig.AppUrl, _appConfig.AppIcon, 
                this._unitySdk.InfuraProjectId, JsonConvert.SerializeObject(rpcMap), 
                walletCallback, providerCallback, errorCallback, eventCallback, extendedInitAllowed, Debug.isDebugBuild);
        }

        private void OnEthereumEvent(string json)
        {
            var payload = JsonConvert.DeserializeObject<JsonRpcPayload>(json);
            EthereumEventReceived?.Invoke(this, new JsonRpcEventArgs(payload, json));
        }

        private void ConnectCallback()
        {
            ProviderConnected?.Invoke(this, EventArgs.Empty);
        }

        public override bool HasSession => _HasMetaMaskJSSession();
        public override event EventHandler<JsonRpcEventArgs> EthereumEventReceived;
        public override event EventHandler<JsonRpcEventArgs> EthereumRequestReceived;
        public override event EventHandler ProviderConnected;
        
        protected override void DoDispose()
        {
            Debug.Log("Do Dispose");
        }
    }
}
#endif