using System;
using JetBrains.Annotations;
using link.magic.unity.sdk.Utility;
using UnityEngine;

namespace link.magic.unity.sdk.Relayer
{
    public class UrlBuilder
    {
        internal static UrlBuilder Instance;

        public static readonly string Host = "https://box.magic.link";
        
        internal readonly string EncodedParams;
        public string apikey;

        internal UrlBuilder(string apikey, CustomNodeConfiguration customNode, string locale = "en-US")
        {
            var options = new CustomNodeOptions();
            options.ETH_NETWORK = customNode;
            options.locale = locale;
            options.API_KEY = apikey;
            options.bundleId = Application.identifier;
            var optionsJsonString = JsonUtility.ToJson(options);
            EncodedParams = MagicUtility.BtoA(optionsJsonString);
        }

        internal UrlBuilder(string apikey, EthNetwork ethNetwork, string locale)
        {
            var options = new EthNetworkOptions();
            options.ETH_NETWORK = ethNetwork.ToString().ToLower();
            options.locale = locale;
            options.API_KEY = apikey;
            options.bundleId = Application.identifier;
            var optionsJsonString = JsonUtility.ToJson(options);
            EncodedParams = MagicUtility.BtoA(optionsJsonString);
        }
    }

    [Serializable]
    internal class EthNetworkOptions : BaseOptions
    {
        public string ETH_NETWORK;
    }

    [Serializable]
    internal class CustomNodeOptions : BaseOptions
    {
        public CustomNodeConfiguration ETH_NETWORK;
    }

    [Serializable]
    public class CustomNodeConfiguration
    {
        [SerializeField] [CanBeNull] internal int? chainId; 
        [SerializeField] internal string rpcUrl;

        public CustomNodeConfiguration(string rpcUrl, int? chainId)
        {
            this.rpcUrl = rpcUrl;
            this.chainId = chainId;
        }
    }
    
    [Serializable]
    public class BaseOptions
    {
        public string host = UrlBuilder.Host;
        public string sdk = "magic-sdk-unity";
        public string API_KEY;
        public string locale;
        public string bundleId;
    }
}