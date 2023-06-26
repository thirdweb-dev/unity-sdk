namespace WalletConnectSharp.Common.Utils
{
    /// <summary>
    /// Core constants used by the Core module
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The core protocol string
        /// </summary>
        public const string CORE_PROTOCOL = "wc";
        
        /// <summary>
        /// The core protocol version
        /// </summary>
        public const int CORE_VERSION = 2;
        
        /// <summary>
        /// The core context string
        /// </summary>
        public const string CORE_CONTEXT = "core";

        /// <summary>
        /// The core storage prefix
        /// </summary>
        public static readonly string CORE_STORAGE_PREFIX = CORE_PROTOCOL + "@" + CORE_VERSION + ":" + CORE_CONTEXT + ":";
    }
}