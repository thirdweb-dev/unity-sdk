using System;
using System.Collections;
using System.Numerics;
using evm.net;
using GalaxySdk.Utils;
using MetaMask.Contracts;
using MetaMask.Samples.Main.Scripts.Utils;
using Nethereum.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetaMask.Unity.Samples
{
    public class TransferDialog : BindableMonoBehavior
    {
        [Inject]
        private MetaMaskUnity _metaMask;

        [Inject]
        private TokenList _tokenList;
        
        public ERC20 contract;
        public TextMeshProUGUI title;
        public TokenList.TokenMetadata tokenMetadata;
        public Button transferButton;
        public TMP_InputField toAddressField;
        public TMP_InputField amountAddressField;

        private bool isLoading;

        private void Start()
        {
            title.text = $"Transfer {tokenMetadata.Name}";

            contract = Contract.Attach<ERC20>(MetaMaskUnity.Instance.Wallet, tokenMetadata.Address);
        }

        public async void Transfer()
        {
            try
            {
                var text = transferButton.GetComponentInChildren<TextMeshProUGUI>();

                isLoading = true;
                StartCoroutine(LoadingText(text));

                var decimals = await contract.Decimals();

                Units tokenUnits = Units.WithDecimals(decimals);

                var weiAmount = tokenUnits.WithValue(BigDecimal.Parse(amountAddressField.text)).ToBigInteger(Units.Wei);

                await contract.Transfer(toAddressField.text, weiAmount);

                _tokenList.HideTransferDialog();
            }
            finally
            {
                isLoading = false;
            }
        }
        
        private IEnumerator LoadingText(TextMeshProUGUI loadingText)
        {
            string ogText = "Loading";
            uint dots = 0;
            while (isLoading)
            {
                dots++;
                if (dots >= 5)
                {
                    dots = 0;
                }
                
                loadingText.text = ogText + (".".RepeatLinq(dots));

                yield return new WaitForSeconds(0.5f);
            }

            loadingText.text = "Transfer";
        }
    }
}