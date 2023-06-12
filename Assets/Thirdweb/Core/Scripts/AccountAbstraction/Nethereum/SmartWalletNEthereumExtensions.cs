using Nethereum.Web3;

namespace Thirdweb.AccountAbstraction
{
    public static class SmartWalletNEthereumExtensions
    {
        public static Web3 CreateWeb3(this SmartWallet smartWallet)
        {
            var client = new SmartWalletClient(smartWallet);
            var account = new SmartWalletAccount(smartWallet, client);
            return new Web3(account, client);
        }
    }
}
