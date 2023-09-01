using System;
using UnityEngine;
using UnityEngine.UI;

namespace MetaMask.Samples.Main.Scripts.Utils.Images
{
    public static class ImageHelper
    {
        public class GenericAttachment<T>
        {
            internal Func<T> grabHolder;
            internal Action<T, Texture2D> holderFunc;
            
            public void ShowUrl(string url)
            {
                if (url.StartsWith("ipfs://"))
                    url = url.Replace("ipfs://", "https://ipfs.io/ipfs/");
                
                ImageDownloadManager.Instance.EnqueueRequest(url, tex =>
                {
                    var holder = grabHolder();
                    holderFunc(holder, tex);
                });
            }
        }

        public static GenericAttachment<Image> With(Func<Image> func)
        {
            return new GenericAttachment<Image>()
            {
                grabHolder = func,
                holderFunc = ((image, tex) => image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero))
            };
        }
    }
}