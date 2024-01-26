using System;
using System.Net.Http;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client.RpcMessages;
using Newtonsoft.Json;

namespace Thirdweb.AccountAbstraction
{
    public static class BundlerClient
    {
        // Bundler requests

        public static async Task<EthGetUserOperationByHasResponse> EthGetUserOperationByHash(string bundlerUrl, string apiKey, object requestId, string userOpHash)
        {
            var response = await BundlerRequest(bundlerUrl, apiKey, requestId, "eth_getUserOperationByHash", userOpHash);
            return JsonConvert.DeserializeObject<EthGetUserOperationByHasResponse>(response.Result.ToString());
        }

        public static async Task<string> EthSendUserOperation(string bundlerUrl, string apiKey, object requestId, UserOperationHexified userOp, string entryPoint)
        {
            var response = await BundlerRequest(bundlerUrl, apiKey, requestId, "eth_sendUserOperation", userOp, entryPoint);
            return response.Result.ToString();
        }

        public static async Task<EthEstimateUserOperationGasResponse> EthEstimateUserOperationGas(string bundlerUrl, string apiKey, object requestId, UserOperationHexified userOp, string entryPoint)
        {
            var response = await BundlerRequest(bundlerUrl, apiKey, requestId, "eth_estimateUserOperationGas", userOp, entryPoint);
            return JsonConvert.DeserializeObject<EthEstimateUserOperationGasResponse>(response.Result.ToString());
        }

        // Paymaster requests

        public static async Task<PMSponsorOperationResponse> PMSponsorUserOperation(string paymasterUrl, string apiKey, object requestId, UserOperationHexified userOp, string entryPoint)
        {
            var response = await BundlerRequest(paymasterUrl, apiKey, requestId, "pm_sponsorUserOperation", userOp, new EntryPointWrapper() { entryPoint = entryPoint });
            try
            {
                return JsonConvert.DeserializeObject<PMSponsorOperationResponse>(response.Result.ToString());
            }
            catch
            {
                return new PMSponsorOperationResponse() { paymasterAndData = response.Result.ToString() };
            }
        }

        // Request

        private static async Task<RpcResponseMessage> BundlerRequest(string url, string apiKey, object requestId, string method, params object[] args)
        {
            using HttpClient client = new HttpClient();
            ThirdwebDebug.Log($"Bundler Request: {method}({JsonConvert.SerializeObject(args)}");
            var requestMessage = new RpcRequestMessage(requestId, method, args);
            string requestMessageJson = JsonConvert.SerializeObject(requestMessage);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent(requestMessageJson, System.Text.Encoding.UTF8, "application/json") };
            if (new Uri(url).Host.EndsWith(".thirdweb.com"))
            {
                httpRequestMessage.Headers.Add("x-sdk-name", "UnitySDK");
                httpRequestMessage.Headers.Add("x-sdk-os", Utils.GetRuntimePlatform());
                httpRequestMessage.Headers.Add("x-sdk-platform", "unity");
                httpRequestMessage.Headers.Add("x-sdk-version", ThirdwebSDK.version);
                httpRequestMessage.Headers.Add("x-client-id", ThirdwebManager.Instance.SDK.session.Options.clientId);
                if (!Utils.IsWebGLBuild())
                    httpRequestMessage.Headers.Add("x-bundle-id", ThirdwebManager.Instance.SDK.session.Options.bundleId);
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
