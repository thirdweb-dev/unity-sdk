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
    public List<NFTQuery_Single> singleQueries;
    public List<NFTQuery_Multi> multiQueries;
    public List<NFTQuery_Owned> ownedQueries;
}

[Serializable]
public struct NFTQuery_Single
{
    public string contractAddress;
    public string tokenID;
    public NFTType type;
}

[Serializable]
public struct NFTQuery_Multi
{
    public string contractAddress;
    public int startID;
    public int count;
    public NFTType type;
}

[Serializable]
public struct NFTQuery_Owned
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

    private void Start()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        LoadNFTs();
    }

    public async void LoadNFTs()
    {
        // Get all the NFTs queried

        List<NFT> nftsToLoad = new List<NFT>();

        foreach (NFTQuery_Single singleQuery in query.singleQueries)
        {
            Contract tempContract = ThirdwebManager.Instance.SDK.GetContract(singleQuery.contractAddress);

            NFT tempNFT = singleQuery.type == NFTType.ERC1155 ?
                await tempContract.ERC1155.Get(singleQuery.tokenID) :
                await tempContract.ERC721.Get(singleQuery.tokenID);

            nftsToLoad.Add(tempNFT);
        }

        foreach (NFTQuery_Multi multiQuery in query.multiQueries)
        {
            Contract tempContract = ThirdwebManager.Instance.SDK.GetContract(multiQuery.contractAddress);

            List<NFT> tempNFTList = multiQuery.type == NFTType.ERC1155 ?
                await tempContract.ERC1155.GetAll(new QueryAllParams() { start = multiQuery.startID, count = multiQuery.count }) :
                await tempContract.ERC721.GetAll(new QueryAllParams() { start = multiQuery.startID, count = multiQuery.count });

            nftsToLoad.AddRange(tempNFTList);
        }

        foreach (NFTQuery_Owned ownedQuery in query.ownedQueries)
        {
            Contract tempContract = ThirdwebManager.Instance.SDK.GetContract(ownedQuery.contractAddress);

            List<NFT> tempNFTList = ownedQuery.type == NFTType.ERC1155 ?
                await tempContract.ERC1155.GetOwned(ownedQuery.owner) :
                await tempContract.ERC721.GetOwned(ownedQuery.owner);

            nftsToLoad.AddRange(tempNFTList);
        }

        // Load all NFTs into the scene

        foreach (NFT nft in nftsToLoad)
        {
            Prefab_NFT nftPrefabScript = Instantiate(nftPrefab, contentParent);
            nftPrefabScript.nft = nft;
            nftPrefabScript.LoadNFT();
            // Potentially wait a little here if you are loading a lot without a private IPFS gateway
            // Could also put this foreach in a separate Coroutine to avoid async object spawning
        }
    }
}
