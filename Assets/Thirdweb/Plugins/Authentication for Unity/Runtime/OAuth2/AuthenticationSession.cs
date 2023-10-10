using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Cdm.Authentication.Browser;
using UnityEngine;

namespace Cdm.Authentication.OAuth2
{
    public class AuthenticationSession : IDisposable
    {
        private readonly AuthorizationCodeFlow _client;
        private readonly IBrowser _browser;

        public TimeSpan loginTimeout { get; set; } = TimeSpan.FromMinutes(10);

        public AuthenticationSession(AuthorizationCodeFlow client, IBrowser browser)
        {
            _client = client;
            _browser = browser;
        }

        public bool ShouldAuthenticate()
        {
            return _client.ShouldRequestAuthorizationCode();
        }

        public bool SupportsUserInfo()
        {
            return _client is IUserInfoProvider;
        }

        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync()
        {
            var tokenResponse = await _client.GetOrRefreshTokenAsync();
            return tokenResponse.GetAuthenticationHeader();
        }

        /// <summary>
        /// Asynchronously authorizes the installed application to access user's protected data.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <exception cref="AuthorizationCodeRequestException"></exception>
        /// <exception cref="AccessTokenRequestException"></exception>
        /// <exception cref="AuthenticationException"></exception>
        public async Task<AccessTokenResponse> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource(loginTimeout);

            try
            {
                // 1. Create authorization request URL.
                Debug.Log("Making authorization request...");

                var redirectUrl = _client.configuration.redirectUri;
                var authorizationUrl = _client.GetAuthorizationUrl();

                // 2. Get authorization code grant using login form in the browser.
                Debug.Log("Getting authorization grant using browser login...");

                using var loginCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellationTokenSource.Token);

                var browserResult = await _browser.StartAsync(authorizationUrl, redirectUrl, loginCancellationTokenSource.Token);
                if (browserResult.status == BrowserStatus.Success)
                {
                    // 3. Exchange authorization code for access and refresh tokens.
                    Debug.Log("Exchanging authorization code for access and refresh tokens...");

#if UNITY_EDITOR
                    Debug.Log($"Redirect URL: {browserResult.redirectUrl}");
#endif
                    return await _client.ExchangeCodeForAccessTokenAsync(browserResult.redirectUrl, cancellationToken);
                }

                if (browserResult.status == BrowserStatus.UserCanceled)
                {
                    throw new AuthenticationException(AuthenticationError.Cancelled, browserResult.error);
                }

                throw new AuthenticationException(AuthenticationError.Other, browserResult.error);
            }
            catch (TaskCanceledException e)
            {
                if (timeoutCancellationTokenSource.IsCancellationRequested)
                    throw new AuthenticationException(AuthenticationError.Timeout, "Operation timed out.");

                throw new AuthenticationException(AuthenticationError.Cancelled, "Operation was cancelled.", e);
            }
        }

        /// <inheritdoc cref="AuthorizationCodeFlow.GetOrRefreshTokenAsync"/>
        public async Task<AccessTokenResponse> GetOrRefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            return await _client.GetOrRefreshTokenAsync(cancellationToken);
        }

        /// <inheritdoc cref="AuthorizationCodeFlow.RefreshTokenAsync(System.Threading.CancellationToken)"/>
        public async Task<AccessTokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            return await _client.RefreshTokenAsync(cancellationToken);
        }

        /// <inheritdoc cref="AuthorizationCodeFlow.RefreshTokenAsync(string,System.Threading.CancellationToken)"/>
        public async Task<AccessTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return await _client.RefreshTokenAsync(refreshToken, cancellationToken);
        }

        /// <summary>
        /// Gets the user info if authentication client supports getting user info.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <returns>The user info if exist; otherwise returns <c>null</c>.</returns>
        /// <seealso cref="SupportsUserInfo"/>
        public async Task<IUserInfo> GetUserInfoAsync(CancellationToken cancellationToken = default)
        {
            if (SupportsUserInfo())
            {
                return await ((IUserInfoProvider)_client).GetUserInfoAsync(cancellationToken);
            }

            return null;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
