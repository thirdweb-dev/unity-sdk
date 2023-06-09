using System.Threading.Tasks;
using link.magic.unity.sdk;
using link.magic.unity.sdk.Relayer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;

namespace Thirdweb.Wallets
{
    public class ThirdwebMagicLink : IThirdwebWallet
    {
        private Web3 _web3;
        private WalletProvider _provider;
        private WalletProvider _signerProvider;
        private string _magicLinkApiKey;

        public ThirdwebMagicLink(string magicLinkApiKey)
        {
            _web3 = null;
            _provider = WalletProvider.MagicLink;
            _signerProvider = WalletProvider.MagicLink;
            _magicLinkApiKey = magicLinkApiKey;
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            if (MagicUnity.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.MagicAuthPrefab);
                await new WaitForSeconds(0.5f);
                MagicUnity.Instance.Initialize(_magicLinkApiKey, new CustomNodeConfiguration(rpc, walletConnection.chainId));
            }

            await MagicUnity.Instance.EnableMagicAuth(walletConnection.email);
            _web3 = new Web3(Magic.Instance.Provider);

            return await GetAddress();
        }

        public async Task Disconnect()
        {
            await MagicUnity.Instance.DisableMagicAuth();
            _web3 = null;
        }

        public Account GetLocalAccount()
        {
            return null;
        }

        public async Task<string> GetAddress()
        {
            var addy = await MagicUnity.Instance.GetAddress();
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
    }
}
