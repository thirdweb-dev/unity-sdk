using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Cdm.Authentication.OAuth2;
using Cdm.Authentication.Utils;

namespace Cdm.Authentication.Clients
{
    public class FacebookAuth : AuthorizationCodeFlow, IUserInfoProvider
    {
        public override string authorizationUrl => "https://www.facebook.com/dialog/oauth";
        public override string accessTokenUrl => "https://graph.facebook.com/oauth/access_token";
        public string userInfoUrl => "https://graph.facebook.com/me";
        
        public FacebookAuth(Configuration configuration) : base(configuration)
        {
        }
        
        public async Task<IUserInfo> GetUserInfoAsync(CancellationToken cancellationToken = default)
        {
            if (accessTokenResponse == null)
                throw new AccessTokenRequestException(new AccessTokenRequestError()
                {
                    code = AccessTokenRequestErrorCode.InvalidGrant,
                    description = "Authentication required."
                }, null);
            
            var authenticationHeader = accessTokenResponse.GetAuthenticationHeader();
            return await UserInfoParser.GetUserInfoAsync<FacebookUserInfo>(
                httpClient, userInfoUrl, authenticationHeader, cancellationToken);
        }
    }
    
    [DataContract]
    public class FacebookUserInfo : IUserInfo
    {
        [DataMember(Name = "id", IsRequired = true)]
        public string id { get; set; }
        
        [DataMember(Name = "first_name")] 
        public string firstName { get; set; }

        [DataMember(Name = "last_name")] 
        public string lastName { get; set; }

        [DataMember(Name = "email")] 
        public string email { get; set; }

        [DataMember(Name = "picture")]
        public PictureData pictureData { get; set; }
        
        public string name => $"{firstName} {lastName}";
        public string picture => pictureData?.url;
        
        [DataContract]
        public class PictureData
        {
            [DataMember(Name = "url", IsRequired = true)] 
            public string url { get; set; }
        }
    }
}