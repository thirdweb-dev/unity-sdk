using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Cdm.Authentication.Clients;
using Cdm.Authentication.OAuth2;
using Cdm.Authentication.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Cdm.Authentication.Mocking
{
    public class MockServer : MonoBehaviour
    {
        private const string UsernameKey = "username";
        private const string LoginPath = "/oauth/login";
        
        [SerializeField]
        [Tooltip("Don't forget to add '/' to the end.")]
        private string _serverUrl = "http://localhost:8001/";

        public string serverUrl
        {
            get => _serverUrl;
            set => _serverUrl = value;
        }

        [Header("Authorization Code Response")]
        [SerializeField]
        private bool _authorizationCodeErrorEnabled;

        public bool authorizationCodeErrorEnabled
        {
            get => _authorizationCodeErrorEnabled;
            set => _authorizationCodeErrorEnabled = value;
        }
        
        [SerializeField]
        private AuthorizationCodeRequestErrorCode _authorizationCodeError;
        
        public AuthorizationCodeRequestErrorCode authorizationCodeError
        {
            get => _authorizationCodeError;
            set => _authorizationCodeError = value;
        }
        
        [Space]
        [SerializeField]
        private List<MockServerAccessTokenResponse> _accessTokenResponses = new List<MockServerAccessTokenResponse>();

        public IList<MockServerAccessTokenResponse> accessTokenResponses => _accessTokenResponses;

        private const string LoginPage = 
            "<!DOCTYPE>" +
            "<html>" +
            "  <head>" +
            "    <title>Mock Auth Server</title>" +
            "  </head>" +
            "  <body>" +
            "    <form method=\"post\" action=\"login\">" +
            "      <label for=\"" + UsernameKey + "\">Username:</label> " +
            "      <input type=\"text\" id=\"username\" name=\"username\"><br><br> " +
            "      <input type=\"submit\" value=\"Login\">" +
            "    </form>" +
            "  </body>" +
            "</html>";
        
        private HttpListener _httpListener;
        private Task _listenTask;
        private bool _runServer = false;
        
        private string _authorizationCode;
        private AuthorizationCodeRequest _authorizationCodeRequest;
        private MockServerAccessTokenResponse _accessTokenResponse;
        
        private void Start()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(serverUrl);
            _httpListener.Start();
            
            _listenTask = HandleIncomingConnectionsAsync();
            Debug.Log($"[{nameof(MockServer)}] Listening for connections on '{serverUrl}'");
        }

        private void OnEnable()
        {
            _runServer = true;
        }

        private void OnDisable()
        {
            _runServer = false;
        }

        private void OnDestroy()
        {
            //_listenTask.GetAwaiter().GetResult();
            _httpListener?.Close();
        }

        private async Task HandleIncomingConnectionsAsync()
        {
            while (_runServer)
            {
                var ctx = await _httpListener.GetContextAsync();
                var request = ctx.Request;
                var response = ctx.Response;
                
                Debug.Log($"[{nameof(MockServer)}] [{request.HttpMethod}] {request.Url.AbsolutePath}");
                
                if (request.HttpMethod == "GET" && request.Url.AbsolutePath == MockServerAuth.AuthorizationPath)
                {
                    await HandleAuthorizationRequest(request, response);
                } 
                else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == LoginPath)
                {
                    await HandleLoginRequest(request, response);
                }
                else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == MockServerAuth.TokenPath)
                {
                    await HandleTokenRequest(request, response);
                }
                else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/favicon.ico")
                {
                    // Ignore.
                }
                else
                {
                    Debug.LogError($"[{nameof(MockServer)}] Invalid request.");
                }
            }
        }

        private async Task HandleAuthorizationRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            _authorizationCode = "";
            _authorizationCodeRequest = null;
            _accessTokenResponse = null;
            
            var query = HttpUtility.ParseQueryString(request.Url.Query);
            if (!JsonHelper.TryGetFromNameValueCollection<AuthorizationCodeRequest>(
                    query, out var authorizationRequest))
            {
                Debug.LogError(
                    $"[{nameof(MockServer)}] No authorization code request data was sent with the query: '{request.Url.Query}'");
                return;
            }

            _authorizationCodeRequest = authorizationRequest;
            
            if (authorizationCodeErrorEnabled)
            {
                var errorResponse = new AuthorizationCodeRequestError()
                {
                    code = authorizationCodeError,
                    state = authorizationRequest.state
                };

                var responseQuery = JsonHelper.ToDictionary(errorResponse);
                var responseUrl = UrlBuilder.New(authorizationRequest.redirectUri)
                    .SetQueryParameters(responseQuery).ToString();
            
                response.Redirect(responseUrl);
                response.Close();
            }
            else
            {
                var data = Encoding.UTF8.GetBytes(LoginPage);
                response.ContentType = "text/html";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = data.LongLength;

                await response.OutputStream.WriteAsync(data, 0, data.Length);
                response.Close();
            }
        }

        private async Task HandleLoginRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (!request.HasEntityBody)
            {
                Debug.LogError($"[{nameof(MockServer)}] No body data was sent with the request.");
                return;
            }
            
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var content = await reader.ReadToEndAsync();
            var query = HttpUtility.ParseQueryString(content);
            
            Dictionary<string, string> responseQuery;
            
            var username = query[UsernameKey];
            _accessTokenResponse = _accessTokenResponses.FirstOrDefault(x => x.username == username);
            if (_accessTokenResponse != null)
            {
                _authorizationCode = Guid.NewGuid().ToString("D");
                
                var authorizationResponse = new AuthorizationCodeResponse()
                {
                    code = _authorizationCode,
                    state = _authorizationCodeRequest.state
                };
                
                responseQuery = JsonHelper.ToDictionary(authorizationResponse);
            }
            else
            {
                Debug.Log($"[{nameof(MockServer)}] User could not found: '{username}'");
                
                var errorResponse = new AuthorizationCodeRequestError()
                {
                    code = AuthorizationCodeRequestErrorCode.ServerError,
                    state = _authorizationCodeRequest.state
                };

                responseQuery = JsonHelper.ToDictionary(errorResponse);
            }
            
            var responseUrl = UrlBuilder.New(_authorizationCodeRequest.redirectUri)
                .SetQueryParameters(responseQuery).ToString();
            
            response.Redirect(responseUrl);
            response.Close();
        }

        private async Task HandleTokenRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (!request.HasEntityBody)
            {
                Debug.LogError($"[{nameof(MockServer)}] No body data was sent with the request.");
                return;
            }
            
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var content = await reader.ReadToEndAsync();
            var query = HttpUtility.ParseQueryString(content);
            
            string responseContent;
            HttpStatusCode responseStatus;
            
            if (JsonHelper.TryGetFromNameValueCollection<AccessTokenRequest>(query, out var accessTokenRequest))
            {
                if (_accessTokenResponse.accessTokenErrorEnabled)
                {
                    var tokenResponse = new AccessTokenRequestError()
                    {
                        code = _accessTokenResponse.accessTokenError
                    };
                    
                    responseStatus = HttpStatusCode.BadRequest;
                    responseContent = JsonConvert.SerializeObject(tokenResponse);   
                }
                else
                {
                    var tokenResponse = new AccessTokenResponse()
                    {
                        accessToken = Guid.NewGuid().ToString("D"),
                        expiresIn = 3600,
                        refreshToken = Guid.NewGuid().ToString("D"),
                        tokenType = "Bearer",
                        scope = _authorizationCodeRequest.scope
                    };
                    responseStatus = HttpStatusCode.OK;
                    responseContent = JsonConvert.SerializeObject(tokenResponse);
                }
            }
            else if (JsonHelper.TryGetFromNameValueCollection<RefreshTokenRequest>(query, out var refreshTokenRequest))
            {
                if (_accessTokenResponse.accessTokenErrorOnRefreshEnabled)
                {
                    var tokenResponse = new AccessTokenRequestError()
                    {
                        code = _accessTokenResponse.accessTokenErrorOnRefresh
                    };
                    
                    responseStatus = HttpStatusCode.BadRequest;
                    responseContent = JsonConvert.SerializeObject(tokenResponse);   
                }
                else
                {
                    var tokenResponse = new AccessTokenResponse()
                    {
                        accessToken = Guid.NewGuid().ToString("D"),
                        expiresIn = 3600,
                        tokenType = "Bearer",
                        scope = _authorizationCodeRequest.scope
                    };
                    
                    responseStatus = HttpStatusCode.OK;
                    responseContent = JsonConvert.SerializeObject(tokenResponse);   
                }
            }
            else
            {
                var error = new AccessTokenRequestError()
                {
                    code = AccessTokenRequestErrorCode.UnsupportedGrantType,
                    description = 
                        "Unsupported grant type. Supported grant types are: 'authorization_code' and 'refresh_token'"
                };

                responseStatus = HttpStatusCode.BadRequest;
                responseContent = JsonConvert.SerializeObject(error);
            }

            var data = Encoding.UTF8.GetBytes(responseContent);
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = data.LongLength;
            response.StatusCode = (int) responseStatus;
            
            await response.OutputStream.WriteAsync(data, 0, data.Length);
            response.Close();
        }
    }
}