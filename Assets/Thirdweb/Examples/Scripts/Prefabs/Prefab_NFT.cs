using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Thirdweb.Unity.Examples
{
    public enum NFTType
    {
        ERC721,
        ERC1155
    }

    public class Prefab_NFT : MonoBehaviour
    {
        [field: SerializeField, Header("UI Elements")]
        private Image NFTImage { get; set; }

        [field: SerializeField]
        private TMP_Text NFTName { get; set; }

        [field: SerializeField]
        private Button NFTButton { get; set; }

        public async void LoadNFT(NFT nft)
        {
            NFTButton.onClick.RemoveAllListeners();

            var imageBytes = await nft.GetNFTImageBytes(ThirdwebManager.Instance.Client);

            var texture = new Texture2D(256, 256);
            texture.LoadImage(imageBytes);
            NFTImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new UnityEngine.Vector2(0.5f, 0.5f));

            NFTName.text = nft.Metadata.Name;

            NFTButton.onClick.AddListener(() => OnNFTButtonClicked(nft.Metadata));
        }

        private void OnNFTButtonClicked(NFTMetadata metadata)
        {
            Debugger.Instance.Log(metadata.Name, JsonConvert.SerializeObject(metadata, Formatting.Indented));
        }
    }
}
