using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading.Tasks;

namespace Thirdweb
{
    [System.Serializable]
    public struct IPFSUploadResult
    {
        public string IpfsHash;
        public string PinSize;
        public string Timestamp;
    }

    public class Storage
    {
        private readonly string BEARER_TOKEN_URI = "https://upload.nftlabs.co/grant";
        private readonly string PIN_URI = "https://api.pinata.cloud/pinning/pinFileToIPFS";

        private string ipfsGatewayUrl = "https://gateway.ipfscdn.io/ipfs/";

        public Storage(ThirdwebSDK.StorageOptions? storageOptions)
        {
            if (storageOptions != null)
            {
                this.ipfsGatewayUrl = string.IsNullOrEmpty(storageOptions.Value.ipfsGatewayUrl) ? "https://gateway.ipfscdn.io/ipfs/" : storageOptions.Value.ipfsGatewayUrl;
            }
        }

        public async Task<IPFSUploadResult> UploadText(string text)
        {
            var path = Application.temporaryCachePath + "/uploadedText.txt";
            await System.IO.File.WriteAllTextAsync(path, text);
            return await UploadFromPath(path);
        }

        public async Task<IPFSUploadResult> UploadFromPath(string path)
        {
            // Get data
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", bytes);

            // Get Token
            string token = "";
            using (UnityWebRequest bearerTokenReq = UnityWebRequest.Get(BEARER_TOKEN_URI))
            {
                bearerTokenReq.SetRequestHeader("X-APP-NAME", "Unity SDK");

                await bearerTokenReq.SendWebRequest();

                if (bearerTokenReq.result != UnityWebRequest.Result.Success)
                    throw new UnityException($"Token Request Failed! Result {bearerTokenReq.downloadHandler.text}");

                token = bearerTokenReq.downloadHandler.text;
            }

            // Pin
            string result = "";
            using (UnityWebRequest pinReq = UnityWebRequest.Post(PIN_URI, form))
            {
                pinReq.SetRequestHeader("Authorization", $"Bearer {token}");

                await pinReq.SendWebRequest();

                if (pinReq.result != UnityWebRequest.Result.Success)
                    throw new UnityException($"Pin Request Failed! Result {pinReq.downloadHandler.text}");

                result = pinReq.downloadHandler.text;
            }

            return JsonConvert.DeserializeObject<IPFSUploadResult>(result);
        }

        public async Task<T> DownloadText<T>(string textURI)
        {
            textURI = textURI.ReplaceIPFS(ipfsGatewayUrl);

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

        public async Task<Sprite> DownloadImage(string imageURI)
        {
            imageURI = imageURI.ReplaceIPFS(ipfsGatewayUrl);

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
