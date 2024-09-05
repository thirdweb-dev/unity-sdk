using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb.Unity
{
    public static class ThirdwebUnityExtensions
    {
        public static async Task<Sprite> GetNFTSprite(this NFT nft, ThirdwebClient client)
        {
            var bytes = await nft.GetNFTImageBytes(client);
            Texture2D texture = new(2, 2);

            bool isLoaded = texture.LoadImage(bytes);
            if (!isLoaded)
            {
                Debug.LogError("Failed to load image from bytes.");
                return null;
            }

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new UnityEngine.Vector2(0.5f, 0.5f));
        }

        public static async Task<SmartWallet> UpgradeToSmartWallet(this IThirdwebWallet personalWallet, BigInteger chainId, SmartWalletOptions smartWalletOptions)
        {
            return await ThirdwebManager.Instance.UpgradeToSmartWallet(personalWallet, chainId, smartWalletOptions);
        }
    }
}
