using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb.Unity
{
    public class CrossPlatformUnityBrowser : IThirdwebBrowser
    {
        IThirdwebBrowser _unityBrowser;

        public CrossPlatformUnityBrowser()
        {
            var go = new GameObject("WebGLInAppWalletBrowser");

#if UNITY_EDITOR
            _unityBrowser = new InAppWalletBrowser();
#elif UNITY_WEBGL
            _unityBrowser = go.AddComponent<WebGLInAppWalletBrowser>();
#elif UNITY_ANDROID
            _unityBrowser = new AndroidBrowser();
#elif UNITY_IOS
            _unityBrowser = new iOSBrowser();
#else
            _unityBrowser = new InAppWalletBrowser();
#endif
        }

        public async Task<BrowserResult> Login(string loginUrl, string customScheme, Action<string> browserOpenAction, CancellationToken cancellationToken = default)
        {
            return await _unityBrowser.Login(loginUrl, customScheme, browserOpenAction, cancellationToken);
        }
    }
}
