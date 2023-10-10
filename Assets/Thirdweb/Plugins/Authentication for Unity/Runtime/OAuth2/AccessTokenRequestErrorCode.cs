using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.Scripting;

namespace Cdm.Authentication.OAuth2
{
    /// <summary>
    /// The authorization server responds with an HTTP 400 (Bad Request) status code (unless specified otherwise) and
    /// includes the following parameters with the response.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [DataContract]
    [Preserve]
    public enum AccessTokenRequestErrorCode
    {
        /// <summary>
        /// The request is missing a required parameter, includes an unsupported parameter value
        /// (other than grant type), repeats a parameter, includes multiple credentials, utilizes more than
        /// one mechanism for authenticating the client, or is otherwise malformed.
        /// </summary>
        [Preserve]
        [EnumMember(Value = "invalid_request")]
        InvalidRequest,

        /// <summary>
        /// Client authentication failed (e.g., unknown client, no client authentication included,
        /// or unsupported authentication method). The authorization server MAY return an HTTP 401 (Unauthorized)
        /// status code to indicate which HTTP authentication schemes are supported.  If the client attempted to
        /// authenticate via the "Authorization" request header field, the authorization server MUST respond with
        /// an HTTP 401 (Unauthorized) status code and include the "WWW-Authenticate" response header field matching
        /// the authentication scheme used by the client.
        /// </summary>
        [Preserve]
        [EnumMember(Value = "invalid_client")]
        InvalidClient,

        /// <summary>
        /// The provided authorization grant (e.g., authorization code, resource owner credentials) or refresh token
        /// is invalid, expired, revoked, does not match the redirection URI used in the authorization request,
        /// or was issued to another client.
        /// </summary>
        [Preserve]
        [EnumMember(Value = "invalid_grant")]
        InvalidGrant,

        /// <summary>
        /// The authenticated client is not authorized to use this authorization grant type.
        /// </summary>
        [Preserve]
        [EnumMember(Value = "unauthorized_client")]
        UnauthorizedClient,

        /// <summary>
        /// The authorization grant type is not supported by the authorization server.
        /// </summary>
        [Preserve]
        [EnumMember(Value = "unsupported_grant_type")]
        UnsupportedGrantType,

        /// <summary>
        /// The requested scope is invalid, unknown, malformed, or exceeds the scope granted by the resource owner.
        /// </summary>
        [Preserve]
        [EnumMember(Value = "invalid_scope")]
        InvalidScope,
    }
}