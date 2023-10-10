using System.Threading;
using System.Threading.Tasks;

namespace Cdm.Authentication.Browser
{
    public interface IBrowser
    {
        Task<BrowserResult> StartAsync(
            string loginUrl, string redirectUrl, CancellationToken cancellationToken = default);
    }
}