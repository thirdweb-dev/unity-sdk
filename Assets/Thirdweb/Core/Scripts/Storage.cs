using System;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Thirdweb
{
    public class Storage
    {
        private readonly string nftStorageApiUrl = "https://api.nft.storage/";
        private readonly HttpClient nftClient = new HttpClient();
        private readonly HttpClient ipfsClient = new HttpClient();

        private string ipfsGatewayUrl = "https://gateway.ipfscdn.io/ipfs/";
        private string apiToken = null;

        public Storage(ThirdwebSDK.StorageOptions? storageOptions)
        {
            if (storageOptions.HasValue)
            {
                this.ipfsGatewayUrl = string.IsNullOrEmpty(storageOptions.Value.ipfsGatewayUrl) ? "https://gateway.ipfscdn.io/ipfs/" : storageOptions.Value.ipfsGatewayUrl;
                this.apiToken = string.IsNullOrEmpty(storageOptions.Value.apiToken) ? null : storageOptions.Value.apiToken;
            }
        }

        void SetupClient()
        {
            if (string.IsNullOrEmpty(apiToken))
                throw new UnityException("You must provide an nft.storage API Token to call function!");

            if (nftClient.DefaultRequestHeaders.Contains("Accept"))
                nftClient.DefaultRequestHeaders.Remove("Accept");

            if (nftClient.DefaultRequestHeaders.Contains("Authorization"))
                nftClient.DefaultRequestHeaders.Remove("Authorization");

            nftClient.DefaultRequestHeaders.Add("Accept", "application/json");
            nftClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiToken);
        }

        public async Task<string> UploadText(string text)
        {
            var path = Application.temporaryCachePath + "/uploadedText.txt";
            await System.IO.File.WriteAllTextAsync(path, text);
            var uploadResponse = await UploadDataFromStringHttpClient(path);
            return $"ipfs://{uploadResponse.value.cid}";
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

        private async Task<string> SendHttpRequest(HttpMethod method, string uri, HttpClient requestClient = null)
        {
            try
            {
                SetupClient();
                if (requestClient == null)
                {
                    requestClient = nftClient;
                }
                HttpRequestMessage request = new HttpRequestMessage(method, uri);
                HttpResponseMessage response = await requestClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Debug.Log("HTTP Request Exception: " + e.Message);
                return null;
            }
        }

        private async Task<string> Upload(string uri, string pathFile)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(pathFile);
            try
            {
                SetupClient();
                using (var content = new ByteArrayContent(bytes))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("*/*");

                    nftClient.Timeout = new TimeSpan(1, 0, 0); // 1 hour should be enough probably

                    var response = await nftClient.PostAsync(uri, content);
                    response.EnsureSuccessStatusCode();
                    Stream responseStream = await response.Content.ReadAsStreamAsync();
                    StreamReader reader = new StreamReader(responseStream);
                    return reader.ReadToEnd();
                }
            }
            catch (HttpRequestException e)
            {
                Debug.Log("HTTP Request Exception: " + e.Message);
                return null;
            }
        }

        public async Task<NFTStorageListFilesResponse> ListFiles(string before = null, int limit = 10)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (before != null)
                query["before"] = before;
            query["limit"] = limit.ToString();
            string queryString = query.ToString();
            string requestUri = nftStorageApiUrl + "?" + queryString;
            string rawResponse = await SendHttpRequest(HttpMethod.Get, requestUri);
            NFTStorageListFilesResponse parsedResponse = JsonUtility.FromJson<NFTStorageListFilesResponse>(rawResponse);
            return parsedResponse;
        }

        public async Task<NFTStorageGetFileResponse> GetFile(string cid)
        {
            string requestUri = nftStorageApiUrl + "/" + cid;
            string rawResponse = await SendHttpRequest(HttpMethod.Get, requestUri);
            NFTStorageGetFileResponse parsedResponse = JsonUtility.FromJson<NFTStorageGetFileResponse>(rawResponse);
            return parsedResponse;
        }

        public async Task<string> GetFileData(string cid)
        {
            string requestUri = "https://" + cid + ".ipfs.dweb.link/";
            string response = await SendHttpRequest(HttpMethod.Get, requestUri, ipfsClient);
            return response;
        }

        public async Task<NFTStorageCheckResponse> CheckFile(string cid)
        {
            string requestUri = nftStorageApiUrl + "/check/" + cid;
            string rawResponse = await SendHttpRequest(HttpMethod.Get, requestUri);
            NFTStorageCheckResponse parsedResponse = JsonUtility.FromJson<NFTStorageCheckResponse>(rawResponse);
            return parsedResponse;
        }

        public async Task<NFTStorageDeleteResponse> DeleteFile(string cid)
        {
            string requestUri = nftStorageApiUrl + "/" + cid;
            string rawResponse = await SendHttpRequest(HttpMethod.Delete, requestUri);
            NFTStorageDeleteResponse parsedResponse = JsonUtility.FromJson<NFTStorageDeleteResponse>(rawResponse);
            return parsedResponse;
        }

        public async Task<NFTStorageUploadResponse> UploadDataFromStringHttpClient(string path)
        {
            string requestUri = nftStorageApiUrl + "/upload";
            string rawResponse = await Upload(requestUri, path);
            NFTStorageUploadResponse parsedResponse = JsonUtility.FromJson<NFTStorageUploadResponse>(rawResponse);
            return parsedResponse;
        }
    }

    [Serializable]
    public class NFTStorageError
    {
        public string name;
        public string message;

        public override string ToString()
        {
            return "NFTStorageError:\n" + $"name: {name}\n" + $"message: {message}\n";
        }
    }

    [Serializable]
    public class NFTStorageFiles
    {
        public string name;
        public string type;

        public override string ToString()
        {
            return "NFTStorageFiles:\n" + $"name: {name}\n" + $"type: {type}\n";
        }
    }

    [Serializable]
    public class NFTStorageDeal
    {
        public string batchRootCid;
        public string lastChange;
        public string miner;
        public string network;
        public string pieceCid;
        public string status;
        public string statusText;
        public int chainDealID;
        public string dealActivation;
        public string dealExpiration;

        public override string ToString()
        {
            return "NFTStorageDeal:\n"
                + $"batchRootCid: {batchRootCid}\n"
                + $"miner: {miner}\n"
                + $"network: {network}\n"
                + $"pieceCid: {pieceCid}\n"
                + $"status: {status}\n"
                + $"statusText: {statusText}\n"
                + $"chainDealID: {chainDealID}\n"
                + $"dealActivation: {dealActivation}\n"
                + $"dealExpiration: {dealExpiration}\n";
        }
    }

    [Serializable]
    public class NFTStoragePin
    {
        public string cid;
        public string name;
        public string status;
        public string created;
        public int size;

        // TODO: add metadata parsing ('meta' property)

        public override string ToString()
        {
            return "NFTStoragePin:\n" + $"cid: {cid}\n" + $"name: {name}\n" + $"status: {status}\n" + $"created: {created}\n" + $"size: {size}\n";
        }
    }

    [Serializable]
    public class NFTStorageNFTObject
    {
        public string cid;
        public int size;
        public string created;
        public string type;
        public string scope;
        public NFTStoragePin pin;
        public NFTStorageFiles[] files;
        public NFTStorageDeal[] deals;

        public override string ToString()
        {
            return "NFTStorageNFTObject:\n"
                + $"cid: {cid}\n"
                + $"size: {size}\n"
                + $"created: {created}\n"
                + $"type: {type}\n"
                + $"scope: {scope}\n"
                + $"pin: {pin.ToString()}\n"
                + $"files: {files.ToString()}\n"
                + $"deals: {deals.ToString()}\n";
        }
    }

    [Serializable]
    public class NFTStorageCheckValue
    {
        public string cid;
        public NFTStoragePin pin;
        public NFTStorageDeal[] deals;

        public override string ToString()
        {
            return "NFTStorageCheckValue:\n" + $"cid: {cid}\n" + $"pin: {pin.ToString()}\n" + $"deals: {deals.ToString()}\n";
        }
    }

    [Serializable]
    public class NFTStorageGetFileResponse
    {
        public bool ok;
        public NFTStorageNFTObject value;
        public NFTStorageError error;

        public override string ToString()
        {
            return "NFTStorageNFTObject:\n" + $"ok: {ok}\n" + $"value: {value.ToString()}\n" + $"error: {error.ToString()}\n";
        }
    }

    [Serializable]
    public class NFTStorageCheckResponse
    {
        public bool ok;
        public NFTStorageCheckValue value;
        public NFTStorageError error;

        public override string ToString()
        {
            return "NFTStorageNFTObject:\n" + $"ok: {ok}\n" + $"value: {value.ToString()}\n" + $"error: {error.ToString()}\n";
        }
    }

    [Serializable]
    public class NFTStorageListFilesResponse
    {
        public bool ok;
        public NFTStorageNFTObject[] value;
        public NFTStorageError error;

        public override string ToString()
        {
            return "NFTStorageNFTObject:\n" + $"ok: {ok}\n" + $"value: {value.ToString()}\n" + $"error: {error.ToString()}\n";
        }
    }

    [Serializable]
    public class NFTStorageUploadResponse
    {
        public bool ok;
        public NFTStorageNFTObject value;
        public NFTStorageError error;

        public override string ToString()
        {
            return "NFTStorageNFTObject:\n" + $"ok: {ok}\n" + $"value: {value.ToString()}\n" + $"error: {error.ToString()}\n";
        }
    }

    [Serializable]
    public class NFTStorageDeleteResponse
    {
        public bool ok;

        public override string ToString()
        {
            return "NFTStorageNFTObject:\n" + $"ok: {ok}\n";
        }
    }
}
