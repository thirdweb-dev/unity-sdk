using System;
using System.Collections.Generic;
using AOT;

#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Cdm.Authentication.Browser
{
    /// <summary>
    /// A session that an app uses to authenticate a user through a web service.
    /// </summary>
    /// <seealso ahref="https://developer.apple.com/documentation/authenticationservices/aswebauthenticationsession?language=objc"/>
    public class ASWebAuthenticationSession : IDisposable
    {
        private static readonly Dictionary<IntPtr, ASWebAuthenticationSessionCompletionHandler> CompletionCallbacks =
            new Dictionary<IntPtr, ASWebAuthenticationSessionCompletionHandler>();
        
        private IntPtr _sessionPtr;

        /// <summary>
        /// A Boolean value that indicates whether the session should ask the browser for a private authentication
        /// session.
        /// </summary>
        /// <remarks>Set this property before you call <see cref="Start"/>. Otherwise it has no effect.</remarks>
        public bool prefersEphemeralWebBrowserSession
        {
            get => Cdm_Auth_ASWebAuthenticationSession_GetPrefersEphemeralWebBrowserSession(_sessionPtr) == 1;
            set => Cdm_Auth_ASWebAuthenticationSession_SetPrefersEphemeralWebBrowserSession(_sessionPtr, value ? 1 : 0);
        }

        /// <summary>
        /// Creates a web authentication session instance.
        /// </summary>
        /// <param name="url">A URL with the http or https scheme pointing to the authentication webpage.</param>
        /// <param name="callbackUrlScheme">The custom URL scheme that the app expects in the callback URL.</param>
        /// <param name="completionHandler">A completion handler the session calls when it completes successfully, or when the user cancels the session.</param>
        /// <seealso ahref="https://developer.apple.com/documentation/authenticationservices/aswebauthenticationsession/2990952-initwithurl?language=objc"/>
        public ASWebAuthenticationSession(string url, string callbackUrlScheme, 
            ASWebAuthenticationSessionCompletionHandler completionHandler)
        {
            _sessionPtr = 
                Cdm_Auth_ASWebAuthenticationSession_InitWithURL(
                    url, callbackUrlScheme, OnAuthenticationSessionCompleted);
            
            CompletionCallbacks.Add(_sessionPtr, completionHandler);
        }

        /// <summary>
        /// Starts a web authentication session.
        /// </summary>
        /// <returns>A Boolean value indicating whether the web authentication session started successfully.</returns>
        /// <seealso ahref="https://developer.apple.com/documentation/authenticationservices/aswebauthenticationsession/2990953-start?language=objc"/>
        public bool Start()
        {
            return Cdm_Auth_ASWebAuthenticationSession_Start(_sessionPtr) == 1;
        }

        /// <summary>
        /// Cancels a web authentication session.
        /// </summary>
        /// <seealso ahref="https://developer.apple.com/documentation/authenticationservices/aswebauthenticationsession/2990951-cancel?language=objc"/>
        public void Cancel()
        {
            Cdm_Auth_ASWebAuthenticationSession_Cancel(_sessionPtr);
        }
        
        public void Dispose()
        {
            CompletionCallbacks.Remove(_sessionPtr);
            _sessionPtr = IntPtr.Zero;
        }
        
#if UNITY_IOS && !UNITY_EDITOR
        private const string DllName = "__Internal";
        
        [DllImport(DllName)]
        private static extern IntPtr Cdm_Auth_ASWebAuthenticationSession_InitWithURL(
            string url, string callbackUrlScheme, AuthenticationSessionCompletedCallback completionHandler);
        
        [DllImport(DllName)]
        private static extern int Cdm_Auth_ASWebAuthenticationSession_Start(IntPtr session);
        
        [DllImport(DllName)]
        private static extern void Cdm_Auth_ASWebAuthenticationSession_Cancel(IntPtr session);

        [DllImport(DllName)]
        private static extern int Cdm_Auth_ASWebAuthenticationSession_GetPrefersEphemeralWebBrowserSession(IntPtr session);

        [DllImport(DllName)]
        private static extern void Cdm_Auth_ASWebAuthenticationSession_SetPrefersEphemeralWebBrowserSession(
            IntPtr session, int enable);
#else

        private const string NotSupportedMsg = "Only iOS platform is supported.";
        
        private static IntPtr Cdm_Auth_ASWebAuthenticationSession_InitWithURL(
            string url, string callbackUrlScheme, AuthenticationSessionCompletedCallback completionHandler)
        {
            throw new NotImplementedException(NotSupportedMsg);
        }

        private static int Cdm_Auth_ASWebAuthenticationSession_Start(IntPtr session)
        {
            throw new NotImplementedException(NotSupportedMsg);
        }

        private static void Cdm_Auth_ASWebAuthenticationSession_Cancel(IntPtr session)
        {
            throw new NotImplementedException(NotSupportedMsg);
        }

        private static int Cdm_Auth_ASWebAuthenticationSession_GetPrefersEphemeralWebBrowserSession(IntPtr session)
        {
            throw new NotImplementedException(NotSupportedMsg);
        }

        private static void Cdm_Auth_ASWebAuthenticationSession_SetPrefersEphemeralWebBrowserSession(
            IntPtr session, int enable)
        {
            throw new NotImplementedException(NotSupportedMsg);
        }
#endif
        public delegate void ASWebAuthenticationSessionCompletionHandler(string callbackUrl, 
            ASWebAuthenticationSessionError error);

        private delegate void AuthenticationSessionCompletedCallback(IntPtr session, string callbackUrl, 
            int errorCode, string errorMessage);
        
        [MonoPInvokeCallback(typeof(AuthenticationSessionCompletedCallback))]
        private static void OnAuthenticationSessionCompleted(IntPtr session, string callbackUrl, 
            int errorCode, string errorMessage)
        {
            if (CompletionCallbacks.TryGetValue(session, out var callback))
            {
                callback?.Invoke(callbackUrl, 
                    new ASWebAuthenticationSessionError((ASWebAuthenticationSessionErrorCode) errorCode, errorMessage));
            }
        }
    }
}