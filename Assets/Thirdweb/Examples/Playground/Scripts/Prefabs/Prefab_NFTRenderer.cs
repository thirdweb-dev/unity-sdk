using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Thirdweb.Unity.Examples
{
    public class Prefab_NFTRenderer : MonoBehaviour
    {
        [Header("QUERY SETTINGS")]
        public string ChainId = "421614";
        public string NFTContractAddress = "0x94894F65d93eb124839C667Fc04F97723e5C4544";
        public NFTType NFTType = NFTType.ERC1155;

        [Header("UI ELEMENTS (DO NOT EDIT)")]
        public Transform contentParent;
        public Prefab_NFT nftPrefab;
        public GameObject loadingPanel;

        private void Start()
        {
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            LoadNFTs();
        }

        public async void LoadNFTs()
        {
            loadingPanel.SetActive(true);

            List<NFT> nftsToLoad = new List<NFT>();

            try
            {
                if (ThirdwebManager.Instance.Client == null)
                {
                    ThirdwebDebug.LogError("ThirdwebManager.Instance.Client is null. Make sure you have a ThirdwebManager in your scene.");
                    return;
                }

                var contract = await ThirdwebContract.Create(ThirdwebManager.Instance.Client, NFTContractAddress, BigInteger.Parse(ChainId));

                if (NFTType == NFTType.ERC721)
                {
                    var nfts = await contract.ERC721_GetAllNFTs();
                    foreach (var nft in nfts)
                    {
                        nftsToLoad.Add(nft);
                    }
                }
                else if (NFTType == NFTType.ERC1155)
                {
                    var nfts = await contract.ERC1155_GetAllNFTs();
                    foreach (var nft in nfts)
                    {
                        nftsToLoad.Add(nft);
                    }
                }
            }
            catch (Exception e)
            {
                ThirdwebDebug.Log($"Error Loading NFTs: {e.Message}");
            }

            // Load all NFTs into the scene

            foreach (NFT nft in nftsToLoad)
            {
                if (!Application.isPlaying)
                {
                    return;
                }

                Prefab_NFT nftPrefabScript = Instantiate(nftPrefab, contentParent);
                nftPrefabScript.LoadNFT(nft);
            }

            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }
    }
}
