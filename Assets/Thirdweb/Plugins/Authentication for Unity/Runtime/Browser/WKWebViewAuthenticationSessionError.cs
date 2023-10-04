namespace Cdm.Authentication.Browser
{
    public class WKWebViewAuthenticationSessionError
    {
        public WKWebViewAuthenticationSessionErrorCode code { get; }
        public string message { get; }
        
        public WKWebViewAuthenticationSessionError(WKWebViewAuthenticationSessionErrorCode code, string message)
        {
            this.code = code;
            this.message = message;
        }
    }
}