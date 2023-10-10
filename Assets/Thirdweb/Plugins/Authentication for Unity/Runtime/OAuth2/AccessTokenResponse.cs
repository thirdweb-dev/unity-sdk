using System;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Cdm.Authentication.OAuth2
{
    [Preserve]
    [DataContract]
    public class AccessTokenResponse
    {
        /// <summary>
        /// Gets or sets the access token issued by the authorization server.
        /// </summary>
        [Preserve]
        [DataMember(IsRequired = true, Name = "access_token")]
        public string accessToken { get; set; }

        /// <summary>
        /// Gets or sets the refresh token which can be used to obtain a new access token.
        /// </summary>
        [Preserve]
        [DataMember(Name = "refresh_token")]
        public string refreshToken { get; set; }

        /// <summary>
        /// Gets or sets the token type as specified in http://tools.ietf.org/html/rfc6749#section-7.1.
        /// </summary>
        [Preserve]
        [DataMember(IsRequired = true, Name = "token_type")]
        public string tokenType { get; set; }

        /// <summary>
        /// Gets or sets the lifetime in seconds of the access token.
        /// </summary>
        [Preserve]
        [DataMember(Name = "expires_in")]
        public long? expiresIn { get; set; }

        /// <summary>
        /// Gets or sets the scope of the access token as specified in http://tools.ietf.org/html/rfc6749#section-3.3.
        /// </summary>
        [Preserve]
        [DataMember(Name = "scope")]
        public string scope { get; set; }

        /// <summary>
        /// The date and time that this token was issued, expressed in UTC.
        /// </summary>
        /// <remarks>
        /// This should be set by the <b>client</b> after the token was received from the server.
        /// </remarks>
        public DateTime? issuedAt { get; set; }

        /// <summary>
        /// Seconds till the <see cref="accessToken"/> expires returned by provider.
        /// </summary>
        public DateTime? expiresAt
        {
            get
            {
                if (issuedAt.HasValue && expiresIn.HasValue)
                {
                    return issuedAt.Value + TimeSpan.FromSeconds(expiresIn.Value);
                }

                return null;
            }
        }

        public AuthenticationHeaderValue GetAuthenticationHeader()
        {
            return new AuthenticationHeaderValue(tokenType, accessToken);
        }

        /// <summary>
        /// Returns true if the token is expired or it's going to expire soon.
        /// </summary>
        /// <remarks>
        /// If a token response does not have <see cref="accessToken"/> then it's considered expired.
        /// If <see cref="expiresAt"/> is <c>null</c>, the token is also considered expired.
        /// </remarks>
        public bool IsExpired()
        {
            return string.IsNullOrEmpty(accessToken) || expiresAt == null || expiresAt < DateTime.UtcNow;
        }

        /// <summary>
        /// Returns true if the <see cref="refreshToken">refresh token</see> is exist.
        /// </summary>
        public bool HasRefreshToken()
        {
            return !string.IsNullOrEmpty(refreshToken);
        }
    }
}