using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;
using Thirdweb.Redcode.Awaiting;
using System.Threading.Tasks;

namespace Thirdweb.Pay
{
    public partial class ThirdwebPay
    {
        /// <summary>
        /// Get a quote containing a TransactionRequest for swapping any token pair.
        /// </summary>
        /// <param name="buyWithCryptoParams">Swap parameters <see cref="BuyWithCryptoQuoteParams"/></param>
        /// <returns>Swap quote object <see cref="BuyWithCryptoQuoteResult"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<BuyWithCryptoQuoteResult> GetBuyWithCryptoQuote(BuyWithCryptoQuoteParams buyWithCryptoParams)
        {
            if (string.IsNullOrEmpty(_sdk.Session.Options.clientId))
            {
                throw new Exception("Client ID is not set. Please set it in the ThirdwebManager.");
            }

            var queryString = new Dictionary<string, string>
            {
                { "fromAddress", buyWithCryptoParams.FromAddress },
                { "fromChainId", buyWithCryptoParams.FromChainId?.ToString() },
                { "fromTokenAddress", buyWithCryptoParams.FromTokenAddress },
                { "fromAmount", buyWithCryptoParams.FromAmount },
                { "fromAmountWei", buyWithCryptoParams.FromAmountWei },
                { "toChainId", buyWithCryptoParams.ToChainId?.ToString() },
                { "toTokenAddress", buyWithCryptoParams.ToTokenAddress },
                { "toAmount", buyWithCryptoParams.ToAmount },
                { "toAmountWei", buyWithCryptoParams.ToAmountWei },
                { "maxSlippageBPS", buyWithCryptoParams.MaxSlippageBPS?.ToString() },
                { "intentId", buyWithCryptoParams.IntentId }
            };

            var queryStringFormatted = string.Join("&", queryString.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{Constants.THIRDWEB_PAY_CRYPTO_QUOTE_ENDPOINT}?{queryStringFormatted}";

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
            var data = JsonConvert.DeserializeObject<GetSwapQuoteResponse>(content);
            return data.Result;
        }
    }
}
