using WalletConnectSharp.Core.Interfaces;

namespace WalletConnectSharp.Core.Models.History
{
    /// <summary>
    /// A class containing all events emitted by the JsonRpcHistory module
    /// </summary>
    public static class HistoryEvents
    {
        /// <summary>
        /// The event id emitted when a new history entry was created
        /// </summary>
        public static readonly string Created = "history_created";
        
        /// <summary>
        /// The event id emitted when a history entry was updated, usually
        /// through when the <see cref="IJsonRpcHistory{T,TR}.Resolve"/> method
        /// is invoked.
        /// </summary>
        public static readonly string Updated = "history_updated";
        
        /// <summary>
        /// The event id emitted when a history entry was deleted.
        /// </summary>
        public static readonly string Deleted = "history_deleted";
        
        /// <summary>
        /// The event id emitted when the history module has synced data to/from storage
        /// </summary>
        public static readonly string Sync = "history_sync";
    }
}
