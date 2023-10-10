using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.Scripting;

namespace Cdm.Authentication.OAuth2
{
    /// <summary>
    /// 
    /// </summary>
    [Preserve]
    [JsonConverter(typeof(StringEnumConverter))]
    [DataContract]
    public enum AuthorizationCodeRequestErrorCode
    {
        /// <summary>
        /// The request is missing a required parameter, includes an invalid parameter value, includes a parameter
        /// more than once, or is otherwise malformed.
        /// </summary>
        [Preserve]
        [EnumMember(Value = "invalid_request")]
        InvalidRequest,

        /// <summary>
        /// The client is not authorized to request an authorization code using this method.
        /// </summary>
        [Preserve]
        [EnumMember(Value = "unauthorized_client")]
        UnauthorizedClient,

        /// <summary>
        /// The resource owner or authorization server denied the request.
        /// </summary>
        [Preserve]
        [EnumMember(Value = "access_denied")]
        AccessDenied,

        /// <summary>
        /// The authorization server does not support obtaining an authorization code using this method.
        /// </summary>
        [Preserve]
        [EnumMember(Value = "unsupported_response_type")]
        UnsupportedResponseType,

        /// <summary>
        /// The requested scope is invalid, unknown, or malformed.
        /// </summary>
        [Preserve]
        [EnumMember(Value = "invalid_scope")]
        InvalidScope,

        /// <summary>
        /// The authorization server encountered an unexpected condition that prevented it from fulfilling the request.
        /// (This error code is needed because a 500 Internal Server Error HTTP status code cannot be returned to
        /// the client via an HTTP redirect.)
        /// </summary>
        [Preserve]
        [EnumMember(Value = "server_error")]
        ServerError,

        /// <summary>
        /// The authorization server is currently unable to handle the request due to a temporary overloading or
        /// maintenance of the server.  (This error code is needed because a 503 Service Unavailable HTTP status code
        /// cannot be returned to the client via an HTTP redirect.)
        /// </summary>
        [Preserve]
        [EnumMember(Value = "temporarily_unavailable")]
        TemporarilyUnavailable
    }
}