namespace WalletConnectSharp.Core.Models
{
    public static class WalletConnectStates
    {
        public static readonly string SessionUpdate = "wc_sessionUpdate";
        public static readonly string SessionRequest = "wc_sessionRequest";
        public static readonly string Connect = "connect";
        public static readonly string Disconnect = "disconnect";
        public static readonly string SessionFailed = "session_failed";
    }
}
