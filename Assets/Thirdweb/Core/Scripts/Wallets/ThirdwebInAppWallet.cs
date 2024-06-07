using System.Threading.Tasks;
using Thirdweb.EWS;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using System.Numerics;

namespace Thirdweb.Wallets
{
    public class ThirdwebInAppWallet : IThirdwebWallet
    {
        private Web3 _web3;
        private readonly WalletProvider _provider;
        private readonly WalletProvider _signerProvider;
        private readonly EmbeddedWallet _embeddedWallet;
        private Account _account;
        private string _email;
        private string _clientId;

        public ThirdwebInAppWallet(string clientId, string bundleId)
        {
            _web3 = null;
            _clientId = clientId;
            _provider = WalletProvider.InAppWallet;
            _signerProvider = WalletProvider.LocalWallet;
            _embeddedWallet = new EmbeddedWallet(clientId, bundleId, "unity", ThirdwebSDK.version);
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            await _embeddedWallet.VerifyThirdwebClientIdAsync("");

            if (InAppWalletUI.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.InAppWalletPrefab);
            }

            var user = await InAppWalletUI.Instance.Connect(_embeddedWallet, walletConnection.email, walletConnection.phoneNumber, walletConnection.authOptions, _clientId);
            _account = user.Account;
            _email = user.EmailAddress;
            _web3 = new Web3(_account, rpc);

            return await GetAddress();
        }

        public async Task Disconnect(bool endSession = true)
        {
            if (endSession)
                await _embeddedWallet.SignOutAsync();
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

        public Task<string> GetEmail()
        {
            return Task.FromResult(_email);
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
