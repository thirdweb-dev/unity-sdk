using System.Numerics;
using System.Threading.Tasks;
using link.magic.unity.sdk;
using link.magic.unity.sdk.Relayer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace Thirdweb.Wallets
{
    public class ThirdwebMagicLink : IThirdwebWallet
    {
        private Web3 _web3;
        private readonly WalletProvider _provider;
        private readonly WalletProvider _signerProvider;
        private readonly string _magicLinkApiKey;
        private Magic _magic;

        public ThirdwebMagicLink(string magicLinkApiKey)
        {
            _web3 = null;
            _provider = WalletProvider.MagicLink;
            _signerProvider = WalletProvider.MagicLink;
            _magicLinkApiKey = magicLinkApiKey;
            _magic = null;
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            _magic = new Magic(_magicLinkApiKey, new CustomNodeConfiguration(rpc, (int)walletConnection.chainId));

            await _magic.Auth.LoginWithEmailOtp(walletConnection.email);
            _web3 = new Web3(_magic.Provider);

            return await GetAddress();
        }

        public async Task Disconnect()
        {
            await _magic.User.Logout();
            _web3 = null;
        }

        public Account GetLocalAccount()
        {
            return null;
        }

        public async Task<string> GetAddress()
        {
            var metadata = await _magic.User.GetMetadata();
            var addy = metadata.publicAddress;
            if (addy != null)
                addy = addy.ToChecksumAddress();
            return addy;
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
