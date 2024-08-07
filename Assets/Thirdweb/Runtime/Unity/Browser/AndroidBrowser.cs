#if UNITY_ANDROID && !UNITY_EDITOR

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb.Unity
{
    public class AndroidBrowser : IThirdwebBrowser
    {
        private TaskCompletionSource<BrowserResult> _taskCompletionSource;

        private string _customScheme;

        public async Task<BrowserResult> Login(ThirdwebClient client, string loginUrl, string customScheme, Action<string> browserOpenAction, CancellationToken cancellationToken = default)
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
                var completedTask = await Task.WhenAny(_taskCompletionSource.Task, Task.Delay(TimeSpan.FromSeconds(120)));
                if (completedTask == _taskCompletionSource.Task)
                {
                    return await _taskCompletionSource.Task;
                }
                else
                {
                    return new BrowserResult(BrowserStatus.Timeout, null, "The operation timed out.");
                }
            }
            catch (TaskCanceledException)
            {
                return new BrowserResult(BrowserStatus.UserCanceled, null, "The operation was cancelled.");
            }
            catch (Exception ex)
            {
                return new BrowserResult(BrowserStatus.UnknownError, null, $"An error occurred: {ex.Message}");
            }
            finally
            {
                Application.deepLinkActivated -= OnDeepLinkActivated;
            }
        }

        private void OpenURL(string url)
        {
            using var thirdwebPluginClass = new AndroidJavaClass("com.thirdweb.unity.ThirdwebAndroidPlugin");
            using var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            thirdwebPluginClass.CallStatic("OpenCustomTab", currentActivity, url);
        }

        private void OnDeepLinkActivated(string url)
        {
            if (_taskCompletionSource.Task.IsCanceled || !url.StartsWith(_customScheme))
                return;

            _taskCompletionSource.TrySetResult(new BrowserResult(BrowserStatus.Success, url));
        }
    }
}

#endif
