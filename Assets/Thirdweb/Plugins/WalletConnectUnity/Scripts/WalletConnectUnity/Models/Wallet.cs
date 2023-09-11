using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectUnity.Models
{
    public class Wallet
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("slug")]
        public string Slug;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("homepage")]
        public string Homepage;

        [JsonProperty("chains")]
        public string[] Chains;

        [JsonProperty("versions")]
        public string[] Versions;

        [JsonProperty("sdks")]
        public string[] Sdks;

        [JsonProperty("app_type")]
        public string AppType;

        [JsonProperty("image_id")]
        public string ImageId;

        [JsonProperty("image_url")]
        public ImageUrls Images;

        [JsonProperty("app")]
        public WalletApp App;

        [JsonProperty("injected")]
        public List<WalletInjection> Injected;

        [JsonProperty("mobile")]
        public WalletLink Mobile;

        [JsonProperty("desktop")]
        public WalletLink Desktop;

        [JsonProperty("supported_standards")]
        public List<WalletStandard> SupportedStandards;

        [JsonProperty("metadata")]
        public WalletMetadata Metadata;

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt;

        public void OpenDeeplink(ConnectedData data, bool useNative = false)
        {
            string uri = "";
#if UNITY_ANDROID
            uri = data.Uri; // Android OS should handle wc: protocol 
#elif UNITY_IOS
            // on iOS, we need to use one of the wallet links
            WalletLink linkData;
            linkData = Application.isMobilePlatform ? this.Mobile : this.Desktop;
            
            var universalUrl = useNative ? linkData.NativeProtocol : linkData.UniversalUrl;

            uri = data.Uri;
            if (!string.IsNullOrWhiteSpace(universalUrl))
            {
                uri = data.Uri;

                if (useNative)
                    uri = universalUrl + "//" + uri;
                else if (universalUrl.EndsWith("/"))
                    uri = universalUrl + uri;
                else
                    uri = universalUrl + "/" + uri;
            }

            if (string.IsNullOrWhiteSpace(uri))
                throw new Exception("Got empty URI when attempting to create WC deeplink");
            
            Debug.Log("Opening URL " + uri);
#endif
            Application.OpenURL(uri);
        }

        public void OpenWallet()
        {
            WalletLink linkData;
            linkData = Application.isMobilePlatform ? this.Mobile : this.Desktop;

            var universalUrl = linkData.UniversalUrl;

            if (string.IsNullOrWhiteSpace(universalUrl))
                throw new Exception("Got empty URI when attempting to create WC deeplink");

            Application.OpenURL(universalUrl);
        }
    }
}
