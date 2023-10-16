#if UNITY_IOS

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb.Browser
{
    public class IOSBrowser : IThirdwebBrowser
    {
        private TaskCompletionSource<BrowserResult> _taskCompletionSource;

        private string _customScheme;

        public async Task<BrowserResult> Login(string loginUrl, string customScheme, CancellationToken cancellationToken = default)
        {
            _taskCompletionSource = new TaskCompletionSource<BrowserResult>();

            cancellationToken.Register(() =>
            {
                _taskCompletionSource?.TrySetCanceled();
            });

            _customScheme = customScheme;

            Application.deepLinkActivated += OnDeepLinkActivated;

            try
            {
                OpenURL(loginUrl);
                var completedTask = await Task.WhenAny(_taskCompletionSource.Task, Task.Delay(TimeSpan.FromSeconds(30)));
                if (completedTask == _taskCompletionSource.Task)
                {
                    return await _taskCompletionSource.Task;
                }
                else
                {
                    return new BrowserResult(BrowserStatus.Timeout, null, "The operation timed out.");
                }
            }
            finally
            {
                Application.deepLinkActivated -= OnDeepLinkActivated;
            }
        }

        [DllImport("__Internal")]
        private static extern void _OpenURL(string url);

        public void OpenURL(string url)
        {
            _OpenURL(url);
        }

        private void OnDeepLinkActivated(string url)
        {
            if (!url.StartsWith(_customScheme))
                return;

            _taskCompletionSource.SetResult(new BrowserResult(BrowserStatus.Success, url));
        }
    }
}

#endif
