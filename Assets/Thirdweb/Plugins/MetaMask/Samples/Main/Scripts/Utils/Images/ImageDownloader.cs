using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MetaMask.Samples.Main.Scripts.Utils.Images
{
    public class ImageDownloader : MonoBehaviour
    {
        private Queue<ImageDownloadRequest> _requests = new Queue<ImageDownloadRequest>();
        private bool flushing;
        private bool isDownloading;
        private float lastRequest;

        public int Capacity = 5;
        public float downloaderTimeout = 10.0f;

        public int QueueSize
        {
            get
            {
                return _requests.Count;
            }
        }

        public bool IsFull
        {
            get
            {
                return QueueSize >= Capacity;
            }
        }

        public bool EnqueueRequest(ImageDownloadRequest request)
        {
            if (IsFull)
                return false;
            
            lastRequest = Time.time;
            _requests.Enqueue(request);
            return true;
        }

        private void FixedUpdate()
        {
            if (flushing) return;
            if (_requests.Count > 0) StartCoroutine(FlushQueue());
            if (isDownloading) return;
            
            if (Time.time - lastRequest >= downloaderTimeout)
                Destroy(this);
        }

        private IEnumerator FlushQueue()
        {
            flushing = true;
            while (_requests.Count > 0)
            {
                var request = _requests.Dequeue();

                yield return DoRequest(request);
            }

            flushing = false;
        }

        private IEnumerator DoRequest(ImageDownloadRequest request)
        {
            isDownloading = true;
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(request.URL))
            {
                yield return www.SendWebRequest();

                switch (www.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(www.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        lastRequest = Time.time;
                        if (request.OnImageDownloaded != null)
                            request.OnImageDownloaded(DownloadHandlerTexture.GetContent(www));
                        break;
                    case UnityWebRequest.Result.InProgress:
                    default:
                        isDownloading = false;
                        throw new ArgumentOutOfRangeException();
                }
            }

            isDownloading = false;
        }
    }
}