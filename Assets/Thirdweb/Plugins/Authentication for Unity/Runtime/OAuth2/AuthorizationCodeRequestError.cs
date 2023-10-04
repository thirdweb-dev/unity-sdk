using System.Runtime.Serialization;
using UnityEngine.Scripting;

namespace Cdm.Authentication.OAuth2
{
    [Preserve]
    [DataContract]
    public class AuthorizationCodeRequestError
    {
        [Preserve]
        [DataMember(IsRequired = true, Name = "error")]
        public AuthorizationCodeRequestErrorCode code { get; set; }

        /// <summary>
        /// OPTIONAL. Human-readable ASCII [<a href="https://www.rfc-editor.org/rfc/rfc6749#ref-USASCII">USASCII</a>]
        /// text providing additional information, used to assist the client developer in understanding
        /// the error that occurred.
        /// </summary>
        [Preserve]
        [DataMember(Name = "error_description")]
        public string description { get; set; }

        /// <summary>
        /// OPTIONAL. A URI identifying a human-readable web page with information about the error, used to provide
        /// the client developer with additional information about the error.
        /// </summary>
        [Preserve]
        [DataMember(Name = "error_uri")]
        public string uri { get; set; }
        
        /// <summary>
        /// REQUIRED if a "state" parameter was present in the client authorization request. The exact value received
        /// from the client.
        /// </summary>
        [Preserve]
        [DataMember(Name = "state")]
        public string state {  get;  set; }
    }
}