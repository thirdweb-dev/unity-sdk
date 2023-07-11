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

            using (UnityWebRequest req = UnityWebRequest.Get(textURI))
            {
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

            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(imageURI))
            {
                await req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Unable to fetch image uri {imageURI} data!");
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
