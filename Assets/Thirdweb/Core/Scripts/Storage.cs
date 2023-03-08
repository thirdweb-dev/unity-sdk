using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading.Tasks;

namespace Thirdweb
{
    public static class Storage
    {
        public static async Task<T> DownloadText<T>(this string textURI)
        {
            textURI = textURI.ReplaceIPFS();

            using (UnityWebRequest req = UnityWebRequest.Get(textURI))
            {
                await req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Unable to fetch text uri {textURI} data!");
                    return default(T);
                }
                string json = req.downloadHandler.text;
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public static async Task<Sprite> DownloadImage(this string imageURI)
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
