using link.magic.unity.sdk.Modules.Auth;
using link.magic.unity.sdk.Modules.User;
using link.magic.unity.sdk.Provider;
using link.magic.unity.sdk.Relayer;

namespace link.magic.unity.sdk
{
    public sealed class Magic
    {
        // static instance
        public static Magic Instance;
        public readonly AuthModule Auth;

        public readonly RpcProvider Provider;
        public readonly UserModule User;

        //Constructor
        public Magic(string apikey, EthNetwork network = EthNetwork.Mainnet, string locale = "en-US")
        {
            var urlBuilder = new UrlBuilder(apikey, network, locale);
            UrlBuilder.Instance = urlBuilder;

            Provider = new RpcProvider(urlBuilder);
            User = new UserModule(Provider);
            Auth = new AuthModule(Provider);
        }

        public Magic(string apikey, CustomNodeConfiguration config, string locale = "en-US")
        {
            var urlBuilder = new UrlBuilder(apikey, config, locale);
            UrlBuilder.Instance = urlBuilder;

            Provider = new RpcProvider(urlBuilder);
            User = new UserModule(Provider);
            Auth = new AuthModule(Provider);
        }
    }

    public enum EthNetwork
    {
        Mainnet,
        Goerli,
    }
}
