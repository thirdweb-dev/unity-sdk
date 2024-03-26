using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Thirdweb.Pay
{
    public static partial class ThirdwebPay
    {
        public static async Task<SwapStatusResult> GetSwapStatus(string transactionHash)
        {
            if (string.IsNullOrEmpty(transactionHash))
            {
                throw new ArgumentNullException(nameof(transactionHash));
            }

            var queryString = new Dictionary<string, string> { { "transactionHash", transactionHash } };

            var queryStringFormatted = string.Join("&", queryString.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{Constants.THIRDWEB_PAY_STATUS_ENDPOINT}?{queryStringFormatted}";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("x-sdk-name", "UnitySDK");
            httpClient.DefaultRequestHeaders.Add("x-sdk-os", Utils.GetRuntimePlatform());
            httpClient.DefaultRequestHeaders.Add("x-sdk-platform", "unity");
            httpClient.DefaultRequestHeaders.Add("x-sdk-version", ThirdwebSDK.version);
            httpClient.DefaultRequestHeaders.Add("x-client-id", ThirdwebManager.Instance.SDK.Session.Options.clientId);
            httpClient.DefaultRequestHeaders.Add("x-bundle-id", ThirdwebManager.Instance.SDK.Session.Options.bundleId);

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                ErrorResponse error;
                try
                {
                    error = JsonConvert.DeserializeObject<ErrorResponse>(await response.Content.ReadAsStringAsync());
                }
                catch
                {
                    error = new ErrorResponse
                    {
                        Error = new ErrorDetails
                        {
                            Message = "Unknown error",
                            Reason = "Unknown",
                            Code = "Unknown",
                            Stack = "Unknown",
                            StatusCode = (int)response.StatusCode
                        }
                    };
                }

                throw new Exception(
                    $"HTTP error! Code: {error.Error.Code} Message: {error.Error.Message} Reason: {error.Error.Reason} StatusCode: {error.Error.StatusCode} Stack: {error.Error.Stack}"
                );
            }

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<SwapStatusResponse>(content);
            return data.Result;
        }
    }
}
