using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Thirdweb
{
    public class StorageDownloader : IStorageDownloader
    {
        public async Task<T> DownloadText<T>(string textURI)
        {
            textURI = textURI.ReplaceIPFS();
            bool isThirdwebRequest = ThirdwebManager.Instance.SDK.storage.IPFSGateway.Contains(".ipfscdn.io");
            if (isThirdwebRequest)
                textURI = textURI.AppendBundleIdQueryParam();

            using (UnityWebRequest req = UnityWebRequest.Get(textURI))
            {
                if (isThirdwebRequest)
                    req.SetRequestHeader("x-client-id", ThirdwebManager.Instance.SDK.storage.ClientId);

                await req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Unable to fetch text uri {textURI} data! {req.error}");
                    return default(T);
                }
                string json = req.downloadHandler.text;
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public async Task<Sprite> DownloadImage(string imageURI)
        {
            imageURI = imageURI.ReplaceIPFS();
            bool isThirdwebRequest = ThirdwebManager.Instance.SDK.storage.IPFSGateway.Contains(".ipfscdn.io");
            if (isThirdwebRequest)
                imageURI = imageURI.AppendBundleIdQueryParam();

            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(imageURI))
            {
                if (isThirdwebRequest)
                    req.SetRequestHeader("x-client-id", ThirdwebManager.Instance.SDK.storage.ClientId);

                await req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Unable to fetch image uri {imageURI} data! {req.error}");
                    return null;
                }
                else
                {
                    Texture2D itemTexture = ((DownloadHandlerTexture)req.downloadHandler).texture;
                    Sprite itemSprite = Sprite.Create(itemTexture, new Rect(0.0f, 0.0f, itemTexture.width, itemTexture.height), new UnityEngine.Vector2(0.5f, 0.5f), 100.0f);
                    return itemSprite;
                }
            }
        }
    }
}
