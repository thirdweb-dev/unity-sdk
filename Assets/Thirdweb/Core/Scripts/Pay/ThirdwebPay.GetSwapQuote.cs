using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;
using Thirdweb.Redcode.Awaiting;
using System.Threading.Tasks;

namespace Thirdweb.Pay
{
    public static partial class ThirdwebPay
    {
        public static async Task<SwapQuoteResult> GetSwapQuote(SwapQuoteParams swapParams)
        {
            var queryString = new Dictionary<string, string>
            {
                { "fromAddress", swapParams.FromAddress },
                { "fromChainId", swapParams.FromChainId?.ToString() },
                { "fromTokenAddress", swapParams.FromTokenAddress },
                { "fromAmount", swapParams.FromAmount },
                { "fromAmountWei", swapParams.FromAmountWei },
                { "toAddress", swapParams.ToAddress },
                { "toChainId", swapParams.ToChainId?.ToString() },
                { "toTokenAddress", swapParams.ToTokenAddress },
                { "toAmount", swapParams.ToAmount },
                { "toAmountWei", swapParams.ToAmountWei },
                { "maxSlippageBPS", swapParams.MaxSlippageBPS?.ToString() }
            };

            var queryStringFormatted = string.Join("&", queryString.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{Constants.THIRDWEB_PAY_QUOTE_ENDPOINT}?{queryStringFormatted}";

            using var request = UnityWebRequest.Get(url);

            request.SetRequestHeader("x-sdk-name", "UnitySDK");
            request.SetRequestHeader("x-sdk-os", Utils.GetRuntimePlatform());
            request.SetRequestHeader("x-sdk-platform", "unity");
            request.SetRequestHeader("x-sdk-version", ThirdwebSDK.version);
            request.SetRequestHeader("x-client-id", Utils.GetClientId());
            request.SetRequestHeader("x-bundle-id", Utils.GetBundleId());

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
            var data = JsonConvert.DeserializeObject<GetSwapQuoteResponse>(content);
            return data.Result;
        }
    }
}
