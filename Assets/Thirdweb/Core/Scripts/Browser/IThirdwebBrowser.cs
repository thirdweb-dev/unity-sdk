using System.Threading;
using System.Threading.Tasks;

namespace Thirdweb.Browser
{
    public interface IThirdwebBrowser
    {
        Task<BrowserResult> Login(string loginUrl, string redirectUrl, CancellationToken cancellationToken = default);
    }

    public enum BrowserStatus
    {
        Success,
        UserCanceled,
        Timeout,
        UnknownError,
    }

    public class BrowserResult
    {
        public BrowserStatus status { get; }

        public string callbackUrl { get; }

        public string error { get; }

        public BrowserResult(BrowserStatus status, string callbackUrl)
        {
            this.status = status;
            this.callbackUrl = callbackUrl;
        }

        public BrowserResult(BrowserStatus status, string callbackUrl, string error)
        {
            this.status = status;
            this.callbackUrl = callbackUrl;
            this.error = error;
        }
    }
}
