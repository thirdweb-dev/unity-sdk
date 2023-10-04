using System;
using Cdm.Authentication.OAuth2;
using UnityEngine;

namespace Cdm.Authentication.Mocking
{
    [Serializable]
    public class MockServerAccessTokenResponse
    {
        public string username;
        
        [Header("Exchanging authorization code with access token phase.")]
        public bool accessTokenErrorEnabled;
        public AccessTokenRequestErrorCode accessTokenError;
        
        [Header("Refreshing access token with the refresh token phase.")]
        public bool accessTokenErrorOnRefreshEnabled;
        public AccessTokenRequestErrorCode accessTokenErrorOnRefresh;
    }
}