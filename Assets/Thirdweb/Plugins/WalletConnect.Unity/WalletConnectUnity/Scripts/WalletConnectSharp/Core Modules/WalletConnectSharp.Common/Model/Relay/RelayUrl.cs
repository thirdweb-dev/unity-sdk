using System;

namespace WalletConnectSharp.Common.Utils
{
    /// <summary>
    /// A helper class for generating the relay url for WalletConnect relay servers
    /// </summary>
    public static class RelayUrl
    {
        /// <summary>
        /// Populate the relayUrl parameters with the other parameters as query parameters
        /// </summary>
        /// <param name="relayUrl">The relay URL to populate</param>
        /// <param name="protocol">The protocol string being used</param>
        /// <param name="version">The protocol version being used</param>
        /// <param name="sdkVersion">The SDK version being used</param>
        /// <param name="projectId">The project Id being used</param>
        /// <param name="auth">The auth code</param>
        /// <returns>The relay URL with the given parameters encoded as url-encoding</returns>
        public static string FormatRelayRpcUrl(string relayUrl, string protocol, string version, string sdkVersion,
            string projectId, string auth)
        {
            var splitUrl = relayUrl.Split("?");
            var ua = FormatUA(protocol, version, sdkVersion);

            var currentParameters = UrlUtils.ParseQs(splitUrl.Length > 1 ? splitUrl[1] : "");
            
            currentParameters.Add("auth", auth);
            currentParameters.Add("projectId", projectId);
            currentParameters.Add("ua", ua);

            var formattedParameters = UrlUtils.StringifyQs(currentParameters);

            return splitUrl[0] + formattedParameters;
        }
        
        /// <summary>
        /// Format the user agent and attach it to the url parameter
        /// </summary>
        /// <param name="protocol">The protocol being used</param>
        /// <param name="version">The protocol version being used</param>
        /// <param name="sdkVersion">The SDK version being used</param>
        /// <returns>The URL parameters given with additional user-agent data attached</returns>
        public static string FormatUA(string protocol, string version, string sdkVersion)
        {
            var os = Environment.OSVersion.Platform.ToString();
            var osVersion = Environment.OSVersion.Version.ToString();

            var osInfo = string.Join("-", os, osVersion);
            var environment = "WalletConnectSharpv2:" + Environment.Version;

            var sdkType = "csharp";

            return string.Join("/", string.Join("-", protocol, version), string.Join("-", sdkType, sdkVersion), osInfo,
                environment);
        }
    }
}