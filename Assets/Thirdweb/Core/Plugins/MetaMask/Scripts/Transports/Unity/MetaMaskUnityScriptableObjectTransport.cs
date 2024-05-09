using System;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif
using MetaMask.Models;
using MetaMask.Unity;
using UnityEngine;

namespace MetaMask.Transports.Unity
{

    public abstract class MetaMaskUnityScriptableObjectTransport : ScriptableObject, IMetaMaskTransport
    {
        public abstract event EventHandler<MetaMaskUnityRequestEventArgs> Requesting;

        public void Initialize()
        {
            DoInitialize();
            
            // if we are on mobile, set the connection mode to deeplink
            ConnectionMode = IsMobile ? TransportMode.Deeplink : TransportMode.QRCode;
        }
        
        public abstract void DoInitialize();

        public abstract void UpdateUrls(string universalLink, string deepLink);

        public abstract void OnConnectRequest();

        public abstract void OnFailure(Exception error);

        public abstract void OnRequest(string id, MetaMaskEthereumRequest request);
        public abstract void OnOTPCode(int code);

        public abstract void OnSessionRequest(MetaMaskSessionData session);

        public abstract void OnSuccess();

        public bool IsMobile => MetaMaskSDK.IsMobile;
        
        public abstract void OnDisconnect();

        public virtual TransportMode ConnectionMode { get; set; }

        protected void OpenDeeplinkURL(string url)
        {
            MetaMaskSDK.OpenDeeplinkURL(url);
        }

    }

    public class MetaMaskUnityRequestEventArgs : EventArgs
    {

        /// <summary>The request to be sent to MetaMask.</summary>
        public readonly MetaMaskEthereumRequest Request;

        /// <summary>Initializes a new instance of the <see cref="MetaMaskUnityRequestEventArgs"/> class.</summary>
        /// <param name="request">The request.</param>
        public MetaMaskUnityRequestEventArgs(MetaMaskEthereumRequest request)
        {
            this.Request = request;
        }

    }

}