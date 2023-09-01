using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using MetaMask.Contracts;
using MetaMask.Samples.Main.Scripts.Utils;
using MetaMask.Unity.Contracts;
using MetaMask.Unity.Utils;
using TMPro;
using UnityEngine;

namespace MetaMask.Unity.Samples
{
    public class NFTList : BindableMonoBehavior
    {
        [Inject]
        private VisualController _controller;
        
        [Inject]
        private MetaMaskUnity _metaMask;

        public List<ScriptableERC721> tokens = new List<ScriptableERC721>();
        public bool IsLoading { get; private set; }
        public GameObject nftPrefab;
        public GameObject tokenListTransform;
        public TextMeshProUGUI loadingText;
        
        private void OnEnable()
        {
            LoadData();

            _controller.enableResultModal = false;
        }

        private void OnDisable()
        {
            _controller.enableResultModal = true;
        }

        private IEnumerator LoadData(ScriptableERC721 token)
        {
            yield return new WaitForTask<bool>(LoadNFT(token));
        }

        private void LoadData()
        {
            IsLoading = true;
            
            foreach(Transform child in tokenListTransform.transform)
            {
                Destroy(child.gameObject);
            }
            
            StartCoroutine(LoadingText());

            foreach (var token in tokens)
            {
                StartCoroutine(LoadData(token));
            }

            IsLoading = false;
        }

        private async Task<bool> LoadNFT(ScriptableERC721 nft)
        {
            var account = _metaMask.Wallet.SelectedAddress;
            var balance = await nft.BalanceOf(account);

            for (int i = 0; i < balance; i++)
            {
                var tokenId = await nft.TokenOfOwnerByIndex(account, i);

                SpawnToken(nft, tokenId);
            }

            return true;
        }

        private void SpawnToken(ERC721PresetMinterPauserAutoId nft, BigInteger id)
        {
            var tokenObj = Instantiate(nftPrefab, tokenListTransform.transform);
            var token = tokenObj.GetComponent<NFTPanel>();
            if (token == null)
                token = tokenObj.AddComponent<NFTPanel>();

            token.CurrentNFT = nft;
            token.TokenId = id;
        }

        private IEnumerator LoadingText()
        {
            if (loadingText == null)
                yield break;
            
            loadingText.gameObject.SetActive(true);
            
            string ogText = "Loading";
            uint dots = 0;
            while (IsLoading)
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
    }
}