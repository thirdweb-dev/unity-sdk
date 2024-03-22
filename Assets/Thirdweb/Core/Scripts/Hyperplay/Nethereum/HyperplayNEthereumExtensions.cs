using Nethereum.Web3;

namespace Thirdweb.Hyperplay
{
    public static class HyperplayNEthereumExtensions
    {
        public static Web3 CreateWeb3(this Hyperplay Hyperplay)
        {
            var client = new HyperplayClient(Hyperplay);
            var account = new HyperplayAccount(Hyperplay, client);
            return new Web3(account, client);
        }
    }
}
