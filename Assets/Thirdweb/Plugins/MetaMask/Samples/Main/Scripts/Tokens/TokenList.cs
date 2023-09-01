using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using MetaMask.Samples.Main.Scripts.Utils;
using MetaMask.Unity.Contracts;
using MetaMask.Unity.Utils;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using BindableMonoBehavior = GalaxySdk.Utils.BindableMonoBehavior;

namespace MetaMask.Unity.Samples
{
    public class TokenList : BindableMonoBehavior
    {
        [GalaxySdk.Utils.Inject]
        private VisualController _controller;
        
        public List<ScriptableERC20> tokens = new List<ScriptableERC20>();
        
        public TextMeshProUGUI loadingText;
        public GameObject tokenListTransform;
        public GameObject tokenPrefab;
        public TransferDialog transferDialog;

        private bool isLoading;
        private Dictionary<string, TokenMetadata> addressToMetadata = new Dictionary<string, TokenMetadata>();

        [Serializable]
        public class TokenMetadata
        {
            [JsonProperty("address")]
            public string Address { get; set; }
            
            [JsonProperty("symbol")]
            public string Symbol { get; set; }
            
            [JsonProperty("decimals")]
            public BigInteger Decimals { get; set; }
            
            [JsonProperty("name")]
            public string Name { get; set; }
            
            [JsonProperty("iconUrl")]
            public string IconUrl { get; set; }
        }

        public class TokenMetadataList : List<TokenMetadata> { }


        private void OnEnable()
        {
            LoadData();

            _controller.enableResultModal = false;
        }

        private void OnDisable()
        {
            _controller.enableResultModal = true;
        }

        private IEnumerator LoadData(ScriptableERC20 token)
        {
            var task = SpawnToken(token);

            yield return new WaitForTask<TokenMetadata>(task);
                
            addressToMetadata.Add(token.Address, task.Result);
        }

        private void LoadData()
        {
            isLoading = true;
            addressToMetadata.Clear();
            
            foreach(Transform child in tokenListTransform.transform)
            {
                Destroy(child.gameObject);
            }
            
            StartCoroutine(LoadingText());

            foreach (var token in tokens)
            {
                StartCoroutine(LoadData(token));
            }
            
            isLoading = false;
        }

        private async Task<TokenMetadata> SpawnToken(ScriptableERC20 contract)
        {
            var tokenName = await contract.Name();
            var tokenSymbol = await contract.Symbol();
            var decimals = await contract.Decimals();

            var metadata = new TokenMetadata()
            {
                Name = tokenName,
                Address = contract.Address,
                Decimals = decimals,
                Symbol = tokenSymbol,
                IconUrl = null  // TODO Fill in somehow
            };
            
            var tokenObj = Instantiate(tokenPrefab, tokenListTransform.transform);
            var token = tokenObj.GetComponent<Token>();

            token.metadata = metadata;
            token.tokenContract = contract;

            return metadata;
        }

        private IEnumerator LoadingText()
        {
            loadingText.gameObject.SetActive(true);
            
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
            
            loadingText.gameObject.SetActive(false);
        }

        public void ShowTransferDialog(TokenMetadata tokenMetadata)
        {
            transferDialog.tokenMetadata = tokenMetadata;
            transferDialog.gameObject.SetActive(true);
        }

        public void HideTransferDialog()
        {
            transferDialog.gameObject.SetActive(false);
        }
    }
}