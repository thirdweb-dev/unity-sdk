namespace WalletConnectSharp.Core.Models.Pairing
{
    public static class PairingEvents
    {
        /// <summary>
        /// The event id for the pairing expire event. Triggered when
        /// a pairing has expired
        /// </summary>
        public const string PairingExpire = "pairing_expire";
        
        /// <summary>
        /// The event id for the pairing ping event. Triggered
        /// when a pairing ping is received
        /// </summary>
        public const string PairingPing = "pairing_ping";
        
        /// <summary>
        /// The event id for the pairing delete event. Triggered
        /// when a pairing has been deleted
        /// </summary>
        public const string PairingDelete = "pairing_delete";
    }
}
