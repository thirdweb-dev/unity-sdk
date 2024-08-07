#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Thirdweb.Unity
{
    public class WebGLInAppWalletBrowser : MonoBehaviour, IThirdwebBrowser
    {
        private static WebGLInAppWalletBrowser _instance;
        private TaskCompletionSource<BrowserResult> _taskCompletionSource;
        private bool _isCallbackInvoked;

        [DllImport("__Internal")]
        private static extern void openPopup(string url, string unityObjectName, string unityCallbackMethod);

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

        public async Task<BrowserResult> Login(ThirdwebClient client, string loginUrl, string redirectUrl, Action<string> browserOpenAction, CancellationToken cancellationToken = default)
        {
            ThirdwebDebug.Log($"WebGLInAppWalletBrowser.Login: {loginUrl}");

            _taskCompletionSource = new TaskCompletionSource<BrowserResult>();

            _isCallbackInvoked = false;

            cancellationToken.Register(() =>
            {
                _taskCompletionSource?.TrySetCanceled();
            });

            string unityObjectName = gameObject.name;
            string unityCallbackMethod = "OnRedirect";

            Uri uri = new(loginUrl);

            openPopup(loginUrl, unityObjectName, unityCallbackMethod);

            var completedTask = await Task.WhenAny(_taskCompletionSource.Task, Task.Delay(TimeSpan.FromSeconds(120), cancellationToken));
            return completedTask == _taskCompletionSource.Task ? await _taskCompletionSource.Task : new BrowserResult(BrowserStatus.Timeout, null, "The operation timed out.");
        }

        public void OnRedirect(string message)
        {
            if (_isCallbackInvoked)
            {
                return;
            }

            if (message == "PopupClosedWithoutAction")
            {
                _taskCompletionSource.SetResult(new BrowserResult(BrowserStatus.UserCanceled, null, "The popup was closed without completing the action."));
            }
            else
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<JObject>(message);

                    if (data["eventType"].ToString() == "oauthSuccessResult")
                    {
                        _isCallbackInvoked = true;
                        _taskCompletionSource.SetResult(new BrowserResult(BrowserStatus.Success, data["authResult"].ToString()));
                    }
                    else if (data["eventType"].ToString() == "oauthFailureResult")
                    {
                        _isCallbackInvoked = true;
                        _taskCompletionSource.SetResult(new BrowserResult(BrowserStatus.UnknownError, null, data["error"].ToString()));
                    }
                }
                catch (Exception ex)
                {
                    _taskCompletionSource.SetResult(new BrowserResult(BrowserStatus.UnknownError, null, $"Failed to parse the message from the popup. Error: {ex.Message}"));
                }
            }
        }
    }
}
#endif
