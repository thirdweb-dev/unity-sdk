using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Network.Models;
using Nethereum.JsonRpc.Client;
using System;
using Nethereum.JsonRpc.Client.RpcMessages;
using System.Linq;
using Newtonsoft.Json;

namespace Thirdweb.Wallets
{
    public class ThirdwebWalletConnect : IThirdwebWallet
    {
        public WalletConnectSignClient Client { get; private set; }

        private Web3 _web3;
        private WalletProvider _provider;
        private WalletProvider _signerProvider;
        private string _walletConnectProjectId;
        private string _address;

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
            (Client, _address, topic) = await WalletConnectUI.Instance.Connect(_walletConnectProjectId, walletConnection.chainId);

            Debug.Log($"Connected to {_address}");
            _web3 = new Web3();
            return await GetAddress();
        }

        public async Task Disconnect()
        {
            await Client.Disconnect(
                "User disconnected",
                new ErrorResponse()
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

        public Task<string> GetAddress()
        {
            var ethAccs = new string[] { _address };
            var addy = ethAccs[0];
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
