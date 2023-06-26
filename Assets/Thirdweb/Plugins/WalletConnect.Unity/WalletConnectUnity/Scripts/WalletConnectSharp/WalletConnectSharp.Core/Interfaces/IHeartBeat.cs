using System.Threading.Tasks;
using WalletConnectSharp.Common;
using WalletConnectSharp.Events.Interfaces;

namespace WalletConnectSharp.Core.Interfaces
{
    /// <summary>
    /// The HeartBeat module emits a pulse event at a specific interval simulating
    /// a heartbeat. It can be used as an setInterval replacement or timing actions
    /// </summary>
    public interface IHeartBeat : IEvents, IModule
    {
        /// <summary>
        /// The interval (in milliseconds) the Pulse event gets emitted/triggered
        /// </summary>
        public int Interval { get; }

        /// <summary>
        /// Initialize the heartbeat module. This will start the pulse event and
        /// will continuously emit the pulse event at the configured interval. If the
        /// HeartBeatCancellationToken is cancelled, then the interval will be halted.
        /// </summary>
        /// <returns></returns>
        public Task Init();
    }
}
