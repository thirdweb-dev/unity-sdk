using UnityEngine;
using System.Threading.Tasks;

namespace Thirdweb
{
    public class Storage
    {
        public string IPFSGateway { get; private set; }
        public string ClientId { get; private set; }

        private readonly IStorageUploader uploader;
        private readonly IStorageDownloader downloader;

        private const string FALLBACK_IPFS_GATEWAY = "https://cloudflare-ipfs.com/ipfs/";

        public Storage(ThirdwebSDK.StorageOptions? storageOptions, string clientId = null)
        {
            this.ClientId = clientId;

            string thirdwebIpfsGateway = $"https://{ClientId}.ipfscdn.io/ipfs/";
            if (storageOptions == null)
            {
                this.IPFSGateway = ClientId != null ? thirdwebIpfsGateway : FALLBACK_IPFS_GATEWAY;
                this.uploader = new StorageUploader();
                this.downloader = new StorageDownloader();
            }
            else
            {
                this.IPFSGateway = string.IsNullOrEmpty(storageOptions?.ipfsGatewayUrl) ? (ClientId != null ? thirdwebIpfsGateway : FALLBACK_IPFS_GATEWAY) : storageOptions?.ipfsGatewayUrl;
                this.uploader = storageOptions.Value.uploaderOverride ?? new StorageUploader();
                this.downloader = storageOptions.Value.downloaderOverride ?? new StorageDownloader();
            }
        }

        public async Task<IPFSUploadResult> UploadText(string text)
        {
            return await uploader.UploadText(text);
        }

        public async Task<IPFSUploadResult> UploadFromPath(string path)
        {
            return await uploader.UploadFromPath(path);
        }

        public async Task<T> DownloadText<T>(string textURI)
        {
            return await downloader.DownloadText<T>(textURI);
        }

        public async Task<Sprite> DownloadImage(string imageURI)
        {
            return await downloader.DownloadImage(imageURI);
        }
    }
}
