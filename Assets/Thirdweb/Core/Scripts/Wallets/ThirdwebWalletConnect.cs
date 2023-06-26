using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Network.Models;

namespace Thirdweb.Wallets
{
    public class ThirdwebWalletConnect : IThirdwebWallet
    {
        private Web3 _web3;
        private WalletProvider _provider;
        private WalletProvider _signerProvider;

        private WalletConnectSignClient _dappClient;
        private string _walletConnectProjectId;

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

            var address = await WalletConnectUI.Instance.Connect(_walletConnectProjectId, walletConnection.chainId);

            // _web3 = new Web3(new WalletConnectClient(WalletConnect.Instance.Session));
            return address;
        }

        public async Task Disconnect()
        {
            await _dappClient.Disconnect(
                "User disconnected",
                new Error()
                {
                    Code = 0,
                    Message = "User disconnected",
                    Data = null
                }
            );
            _web3 = null;
        }

        public Account GetLocalAccount()
        {
            return null;
        }

        public async Task<string> GetAddress()
        {
            var ethAccs = await _dappClient.Request<object[], string[]>("eth_accounts", new object[] { });
            var addy = ethAccs[0];
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
