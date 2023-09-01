using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MetaMask.Samples.Main.Scripts.Utils.Images
{
    public class ImageDownloadManager : Singleton<ImageDownloadManager>
    {
        public ImageDownloader[] Downloaders
        {
            get
            {
                return GetComponents<ImageDownloader>();
            }
        }

        public int TotalQueueSize
        {
            get
            {
                return Downloaders.Sum(d => d.QueueSize);
            }
        }

        public int TotalCapacity
        {
            get
            {
                return Downloaders.Sum(d => d.Capacity);
            }
        }
        
        public void EnqueueRequest(string request, UnityAction<Texture2D> action)
        {
            // Do we need more downloaders?
            if (TotalQueueSize >= TotalCapacity / 2)
            {
                Scale();
            }

            // Find downloader to pass request to
            var downloaders = GetComponents<ImageDownloader>().Where(d => !d.IsFull);

            foreach (var downloader in downloaders)
            {
                var result = downloader.EnqueueRequest(new ImageDownloadRequest(request, action));
                if (result)
                    return;
            }

            throw new IOException("No downloaders can fetch this request: " + request);
        }

        private void Scale()
        {
            gameObject.AddComponent<ImageDownloader>();
        }
    }
}