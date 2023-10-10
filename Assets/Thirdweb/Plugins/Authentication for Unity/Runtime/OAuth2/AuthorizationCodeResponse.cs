using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Cdm.Authentication.OAuth2
{
    /// <summary>
    ///  If the resource owner grants the access request, the authorization server issues an authorization code and
    /// delivers it to the client by adding the following parameters to the query component of the redirection URI
    /// using the "application/x-www-form-urlencoded" format,
    /// per <a href="https://www.rfc-editor.org/rfc/rfc6749#appendix-B">Appendix B</a>.
    /// </summary>
    [Preserve]
    [DataContract]
    public class AuthorizationCodeResponse
    {
        /// <summary>
        /// Gets or sets the authorization code received from the authorization server.
        /// </summary>
        [Preserve]
        [DataMember(IsRequired = true, Name = "code")]
        public string code { get; set; }
        
        /// <summary>
        /// The exact value received from the client while making the authorization request as specified in
        /// <see cref="AuthorizationCodeRequest.state"/>.
        /// </summary>
        [Preserve]
        [DataMember(Name = "state")]
        public string state { get; set; }
    }
}