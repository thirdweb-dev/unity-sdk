using System;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using evm.net.Network;
using MetaMask.Contracts;
using MetaMask.Samples.Main.Scripts.Utils.Images;
using MetaMask.Unity.Utils;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetaMask.Unity.Samples
{
    public class NFTPanel : MonoBehaviour, INFTHolder
    {
        private ERC721 currentNFT;
        private BigInteger currentId;
        private bool needsUpdate;
        
        public ERC721 CurrentNFT
        {
            get
            {
                return currentNFT;
            }
            set
            {
                currentNFT = value;

                needsUpdate = true;
            }
        }

        public BigInteger TokenId
        {
            get
            {
                return currentId;
            }
            set
            {
                currentId = value;

                needsUpdate = true;
            }
        }

        public TextMeshProUGUI collectionNameText;
        public TextMeshProUGUI nameOrAddressText;
        public Image nftImage;

        private void Start()
        {
            if (needsUpdate)
                StartCoroutine(RunUpdateNft());
        }

        private void FixedUpdate()
        {
            if (needsUpdate)
                StartCoroutine(RunUpdateNft());
        }

        private IEnumerator RunUpdateNft()
        {
            needsUpdate = false;  
            yield return new WaitForTask<bool>(UpdateNft());
        }
        
        private async Task<bool> UpdateNft()
        {
            var uri = await currentNFT.TokenURI(currentId);

            var service = HttpServiceFactory.NewHttpService();

            var json = await service.Get(uri);

            var metadata = JsonConvert.DeserializeObject<Metadata>(json);

            var address = currentNFT.Address;
            collectionNameText.text =
                $"{address.Substring(0, 4)}...{address.Substring(address.Length - 5, 5)}";
            
            nameOrAddressText.text = metadata.Name ?? TokenId.ToString();
            
            ImageHelper.With(() => nftImage).ShowUrl(metadata.Image);

            return true;
        }
    }
}