using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Cdm.Authentication.OAuth2
{
    /// <summary>
    /// OAuth 2.0 request for an access token using an authorization code as specified in
    /// http://tools.ietf.org/html/rfc6749#section-4.1.1.
    /// </summary>
    [Preserve]
    [DataContract]
    public class AuthorizationCodeRequest
    {
        /// <summary>
        /// Gets  the response type which is the 'code'.
        /// </summary>
        [Preserve]
        [DataMember(Name = "response_type", IsRequired = true)]
        public string responseType => "code";
        
        /// <summary>
        /// Gets or sets the client identifier as specified in https://www.rfc-editor.org/rfc/rfc6749#section-2.2.
        /// </summary>
        [Preserve]
        [DataMember(Name = "client_id", IsRequired = true)]
        public string clientId { get; set; }
        
        /// <summary>
        /// Gets or sets the URI that the authorization server directs the resource owner's user-agent back to the
        /// client after a successful authorization grant, as specified in
        /// http://tools.ietf.org/html/rfc6749#section-3.1.2 or <c>null</c> for none.
        /// </summary>
        [Preserve]
        [DataMember(Name = "redirect_uri")]
        public string redirectUri { get; set; }
        
        /// <summary>
        /// Gets or sets space-separated list of scopes, as specified in
        /// http://tools.ietf.org/html/rfc6749#section-3.3 or <c>null</c> for none.
        /// </summary>
        [Preserve]
        [DataMember(Name = "scope")]
        public string scope { get; set; }

        /// <summary>
        /// Gets or sets the state (an opaque value used by the client to maintain state between the request and
        /// callback, as mentioned in http://tools.ietf.org/html/rfc6749#section-3.1.2.2 or <c>null</c> for none.
        /// </summary>
        [Preserve]
        [DataMember(Name = "state")]
        public string state { get; set; }
    }
}