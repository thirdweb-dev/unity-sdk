using System;
using System.Collections.Generic;
using UnityEngine;
using Thirdweb;

[Serializable]
public enum NFTType
{
    ERC721,
    ERC1155
}

[Serializable]
public struct NFTQuery
{
    public List<SingleQuery> loadOneNft;
    public List<MultiQuery> loadMultipleNfts;
    public List<OwnedQuery> loadOwnedNfts;
}

[Serializable]
public struct SingleQuery
{
    public string contractAddress;
    public string tokenID;
    public NFTType type;
}

[Serializable]
public struct MultiQuery
{
    public string contractAddress;
    public int startID;
    public int count;
    public NFTType type;
}

[Serializable]
public struct OwnedQuery
{
    public string contractAddress;
    public string owner;
    public NFTType type;
}

public class Prefab_NFTLoader : MonoBehaviour
{
    [Header("SETTINGS")]
    public NFTQuery query;

    [Header("UI ELEMENTS (DO NOT EDIT)")]
    public Transform contentParent;
    public Prefab_NFT nftPrefab;
    public GameObject loadingPanel;

    private void Start()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // FindObjectOfType<Prefab_ConnectWallet>()?.OnConnectedCallback.AddListener(() => LoadNFTs());
        // FindObjectOfType<Prefab_ConnectWallet>()?.OnConnectedCallback.AddListener(() => LoadNFTs());

        LoadNFTs();
    }

    public async void LoadNFTs()
    {
        loadingPanel.SetActive(true);
        List<NFT> nftsToLoad = new List<NFT>();

        try
        {
            // Get all the NFTs queried

            foreach (SingleQuery singleQuery in query.loadOneNft)
            {
                Contract tempContract = ThirdwebManager.Instance.SDK.GetContract(singleQuery.contractAddress);

                NFT tempNFT = singleQuery.type == NFTType.ERC1155 ? await tempContract.ERC1155.Get(singleQuery.tokenID) : await tempContract.ERC721.Get(singleQuery.tokenID);

                nftsToLoad.Add(tempNFT);
            }
        }
        catch (Exception e)
        {
            print($"Error Loading SingleQuery NFTs: {e.Message}");
        }

        try
        {
            foreach (MultiQuery multiQuery in query.loadMultipleNfts)
            {
                Contract tempContract = ThirdwebManager.Instance.SDK.GetContract(multiQuery.contractAddress);

                List<NFT> tempNFTList =
                    multiQuery.type == NFTType.ERC1155
                        ? await tempContract.ERC1155.GetAll(new QueryAllParams() { start = multiQuery.startID, count = multiQuery.count })
                        : await tempContract.ERC721.GetAll(new QueryAllParams() { start = multiQuery.startID, count = multiQuery.count });

                nftsToLoad.AddRange(tempNFTList);
            }
        }
        catch (Exception e)
        {
            print($"Error Loading MultiQuery NFTs: {e.Message}");
        }

        try
        {
            foreach (OwnedQuery ownedQuery in query.loadOwnedNfts)
            {
                Contract tempContract = ThirdwebManager.Instance.SDK.GetContract(ownedQuery.contractAddress);

                List<NFT> tempNFTList = ownedQuery.type == NFTType.ERC1155 ? await tempContract.ERC1155.GetOwned(ownedQuery.owner) : await tempContract.ERC721.GetOwned(ownedQuery.owner);

                nftsToLoad.AddRange(tempNFTList);
            }
        }
        catch (Exception e)
        {
            print($"Error Loading OwnedQuery NFTs: {e.Message}");
        }

        // Load all NFTs into the scene

        foreach (NFT nft in nftsToLoad)
        {
            Prefab_NFT nftPrefabScript = Instantiate(nftPrefab, contentParent);
            nftPrefabScript.LoadNFT(nft);
            // Potentially wait a little here if you are loading a lot without a private IPFS gateway
            // Could also put this foreach in a separate Coroutine to avoid async object spawning
        }

        loadingPanel.SetActive(false);
    }
}
