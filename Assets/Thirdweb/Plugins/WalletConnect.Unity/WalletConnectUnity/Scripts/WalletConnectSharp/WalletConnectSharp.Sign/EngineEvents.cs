using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models.Engine.Events;

namespace WalletConnectSharp.Sign
{
    /// <summary>
    /// A static class that holds all event ids the <see cref="Engine"/> module
    /// will emit
    /// </summary>
    public static class EngineEvents
    {
        /// <summary>
        /// The event id for the session expire event. Triggered
        /// when a session has expired
        /// </summary>
        public const string SessionExpire = "session_expire";
        
        /// <summary>
        /// The event id for the pairing expire event. Triggered when
        /// a pairing has expired
        /// </summary>
        public const string PairingExpire = "pairing_expire";
        
        /// <summary>
        /// The event id for the session proposal event. Triggered
        /// when a new session proposal is received
        /// </summary>
        public const string SessionProposal = "session_proposal";
        
        /// <summary>
        /// The event id for the session connect event. Triggered
        /// when a new session has been connected
        /// </summary>
        public const string SessionConnect = "session_connect";
        
        /// <summary>
        /// The event id for the session update event. Triggered
        /// when a session's data has been updated
        /// </summary>
        public const string SessionUpdate = "session_update";
        
        /// <summary>
        /// The event id for the session extend event. Triggered
        /// when a session's expiration date has been extended
        /// </summary>
        public const string SessionExtend = "session_extend";
        
        /// <summary>
        /// The event id for the session ping event. Triggered
        /// when a session ping is received
        /// </summary>
        public const string SessionPing = "session_ping";
        
        /// <summary>
        /// The event id for the pairing ping event. Triggered
        /// when a pairing ping is received
        /// </summary>
        public const string PairingPing = "pairing_ping";
        
        /// <summary>
        /// The event id for the session delete event. Triggered
        /// when a session has been deleted
        /// </summary>
        public const string SessionDelete = "session_delete";
        
        /// <summary>
        /// The event id for the pairing delete event. Triggered
        /// when a pairing has been deleted
        /// </summary>
        public const string PairingDelete = "pairing_delete";
        
        /// <summary>
        /// The event id for the session request event. Triggered
        /// when ANY session request is received during a session
        /// </summary>
        public const string SessionRequest = "session_request";
        
        /// <summary>
        /// The event id for the session event event. Triggered
        /// when ANY session event is received during a session
        ///
        /// NOTE: Session events are only received/triggered through the
        /// <see cref="IEngineAPI.Emit{T}(string, EventData{T}, string)"/> function.
        /// </summary>
        public const string SessionEvent = "session_event";
    }
}
