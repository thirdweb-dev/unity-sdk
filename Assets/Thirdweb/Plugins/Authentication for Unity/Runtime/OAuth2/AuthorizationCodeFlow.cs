using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Cdm.Authentication.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Cdm.Authentication.OAuth2
{
    /// <summary>
    /// Supports 'Authorization Code' flow. Enables user sign-in and access to web APIs on behalf of the user.
    ///
    /// The OAuth 2.0 authorization code grant type, enables a client application to obtain
    /// authorized access to protected resources like web APIs. The auth code flow requires a user-agent that supports
    /// redirection from the authorization server back to your application. For example, a web browser, desktop,
    /// or mobile application operated by a user to sign in to your app and access their data.
    /// </summary>
    public abstract class AuthorizationCodeFlow : IDisposable
    {
        /// <summary>
        /// The endpoint for authorization server. This is used to get the authorization code.
        /// </summary>
        public abstract string authorizationUrl { get; }

        /// <summary>
        /// The endpoint for authentication server. This is used to exchange the authorization code for an access token.
        /// </summary>
        public abstract string accessTokenUrl { get; }

        /// <summary>
        /// The state; any additional information that was provided by application and is posted back by service.
        /// </summary>
        /// <seealso cref="AuthorizationCodeRequest.state"/>
        public string state { get; private set; }

        /// <summary>
        /// Gets the client configuration for the authentication method.
        /// </summary>
        public Configuration configuration { get; }

        protected AccessTokenResponse accessTokenResponse { get; private set; }
        protected HttpClient httpClient { get; }

        protected AuthorizationCodeFlow(Configuration configuration)
        {
            this.configuration = configuration;

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true, NoStore = true };
        }

        /// <summary>
        /// Determines the need for retrieval of a new authorization code.
        /// </summary>
        /// <returns>Indicates if a new authorization code needs to be retrieved.</returns>
        public bool ShouldRequestAuthorizationCode()
        {
            return accessTokenResponse == null || !accessTokenResponse.HasRefreshToken();
        }

        /// <summary>
        ///  Determines the need for retrieval of a new access token using the refresh token.
        /// </summary>
        /// <remarks>
        /// If <see cref="accessTokenResponse"/> does not exist, then get new authorization code first.
        /// </remarks>
        /// <returns>Indicates if a new access token needs to be retrieved.</returns>
        /// <seealso cref="ShouldRequestAuthorizationCode"/>
        public bool ShouldRefreshToken()
        {
            return accessTokenResponse.IsNullOrExpired();
        }

        /// <summary>,
        /// Gets an authorization code request URI with the specified <see cref="configuration"/>.
        /// </summary>
        /// <returns>The authorization code request URI.</returns>
        public string GetAuthorizationUrl()
        {
            // Generate new state.
            state = Guid.NewGuid().ToString("D");

            var parameters = GetAuthorizationUrlParameters();

            return UrlBuilder.New(authorizationUrl).SetQueryParameters(parameters).ToString();
        }

        protected virtual Dictionary<string, string> GetAuthorizationUrlParameters()
        {
            return JsonHelper.ToDictionary(
                new AuthorizationCodeRequest()
                {
                    clientId = configuration.clientId,
                    redirectUri = configuration.redirectUri,
                    scope = configuration.scope,
                    state = state
                }
            );
        }

        /// <summary>
        /// Asynchronously exchanges code with a token.
        /// </summary>
        /// <param name="redirectUrl">
        /// <see cref="Cdm.Authentication.Browser.BrowserResult.redirectUrl">Redirect URL</see> which is retrieved
        /// from the browser result.
        /// </param>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <returns>Access token response which contains the access token.</returns>
        /// <exception cref="AuthorizationCodeRequestException"></exception>
        /// <exception cref="Exception"></exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="AccessTokenRequestException"></exception>
        public virtual async Task<AccessTokenResponse> ExchangeCodeForAccessTokenAsync(string redirectUrl, CancellationToken cancellationToken = default)
        {
            var authorizationResponseUri = new Uri(redirectUrl);
            var query = HttpUtility.ParseQueryString(authorizationResponseUri.Query);

            // Is there any error?
            if (JsonHelper.TryGetFromNameValueCollection<AuthorizationCodeRequestError>(query, out var authorizationError))
                throw new AuthorizationCodeRequestException(authorizationError);

            if (!JsonHelper.TryGetFromNameValueCollection<AuthorizationCodeResponse>(query, out var authorizationResponse))
                throw new Exception("Authorization code could not get.");

            // Validate authorization response state.
            if (!string.IsNullOrEmpty(state) && state != authorizationResponse.state)
                throw new SecurityException($"Invalid state got: {authorizationResponse.state}");

            var parameters = GetAccessTokenParameters(authorizationResponse.code);

            Debug.Assert(parameters != null);

            accessTokenResponse = await GetAccessTokenInternalAsync(new FormUrlEncodedContent(parameters), cancellationToken);
            return accessTokenResponse;
        }

        protected virtual Dictionary<string, string> GetAccessTokenParameters(string code)
        {
            return JsonHelper.ToDictionary(
                new AccessTokenRequest()
                {
                    code = code,
                    clientId = configuration.clientId,
                    clientSecret = configuration.clientSecret,
                    redirectUri = configuration.redirectUri
                }
            );
        }

        /// <summary>
        /// Gets the access token immediately from cache if <see cref="ShouldRefreshToken"/> is <c>false</c>;
        /// or refreshes and returns it using the refresh token.
        /// if available.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <exception cref="AccessTokenRequestException">If access token cannot be granted.</exception>
        public async Task<AccessTokenResponse> GetOrRefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            if (ShouldRefreshToken())
            {
                return await RefreshTokenAsync(cancellationToken);
            }

            // Return from the cache immediately.
            return accessTokenResponse;
        }

        /// <summary>
        /// Asynchronously refreshes an access token using the refresh token from the <see cref="accessTokenResponse"/>.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <returns>Token response which contains the access token and the refresh token.</returns>
        public async Task<AccessTokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            if (accessTokenResponse == null)
                throw new AccessTokenRequestException(new AccessTokenRequestError() { code = AccessTokenRequestErrorCode.InvalidGrant, description = "Authentication required." }, null);

            return await RefreshTokenAsync(accessTokenResponse.refreshToken, cancellationToken);
        }

        /// <summary>
        /// Asynchronously refreshes an access token using a refresh token.
        /// </summary>
        /// <param name="refreshToken">Refresh token which is used to get a new access token.</param>
        /// <param name="cancellationToken">Cancellation token to cancel operation.</param>
        /// <returns>Token response which contains the access token and the input refresh token.</returns>
        public async Task<AccessTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                var error = new AccessTokenRequestError() { code = AccessTokenRequestErrorCode.InvalidGrant, description = "Refresh token does not exist." };

                throw new AccessTokenRequestException(error, null);
            }

            var parameters = JsonHelper.ToDictionary(new RefreshTokenRequest() { refreshToken = refreshToken, scope = configuration.scope });

            Debug.Assert(parameters != null);

            var tokenResponse = await GetAccessTokenInternalAsync(new FormUrlEncodedContent(parameters), cancellationToken);
            if (!tokenResponse.HasRefreshToken())
            {
                tokenResponse.refreshToken = refreshToken;
            }

            accessTokenResponse = tokenResponse;
            return accessTokenResponse;
        }

        private async Task<AccessTokenResponse> GetAccessTokenInternalAsync(FormUrlEncodedContent content, CancellationToken cancellationToken = default)
        {
            Debug.Assert(content != null);

            var authString = $"{configuration.clientId}:{configuration.clientSecret}";
            var base64AuthString = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, accessTokenUrl);
            tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64AuthString);
            tokenRequest.Content = content;

