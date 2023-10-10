namespace Cdm.Authentication.Browser
{
    public class ASWebAuthenticationSessionError
    {
        public ASWebAuthenticationSessionErrorCode code { get; }
        public string message { get; }
        
        public ASWebAuthenticationSessionError(ASWebAuthenticationSessionErrorCode code, string message)
        {
            this.code = code;
            this.message = message;
        }
    }
}