namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// Event id constants for JsonRpcProvider
    /// </summary>
    public static class ProviderEvents
    {
        /// <summary>
        /// The event id for the Payload event
        /// </summary>
        public static readonly string Payload = "payload";
        
        /// <summary>
        /// The event id of the Connect event
        /// </summary>
        public static readonly string Connect = "connect";
        
        /// <summary>
        /// The event id of the Disconnect event
        /// </summary>
        public static readonly string Disconnect = "disconnect";
        
        /// <summary>
        /// The event id of the Error event
        /// </summary>
        public static readonly string Error = "error";
        
        /// <summary>
        /// The event id of the RawRequestMessage event
        /// </summary>
        public static readonly string RawRequestMessage = "message";
    }
}
