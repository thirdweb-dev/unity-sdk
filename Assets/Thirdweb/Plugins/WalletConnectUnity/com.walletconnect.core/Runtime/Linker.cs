using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Core.Models.Publisher;
using WalletConnectSharp.Sign.Models;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;

namespace WalletConnectUnity.Core
{
    public class Linker : IDisposable
    {
        private readonly IWalletConnect _walletConnect;
        private readonly Dictionary<string, uint> _sessionMessagesCounter = new();

        protected bool disposed;

        public Linker(IWalletConnect walletConnect)
        {
            _walletConnect = walletConnect;

            RegisterEventListeners();
        }

        private void RegisterEventListeners()
        {
            _walletConnect.SignClient.Core.Relayer.Publisher.OnPublishedMessage += OnPublisherPublishedMessage;
        }

        public static void OpenSessionProposalDeepLink(string uri, Wallet wallet)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException($"[Linker] Uri cannot be empty.");

#if UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            // In editor we cannot open _mobile_ deep links, so we just log the uri
            Debug.Log($"[Linker] Requested to open mobile deep link. The uri: {uri}");
#else
            var link = Application.isMobilePlatform ? wallet.MobileLink : wallet.DesktopLink;

            if (string.IsNullOrWhiteSpace(link))
                throw new Exception(
                    $"[Linker] No link found for {Application.platform} platform in wallet {wallet.Name}.");

            var url = BuildConnectionDeepLink(link, uri);

            WCLogger.Log($"[Linker] Opening URL {url}");

            Application.OpenURL(url);
#endif
        }

        public static void OpenSessionRequestDeepLink(in SessionStruct session)
        {
            if (string.IsNullOrWhiteSpace(session.Topic))
                throw new Exception("[Linker] No session topic found in provided session. Cannot open deep link.");

            if (session.Peer.Metadata == null)
                return;

            var redirectNative = session.Peer.Metadata.Redirect?.Native;

            if (string.IsNullOrWhiteSpace(redirectNative))
            {
                if (!WalletUtils.TryGetRecentWallet(out var recentWallet))
                    return;

                Debug.LogWarning(
                    $"[Linker] No redirect found for {session.Peer.Metadata.Name}. Using deep link from the Recent Wallet."
                );

                redirectNative = Application.isMobilePlatform ? recentWallet.MobileLink : recentWallet.DesktopLink;
                if (!redirectNative.EndsWith("://"))
                    redirectNative = $"{redirectNative}://";

                Application.OpenURL(redirectNative);
            }
            else
            {
                WCLogger.Log($"[Linker] Open native deep link: {redirectNative}");

                if (!redirectNative.EndsWith("://"))
                    redirectNative = $"{redirectNative}://";

                Application.OpenURL(redirectNative);
            }
        }

        public static string BuildConnectionDeepLink(string appLink, string wcUri)
        {
            if (string.IsNullOrWhiteSpace(wcUri))
                throw new ArgumentException($"[Linker] Uri cannot be empty.");

            if (string.IsNullOrWhiteSpace(appLink))
                throw new ArgumentException($"[Linker] Native link cannot be empty.");

            var safeAppUrl = appLink;
            if (!safeAppUrl.Contains("://"))
            {
                safeAppUrl = safeAppUrl.Replace("/", "").Replace(":", "");
                safeAppUrl = $"{safeAppUrl}://";
            }

            if (!safeAppUrl.EndsWith('/'))
                safeAppUrl = $"{safeAppUrl}/";

            var encodedWcUrl = Uri.EscapeDataString(wcUri);

            return $"{safeAppUrl}wc?uri={encodedWcUrl}";
        }

        public void OpenSessionRequestDeepLink(string sessionTopic)
        {
            var session = _walletConnect.SignClient.Session.Get(sessionTopic);
            OpenSessionRequestDeepLink(in session);
        }

        public virtual void OpenSessionRequestDeepLink()
        {
            var session = _walletConnect.ActiveSession;
            OpenSessionRequestDeepLink(in session);
        }

        protected virtual void OnPublisherPublishedMessage(object sender, PublishParams publishParams)
        {
            WalletConnect.UnitySyncContext.Post(_ =>
            {
                if (string.IsNullOrWhiteSpace(publishParams.Topic))
                    return;

                if (_sessionMessagesCounter.TryGetValue(publishParams.Topic, out var messageCount))
                {
                    WCLogger.Log($"[Linker] OnPublisherPublishedMessage. Message count: {messageCount}");
                    if (messageCount != 0)
                    {
                        _sessionMessagesCounter[publishParams.Topic] = messageCount - 1;
                        OpenSessionRequestDeepLink(publishParams.Topic);
                    }
                }
            }, null);
        }

        public void OpenSessionRequestDeepLinkAfterMessageFromSession(string sessionTopic)
        {
            WCLogger.Log($"[Linker] OpenSessionRequestDeepLinkAfterMessageFromSession. Topic: {sessionTopic}");
            if (_sessionMessagesCounter.TryGetValue(sessionTopic, out var messageCount))
                _sessionMessagesCounter[sessionTopic] = messageCount + 1;
            else
                _sessionMessagesCounter.Add(sessionTopic, 1);
        }

        public static bool CanOpenURL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
#if !UNITY_EDITOR && UNITY_IOS
            return _CanOpenURL(url);
#elif !UNITY_EDITOR && UNITY_ANDROID
            using (var urlCheckerClass = new AndroidJavaClass("com.walletconnect.unity.Linker"))
            using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                return urlCheckerClass.CallStatic<bool>("canOpenURL", currentActivity, url);
            }
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"[Linker] Exception for url {url}: {e.Message}");
            }

            return false;
        }

#if !UNITY_EDITOR && UNITY_IOS
        [DllImport("__Internal")]
        public static extern bool _CanOpenURL(string url);
#endif

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
                _walletConnect.SignClient.Core.Relayer.Publisher.OnPublishedMessage -= OnPublisherPublishedMessage;

            disposed = true;
        }
    }
}