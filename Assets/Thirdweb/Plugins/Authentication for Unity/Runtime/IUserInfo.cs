namespace Cdm.Authentication
{
    public interface IUserInfo
    {
        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        public string id { get; }
        
        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        public string name { get; }
        
        /// <summary>
        /// Gets the email address of the user.
        /// </summary>
        public string email { get; }

        /// <summary>
        /// Gets the user picture URL.
        /// </summary>
        public string picture { get; }
    }
}