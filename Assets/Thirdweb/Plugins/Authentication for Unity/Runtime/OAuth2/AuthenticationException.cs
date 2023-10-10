using System;

namespace Cdm.Authentication.OAuth2
{
    public class AuthenticationException : Exception
    {
        public AuthenticationError error { get; }

        public AuthenticationException(AuthenticationError error)
        {
            this.error = error;
        }

        public AuthenticationException(AuthenticationError error, string message) : base(message)
        {
            this.error = error;
        }

        public AuthenticationException(AuthenticationError error, string message, Exception innerException)
            : base(message, innerException)
        {
            this.error = error;
        }
    }
}