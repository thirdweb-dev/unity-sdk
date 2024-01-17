using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectUnity.Core
{
    public class WalletConnect : IWalletConnect
    {
        private static readonly Lazy<IWalletConnect> LazyInstance = new(() => new WalletConnect());
        public static IWalletConnect Instance { get; } = LazyInstance.Value;

        public ISignClient SignClient { get; private set; }

        public Linker Linker { get; private set; }

        public SessionStruct ActiveSession
        {
            get => _activeSession;
            private set
            {
                _activeSession = value;
                if (!string.IsNullOrWhiteSpace(value.Topic))
                {
                    ActiveSessionChanged?.Invoke(this, value);
                }
            }
        }

        public bool IsInitialized { get; private set; }

        public bool IsConnected => !string.IsNullOrWhiteSpace(ActiveSession.Topic);

        private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);

        public event EventHandler<SessionStruct> ActiveSessionChanged;

        private SessionStruct _activeSession;
        protected bool disposed;

        public async Task<IWalletConnect> InitializeAsync()
        {
            try
            {
                await _initializationSemaphore.WaitAsync();

                if (IsInitialized)
                {
                    Debug.LogError("[WalletConnectUnity] Already initialized");
                    return this;
                }

                var projectConfig = ProjectConfiguration.Load();

                Assert.IsNotNull(projectConfig, $"Project configuration not found. Expected to find it at <i>{ProjectConfiguration.ConfigPath}</i>");
                Assert.IsFalse(string.IsNullOrWhiteSpace(projectConfig.Id), $"Project ID is not set in the project configuration asset ( <i>{ProjectConfiguration.ConfigPath}</i> ).");
                Assert.IsFalse(
                    projectConfig.Metadata == null || string.IsNullOrWhiteSpace(projectConfig.Metadata.Name),
                    $"Project name is not set in the project configuration asset ( <i>{ProjectConfiguration.ConfigPath}</i> )."
                );

                if (projectConfig.LoggingEnabled)
                    WCLogger.Logger = new Logger();

                var storage = BuildStorage();

                SignClient = await WalletConnectSignClient.Init(
                    new SignClientOptions
                    {
                        Metadata = projectConfig.Metadata,
                        Name = projectConfig.Metadata.Name,
                        ProjectId = projectConfig.Id,
                        Storage = storage,
                        RelayUrlBuilder = new UnityRelayUrlBuilder(),
                        ConnectionBuilder = new NativeWebSocketConnectionBuilder()
                    }
                );

                SignClient.SessionConnected += OnSessionConnected;
                SignClient.SessionUpdated += OnSessionUpdated;
                SignClient.SessionDeleted += OnSessionDeleted;

                Linker = new Linker(this);

                IsInitialized = true;

                return this;
            }
            finally
            {
                _initializationSemaphore.Release();
            }
        }

        public async Task<bool> TryResumeSessionAsync()
        {
            var sessions = SignClient.Session.Values;
            if (sessions.Length == 0)
                return false;

            var session = sessions.FirstOrDefault(session => session.Acknowledged == true);

            if (string.IsNullOrWhiteSpace(session.Topic))
                return false;

            if (!session.Expiry.HasValue || Clock.IsExpired(session.Expiry.Value))
            {
                var acknowledgement = await SignClient.Extend(session.Topic);
                await acknowledgement.Acknowledged();
            }

            ActiveSession = session;

            return true;
        }

        public Task<ConnectedData> ConnectAsync(ConnectOptions options)
        {
            return SignClient.Connect(options);
        }

        public Task<TResponse> RequestAsync<TRequestData, TResponse>(TRequestData data)
        {
            ThrowIfNoActiveSession();

            var activeSessionTopic = ActiveSession.Topic;

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Linker.OpenSessionRequestDeepLinkAfterMessageFromSession(activeSessionTopic);
#endif

            return SignClient.Request<TRequestData, TResponse>(activeSessionTopic, data);
        }

        public Task DisconnectAsync()
        {
            ThrowIfNoActiveSession();
            return SignClient.Disconnect(ActiveSession.Topic, new SessionDelete());
        }

        private void OnSessionConnected(object sender, SessionStruct session)
        {
            ActiveSession = session;
        }

        private void OnSessionUpdated(object sender, SessionEvent sessionEvent)
        {
            ActiveSession = SignClient.Session.Values.First(s => s.Topic == sessionEvent.Topic);
        }

        private void OnSessionDeleted(object sender, SessionEvent _)
        {
            ActiveSession = default;
        }

        private static IKeyValueStorage BuildStorage()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            var currentSyncContext = SynchronizationContext.Current;
            if (currentSyncContext.GetType().FullName != "UnityEngine.UnitySynchronizationContext")
                throw new Exception(
                    $"[WalletConnectUnity] SynchronizationContext is not of type UnityEngine.UnitySynchronizationContext. Current type is <i>{currentSyncContext.GetType().FullName}</i>. When targeting WebGL, Make sure to initialize WalletConnect from the main thread.");

            return new PlayerPrefsStorage(currentSyncContext);
#else
            var path = $"{Application.persistentDataPath}/WalletConnect/storage.json";
            WCLogger.Log($"[WalletConnectUnity] Using storage path <i>{path}</i>");
            return new FileSystemStorage(path);
#endif
        }

        private void ThrowIfNoActiveSession()
        {
            if (!IsInitialized || string.IsNullOrWhiteSpace(ActiveSession.Topic))
                throw new Exception("No active session");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                SignClient?.Dispose();
                Linker?.Dispose();

                _initializationSemaphore?.Dispose();
            }

            disposed = true;
        }
    }
}
