using System.Numerics;
using System.Threading.Tasks;
using MetaMask.NEthereum;
using MetaMask.Unity;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using Thirdweb.Redcode.Awaiting;

namespace Thirdweb.Wallets
{
    public class ThirdwebMetamask : IThirdwebWallet
    {
        private Web3 _web3;
        private readonly WalletProvider _provider;
        private readonly WalletProvider _signerProvider;

        public ThirdwebMetamask()
        {
            _web3 = null;
            _provider = WalletProvider.Metamask;
            _signerProvider = WalletProvider.Metamask;
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            if (MetamaskUI.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.MetamaskPrefab);
                await new WaitForSeconds(1f);
            }
            await MetamaskUI.Instance.Connect();
            _web3 = MetaMaskUnity.Instance.CreateWeb3();
            return await GetAddress();
        }

        public Task Disconnect()
        {
            MetaMaskUnity.Instance.Disconnect(true);
            _web3 = null;
            return Task.CompletedTask;
        }

        public Account GetLocalAccount()
        {
            return null;
        }

        public Task<string> GetAddress()
        {
            var addy = MetaMaskUnity.Instance.Wallet.SelectedAddress;
            if (addy != null)
                addy = addy.ToChecksumAddress();
            return Task.FromResult(addy);
        }

        public Task<string> GetEmail()
        {
            return Task.FromResult("");
        }

        public async Task<string> GetSignerAddress()
        {
            return await GetAddress();
        }

        public WalletProvider GetProvider()
        {
            return _provider;
        }

        public WalletProvider GetSignerProvider()
        {
            return _signerProvider;
        }

        public Task<Web3> GetWeb3()
        {
            return Task.FromResult(_web3);
        }

        public Task<Web3> GetSignerWeb3()
        {
            return Task.FromResult(_web3);
        }

        public Task<bool> IsConnected()
        {
            return Task.FromResult(_web3 != null);
        }

        public Task<NetworkSwitchAction> PrepareForNetworkSwitch(BigInteger newChainId, string newRpc)
        {
            return Task.FromResult(NetworkSwitchAction.ContinueSwitch);
        }
    }
}
