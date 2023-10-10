using System.Threading;
using System.Threading.Tasks;

namespace Cdm.Authentication
{
    public interface IUserInfoProvider
    {
        /// <summary>
        /// Obtains user information using third-party authentication service using data provided via callback request.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task<IUserInfo> GetUserInfoAsync(CancellationToken cancellationToken = default);
    }
}