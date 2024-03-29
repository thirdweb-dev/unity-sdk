using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

namespace Thirdweb.Examples
{
    public class Prefab_NFT : MonoBehaviour
    {
        [Header("UI ELEMENTS")]
        public Image nftImage;
        public TMP_Text nftName;
        public Button nftButton;

        public async void LoadNFT(NFT nft)
        {
            nftName.text = nft.metadata.name;
            nftImage.sprite = await ThirdwebManager.Instance.SDK.Storage.DownloadImage(nft.metadata.image);
            nftButton.onClick.RemoveAllListeners();
            nftButton.onClick.AddListener(() => DoSomething(nft));
        }

        void DoSomething(NFT nft)
        {
            Debugger.Instance.Log(nft.metadata.name, nft.ToString());
        }
    }
}
