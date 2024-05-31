using UnityEngine;
using System.Threading.Tasks;
using Thirdweb.Redcode.Awaiting;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;

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

    public class Storage
    {
        public string IPFSGateway { get; private set; }

        private readonly ThirdwebSDK _sdk;

        private const string FALLBACK_IPFS_GATEWAY = "https://cloudflare-ipfs.com/ipfs/";
        private readonly string PIN_URI = "https://storage.thirdweb.com/ipfs/upload";

        public Storage(ThirdwebSDK sdk)
        {
            this._sdk = sdk;

            string thirdwebIpfsGateway = $"https://{_sdk.Session.Options.clientId}.ipfscdn.io/ipfs/";
            if (sdk.Session.Options.storage == null)
            {
                this.IPFSGateway = _sdk.Session.Options.clientId != null ? thirdwebIpfsGateway : FALLBACK_IPFS_GATEWAY;
            }
            else
            {
                this.IPFSGateway = string.IsNullOrEmpty(sdk.Session.Options.storage?.ipfsGatewayUrl)
                    ? (_sdk.Session.Options.clientId != null ? thirdwebIpfsGateway : FALLBACK_IPFS_GATEWAY)
                    : sdk.Session.Options.storage?.ipfsGatewayUrl;
            }
        }

        public async Task<IPFSUploadResult> UploadText(string text)
        {
            var path = Application.temporaryCachePath + "/uploadedText.txt";
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
            if (Utils.IsWebGLBuild())
            {
                System.IO.File.WriteAllText(path, text); // WebGL doesn't support async file writing
                await new WaitForSeconds(3f);
            }
            else
            {
                await System.IO.File.WriteAllTextAsync(path, text);
            }
            return await UploadFromPath(path);
        }

        public async Task<IPFSUploadResult> UploadFromPath(string path)
        {
            if (string.IsNullOrEmpty(_sdk.Session.Options.clientId))
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
                pinReq.SetRequestHeader("x-client-id", _sdk.Session.Options.clientId);
                if (!Utils.IsWebGLBuild())
                    pinReq.SetRequestHeader("x-bundle-id", _sdk.Session.Options.bundleId);

                await pinReq.SendWebRequest();

                if (pinReq.result != UnityWebRequest.Result.Success)
                    throw new UnityException($"Pin Request Failed! {pinReq.error}");

                result = pinReq.downloadHandler.text;
            }

            return JsonConvert.DeserializeObject<IPFSUploadResult>(result);
        }

        public async Task<T> DownloadText<T>(string textURI)
        {
            if (string.IsNullOrEmpty(textURI))
            {
                ThirdwebDebug.LogWarning($"Unable to download text from empty uri!");
                return default;
            }

            textURI = textURI.ReplaceIPFS(IPFSGateway);
            bool isThirdwebRequest = new Uri(textURI).Host.EndsWith(".ipfscdn.io");
            if (isThirdwebRequest)
                textURI = textURI.AppendBundleIdQueryParam(_sdk.Session.Options.bundleId);

            using UnityWebRequest req = UnityWebRequest.Get(textURI);
            if (isThirdwebRequest)
            {
                req.SetRequestHeader("x-sdk-name", "UnitySDK");
                req.SetRequestHeader("x-sdk-os", Utils.GetRuntimePlatform());
                req.SetRequestHeader("x-sdk-platform", "unity");
                req.SetRequestHeader("x-sdk-version", ThirdwebSDK.version);
                req.SetRequestHeader("x-client-id", _sdk.Session.Options.clientId);
            }

            await req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                ThirdwebDebug.LogWarning($"Unable to fetch text uri {textURI} data! {req.error}");
                return default;
            }
            string json = req.downloadHandler.text;

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)json;
                }
                else
                {
                    ThirdwebDebug.LogWarning($"Unable to parse text uri {textURI} data to {typeof(T).Name}!");
                    return default;
                }
            }
        }

        public async Task<Sprite> DownloadImage(string imageURI)
        {
            if (string.IsNullOrEmpty(imageURI))
            {
                ThirdwebDebug.LogWarning($"Unable to download image from empty uri!");
                return null;
            }

            imageURI = imageURI.ReplaceIPFS(IPFSGateway);
            bool isThirdwebRequest = new Uri(imageURI).Host.EndsWith(".ipfscdn.io");
            if (isThirdwebRequest)
                imageURI = imageURI.AppendBundleIdQueryParam(_sdk.Session.Options.bundleId);

            using UnityWebRequest req = UnityWebRequestTexture.GetTexture(imageURI);
            if (isThirdwebRequest)
            {
                req.SetRequestHeader("x-sdk-name", "UnitySDK");
                req.SetRequestHeader("x-sdk-os", Utils.GetRuntimePlatform());
                req.SetRequestHeader("x-sdk-platform", "unity");
                req.SetRequestHeader("x-sdk-version", ThirdwebSDK.version);
                req.SetRequestHeader("x-client-id", _sdk.Session.Options.clientId);
            }

            await req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                ThirdwebDebug.LogWarning($"Unable to fetch image uri {imageURI} data! {req.error}");
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
