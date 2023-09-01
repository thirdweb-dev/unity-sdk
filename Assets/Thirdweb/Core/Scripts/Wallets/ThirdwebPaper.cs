using System.Threading.Tasks;
using Paper;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using System.Numerics;

namespace Thirdweb.Wallets
{
    public class ThirdwebPaper : IThirdwebWallet
    {
        private Web3 _web3;
        private readonly WalletProvider _provider;
        private readonly WalletProvider _signerProvider;
        private readonly PaperEmbeddedWalletSdk _paper;
        private Account _account;

        public ThirdwebPaper(string paperClientId)
        {
            _web3 = null;
            _provider = WalletProvider.Paper;
            _signerProvider = WalletProvider.LocalWallet;
            _paper = new PaperEmbeddedWalletSdk(paperClientId);
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            if (PaperUI.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.PaperPrefab);
            }

            _account = (await PaperUI.Instance.Connect(_paper, walletConnection.email)).Account;
            _web3 = new Web3(_account, rpc);

            return await GetAddress();
        }

        public async Task Disconnect()
        {
            await _paper.Logout();
            _account = null;
            _web3 = null;
        }

        public Account GetLocalAccount()
        {
            return _account;
        }

        public Task<string> GetAddress()
        {
            var addy = _account.Address;
            if (addy != null)
                addy = addy.ToChecksumAddress();
            return Task.FromResult(addy);
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
            _account = new Account(_account.PrivateKey, newChainId);
            _web3 = new Web3(_account, newRpc);
            return Task.FromResult(NetworkSwitchAction.Handled);
        }
    }
}
