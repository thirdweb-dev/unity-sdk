using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;

namespace Thirdweb.Pay
{
    public partial class ThirdwebPay
    {
        /// <summary>
        /// Get buy history, supports cursor and pagination.
        /// </summary>
        /// <param name="walletAddress">User wallet address to get buy history for</param>
        /// <param name="start">Offset for the records</param>
        /// <param name="count">Number of records to retrieve</param>
        /// <param name="cursor">Cursor for paging through the history</param>
        /// <param name="pageSize">Buy statuses to query for</param>
        /// <returns>Buy history object <see cref="BuyHistoryResult"/></returns>
        /// <exception cref="Exception"></exception>
        public async Task<BuyHistoryResult> GetBuyHistory(string walletAddress, int start, int count, string cursor = null, int? pageSize = null)
        {
            if (string.IsNullOrEmpty(_sdk.Session.Options.clientId))
            {
                throw new Exception("Client ID is not set. Please set it in the ThirdwebManager.");
            }

            var queryString = new Dictionary<string, string>
            {
                { "walletAddress", walletAddress },
                { "start", start.ToString() },
                { "count", count.ToString() },
                { "cursor", cursor },
                { "pageSize", pageSize?.ToString() }
            };

            var queryStringFormatted = string.Join("&", queryString.Where(kv => kv.Value != null).Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            var url = $"{Constants.THIRDWEB_PAY_HISTORY_ENDPOINT}?{queryStringFormatted}";

            using var request = UnityWebRequest.Get(url);

            var headers = Utils.GetThirdwebHeaders(_sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Delay(100);
            }

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
            ThirdwebDebug.Log($"GetBuyHistory response: {content}");
            var data = JsonConvert.DeserializeObject<BuyHistoryResponse>(content);
            return data.Result;
        }
    }
}
