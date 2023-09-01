using System.Collections;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using evm.net;
using MetaMask.Unity.Contracts;
using MetaMask.Unity.Utils;
using UnityEngine;
using UnityEngine.UI;
using BindableMonoBehavior = GalaxySdk.Utils.BindableMonoBehavior;

namespace MetaMask.Unity.Samples
{
    [RequireComponent(typeof(Text))]
    public class TokenBalanceText : BindableMonoBehavior
    {
        [GalaxySdk.Utils.BindComponent]
        private Text _text;

        [GalaxySdk.Utils.Inject]
        private MetaMaskUnity _metaMask;
        private bool isRunning;
        private CancellationToken _token;

        public ScriptableERC20 erc20;
        public bool ShowNativeBalance;
        // TODO Grab automatically somehow
        public string NativeTokenSymbol;
        
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
                    var balanceTask = FetchBalance();
                    yield return new WaitForTask<string>(balanceTask);
                    
                    _text.text = balanceTask.Result;

                    yield return new WaitForSeconds(3);
                }
            }
            finally
            {
                isRunning = false;
            }
        }

        private async Task<string> FetchBalance()
        {
            var owner = _metaMask.Wallet.SelectedAddress;
            if (ShowNativeBalance)
            {
                object[] balanceOfArgs =
                {
                    owner,
                    "latest"
                };
                var result = await _metaMask.Wallet.Request<BigInteger>("eth_getBalance", balanceOfArgs);

                return $"{NativeTokenSymbol}: {Units.Wei.WithValue(result).To(Units.Ether)}";
            }

            var decimals = await erc20.Decimals();
            var balance = await erc20.BalanceOf(owner);
            var tokenName = await erc20.Symbol();

            return $"{tokenName}: {Units.Wei.WithValue(balance).To(Units.WithDecimals(decimals))}";
        }
    }
}