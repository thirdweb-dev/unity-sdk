using System.Threading.Tasks;
using System.Numerics;
using Nethereum.Contracts;
using UnityEngine;
using Nethereum.RPC.Eth.DTOs;
using MinimalForwarder = Thirdweb.Contracts.Forwarder.ContractDefinition;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Thirdweb.Contracts.Forwarder.ContractDefinition;
using Nethereum.RPC.Eth.Transactions;
using Thirdweb.Redcode.Awaiting;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.QueryHandlers.MultiCall;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0618

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
                    ThirdwebDebug.Log("Sending accountless query, make sure a wallet is connected if this was not intended.");
                    warned = true;
                }
            }

            var queryHandler = Utils.GetWeb3().Eth.GetContractQueryHandler<TWFunction>();
            return await queryHandler.QueryAsync<TWResult>(contractAddress, functionMessage);
        }

        public static async Task<TWResult[]> ThirdwebMulticallRead<TWFunction, TWResult>(string contractAddress, TWFunction[] functionMessages)
            where TWFunction : FunctionMessage, new()
            where TWResult : IFunctionOutputDTO, new()
        {
            MultiQueryHandler multiqueryHandler = Utils.GetWeb3().Eth.GetMultiQueryHandler();
            var calls = new List<MulticallInputOutput<TWFunction, TWResult>>();
            for (int i = 0; i < functionMessages.Length; i++)
            {
                calls.Add(new MulticallInputOutput<TWFunction, TWResult>(functionMessages[i], contractAddress));
            }
            var results = await multiqueryHandler.MultiCallAsync(MultiQueryHandler.DEFAULT_CALLS_PER_REQUEST, calls.ToArray()).ConfigureAwait(false);
            return calls.Select(x => x.Output).ToArray();
        }

        public static async Task<TransactionResult> ThirdwebWrite<TWFunction>(string contractAddress, TWFunction functionMessage, BigInteger? weiValue = null, BigInteger? gasOverride = null)
            where TWFunction : FunctionMessage, new()
        {
            var receipt = await ThirdwebWriteRawResult(contractAddress, functionMessage, weiValue, gasOverride);
            return receipt.ToTransactionResult();
        }

        public static async Task<TransactionReceipt> ThirdwebWriteRawResult<TWFunction>(string contractAddress, TWFunction functionMessage, BigInteger? weiValue = null, BigInteger? gasOverride = null)
            where TWFunction : FunctionMessage, new()
        {
            string txHash = null;

            functionMessage.FromAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
            functionMessage.AmountToSend = weiValue ?? 0;

            if (gasOverride.HasValue)
            {
                functionMessage.Gas = gasOverride.Value;
            }
            else
            {
                try
                {
                    var gasEstimator = Utils.GetWeb3().Eth.GetContractTransactionHandler<TWFunction>();
                    var gas = await gasEstimator.EstimateGasAsync(contractAddress, functionMessage);
                    functionMessage.Gas = gas.Value < 100000 ? 100000 : gas.Value;
                }
                catch (System.InvalidOperationException e)
                {
                    ThirdwebDebug.LogWarning($"Failed to estimate gas for transaction, proceeding with 100k gas: {e}");
                    functionMessage.Gas = 100000;
                }
            }

            bool isGasless = ThirdwebManager.Instance.SDK.session.Options.gasless.HasValue && ThirdwebManager.Instance.SDK.session.Options.gasless.Value.openzeppelin.HasValue;

            if (!isGasless)
            {
                if (
                    ThirdwebManager.Instance.SDK.session.ActiveWallet.GetSignerProvider() == WalletProvider.LocalWallet
                    && ThirdwebManager.Instance.SDK.session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet
                )
                {
                    var web3 = await ThirdwebManager.Instance.SDK.session.ActiveWallet.GetSignerWeb3();
                    var transactionHandler = web3.Eth.GetContractTransactionHandler<TWFunction>();
                    txHash = await transactionHandler.SendRequestAsync(contractAddress, functionMessage);
                }
                else
                {
                    var transaction = new EthSendTransaction(ThirdwebManager.Instance.SDK.session.Web3.Client);
                    var transactionInput = functionMessage.CreateTransactionInput(contractAddress);
                    txHash = await transaction.SendRequestAsync(transactionInput);
                }
            }
            else
            {
                string relayerUrl = ThirdwebManager.Instance.SDK.session.Options.gasless.Value.openzeppelin?.relayerUrl;
                string forwarderAddress = ThirdwebManager.Instance.SDK.session.Options.gasless.Value.openzeppelin?.relayerForwarderAddress;
                string forwarderDomain = ThirdwebManager.Instance.SDK.session.Options.gasless.Value.openzeppelin?.domainName;
                string forwarderVersion = ThirdwebManager.Instance.SDK.session.Options.gasless.Value.openzeppelin?.domainVersion;

                functionMessage.Nonce = (
                    await ThirdwebRead<MinimalForwarder.GetNonceFunction, MinimalForwarder.GetNonceOutputDTO>(
                        forwarderAddress,
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

                var signature = await EIP712.GenerateSignature_MinimalForwarder(forwarderDomain, forwarderVersion, ThirdwebManager.Instance.SDK.session.ChainId, forwarderAddress, request);

                var postData = new RelayerRequest(request, signature, forwarderAddress);

                using UnityWebRequest req = UnityWebRequest.Post(relayerUrl, "");
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(postData));
                req.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                await req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    throw new UnityException(
                        $"Forward Request Failed!\nError: {req.downloadHandler.text}\nRelayer URL: {relayerUrl}\nRelayer Forwarder Address: {forwarderAddress}\nRequest: {request}\nSignature: {signature}\nPost Data: {postData}"
                    );
                }
                else
                {
                    var response = JsonConvert.DeserializeObject<RelayerResponse>(req.downloadHandler.text);
                    if (response.status != "success")
                    {
                        throw new UnityException(
                            $"Forward Request Failed!\nError: {req.downloadHandler.text}\nRelayer URL: {relayerUrl}\nRelayer Forwarder Address: {forwarderAddress}\nRequest: {request}\nSignature: {signature}\nPost Data: {postData}"
                        );
                    }
                    var result = JsonConvert.DeserializeObject<RelayerResult>(response.result);
                    txHash = result.txHash;
                }
            }
            ThirdwebDebug.Log("txHash: " + txHash);
            return await Transaction.WaitForTransactionResultRaw(txHash);
        }
    }

    [System.Serializable]
    public struct RelayerResponse
    {
        [JsonProperty("status")]
        public string status;

        [JsonProperty("result")]
        public string result;
    }

    [System.Serializable]
    public struct RelayerResult
    {
        [JsonProperty("txHash")]
        public string txHash;
    }

    [System.Serializable]
    public struct RelayerRequest
    {
        [JsonProperty("request")]
        public MinimalForwarder.ForwardRequest request;

        [JsonProperty("signature")]
        public string signature;

        [JsonProperty("forwarderAddress")]
        public string forwarderAddress;

        [JsonProperty("type")]
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
