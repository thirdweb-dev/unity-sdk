namespace WalletConnectSharp.Core.Models.Relay
{
    /// <summary>
    /// A static class that contains the several event ids emitted by the Relayer module
    /// </summary>
    public static class RelayerEvents
    {
        public static readonly string TransportClosed = "relayer_transport_closed";

        public static readonly string ConnectionStalled = "relayer_connection_stalled";

        /// <summary>
        /// The event id for the publish event
        /// </summary>
        public static readonly string Publish = "relayer_publish";

        /// <summary>
        /// The event id for the message event
        /// </summary>
        public static readonly string Message = "relayer_message";

        /// <summary>
        /// The event id for the connect event
        /// </summary>
        public static readonly string Connect = "relayer_connect";

        /// <summary>
        /// The event id for the disconnect event
        /// </summary>
        public static readonly string Disconnect = "relayer_disconnect";

        /// <summary>
        /// The event id for the error event
        /// </summary>
        public static readonly string Error = "relayer_error";
    }
}
