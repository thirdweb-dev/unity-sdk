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

    public async void LoadNFT(NFT nft)
    {
        nftName.text = nft.metadata.name;
        nftImage.sprite = await ThirdwebManager.Instance.SDK.storage.DownloadImage(nft.metadata.image);
    }
}
