using System.Threading.Tasks;
using Paper;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;

namespace Thirdweb.Wallets
{
    public class ThirdwebPaper : IThirdwebWallet
    {
        private Web3 _web3;
        private WalletProvider _provider;
        private WalletProvider _signerProvider;
        private EmbeddedWallet _paper;
        private User _user;

        public ThirdwebPaper(string paperClientId)
        {
            _web3 = null;
            _provider = WalletProvider.Paper;
            _signerProvider = WalletProvider.LocalWallet;
            _paper = new EmbeddedWallet(paperClientId);
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            if (PaperUI.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.PaperPrefab);
            }

            _user = await PaperUI.Instance.Connect(_paper, walletConnection.email);
            _web3 = new Web3(_user.Account, ThirdwebManager.Instance.SDK.session.RPC);

            return await GetAddress();
        }

        public async Task Disconnect()
        {
            await _paper.SignOutAsync();
            _user = null;
            _web3 = null;
        }

        public Account GetLocalAccount()
        {
            return _user.Account;
        }

        public Task<string> GetAddress()
        {
            var addy = _user.Account.Address;
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
    }
}
