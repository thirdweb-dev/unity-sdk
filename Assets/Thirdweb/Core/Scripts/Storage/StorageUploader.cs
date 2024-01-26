using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Thirdweb.Redcode.Awaiting;

namespace Thirdweb
{
    [System.Serializable]
    public struct IPFSUploadResult
    {
        public string IpfsHash;
        public string PinSize;
        public string Timestamp;

        public override readonly string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class StorageUploader : IStorageUploader
    {
        private readonly string PIN_URI = "https://storage.thirdweb.com/ipfs/upload";

        public async Task<IPFSUploadResult> UploadText(string text)
        {
            var path = Application.temporaryCachePath + "/uploadedText.txt";
            await System.IO.File.WriteAllTextAsync(path, text);
            return await UploadFromPath(path);
        }

        public async Task<IPFSUploadResult> UploadFromPath(string path)
        {
            if (string.IsNullOrEmpty(ThirdwebManager.Instance.SDK.storage.ClientId))
                throw new UnityException("You cannot use default Upload features without setting a Client ID in the ThirdwebManager.");

            // Get data
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            var form = new WWWForm();
            form.AddBinaryData("file", bytes);

            // Pin
            string result = "";
            using (UnityWebRequest pinReq = UnityWebRequest.Post(PIN_URI, form))
            {
                pinReq.SetRequestHeader("x-sdk-name", "UnitySDK");
                pinReq.SetRequestHeader("x-sdk-os", Utils.GetRuntimePlatform());
                pinReq.SetRequestHeader("x-sdk-platform", "unity");
                pinReq.SetRequestHeader("x-sdk-version", ThirdwebSDK.version);
                pinReq.SetRequestHeader("x-client-id", ThirdwebManager.Instance.SDK.storage.ClientId);
                if (!Utils.IsWebGLBuild())
                    pinReq.SetRequestHeader("x-bundle-id", ThirdwebManager.Instance.SDK.session.Options.bundleId);

                await pinReq.SendWebRequest();

                if (pinReq.result != UnityWebRequest.Result.Success)
                    throw new UnityException($"Pin Request Failed! {pinReq.error}");

                result = pinReq.downloadHandler.text;
            }

            return JsonConvert.DeserializeObject<IPFSUploadResult>(result);
        }
    }
}
