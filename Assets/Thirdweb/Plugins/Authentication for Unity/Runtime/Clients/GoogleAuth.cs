using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Cdm.Authentication.OAuth2;
using Cdm.Authentication.Utils;

namespace Cdm.Authentication.Clients
{
    public class GoogleAuth : AuthorizationCodeFlow, IUserInfoProvider
    {
        public GoogleAuth(Configuration configuration) : base(configuration)
        {
        }

        public override string authorizationUrl => "https://accounts.google.com/o/oauth2/auth";
        public override string accessTokenUrl => "https://accounts.google.com/o/oauth2/token";
        public string userInfoUrl => "https://www.googleapis.com/oauth2/v1/userinfo";

        public async Task<IUserInfo> GetUserInfoAsync(CancellationToken cancellationToken = default)
        {
            if (accessTokenResponse == null)
                throw new AccessTokenRequestException(new AccessTokenRequestError()
                {
                    code = AccessTokenRequestErrorCode.InvalidGrant,
                    description = "Authentication required."
                }, null);
            
            var authenticationHeader = accessTokenResponse.GetAuthenticationHeader();
            return await UserInfoParser.GetUserInfoAsync<GoogleUserInfo>(
                httpClient, userInfoUrl, authenticationHeader, cancellationToken);
        }
    }
    
    [DataContract]
    public class GoogleUserInfo : IUserInfo
    {
        [DataMember(Name = "id", IsRequired = true)]
        public string id { get; set; }

        [DataMember(Name = "name")] 
        public string name { get; set; }

        [DataMember(Name = "given_name")] 
        public string givenName { get; set; }

        [DataMember(Name = "family_name")] 
        public string familyName { get; set; }

        [DataMember(Name = "email")] 
        public string email { get; set; }

        [DataMember(Name = "picture")] 
        public string picture { get; set; }
    }
}