using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;
using Thirdweb.Redcode.Awaiting;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Thirdweb.Pay
{
    public partial class ThirdwebPay
    {
        /// <summary>
        /// Get a quote containing an onramp link for a fiat to crypto swap.
        /// </summary>
        /// <param name="buyWithFiatParams">Fiat onramp parameters <see cref="BuyWithFiatQuoteParams"/></param>
        /// <returns>Fiat quote object <see cref="BuyWithFiatQuoteResult"/></returns>
        public async Task<BuyWithFiatQuoteResult> GetBuyWithFiatQuote(BuyWithFiatQuoteParams buyWithFiatParams)
        {
            if (string.IsNullOrEmpty(_sdk.Session.Options.clientId))
            {
                throw new Exception("Client ID is not set. Please set it in the ThirdwebManager.");
            }

            var queryString = new Dictionary<string, string>
            {
                { "fromCurrencySymbol", buyWithFiatParams.FromCurrencySymbol },
                { "fromAmount", buyWithFiatParams.FromAmount },
                { "fromAmountUnits", buyWithFiatParams.FromAmountUnits },
                { "toAddress", buyWithFiatParams.ToAddress },
                { "toChainId", buyWithFiatParams.ToChainId },
                { "toTokenAddress", buyWithFiatParams.ToTokenAddress },
                { "toAmount", buyWithFiatParams.ToAmount },
                { "toAmountWei", buyWithFiatParams.ToAmountWei },
                { "maxSlippageBPS", buyWithFiatParams.MaxSlippageBPS?.ToString() }
            };

            var queryStringFormatted = string.Join("&", queryString.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{Constants.THIRDWEB_PAY_FIAT_QUOTE_ENDPOINT}?{queryStringFormatted}";
            url += buyWithFiatParams.IsTestMode ? "&isTestMode=true" : "&isTestMode=false";

            using var request = UnityWebRequest.Get(url);

            var headers = Utils.GetThirdwebHeaders(_sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

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
            var data = JsonConvert.DeserializeObject<GetFiatQuoteResponse>(content);
            return data.Result;
        }
    }
}