#if UNITY_EDITOR
            Debug.Log($"{tokenRequest}");
            Debug.Log($"{await tokenRequest.Content.ReadAsStringAsync()}");
#endif

            var response = await httpClient.SendAsync(tokenRequest, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();

#if UNITY_EDITOR
                Debug.Log(responseJson);
#endif

                var tokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(responseJson);
                tokenResponse.issuedAt = DateTime.UtcNow;
                return tokenResponse;
            }

            AccessTokenRequestError error = null;
            try
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                error = JsonConvert.DeserializeObject<AccessTokenRequestError>(errorJson);
            }
            catch (Exception)
            {
                // ignored
            }

            throw new AccessTokenRequestException(error, response.StatusCode);
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }

        /// <summary>
        /// The configuration of third-party authentication service client.
        /// </summary>
        [DataContract]
        public struct Configuration
        {
            /// <summary>
            /// The client identifier issued to the client during the registration process described by
            /// <a href="https://www.rfc-editor.org/rfc/rfc6749#section-2.2">Section 2.2</a>.
            /// </summary>
            [DataMember(Name = "client_id", IsRequired = true)]
            public string clientId { get; set; }

            /// <summary>
            /// The client secret. The client MAY omit the parameter if the client secret is an empty string.
            /// </summary>
            [DataMember(Name = "client_secret")]
            public string clientSecret { get; set; }

            /// <summary>
            /// The authorization and token endpoints allow the client to specify the scope of the access request using
            /// the "scope" request parameter.  In turn, the authorization server uses the "scope" response parameter to
            /// inform the client of the scope of the access token issued. The value of the scope parameter is expressed
            /// as a list of space- delimited, case-sensitive strings.  The strings are defined by the authorization server.
            /// If the value contains multiple space-delimited strings, their order does not matter, and each string adds an
            /// additional access range to the requested scope.
            /// </summary>
            [DataMember(Name = "scope")]
            public string scope { get; set; }

            /// <summary>
            /// After completing its interaction with the resource owner, the authorization server directs the resource
            /// owner's user-agent back to the client. The authorization server redirects the user-agent to the client's
            /// redirection endpoint previously established with the authorization server during the client registration
            /// process or when making the authorization request.
            /// </summary>
            /// <remarks>
            /// The redirection endpoint URI MUST be an absolute URI as defined by
            /// <a href="https://www.rfc-editor.org/rfc/rfc3986#section-4.3">[RFC3986] Section 4.3</a>.
            /// The endpoint URI MAY include an "application/x-www-form-urlencoded" formatted (per
            /// <a href="https://www.rfc-editor.org/rfc/rfc6749#appendix-B">Appendix B</a>) query
            /// component (<a href="https://www.rfc-editor.org/rfc/rfc3986#section-3.4">[RFC3986] Section 3.4</a>),
            /// which MUST be retained when adding additional query parameters. The endpoint URI MUST NOT include
            /// a fragment component.
            /// </remarks>
            [DataMember(Name = "redirect_uri")]
            public string redirectUri { get; set; }
        }
    }
}
