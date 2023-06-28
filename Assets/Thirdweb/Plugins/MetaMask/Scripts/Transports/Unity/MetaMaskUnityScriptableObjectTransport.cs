using System;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif
using MetaMask.Models;
using UnityEngine;

namespace MetaMask.Transports.Unity
{

    public abstract class MetaMaskUnityScriptableObjectTransport : ScriptableObject, IMetaMaskTransport
    {
        public bool isWebGL
        {
            get
            {
#if UNITY_WEBGL
                return true;
#else
                return false;

#endif
            }
        }

        public abstract event EventHandler<MetaMaskUnityConnectEventArgs> Connecting;
        public abstract event EventHandler<MetaMaskUnityRequestEventArgs> Requesting;

        public abstract string UserAgent { get; }

        public abstract void Initialize();

        public abstract void Connect(string url);

        public abstract void OnConnectRequest(string url);

        public abstract void OnFailure(Exception error);

        public abstract void OnRequest(string id, MetaMaskEthereumRequest request);

        public abstract void OnSessionRequest(MetaMaskSessionData session);

        public abstract void OnSuccess();
        
#if UNITY_WEBGL
        [DllImport("__Internal")]
        public static extern void OpenMetaMaskDeeplink(string url);
        
        [DllImport("__Internal")]
        public static extern bool WebGLIsMobile();
#endif

        public bool IsMobile
        {
            get
            {
                #if UNITY_WEBGL
                    return WebGLIsMobile();
                #else
                    return Application.isMobilePlatform;
                #endif
            }
        }

        public void OpenDeeplinkURL(string url)
        {
#if UNITY_WEBGL
            OpenMetaMaskDeeplink(url);
#else
            Application.OpenURL(url);
#endif
        }

    }

    public class MetaMaskUnityConnectEventArgs : EventArgs
    {
        /// <summary>The Url to be called</summary>
        public readonly string Url;

        /// <summary>Initializes a new instance of the <see cref="MetaMaskUnityConnectEventArgs"/> class.</summary>
        /// <param name="url">The URL of the Unity application.</param>
        public MetaMaskUnityConnectEventArgs(string url)
        {
            this.Url = url;
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