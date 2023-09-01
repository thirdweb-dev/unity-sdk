using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MetaMask.Contracts;
using TMPro;
using UnityEngine;
using evm.net;
using MetaMask.Unity.Utils;
using BindableMonoBehavior = GalaxySdk.Utils.BindableMonoBehavior;

namespace MetaMask.Unity.Samples
{
    public class TokenPanel : BindableMonoBehavior
    {
        [Serializable]
        public class TokenAddresses
        {
            public string ChainId;
            public List<string> tokenAddresses = new List<string>();
        }
        
        [GalaxySdk.Utils.Inject]
        private MetaMaskUnity _metaMask;
        private List<ERC20> contracts = new List<ERC20>();
        private bool isRunning;
        private CancellationToken _token;
        
        public List<TokenAddresses> tokenAddresses = new List<TokenAddresses>();
        public TextMeshProUGUI balanceText;

        void Start()
        {
            if (_metaMask.Wallet.IsConnected)
            {
                SetupWallet();
            }
            else
            {
                _metaMask.Wallet.Events.WalletConnected += (_, _) => SetupWallet();
            }
        }

        private void SetupWallet()
        {
            SetupContracts();

            _metaMask.Wallet.Events.ChainIdChanged += (_, _) => SetupContracts();
        }

        private void SetupContracts()
        {
            var addresses = tokenAddresses.FirstOrDefault(ta => ta.ChainId == _metaMask.Wallet.SelectedChainId)?.tokenAddresses;

            if (addresses == null)
                return;
            
            var provider = _metaMask.Wallet;
            contracts = addresses.Select(address => Contract.Attach<ERC20>(provider, address)).ToList();

            StartCoroutine(FetchBalanceLoop());
        }

        private IEnumerator FetchBalanceLoop()
        {
            if (isRunning)
                yield return null;

            try
            {
                _token = new CancellationToken();
                isRunning = true;
                while (isActiveAndEnabled)
                {
                    // Wait until we have an address
                    if (string.IsNullOrEmpty(_metaMask.Wallet.SelectedAddress))
                    {
                        yield return new WaitForSeconds(1);
                        continue;
                    }

                    _token.ThrowIfCancellationRequested();
                    string balance;
                    var balanceTask = FetchBalances();
                    yield return new WaitForTask<string>(balanceTask);
                    
                    balanceText.text = balanceTask.Result;

                    yield return new WaitForSeconds(3);
                }
            }
            finally
            {
                isRunning = false;
            }
        }

        private async Task<string> FetchBalances()
        {
            string text = "";
            var owner = _metaMask.Wallet.SelectedAddress;

            foreach (var contract in contracts)
            {
                var decimals = await contract.Decimals();
                var balance = await contract.BalanceOf(_metaMask.Wallet.SelectedAddress);
                var tokenName = await contract.Symbol();

                text += $"{tokenName}: {Units.Wei.WithValue(balance).To(Units.WithDecimals(decimals))}\n";
            }

            return text;
        }
    }
}