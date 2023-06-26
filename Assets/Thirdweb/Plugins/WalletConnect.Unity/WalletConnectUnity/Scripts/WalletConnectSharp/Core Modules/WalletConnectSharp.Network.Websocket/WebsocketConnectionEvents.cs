namespace WalletConnectSharp.Network.Websocket
{
    /// <summary>
    /// Constants defining WebsocketConnection eventIds
    /// </summary>
    public static class WebsocketConnectionEvents
    {
        /// <summary>
        /// The eventId for the Payload event
        /// </summary>
        public const string Payload = "payload";
        
        /// <summary>
        /// The eventId for the Close event
        /// </summary>
        public const string Close = "close";
        
        /// <summary>
        /// The eventId of the RegisterError event
        /// </summary>
        public const string RegisterError = "register_error";
        
        /// <summary>
        /// The eventId of the Open event
        /// </summary>
        public const string Open = "open";
        
        /// <summary>
        /// The eventId of the Error event
        /// </summary>
        public const string Error = "error";
    }
}
