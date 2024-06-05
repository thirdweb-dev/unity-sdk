using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using System.Numerics;
using Thirdweb.Redcode.Awaiting;
using WalletConnectUnity.Core;

namespace Thirdweb.Wallets
{
    public class ThirdwebWalletConnect : IThirdwebWallet
    {
        private Web3 _web3;

        private readonly WalletProvider _provider;
        private readonly WalletProvider _signerProvider;
        private readonly string[] _supportedChains;
        private readonly string[] _includedWalletIds;

        public ThirdwebWalletConnect(string[] supportedChains, string[] includedWalletIds)
        {
            _web3 = null;
            _provider = WalletProvider.WalletConnect;
            _signerProvider = WalletProvider.WalletConnect;
            _supportedChains = supportedChains;
            _includedWalletIds = includedWalletIds;
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            if (WalletConnectUI.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.WalletConnectPrefab);
                await new WaitForSeconds(0.5f);
            }

            await WalletConnectUI.Instance.Connect(_supportedChains, _includedWalletIds);
            await WalletConnect.Instance.SignClient.AddressProvider.SetDefaultChainIdAsync($"eip155:{walletConnection.chainId}");
            _web3 = new Web3(rpc);
            return await GetAddress();
        }

        public async Task Disconnect(bool endSession = true)
        {
            await WalletConnect.Instance.DisconnectAsync();

            _web3 = null;
        }

        public Account GetLocalAccount()
        {
            return null;
        }

        public Task<string> GetAddress()
        {
            var ethAccs = new string[] { WalletConnect.Instance.SignClient.AddressProvider.CurrentAddress().Address };
            var addy = ethAccs[0];
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

        public async Task<NetworkSwitchAction> PrepareForNetworkSwitch(BigInteger newChainId, string newRpc)
        {
            await WalletConnect.Instance.SignClient.AddressProvider.SetDefaultChainIdAsync($"eip155:{newChainId}");
            return NetworkSwitchAction.Handled;
        }
    }
}
