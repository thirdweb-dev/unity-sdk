using System.Threading.Tasks;
using System.Numerics;
using Nethereum.Contracts;
using UnityEngine;
using Nethereum.RPC.Eth.DTOs;
using MinimalForwarder = Thirdweb.Contracts.Forwarder.ContractDefinition;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Thirdweb.Contracts.Forwarder.ContractDefinition;

namespace Thirdweb
{
    public static class TransactionManager
    {
        private static bool warned;

        public static async Task<TWResult> ThirdwebRead<TWFunction, TWResult>(string contractAddress, TWFunction functionMessage)
            where TWFunction : FunctionMessage, new()
        {
            try
            {
                functionMessage.FromAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            }
            catch (System.Exception)
            {
                if (!warned)
                {
                    Debug.LogWarning("Sending accountless query, make sure a wallet is connected if this was not intended.");
                    warned = true;
                }
            }
            var queryHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetContractQueryHandler<TWFunction>();
            return await queryHandler.QueryAsync<TWResult>(contractAddress, functionMessage);
        }

        public static async Task<TransactionResult> ThirdwebWrite<TWFunction>(string contractAddress, TWFunction functionMessage, BigInteger? weiValue = null)
            where TWFunction : FunctionMessage, new()
        {
            var receipt = await ThirdwebWriteRawResult(contractAddress, functionMessage, weiValue);
            return receipt.ToTransactionResult();
        }

        public static async Task<TransactionReceipt> ThirdwebWriteRawResult<TWFunction>(string contractAddress, TWFunction functionMessage, BigInteger? weiValue = null)
            where TWFunction : FunctionMessage, new()
        {
            functionMessage.FromAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            functionMessage.AmountToSend = weiValue ?? 0;

            var transactionHandler = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetContractTransactionHandler<TWFunction>();
            var gas = await transactionHandler.EstimateGasAsync(contractAddress, functionMessage);
            functionMessage.Gas = gas.Value < 100000 ? 100000 : gas;

            if (
                ThirdwebManager.Instance.SDK.options.gasless != null
                && ThirdwebManager.Instance.SDK.options.gasless.Value.openzeppelin != null
                && !string.IsNullOrEmpty(ThirdwebManager.Instance.SDK.options.gasless.Value.openzeppelin.Value.relayerUrl)
                && !string.IsNullOrEmpty(ThirdwebManager.Instance.SDK.options.gasless.Value.openzeppelin.Value.relayerForwarderAddress)
            )
            {
                string relayerUrl = ThirdwebManager.Instance.SDK.options.gasless.Value.openzeppelin?.relayerUrl;
                string relayerForwarderAddress = ThirdwebManager.Instance.SDK.options.gasless.Value.openzeppelin?.relayerForwarderAddress;

                functionMessage.Nonce = (
                    await ThirdwebRead<MinimalForwarder.GetNonceFunction, MinimalForwarder.GetNonceOutputDTO>(
                        relayerForwarderAddress,
                        new MinimalForwarder.GetNonceFunction() { From = functionMessage.FromAddress }
                    )
                ).ReturnValue1;

                var request = new MinimalForwarder.ForwardRequest()
                {
                    From = functionMessage.FromAddress,
                    To = contractAddress,
                    Value = functionMessage.AmountToSend,
                    Gas = functionMessage.Gas.Value,
                    Nonce = functionMessage.Nonce.Value,
                    Data = functionMessage.GetCallData().ByteArrayToHexString()
                };

                var signature = await EIP712.GenerateSignature_MinimalForwarder("GSNv2 Forwarder", "0.0.1", ThirdwebManager.Instance.SDK.nativeSession.lastChainId, relayerForwarderAddress, request);

                var postData = new RelayerRequest(request, signature, relayerForwarderAddress);

                string txHash = null;

                Debug.Log(JsonConvert.SerializeObject(postData));

                using (UnityWebRequest req = UnityWebRequest.Post(relayerUrl, ""))
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postData));
                    req.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                    req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                    req.SetRequestHeader("Content-Type", "application/json");
                    await req.SendWebRequest();
                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        req.Dispose();
                        throw new UnityException(
                            $"Forward Request Failed!\nError: {req.downloadHandler.text}\nRelayer URL: {relayerUrl}\nRelayer Forwarder Address: {relayerForwarderAddress}\nRequest: {request}\nSignature: {signature}\nPost Data: {postData}"
                        );
                    }
                    else
                    {
                        txHash = req.downloadHandler.text;
                    }
                }

                return await ThirdwebManager.Instance.SDK.nativeSession.web3.TransactionReceiptPolling.PollForReceiptAsync(txHash);
            }
            else
            {
                return await transactionHandler.SendRequestAndWaitForReceiptAsync(contractAddress, functionMessage);
            }
        }

        [System.Serializable]
        public struct RelayerRequest
        {
            public MinimalForwarder.ForwardRequest request;
            public string signature;
            public string forwarderAddress;
            public string type;

            public RelayerRequest(ForwardRequest request, string signature, string forwarderAddress)
            {
                this.request = request;
                this.signature = signature;
                this.forwarderAddress = forwarderAddress;
                this.type = "forward";
            }
        }
    }
}
