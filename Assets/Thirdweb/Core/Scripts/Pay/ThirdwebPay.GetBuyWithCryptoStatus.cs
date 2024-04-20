using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Thirdweb.Redcode.Awaiting;

namespace Thirdweb.Pay
{
    public static partial class ThirdwebPay
    {
        /// <summary>
        /// Get swap status for a transaction hash.
        /// </summary>
        /// <param name="transactionHash">Transaction hash to get swap status for</param>
        /// <returns>Swap status object <see cref="BuyWithCryptoStatusResult"/></returns>
        public static async Task<BuyWithCryptoStatusResult> GetBuyWithCryptoStatus(string transactionHash)
        {
            if (string.IsNullOrEmpty(Utils.GetClientId()))
            {
                throw new Exception("Client ID is not set. Please set it in the ThirdwebManager.");
            }

            if (string.IsNullOrEmpty(transactionHash))
            {
                throw new ArgumentNullException(nameof(transactionHash));
            }

            var queryString = new Dictionary<string, string> { { "transactionHash", transactionHash } };

            var queryStringFormatted = string.Join("&", queryString.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{Constants.THIRDWEB_PAY_CRYPTO_STATUS_ENDPOINT}?{queryStringFormatted}";

            using var request = UnityWebRequest.Get(url);

            request.SetRequestHeader("x-sdk-name", "UnitySDK");
            request.SetRequestHeader("x-sdk-os", Utils.GetRuntimePlatform());
            request.SetRequestHeader("x-sdk-platform", "unity");
            request.SetRequestHeader("x-sdk-version", ThirdwebSDK.version);
            request.SetRequestHeader("x-client-id", ThirdwebManager.Instance.SDK.Session.Options.clientId);
            if (!Utils.IsWebGLBuild())
                request.SetRequestHeader("x-bundle-id", ThirdwebManager.Instance.SDK.Session.Options.bundleId);

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                ErrorResponse error;
                try
                {
                    error = JsonConvert.DeserializeObject<ErrorResponse>(request.downloadHandler.text);
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
                            StatusCode = (int)request.responseCode
                        }
                    };
                }

                throw new Exception(
                    $"HTTP error! Code: {error.Error.Code} Message: {error.Error.Message} Reason: {error.Error.Reason} StatusCode: {error.Error.StatusCode} Stack: {error.Error.Stack}"
                );
            }

            var content = request.downloadHandler.text;
            var data = JsonConvert.DeserializeObject<SwapStatusResponse>(content);
            return data.Result;
        }
    }
}
