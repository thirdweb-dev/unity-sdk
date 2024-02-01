using MetaMask.Unity;
using Nethereum.Web3;

namespace MetaMask.NEthereum
{
    public static class MetaMaskNEthereumExtensions
    {
        public static Web3 CreateWeb3(this MetaMaskUnity metaMaskUnity)
        {
            return metaMaskUnity.Wallet.CreateWeb3();
        }
        
        public static Web3 CreateWeb3(this MetaMaskWallet metaMaskWallet)
        {
            var client = new MetaMaskClient(metaMaskWallet);
            var account = new MetaMaskAccount(metaMaskWallet, client);
            return new Web3(account, client);
        }
    }
}