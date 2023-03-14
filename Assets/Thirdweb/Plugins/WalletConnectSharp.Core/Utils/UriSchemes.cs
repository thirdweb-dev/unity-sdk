namespace WalletConnectSharp.Core.Utils
{
    /// <summary>
    /// This is a helper class for URI related schemes that are not in .Net Standard 2.1.
    /// TODO: Remove this class when Unity supports .Net > 6.0.0
    /// </summary>
    public static class UriSchemes
    {
        public static readonly string UriSchemeWs = "ws";
        public static readonly string UriSchemeWss = "wss";
    }
}
