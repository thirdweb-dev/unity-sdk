using System;
using System.Text;
using UnityEngine.Networking;

namespace WalletConnectUnity.Core.Networking
{
    public static class UnityWebRequestWallets
    {
        public static UnityWebRequest GetWallets(string url, in UrlQueryParams queryParams)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException(url);
            if (string.IsNullOrWhiteSpace(queryParams.Page)) throw new ArgumentException(queryParams.Page);
            if (string.IsNullOrWhiteSpace(queryParams.Entries)) throw new ArgumentException(queryParams.Entries);

            var sb = new StringBuilder(url);

            sb.Append("?page=").Append(queryParams.Page);
            sb.Append("&entries=").Append(queryParams.Entries);

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
                sb.Append("&search=").Append(queryParams.Search);

            if (!string.IsNullOrWhiteSpace(queryParams.Include))
                sb.Append("&include=").Append(queryParams.Include);

            if (!string.IsNullOrWhiteSpace(queryParams.Exclude))
                sb.Append("&exclude=").Append(queryParams.Exclude);

            if (!string.IsNullOrWhiteSpace(queryParams.Platform))
                sb.Append("&platform=").Append(queryParams.Platform);

            return UnityWebRequest
                .Get(sb.ToString())
                .SetWalletConnectRequestHeaders();
        }
    }

    public class UnityWebRequestWalletsFactory
    {
        private readonly string _url;
        private readonly string[] _includedWalletIds;
        private readonly string[] _excludedWalletIds;

        private const string Platform =
#if UNITY_ANDROID
            "android";
#elif UNITY_IOS
            "ios";
#else
            null;
#endif

        public UnityWebRequestWalletsFactory(string url = "https://api.web3modal.com/getWallets", string[] includedWalletIds = null, string[] excludedWalletIds = null)
        {
            _url = url;
            _includedWalletIds = includedWalletIds;
            _excludedWalletIds = excludedWalletIds;
        }
        

        public UnityWebRequest GetWallets(int page, int entries, string search = null)
        {
            var queryParams = new UrlQueryParams
            {
                Page = page.ToString(),
                Entries = entries.ToString(),
                Include = _includedWalletIds is { Length: > 0 }
                    ? string.Join(",", _includedWalletIds)
                    : null,
                Exclude = _excludedWalletIds is { Length: > 0 }
                    ? string.Join(",", _excludedWalletIds)
                    : null,
                Platform = Platform,
                Search = search,
            };

            return UnityWebRequestWallets.GetWallets(_url, in queryParams);
        }
    }
}