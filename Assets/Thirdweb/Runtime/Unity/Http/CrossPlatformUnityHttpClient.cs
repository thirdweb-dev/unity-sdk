using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb.Unity
{
    public class CrossPlatformUnityHttpClient : IThirdwebHttpClient
    {
        IThirdwebHttpClient _httpClient;

        public CrossPlatformUnityHttpClient()
        {
#if UNITY_EDITOR
            _httpClient = new ThirdwebHttpClient();
#elif UNITY_WEBGL
            _httpClient = new Helpers.UnityThirdwebHttpClient();
#else
            _httpClient = new ThirdwebHttpClient();
#endif
        }

        public Dictionary<string, string> Headers => _httpClient.Headers;

        public void AddHeader(string key, string value)
        {
            _httpClient.AddHeader(key, value);
        }

        public void ClearHeaders()
        {
            _httpClient.ClearHeaders();
        }

        public Task<ThirdwebHttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return _httpClient.DeleteAsync(requestUri, cancellationToken);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public Task<ThirdwebHttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return _httpClient.GetAsync(requestUri, cancellationToken);
        }

        public Task<ThirdwebHttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            return _httpClient.PostAsync(requestUri, content, cancellationToken);
        }

        public Task<ThirdwebHttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        {
            return _httpClient.PutAsync(requestUri, content, cancellationToken);
        }

        public void RemoveHeader(string key)
        {
            _httpClient.RemoveHeader(key);
        }

        public void SetHeaders(Dictionary<string, string> headers)
        {
            _httpClient.SetHeaders(headers);
        }
    }
}
