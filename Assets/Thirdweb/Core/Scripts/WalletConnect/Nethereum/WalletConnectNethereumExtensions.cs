using Nethereum.Web3;
using Thirdweb.WalletConnect;

namespace Thirdweb.WalletConnect
{
    public static class WalletConnectNEthereumExtensions
    {
        public static Web3 CreateWeb3(this WalletConnect walletConnect)
        {
            var client = new WalletConnectClient(walletConnect);
            var account = new WalletConnectAccount(walletConnect, client);
            return new Web3(account, client);
        }
    }
}
