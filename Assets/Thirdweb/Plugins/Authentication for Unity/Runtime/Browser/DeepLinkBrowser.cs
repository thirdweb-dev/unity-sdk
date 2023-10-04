using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cdm.Authentication.Browser
{
    /// <summary>
    /// OAuth 2.0 verification browser that waits for a call with
    /// the authorization verification code through a custom scheme (aka protocol).
    /// </summary>
    /// <see href="https://docs.unity3d.com/ScriptReference/Application-deepLinkActivated.html"/>
    public class DeepLinkBrowser : IBrowser
    {
        private TaskCompletionSource<BrowserResult> _taskCompletionSource;

        public async Task<BrowserResult> StartAsync(string loginUrl, string redirectUrl, CancellationToken cancellationToken = default)
        {
            _taskCompletionSource = new TaskCompletionSource<BrowserResult>();

            cancellationToken.Register(() =>
            {
                _taskCompletionSource?.TrySetCanceled();
            });

            Application.deepLinkActivated += OnDeepLinkActivated;

            try
            {
                Application.OpenURL(loginUrl);
                return await _taskCompletionSource.Task;
            }
            finally
            {
                Application.deepLinkActivated -= OnDeepLinkActivated;
            }
        }

        private void OnDeepLinkActivated(string url)
        {
            _taskCompletionSource.SetResult(
                new BrowserResult(BrowserStatus.Success, url));
        }
    }
}