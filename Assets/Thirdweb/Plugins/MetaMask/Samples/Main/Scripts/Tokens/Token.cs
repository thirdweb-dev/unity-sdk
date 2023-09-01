using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using evm.net;
using MetaMask.Contracts;
using MetaMask.Samples.Main.Scripts.Utils.Images;
using MetaMask.Unity.Contracts;
using MetaMask.Unity.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BindableMonoBehavior = GalaxySdk.Utils.BindableMonoBehavior;

namespace MetaMask.Unity.Samples
{
    public class Token : BindableMonoBehavior
    {
        [GalaxySdk.Utils.Inject]
        private MetaMaskUnity _metaMask;

        [GalaxySdk.Utils.Inject]
        private TokenList _tokenList;
        
        public TextMeshProUGUI tokenLabel;
        public Image tokenImage;
        public Button transferButton;
        public Button revokeApprovalButton;
        private bool isRunning;
        private CancellationToken _token;

        public TokenList.TokenMetadata metadata;
        public ScriptableERC20 tokenContract;

        private void Start()
        {
            if (_metaMask.Wallet.IsConnected)
            {
                SetupWallet();
            }
            else
            {
                _metaMask.Wallet.Events.WalletConnected += (_, _) => SetupWallet();
            }

            tokenLabel.text = "Loading...";
            if (!string.IsNullOrWhiteSpace(metadata.IconUrl))
                ImageHelper.With(() => tokenImage).ShowUrl(metadata.IconUrl);
            else
                tokenImage.enabled = false;
            
            transferButton.onClick.AddListener(ShowTransferDialog);
            revokeApprovalButton.onClick.AddListener(ShowRevokeApprovalDialog);
        }

        private void ShowRevokeApprovalDialog()
        {
            throw new System.NotImplementedException();
        }

        private async void ShowTransferDialog()
        {
            this._tokenList.ShowTransferDialog(metadata);
        }

        private void SetupWallet()
        {
            SetupContracts();

            _metaMask.Wallet.Events.ChainIdChanged += (_, _) => SetupContracts();
        }

        private void SetupContracts()
        {
            if (!tokenContract.HasAddressForSelectedChain)
            {
                Destroy(gameObject);
                return;
            }
            
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
                    
                    tokenLabel.text = balanceTask.Result;

                    yield return new WaitForSeconds(90);
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

            var balance = await tokenContract.BalanceOf(owner);

            var convertedBalance = Units.Wei.WithValue(balance).To(Units.WithDecimals(metadata.Decimals));

            return $"{metadata.Name} - {convertedBalance} {metadata.Symbol}";
        }
    }
}