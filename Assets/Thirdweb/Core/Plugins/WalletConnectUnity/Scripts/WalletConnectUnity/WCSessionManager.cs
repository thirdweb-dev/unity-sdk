using System;
using System.Linq;
using System.Threading.Tasks;
using WalletConnect.UnityBinder;
using UnityEngine;
using UnityEngine.Events;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Sign.Models;

namespace WalletConnect
{
    public class WCSessionManager : BindableMonoBehavior
    {
        [Inject]
        private WCSignClient _walletConnect;

        [Inject]
        private WalletConnectUnity _wcUnity;

        public bool ResumeOnStart = true;

        // Attempt to extend the session if it has expired
        public bool AutoAttemptExtendSession = true;

        public event EventHandler<SessionStruct> SessionResumed;
        public event EventHandler<string> SessionResumeFailed;
        public event EventHandler NoSessionToResume;

        [SerializeField]
        private NoSessionToResumeEvent _noSessionToResume;

        [SerializeField]
        private SessionResumeFailedEvent _sessionResumeFailed;

        [SerializeField]
        private SessionResumedEvent _sessionResumedEvent;

        [Serializable]
        public class NoSessionToResumeEvent : UnityEvent { }

        [Serializable]
        public class SessionResumeFailedEvent : UnityEvent<string> { }

        [Serializable]
        public class SessionResumedEvent : UnityEvent<SessionStruct> { }

        // Start is called before the first frame update
        async void Start()
        {
            if (ResumeOnStart)
            {
                await ResumeFirstSession();
            }
        }

        public async Task<bool> ResumeSession(SessionStruct session)
        {
            // Ensure we are initialized
            await _walletConnect.InitSignClient();

            Debug.Log("Restoring session with " + session.Peer.Metadata.Name);

            if (session.Expiry != null && Clock.IsExpired((long)session.Expiry))
            {
                Debug.LogWarning("The session with " + session.Peer.Metadata.Name + " has expired");
                if (AutoAttemptExtendSession)
                {
                    Debug.Log("Attempting session extend...");
                    try
                    {
                        var ack = await _walletConnect.Extend(session.Topic);

                        await ack.Acknowledged();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        Debug.LogError("Session extend failed");
                        SessionResumeFailure(e.Message);
                        return false;
                    }
                }
                else
                {
                    var msg = "Session has expired, and AutoAttemptExtendSession is set to false";
                    Debug.LogError(msg);
                    SessionResumeFailure(msg);
                    return false;
                }
            }

            SessionResume(session);

            return true;
        }

        public async Task<bool> ResumeFirstSession()
        {
            // Ensure we are initialized
            await _walletConnect.InitSignClient();

            // Find the first session, and check if it's still active
            var sessions = _walletConnect.Session.Values;
            if (sessions.Length == 0)
            {
                NoSessions();
                return false;
            }
            else if (sessions.Length > 1)
            {
                Debug.LogWarning("Multiple sessions found, will attempt the first one found");
            }

            var firstSession = sessions.First();

            return await ResumeSession(firstSession);
        }

        public async Task<bool> ResumeAnySession()
        {
            // Ensure we are initialized
            await _walletConnect.InitSignClient();

            var sessions = _walletConnect.Session.Values;
            if (sessions.Length == 0)
            {
                NoSessions();
                return false;
            }

            foreach (var session in sessions)
            {
                var result = await ResumeSession(session);
                if (result)
                    return true;
            }

            return false;
        }

        protected void SessionResume(SessionStruct session)
        {
            _walletConnect.OnSessionApproval(session);

            if (SessionResumed != null)
                SessionResumed(this, session);

            if (_sessionResumedEvent != null)
                _sessionResumedEvent.Invoke(session);
        }

        protected void NoSessions()
        {
            if (NoSessionToResume != null)
                NoSessionToResume(this, EventArgs.Empty);

            if (_noSessionToResume != null)
                _noSessionToResume.Invoke();
        }

        protected void SessionResumeFailure(string reason)
        {
            if (SessionResumeFailed != null)
                SessionResumeFailed(this, reason);

            if (_sessionResumeFailed != null)
                _sessionResumeFailed.Invoke(reason);
        }

        private void OnValidate()
        {
            if (_wcUnity != null)
                _wcUnity.StorageType = WCStorageType.Disk;
            else if (Application.isEditor)
            {
                var wcUnity = FindObjectOfType<WalletConnectUnity>();
                if (wcUnity != null)
                    wcUnity.StorageType = WCStorageType.Disk;
            }
        }
    }
}
