using System.Numerics;
using System.Threading.Tasks;
using MetaMask;
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
        public class MetaMaskThirdwebConfig : MetaMaskConfig
        {
            public void SetDefaults(MetaMaskConfig defaults)
            {
                this.encrypt = defaults.Encrypt;
                this.log = defaults.Log;
                this.encryptionPassword = defaults.EncryptionPassword;
                this.userAgent = defaults.UserAgent;
                this.socketUrl = defaults.SocketUrl;
            }

            public void UpdateConfig(string appName, string appUrl)
            {
                this.appName = appName;
                this.appUrl = appUrl;
            }
        }

        private Web3 _web3;
        private readonly WalletProvider _provider;
        private readonly WalletProvider _signerProvider;
        private readonly string _appName;
        private readonly string _appUrl;

        public ThirdwebMetamask(string appName, string appUrl)
        {
            _web3 = null;
            _provider = WalletProvider.Metamask;
            _signerProvider = WalletProvider.Metamask;
            _appName = appName;
            _appUrl = appUrl;
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            if (MetamaskUI.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.MetamaskPrefab);
                await new WaitForSeconds(1f);
                SetupMetaMask();
            }
            await MetamaskUI.Instance.Connect();
            _web3 = MetaMaskUnity.Instance.CreateWeb3();
            return await GetAddress();
        }

        private void SetupMetaMask()
        {
            var config = ScriptableObject.CreateInstance<MetaMaskThirdwebConfig>();
            var defaults = MetaMaskConfig.DefaultInstance;

            config.SetDefaults(defaults);

            config.UpdateConfig(_appName, _appUrl);

            MetaMaskUnity.Instance.Initialize(config);
        }

        public Task Disconnect(bool endSession = true)
        {
            MetaMaskUnity.Instance.Disconnect(endSession);
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
