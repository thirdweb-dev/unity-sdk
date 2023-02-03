using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Thirdweb;
using UnityEngine.Networking;

public class Prefab_NFT : MonoBehaviour
{
    [Header("UI ELEMENTS")]
    public Image nftImage;
    public TMP_Text nftName;

    public void LoadNFT(NFT nft)
    {
        nftName.text = nft.metadata.name;
        StartCoroutine(LoadNFTRoutine(nft));
    }

    IEnumerator LoadNFTRoutine(NFT nft)
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
