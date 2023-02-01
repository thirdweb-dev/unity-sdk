using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Thirdweb;
using UnityEngine.Networking;

public class Prefab_LoadNFT : MonoBehaviour
{
    [Header("SETTINGS")]
    public string contractAddress = "0x2e01763fA0e15e07294D74B63cE4b526B321E389";
    public int tokenID = 0;
    public bool isERC1155 = false;

    [Header("UI ELEMENTS (DO NOT EDIT)")]
    public Image nftImage;
    public TMP_Text nftMetadata;

    private void Start()
    {
#if !UNITY_EDITOR
        OnLoadNFT();
#endif
    }

    public async void OnLoadNFT()
    {
        try
        {
            Contract contract = ThirdwebManager.Instance.SDK.GetContract(contractAddress);
            NFT nft = isERC1155 ? await contract.ERC1155.Get(tokenID.ToString()) : await contract.ERC721.Get(tokenID.ToString());
            nftMetadata.text = nft.ToString();
            StartCoroutine(DownloadNFT(nft));
            print($"Successfully retrieved NFT Metadata!");
        }
        catch (System.Exception e)
        {
            print($"Error Loading NFT: {e.Message}");
        }
    }

    IEnumerator DownloadNFT(NFT nft)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(nft.metadata.image))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                print($"Could not fetch id: {nft.metadata.id} - image: {nft.metadata.image}");
            }
            else
            {
                Texture2D itemTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                Sprite itemSprite = Sprite.Create(itemTexture, new Rect(0.0f, 0.0f, itemTexture.width, itemTexture.height), new UnityEngine.Vector2(0.5f, 0.5f), 100.0f);
                nftImage.sprite = itemSprite;
                print($"Successfully retrieved NFT Image!");
            }
        }
    }
}
