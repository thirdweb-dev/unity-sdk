using UnityEngine;
using System.Threading.Tasks;

namespace Thirdweb
{
    public class Storage
    {
        public string IPFSGateway { get; private set; }

        private IStorageUploader uploader;
        private IStorageDownloader downloader;

        private const string DEFAULT_IPFS_GATEWAY = "https://gateway.ipfscdn.io/ipfs/";

        public Storage(ThirdwebSDK.StorageOptions? storageOptions)
        {
            if (storageOptions == null)
            {
                this.IPFSGateway = DEFAULT_IPFS_GATEWAY;
                this.uploader = new StorageUploader();
                this.downloader = new StorageDownloader();
            }
            else
            {
                this.IPFSGateway = string.IsNullOrEmpty(storageOptions.Value.ipfsGatewayUrl) ? DEFAULT_IPFS_GATEWAY : storageOptions.Value.ipfsGatewayUrl;
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
