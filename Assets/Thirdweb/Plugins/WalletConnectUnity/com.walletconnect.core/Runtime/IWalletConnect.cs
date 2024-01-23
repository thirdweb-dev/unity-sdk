using System;
using System.Threading;
using System.Threading.Tasks;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectUnity.Core
{
    public interface IWalletConnect : IDisposable
    {
        public static SynchronizationContext UnitySyncContext { get; }
        
        public ISignClient SignClient { get; }

        public Linker Linker { get; }

        public SessionStruct ActiveSession { get; }

        public bool IsInitialized { get; }

        public bool IsConnected { get; }

        public event EventHandler<SessionStruct> ActiveSessionChanged;
        public event EventHandler<SessionStruct> SessionConnected;
        public event EventHandler<SessionStruct> SessionUpdated;
        public event EventHandler SessionDisconnected;

        public Task<IWalletConnect> InitializeAsync();

        public Task<bool> TryResumeSessionAsync();

        public Task<ConnectedData> ConnectAsync(ConnectOptions options);

        public Task<TResponse> RequestAsync<TRequestData, TResponse>(TRequestData data);

        public Task DisconnectAsync();
    }
}