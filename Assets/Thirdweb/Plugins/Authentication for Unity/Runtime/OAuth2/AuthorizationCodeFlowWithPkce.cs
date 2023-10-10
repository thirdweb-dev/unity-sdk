using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Cdm.Authentication.OAuth2
{
    /// <summary>
    /// OAuth 2.0 'Authorization Code' flow with PKCE (Proof Key for Code Exchange).
    ///
    /// PKCE (RFC 7636) is an extension to the Authorization Code flow to prevent CSRF and authorization code injection
    /// attacks. PKCE is not a form of client authentication, and PKCE is not a replacement for a client secret or
    /// other client authentication. PKCE is recommended even if a client is using a client secret or other form
    /// of client authentication like private_key_jwt.
    ///
    /// https://www.rfc-editor.org/rfc/rfc7636
    /// </summary>
    public abstract class AuthorizationCodeFlowWithPkce : AuthorizationCodeFlow
    {
        private string _codeVerifier;

        protected AuthorizationCodeFlowWithPkce(Configuration configuration)
            : base(configuration) { }

        protected override Dictionary<string, string> GetAuthorizationUrlParameters()
        {
            var parameters = base.GetAuthorizationUrlParameters();

            _codeVerifier = GenerateRandomDataBase64url(32);
            var codeChallenge = Base64UrlEncodeNoPadding(Sha256Ascii(_codeVerifier));

            parameters.Add("code_challenge", codeChallenge);
            parameters.Add("code_challenge_method", "S256");

            return parameters;
        }

        protected override Dictionary<string, string> GetAccessTokenParameters(string code)
        {
            var parameters = base.GetAccessTokenParameters(code);
            parameters.Add("code_verifier", _codeVerifier);
            return parameters;
        }

        private static string GenerateRandomDataBase64url(uint length)
        {
            var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64UrlEncodeNoPadding(bytes);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static string Base64UrlEncodeNoPadding(byte[] buffer)
        {
            var base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string, which is assumed to be ASCII.
        /// </summary>
        private static byte[] Sha256Ascii(string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text);
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                return sha256.ComputeHash(bytes);
            }
        }
    }
}
