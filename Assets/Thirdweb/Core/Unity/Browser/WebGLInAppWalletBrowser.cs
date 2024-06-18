#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb.Unity
{
    public class WebGLInAppWalletBrowser : MonoBehaviour, IThirdwebBrowser
    {
        private static WebGLInAppWalletBrowser _instance;
        private TaskCompletionSource<BrowserResult> _taskCompletionSource;
        private bool _isCallbackInvoked;

        [DllImport("__Internal")]
        private static extern void openPopup(
            string url,
            string redirectUrl,
            string unityObjectName,
            string unityCallbackMethod
        );

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public async Task<BrowserResult> Login(
            string loginUrl,
            string redirectUrl,
            Action<string> browserOpenAction,
            CancellationToken cancellationToken = default
        )
        {
            _taskCompletionSource = new TaskCompletionSource<BrowserResult>();

            cancellationToken.Register(() =>
            {
                _taskCompletionSource?.TrySetCanceled();
            });

            redirectUrl = AddForwardSlashIfNecessary(redirectUrl);
            string unityObjectName = gameObject.name;
            string unityCallbackMethod = "OnRedirect";

            openPopup(loginUrl, redirectUrl, unityObjectName, unityCallbackMethod);

            var completedTask = await Task.WhenAny(
                _taskCompletionSource.Task,
                Task.Delay(TimeSpan.FromSeconds(30), cancellationToken)
            );
            return completedTask == _taskCompletionSource.Task
                ? await _taskCompletionSource.Task
                : new BrowserResult(BrowserStatus.Timeout, null, "The operation timed out.");
        }

        public void OnRedirect(string url)
        {
            if (string.IsNullOrEmpty(url) || !url.StartsWith("http://localhost:8789/"))
            {
                return;
            }

            if (!_isCallbackInvoked)
            {
                _isCallbackInvoked = true;
                _taskCompletionSource.SetResult(new BrowserResult(BrowserStatus.Success, url));
            }
        }

        private string AddForwardSlashIfNecessary(string url)
        {
            if (!url.EndsWith("/"))
            {
                url += "/";
            }
            return url;
        }
    }
}
#endif
