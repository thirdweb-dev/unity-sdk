using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using WalletConnectSharp.Sign;
using System;
using Thirdweb.WalletConnect;
using System.Numerics;
using Thirdweb.Redcode.Awaiting;

namespace Thirdweb.Wallets
{
    public class ThirdwebWalletConnect : IThirdwebWallet
    {
        private Web3 _web3;
        private WalletProvider _provider;
        private WalletProvider _signerProvider;
        private string _walletConnectProjectId;
        private string _address;

        private WalletConnect.WalletConnect _walletConnect;

        public ThirdwebWalletConnect(string walletConnectProjectId)
        {
            _web3 = null;
            _provider = WalletProvider.WalletConnect;
            _signerProvider = WalletProvider.WalletConnect;
            _walletConnectProjectId = walletConnectProjectId;
        }

        public async Task<string> Connect(WalletConnection walletConnection, string rpc)
        {
            if (WalletConnectUI.Instance == null)
            {
                GameObject.Instantiate(ThirdwebManager.Instance.WalletConnectPrefab);
                await new WaitForSeconds(0.5f);
            }

            string topic = null;
            string eipChainId = null;
            WalletConnectSignClient client = null;
            (client, _address, eipChainId) = await WalletConnectUI.Instance.Connect(_walletConnectProjectId, walletConnection.chainId);

            Debug.Log($"Connected to {_address} with topic {topic} and chainId {eipChainId}");

            var accounts = new string[] { await GetAddress() };
            _walletConnect = new WalletConnect.WalletConnect(accounts, eipChainId, client);
            _web3 = _walletConnect.CreateWeb3();
            return accounts[0];
        }

        public async Task Disconnect()
        {
            try
            {
                await _walletConnect.Disconnect();
            }
            catch (Exception e)
            {
                Debug.Log("Error disconnecting sign client: " + e.Message);
            }

            _web3 = null;
        }

        public Account GetLocalAccount()
        {
            return null;
        }

        public Task<string> GetAddress()
        {
            var ethAccs = new string[] { _address };
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

        public Task<NetworkSwitchAction> PrepareForNetworkSwitch(BigInteger newChainId, string newRpc)
        {
            return Task.FromResult(NetworkSwitchAction.Unsupported);
        }
    }
}
