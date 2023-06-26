using System.Text.RegularExpressions;

namespace WalletConnectSharp.Network
{
    /// <summary>
    /// A helper class to aid in validation
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// Regex used to validate the WS/WSS protocol string
        /// </summary>
        public const string WS_REGEX = "^wss?:";
        
        /// <summary>
        /// Regex used to validate the HTTP/HTTPS protocol string
        /// </summary>
        public const string HTTP_REGEX = "^https?:";

        /// <summary>
        /// Given a URL, return only the protocol string
        /// </summary>
        /// <param name="url">The URL to pull the protocol string from</param>
        /// <returns>A substring only containing the URL protocol</returns>
        public static string GetUrlProtocol(string url)
        {
            return Regex.Match(url, "^\\w+:/", RegexOptions.IgnoreCase).Value;
        }

        /// <summary>
        /// Match the URL's protocol to the given regex. The URL provided can be the full URL
        /// </summary>
        /// <param name="url">The URL to test against the provided regex</param>
        /// <param name="regex">The regex to used to test against the URL's protocol</param>
        /// <returns>True if the regex matches the URL's protocol, otherwise false</returns>
        public static bool MatchRegexProtocol(string url, string regex)
        {
            var protocol = GetUrlProtocol(url);
            if (protocol == null) return false;
            return Regex.IsMatch(protocol, regex);
        }

        /// <summary>
        /// Whether a given URL string is WS/WSS
        /// </summary>
        /// <param name="url">The URL string to check</param>
        /// <returns>True if the URL provided is a WS url, false otherwise</returns>
        public static bool IsWsUrl(string url)
        {
            return MatchRegexProtocol(url, WS_REGEX);
        }

        /// <summary>
        /// Whether a given URL string is HTTP/HTTPS
        /// </summary>
        /// <param name="url">The URL string to test</param>
        /// <returns>True if the URL provided is a HTTP/HTTPS url, false otherwise</returns>
        public static bool IsHttpUrl(string url)
        {
            return MatchRegexProtocol(url, HTTP_REGEX);
        }

        /// <summary>
        /// Whether a given URL is a localhost connection using WS/WSS
        /// </summary>
        /// <param name="url">The URL to test</param>
        /// <returns>True if the URL provided is a localhost connection using WS/WSS, false otherwise</returns>
        public static bool IsLocalhost(string url)
        {
            return Regex.IsMatch(url, "wss?://localhost(:d{2,5})?");
        }
    }
}
