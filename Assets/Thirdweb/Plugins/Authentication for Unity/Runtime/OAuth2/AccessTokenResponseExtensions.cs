
namespace Cdm.Authentication.OAuth2
{
    public static class AccessTokenResponseExtensions
    {
        public static bool IsNullOrExpired(this AccessTokenResponse accessTokenResponse)
        {
            return accessTokenResponse == null || accessTokenResponse.IsExpired();
        }
    }
}