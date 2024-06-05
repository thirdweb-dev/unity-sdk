using System.Threading.Tasks;
using Nethereum.Web3;
using Thirdweb.AccountAbstraction;
using Nethereum.Web3.Accounts;
using System.Numerics;

namespace Thirdweb.Wallets
{
    public class ThirdwebSmartWallet : IThirdwebWallet
    {
        private SmartWallet _smartWallet;
        public SmartWallet SmartWallet
        {
            get { return _smartWallet; }
        }

        private Web3 _web3;
        private readonly WalletProvider _provider;
        private readonly WalletProvider _signerProvider;
        private readonly IThirdwebWallet _personalWallet;
        private readonly ThirdwebSDK _sdk;

        public ThirdwebSmartWallet(IThirdwebWallet personalWallet, ThirdwebSDK sdk)
        {
            _web3 = null;
            _provider = WalletProvider.SmartWallet;
            _signerProvider = personalWallet.GetProvider();
            _personalWallet = personalWallet;
            _sdk = sdk;
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            _smartWallet = new SmartWallet(_personalWallet, _sdk);
            await _smartWallet.Initialize(walletConnection.smartWalletAccountOverride);
            _web3 = _smartWallet.CreateWeb3();
            return await GetAddress();
        }

        public async Task Disconnect(bool endSession = true)
        {
            _web3 = null;
            await _personalWallet.Disconnect(endSession);
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

        public async Task<string> GetEmail()
        {
            return await _personalWallet.GetEmail();
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

        public Task<NetworkSwitchAction> PrepareForNetworkSwitch(BigInteger newChainId, string newRpc)
        {
            return Task.FromResult(NetworkSwitchAction.Unsupported);
        }
    }
}
