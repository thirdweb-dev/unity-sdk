using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using WalletConnectSharp.NEthereum.Client;
using WalletConnectSharp.Unity;

namespace Thirdweb.Wallets
{
    public class ThirdwebWalletConnect : IThirdwebWallet
    {
        private Web3 _web3;
        private WalletProvider _provider;
        private WalletProvider _signerProvider;

        public ThirdwebWalletConnect()
        {
            _web3 = null;
            _provider = WalletProvider.WalletConnectV1;
            _signerProvider = WalletProvider.WalletConnectV1;
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            if (WalletConnect.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.WalletConnectPrefab);
                await new WaitForSeconds(0.5f);
                WalletConnect.Instance.Initialize();
            }

            await WalletConnect.Instance.EnableWalletConnect();
            _web3 = new Web3(new WalletConnectClient(WalletConnect.Instance.Session));
            return await GetAddress();
        }

        public async Task Disconnect()
        {
            await WalletConnect.Instance.DisableWalletConnect();
            _web3 = null;
        }

        public Account GetLocalAccount()
        {
            return null;
        }

        public Task<string> GetAddress()
        {
            var addy = WalletConnect.Instance?.Session?.Accounts[0];
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
