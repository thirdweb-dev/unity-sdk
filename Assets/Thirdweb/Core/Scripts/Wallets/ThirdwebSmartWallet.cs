using System.Threading.Tasks;
using Nethereum.Web3;
using Thirdweb.AccountAbstraction;
using Nethereum.Web3.Accounts;

namespace Thirdweb.Wallets
{
    public class ThirdwebSmartWallet : IThirdwebWallet
    {
        private Web3 _web3;
        private WalletProvider _provider;
        private WalletProvider _signerProvider;
        private IThirdwebWallet _personalWallet;
        private SmartWallet _smartWallet;
        private ThirdwebSDK.SmartWalletConfig _config;

        public ThirdwebSmartWallet(IThirdwebWallet personalWallet, ThirdwebSDK.SmartWalletConfig config)
        {
            _web3 = null;
            _provider = WalletProvider.SmartWallet;
            _signerProvider = personalWallet.GetProvider();
            _personalWallet = personalWallet;
            _config = config;
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            _smartWallet = new SmartWallet(await _personalWallet.GetWeb3(), _config);
            await _smartWallet.Initialize();
            _web3 = _smartWallet.CreateWeb3();
            return await GetAddress();
        }

        public Task Disconnect()
        {
            _web3 = null;
            return Task.CompletedTask;
        }

        public Account GetLocalAccount()
        {
            return _personalWallet.GetLocalAccount();
        }

        public Task<string> GetAddress()
        {
            var addy = _smartWallet.Accounts[0];
            if (addy != null)
                addy = addy.ToChecksumAddress();
            return Task.FromResult(addy);
        }

        public async Task<string> GetSignerAddress()
        {
            var addy = await _smartWallet.GetPersonalAddress();
            if (addy != null)
                addy = addy.ToChecksumAddress();
            return addy;
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

        public async Task<Web3> GetSignerWeb3()
        {
            return await _personalWallet.GetWeb3();
        }

        public Task<bool> IsConnected()
        {
            return Task.FromResult(_web3 != null);
        }
    }
}
