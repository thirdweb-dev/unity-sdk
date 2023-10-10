namespace Cdm.Authentication.Browser
{
    public class BrowserResult
    {
        /// <summary>
        /// The browser status indicates the operation is whether success or not.
        /// </summary>
        public BrowserStatus status { get; }
        
        /// <summary>
        /// After a user successfully authorizes an application, the authorization server will redirect the user back
        /// to the application with the redirect URL. Use this if only if <see cref="status"/> is
        /// <see cref="BrowserStatus.Success"/>.
        /// </summary>
        public string redirectUrl { get; }
        
        /// <summary>
        /// The error description if an error is exist. You can use this value if <see cref="status"/> is not
        /// <see cref="BrowserStatus.Success"/>.
        /// </summary>
        public string error { get; }

        public BrowserResult(BrowserStatus status, string redirectUrl)
        {
            this.status = status;
            this.redirectUrl = redirectUrl;
        }
        
        public BrowserResult(BrowserStatus status, string redirectUrl, string error)
        {
            this.status = status;
            this.redirectUrl = redirectUrl;
            this.error = error;
        }
    }
}