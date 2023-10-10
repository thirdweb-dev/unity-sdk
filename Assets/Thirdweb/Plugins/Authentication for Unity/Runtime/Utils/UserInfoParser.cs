using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Cdm.Authentication.Utils
{
    public static class UserInfoParser
    {
        public static async Task<IUserInfo> GetUserInfoAsync<T>(HttpClient httpClient, string url,
            AuthenticationHeaderValue authenticationHeader, CancellationToken cancellationToken = default)
            where T : IUserInfo
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = authenticationHeader;

#if UNITY_EDITOR
            Debug.Log($"{request}");
#endif

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userInfo = JsonConvert.DeserializeObject<T>(content);

#if UNITY_EDITOR
                Debug.Log(content);
#endif

                return userInfo;
            }

            throw new Exception("User info could not parsed.");
        }
    }
}