using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Thirdweb
{
    [System.Serializable]
    public struct IPFSUploadResult
    {
        public string IpfsHash;
        public string PinSize;
        public string Timestamp;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class StorageUploader : IStorageUploader
    {
        private readonly string BEARER_TOKEN_URI = "https://upload.nftlabs.co/grant";
        private readonly string PIN_URI = "https://api.pinata.cloud/pinning/pinFileToIPFS";

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
                if (BEARER_TOKEN_URI.Contains("nftlabs.co"))
                    bearerTokenReq.SetRequestHeader("Authorization", $"Bearer {ThirdwebManager.Instance.SDK.session.Options.apiKey}");

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
    }
}
