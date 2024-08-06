using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Networking;

namespace WalletConnectUnity.UI
{
    public interface IImageHandler<in TImage>
    {
        void SetImageSprite(TImage image, Sprite sprite);
        void ClearImageSprite(TImage image);
    }

    public static class RemoteSpriteFactory
    {
        private static readonly Dictionary<string, object> UriSpritesMap = new();
        private static readonly Dictionary<Type, object> ImageHandlers = new();

        public static void RegisterImageHandler<TImage>(IImageHandler<TImage> handler)
            where TImage : class
        {
            ImageHandlers[typeof(TImage)] = handler;
        }

        public static RemoteSprite<TImage> GetRemoteSprite<TImage>(string uri)
            where TImage : class
        {
            if (!ImageHandlers.TryGetValue(typeof(TImage), out var handlerObj) || handlerObj is not IImageHandler<TImage> handler)
                throw new InvalidOperationException($"No handler registered for type {typeof(TImage).Name}.");

            if (UriSpritesMap.TryGetValue(uri, out var spriteObj) && spriteObj is RemoteSprite<TImage> sprite)
            {
                return sprite;
            }
            else
            {
                var newSprite = new RemoteSprite<TImage>(uri, handler);
                UriSpritesMap[uri] = newSprite;
                return newSprite;
            }
        }
    }

    public class RemoteSprite<TImage>
        where TImage : class
    {
        private readonly string _uri;
        private bool _isLoading;
        private Sprite _sprite;
        private bool _isLoaded;
        private readonly WCLoadingAnimator _loadingAnimator;
        private readonly HashSet<TImage> _subscribedImages = new();
        private readonly IImageHandler<TImage> _imageHandler;

        internal RemoteSprite(string uri, IImageHandler<TImage> imageHandler)
        {
            _uri = uri;
            _imageHandler = imageHandler;
            _loadingAnimator = WCLoadingAnimator.Instance;
        }

        public void SubscribeImage(TImage image)
        {
            if (!_isLoaded && !_isLoading)
                UnityEventsDispatcher.Instance.StartCoroutine(LoadRemoteSprite());

            if (_isLoaded)
            {
                SetImage(image);
            }
            else
            {
                if (_loadingAnimator != null)
                    _loadingAnimator.Subscribe(image);
            }

            _subscribedImages.Add(image);
        }

        public void UnsubscribeImage(TImage image)
        {
            _imageHandler.ClearImageSprite(image);
            _subscribedImages.Remove(image);
        }

        private void SetImage(TImage image)
        {
            if (_loadingAnimator != null)
                _loadingAnimator.Unsubscribe(image);

            _imageHandler.SetImageSprite(image, _sprite);
        }

        private IEnumerator LoadRemoteSprite()
        {
            _isLoading = true;

            using (var uwr = UnityWebRequestTexture.GetTexture(_uri))
            {
                uwr.SetWalletConnectRequestHeaders().SetRequestHeader("accept", "image/jpeg,image/png");

                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Failed to load remote sprite from {_uri}: {uwr.error}");
                }
                else
                {
                    // While UnityWebRequest creates texture in the background (on other thread), some finishing work is done on main thread.
                    // Skipping a few frames here to let Unity finish its work to reduce CPU spikes.
                    for (var i = 0; i < 5; i++)
                        yield return null;

                    var texture = DownloadHandlerTexture.GetContent(uwr);
                    var rect = new Rect(0.0f, 0.0f, texture.width, texture.height);
                    _sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100.0f);
                    _isLoaded = true;

                    foreach (var image in _subscribedImages)
                        SetImage(image);
                }
            }

            _isLoading = false;
        }
    }

    public static class RemoteSpriteExtensions
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Initialize()
        {
            RemoteSpriteFactory.RegisterImageHandler(new ImageHandlerUgui());
            RemoteSpriteFactory.RegisterImageHandler(new ImageHandlerUtk());
        }
    }

    public class ImageHandlerUgui : IImageHandler<UnityEngine.UI.Image>
    {
        public void SetImageSprite(UnityEngine.UI.Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.color = Color.white;
        }

        public void ClearImageSprite(UnityEngine.UI.Image image)
        {
            image.sprite = null;
        }
    }

    public class ImageHandlerUtk : IImageHandler<UnityEngine.UIElements.Image>
    {
        public void SetImageSprite(UnityEngine.UIElements.Image image, Sprite sprite)
        {
            image.sprite = sprite;
        }

        public void ClearImageSprite(UnityEngine.UIElements.Image image)
        {
            if (image == null)
                return;

            image.sprite = null;
        }
    }
}
