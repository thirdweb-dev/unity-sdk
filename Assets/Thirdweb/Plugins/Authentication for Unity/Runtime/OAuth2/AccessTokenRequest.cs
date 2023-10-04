using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Cdm.Authentication.OAuth2
{
    /// <summary>
    /// OAuth 2.0 request for an access token using an authorization code as specified in
    /// http://tools.ietf.org/html/rfc6749#section-4.1.3.
    /// </summary>
    [Preserve]
    [DataContract]
    public class AccessTokenRequest
    {
        /// <summary>
        /// Gets the authorization grant type as <c>'authorization_code'</c>.
        /// </summary>
        [Preserve]
        [DataMember(IsRequired = true, Name = "grant_type")]
        public string grantType => "authorization_code";
        
        /// <summary>
        /// Gets or sets the authorization code received from the authorization server.
        /// </summary>
        [Preserve]
        [DataMember(IsRequired = true, Name = "code")]
        public string code { get; set; }
        
        /// <summary>
        /// Gets or sets the client identifier as described in https://www.rfc-editor.org/rfc/rfc6749#section-3.2.1.
        /// </summary>
        [Preserve]
        [DataMember(IsRequired = true, Name = "client_id")]
        public string clientId { get; set; }
        
        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        [Preserve]
        [DataMember(Name = "client_secret")]
        public string clientSecret { get; set; }
        
        /// <summary>
        /// Gets or sets the redirect URI parameter matching the redirect URI parameter in the authorization request.
        /// </summary>
        [Preserve]
        [DataMember(Name = "redirect_uri")]
        public string redirectUri { get; set; }
    }
}