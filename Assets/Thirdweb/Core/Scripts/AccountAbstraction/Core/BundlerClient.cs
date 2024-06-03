using System;
using System.Net.Http;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client.RpcMessages;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json;

namespace Thirdweb.AccountAbstraction
{
    public static class BundlerClient
    {
        // Bundler requests

        public static async Task<EthGetUserOperationByHashResponse> EthGetUserOperationByHash(string bundlerUrl, string apiKey, string bundleId, object requestId, string userOpHash)
        {
            var response = await BundlerRequest(bundlerUrl, apiKey, bundleId, requestId, "eth_getUserOperationByHash", userOpHash);
            return JsonConvert.DeserializeObject<EthGetUserOperationByHashResponse>(response.Result.ToString());
        }

        public static async Task<EthGetUserOperationReceiptResponse> EthGetUserOperationReceipt(string bundlerUrl, string apiKey, string bundleId, object requestId, string userOpHash)
        {
            var response = await BundlerRequest(bundlerUrl, apiKey, bundleId, requestId, "eth_getUserOperationReceipt", userOpHash);
            return JsonConvert.DeserializeObject<EthGetUserOperationReceiptResponse>(response.Result.ToString());
        }

        public static async Task<string> EthSendUserOperation(string bundlerUrl, string apiKey, string bundleId, object requestId, UserOperationHexified userOp, string entryPoint)
        {
            var response = await BundlerRequest(bundlerUrl, apiKey, bundleId, requestId, "eth_sendUserOperation", userOp, entryPoint);
            return response.Result.ToString();
        }

        public static async Task<EthEstimateUserOperationGasResponse> EthEstimateUserOperationGas(
            string bundlerUrl,
            string apiKey,
            string bundleId,
            object requestId,
            UserOperationHexified userOp,
            string entryPoint
        )
        {
            var response = await BundlerRequest(bundlerUrl, apiKey, bundleId, requestId, "eth_estimateUserOperationGas", userOp, entryPoint);
            return JsonConvert.DeserializeObject<EthEstimateUserOperationGasResponse>(response.Result.ToString());
        }

        public static async Task<ThirdwebGetUserOperationGasPriceResponse> ThirdwebGetUserOperationGasPrice(string bundlerUrl, string apiKey, string bundleId, object requestId)
        {
            var response = await BundlerRequest(bundlerUrl, apiKey, bundleId, requestId, "thirdweb_getUserOperationGasPrice");
            return JsonConvert.DeserializeObject<ThirdwebGetUserOperationGasPriceResponse>(response.Result.ToString());
        }

        // Paymaster requests

        public static async Task<PMSponsorOperationResponse> PMSponsorUserOperation(
            string paymasterUrl,
            string apiKey,
            string bundleId,
            object requestId,
            UserOperationHexified userOp,
            string entryPoint
        )
        {
            var response = await BundlerRequest(paymasterUrl, apiKey, bundleId, requestId, "pm_sponsorUserOperation", userOp, new EntryPointWrapper() { entryPoint = entryPoint });
            try
            {
                return JsonConvert.DeserializeObject<PMSponsorOperationResponse>(response.Result.ToString());
            }
            catch
            {
                return new PMSponsorOperationResponse() { paymasterAndData = response.Result.ToString() };
            }
        }

        public static async Task<ZkPaymasterDataResponse> ZkPaymasterData(string paymasterUrl, string apiKey, string bundleId, object requestId, TransactionInput txInput)
        {
            var response = await BundlerRequest(paymasterUrl, apiKey, bundleId, requestId, "zk_paymasterData", txInput);
            try
            {
                return JsonConvert.DeserializeObject<ZkPaymasterDataResponse>(response.Result.ToString());
            }
            catch
            {
                return new ZkPaymasterDataResponse() { paymaster = null, paymasterInput = null };
            }
        }

        public static async Task<ZkBroadcastTransactionResponse> ZkBroadcastTransaction(string paymasterUrl, string apiKey, string bundleId, object requestId, object txInput)
        {
            var response = await BundlerRequest(paymasterUrl, apiKey, bundleId, requestId, "zk_broadcastTransaction", txInput);
            return JsonConvert.DeserializeObject<ZkBroadcastTransactionResponse>(response.Result.ToString());
        }

        // Request

        private static async Task<RpcResponseMessage> BundlerRequest(string url, string apiKey, string bundleId, object requestId, string method, params object[] args)
        {
            using HttpClient client = new HttpClient();
            ThirdwebDebug.Log($"Bundler Request: {method}({JsonConvert.SerializeObject(args)}");
            var requestMessage = new RpcRequestMessage(requestId, method, args);
            string requestMessageJson = JsonConvert.SerializeObject(requestMessage);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent(requestMessageJson, System.Text.Encoding.UTF8, "application/json") };
            if (Utils.IsThirdwebRequest(url))
            {
                var headers = Utils.GetThirdwebHeaders(apiKey, bundleId);
                foreach (var header in headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            var httpResponse = await client.SendAsync(httpRequestMessage);

            if (!httpResponse.IsSuccessStatusCode)
                throw new Exception($"Bundler Request Failed. Error: {httpResponse.StatusCode} - {httpResponse.ReasonPhrase} - {await httpResponse.Content.ReadAsStringAsync()}");

            var httpResponseJson = await httpResponse.Content.ReadAsStringAsync();
            ThirdwebDebug.Log($"Bundler Response: {httpResponseJson}");

            var response = JsonConvert.DeserializeObject<RpcResponseMessage>(httpResponseJson);
            if (response.Error != null)
                throw new Exception($"Bundler Request Failed. Error: {response.Error.Code} - {response.Error.Message} - {response.Error.Data}");
            return response;
        }
    }
}
