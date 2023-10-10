using System;

namespace Cdm.Authentication.OAuth2
{
    public class AuthorizationCodeRequestException : Exception
    {
        public AuthorizationCodeRequestError error { get; }

        public AuthorizationCodeRequestException(AuthorizationCodeRequestError error)
        {
            this.error = error;
        }

        public AuthorizationCodeRequestException(AuthorizationCodeRequestError error, string message) : base(message)
        {
            this.error = error;
        }

        public AuthorizationCodeRequestException(AuthorizationCodeRequestError error, string message, Exception innerException)
            : base(message, innerException)
        {
            this.error = error;
        }
    }
}