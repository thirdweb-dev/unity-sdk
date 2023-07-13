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
        private readonly string PIN_URI = "https://uploadLink.thirdweb.com/upload";

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

            // Pin
            string result = "";
            using (UnityWebRequest pinReq = UnityWebRequest.Post(PIN_URI, form))
            {
                pinReq.SetRequestHeader("x-client-id", ThirdwebManager.Instance.SDK.session.Options.clientId);
                if (!Utils.IsWebGLBuild())
                    pinReq.SetRequestHeader("x-bundle-id", Utils.GetBundleId());

                await pinReq.SendWebRequest();

                if (pinReq.result != UnityWebRequest.Result.Success)
                    throw new UnityException($"Pin Request Failed! Result {pinReq.downloadHandler.text}");

                result = pinReq.downloadHandler.text;
            }

            return JsonConvert.DeserializeObject<IPFSUploadResult>(result);
        }
    }
}
