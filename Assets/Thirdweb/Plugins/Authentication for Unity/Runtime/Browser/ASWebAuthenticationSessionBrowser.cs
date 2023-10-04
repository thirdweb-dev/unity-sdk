using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cdm.Authentication.Browser
{
    public class ASWebAuthenticationSessionBrowser : IBrowser
    {
        private TaskCompletionSource<BrowserResult> _taskCompletionSource;

        /// <summary>
        /// Indicates whether the session should ask the browser for a private authentication
        /// session.
        /// </summary>
        /// <remarks>
        /// Set this property before you call <see cref="StartAsync"/>. Otherwise it has no effect.
        /// </remarks>
        public bool prefersEphemeralWebBrowserSession { get; set; } = false;
        
        public async Task<BrowserResult> StartAsync(
            string loginUrl, string redirectUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(loginUrl))
                throw new ArgumentNullException(nameof(loginUrl));
            
            if (string.IsNullOrEmpty(redirectUrl))
                throw new ArgumentNullException(nameof(redirectUrl));
            
            _taskCompletionSource = new TaskCompletionSource<BrowserResult>();
            
            // Discard URL parameters. They are not valid for iOS URL Scheme.
            redirectUrl = redirectUrl.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries)[0];
            
            using var authenticationSession =
                new ASWebAuthenticationSession(loginUrl, redirectUrl, AuthenticationSessionCompletionHandler);
            authenticationSession.prefersEphemeralWebBrowserSession = prefersEphemeralWebBrowserSession;
            
            cancellationToken.Register(() =>
            {
                _taskCompletionSource?.TrySetCanceled();
            });

            try
            {
                if (!authenticationSession.Start())
                {
                    _taskCompletionSource.SetResult(
                        new BrowserResult(BrowserStatus.UnknownError, "Browser could not be started."));
                }

                return await _taskCompletionSource.Task;
            }
            catch (TaskCanceledException)
            {
                // In case of timeout cancellation.
                authenticationSession?.Cancel();
                throw;
            }
        }

        private void AuthenticationSessionCompletionHandler(string callbackUrl, ASWebAuthenticationSessionError error)
        {
            if (error.code == ASWebAuthenticationSessionErrorCode.None)
            {
                _taskCompletionSource.SetResult(
                    new BrowserResult(BrowserStatus.Success, callbackUrl));   
            }
            else if (error.code == ASWebAuthenticationSessionErrorCode.CanceledLogin)
            {
                _taskCompletionSource.SetResult(
                    new BrowserResult(BrowserStatus.UserCanceled, callbackUrl, error.message));
            }
            else
            {
                _taskCompletionSource.SetResult(
                    new BrowserResult(BrowserStatus.UnknownError, callbackUrl, error.message));
            }
        }
    }
}