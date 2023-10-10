using System;
using System.Collections.Generic;
using AOT;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Cdm.Authentication.Browser
{
    public class WKWebViewAuthenticationSession : IDisposable
    {
        private static readonly Dictionary<IntPtr, WKWebViewAuthenticationSessionCompletionHandler>
            CompletionCallbacks = new Dictionary<IntPtr, WKWebViewAuthenticationSessionCompletionHandler>();

        private IntPtr _sessionPtr;

        public WKWebViewAuthenticationSession(string url, string callbackUrlScheme,
            WKWebViewAuthenticationSessionCompletionHandler completionHandler)
        {
            _sessionPtr = Cdm_Auth_WKWebViewAuthenticationSession_Init(
                url, callbackUrlScheme, OnAuthenticationSessionCompleted);

            CompletionCallbacks.Add(_sessionPtr, completionHandler);
        }

        public bool Start()
        {
            return Cdm_Auth_WKWebViewAuthenticationSession_Start(_sessionPtr) == 1;
        }

        public void Cancel()
        {
            Cdm_Auth_WKWebViewAuthenticationSession_Cancel(_sessionPtr);
        }

        public void Dispose()
        {
            Cdm_Auth_WKWebViewAuthenticationSession_Dispose(_sessionPtr);
            CompletionCallbacks.Remove(_sessionPtr);
            _sessionPtr = IntPtr.Zero;
        }

#if UNITY_IOS && !UNITY_EDITOR
        private const string DllName = "__Internal";
        
        [DllImport(DllName)]
        private static extern IntPtr Cdm_Auth_WKWebViewAuthenticationSession_Init(string url, string callbackUrlScheme,
            AuthenticationSessionCompletedCallback completionHandler);
        
        [DllImport(DllName)]
        private static extern int Cdm_Auth_WKWebViewAuthenticationSession_Start(IntPtr session);
        
        [DllImport(DllName)]
        private static extern void Cdm_Auth_WKWebViewAuthenticationSession_Cancel(IntPtr session);
        
        [DllImport(DllName)]
        private static extern void Cdm_Auth_WKWebViewAuthenticationSession_Dispose(IntPtr session);
#else
        private const string NotSupportedMsg = "Only iOS platform is supported.";

        private static IntPtr Cdm_Auth_WKWebViewAuthenticationSession_Init(string url, string callbackUrlScheme,
            AuthenticationSessionCompletedCallback completionHandler)
        {
            throw new NotImplementedException(NotSupportedMsg);
        }

        private static int Cdm_Auth_WKWebViewAuthenticationSession_Start(IntPtr session)
        {
            throw new NotImplementedException(NotSupportedMsg);
        }

        private static void Cdm_Auth_WKWebViewAuthenticationSession_Cancel(IntPtr session)
        {
            throw new NotImplementedException(NotSupportedMsg);
        }
        
        private static void Cdm_Auth_WKWebViewAuthenticationSession_Dispose(IntPtr session)
        {
        }
#endif

        public delegate void WKWebViewAuthenticationSessionCompletionHandler(string callbackUrl,
            WKWebViewAuthenticationSessionError error);

        private delegate void AuthenticationSessionCompletedCallback(IntPtr session, string callbackUrl,
            int errorCode, string errorMessage);

        [MonoPInvokeCallback(typeof(AuthenticationSessionCompletedCallback))]
        private static void OnAuthenticationSessionCompleted(IntPtr session, string callbackUrl,
            int errorCode, string errorMessage)
        {
            if (CompletionCallbacks.TryGetValue(session, out var callback))
            {
                callback?.Invoke(callbackUrl,
                    new WKWebViewAuthenticationSessionError((WKWebViewAuthenticationSessionErrorCode)errorCode,
                        errorMessage));
            }
        }
    }
}