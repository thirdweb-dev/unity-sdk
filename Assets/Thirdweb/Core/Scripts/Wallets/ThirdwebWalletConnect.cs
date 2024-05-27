using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using System;
using System.Numerics;
using Thirdweb.Redcode.Awaiting;
using WalletConnectUnity.Core;
using System.Linq;
using System.Collections.Generic;
using WalletConnectSharp.Sign.Models;

namespace Thirdweb.Wallets
{
    public class ThirdwebWalletConnect : IThirdwebWallet
    {
        private Web3 _web3;
        private WalletProvider _provider;
        private WalletProvider _signerProvider;
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

            await WalletConnectUI.Instance.Connect(_walletConnectProjectId, walletConnection.chainId);
            _web3 = new Web3(rpc);
            return await GetAddress();
        }

        public async Task Disconnect(bool endSession = true)
        {
            if (endSession)
            {
                try
                {
                    await WalletConnect.Instance.DisconnectAsync();
                }
                catch (Exception e)
                {
                    ThirdwebDebug.LogWarning($"Error disconnecting WalletConnect: {e.Message}");
                }
            }

            _web3 = null;
        }

        public Account GetLocalAccount()
        {
            return null;
        }

        public Task<string> GetAddress()
        {
            var ethAccs = new string[] { WalletConnect.Instance.ActiveSession.CurrentAddress(WalletConnect.Instance.ActiveChainId).Address };
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
            return Task.FromResult(NetworkSwitchAction.ContinueSwitch);
        }
    }
}
