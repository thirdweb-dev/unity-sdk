using System.Threading;
using System.Threading.Tasks;

namespace Thirdweb.Browser
{
    public class CrossPlatformBrowser : IThirdwebBrowser
    {
        private IThirdwebBrowser _browser;

        public async Task<BrowserResult> Login(string loginUrl, string redirectUrl, CancellationToken cancellationToken = default)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _browser = new AndroidBrowser();
#elif UNITY_IOS && !UNITY_EDITOR
            _browser = new IOSBrowser();
#else
            _browser = new StandaloneBrowser();
#endif

            return await _browser.Login(loginUrl, redirectUrl, cancellationToken);
        }
    }
}
