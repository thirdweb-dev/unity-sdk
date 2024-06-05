using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Transactions;
using Newtonsoft.Json;
using UnityEngine;
using MinimalForwarder = Thirdweb.Contracts.Forwarder.ContractDefinition;
using UnityEngine.Networking;
using Thirdweb.Redcode.Awaiting;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding;
using System;
using Newtonsoft.Json.Linq;

#pragma warning disable CS0618

namespace Thirdweb
{
    /// <summary>
    /// Represents the type of transaction.
    /// </summary>
    public enum TransactionType
    {
        LegacyTransaction = -1,
        LegacyChainTransaction = -2,
        Legacy = 0,
        EIP1559 = 2
    }

    /// <summary>
    /// Represents the gas costs for a transaction.
    /// </summary>
    public struct GasCosts
    {
        public string ether;
        public BigInteger wei;
    }

    /// <summary>
    /// Represents an Ethereum transaction.
    /// </summary>
    public class Transaction
    {
        public Contract Contract { get; private set; }
        public string FunctionName { get; private set; }
        public object[] FunctionArgs { get; private set; }
        public TransactionInput Input { get; private set; }

        private readonly ThirdwebSDK _sdk;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class.
        /// </summary>
        /// <param name="contract">The contract associated with the transaction.</param>
        /// <param name="txInput">The transaction input.</param>
        public Transaction(Contract contract, TransactionInput txInput, string fnName, object[] fnArgs)
        {
            this.Contract = contract;
            this.Input = txInput;
            this.FunctionName = fnName;
            this.FunctionArgs = fnArgs;
            this._sdk = contract._sdk;
        }

        public Transaction(ThirdwebSDK sdk, TransactionInput txInput)
        {
            this.Input = txInput;
            this._sdk = sdk;
        }

        /// <summary>
        /// Returns a JSON string representation of the transaction input.
        /// </summary>
        /// <returns>The JSON string representation of the transaction input.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(Input);
        }

        /// <summary>
        /// Sets the max priority fee per gas for the transaction.
        /// </summary>
        /// <param name="maxPriorityFeePerGas">The max priority fee per gas to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetMaxPriorityFeePerGas(string maxPriorityFeePerGas)
        {
            Input.MaxPriorityFeePerGas = BigInteger.Parse(maxPriorityFeePerGas).ToHexBigInteger();
            return this;
        }

        /// <summary>
        /// Sets the max fee per gas for the transaction.
        /// </summary>
        /// <param name="maxFeePerGas">The max fee per gas to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetMaxFeePerGas(string maxFeePerGas)
        {
            Input.MaxFeePerGas = BigInteger.Parse(maxFeePerGas).ToHexBigInteger();
            return this;
        }

        /// <summary>
        /// Sets the data for the transaction.
        /// </summary>
        /// <param name="data">The data to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetData(string data)
        {
            Input.Data = data;
            return this;
        }

        /// <summary>
        /// Sets the value (in Ether) to be sent with the transaction.
        /// </summary>
        /// <param name="ethValue">The value in Ether to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetValue(string ethValue)
        {
            Input.Value = BigInteger.Parse(ethValue.ToWei()).ToHexBigInteger();
            return this;
        }

        /// <summary>
        /// Sets the sender address for the transaction.
        /// </summary>
        /// <param name="from">The sender address to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetFrom(string from)
        {
            Input.From = from;
            return this;
        }

        /// <summary>
        /// Sets the gas limit for the transaction.
        /// </summary>
        /// <param name="gas">The gas limit to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetGasLimit(string gas)
        {
            Input.Gas = BigInteger.Parse(gas).ToHexBigInteger();
            return this;
        }

        /// <summary>
        /// Sets the recipient address for the transaction.
        /// </summary>
        /// <param name="to">The recipient address to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetTo(string to)
        {
            Input.To = to;
            return this;
        }

        /// <summary>
        /// Sets the type of the transaction.
        /// </summary>
        /// <param name="type">The transaction type to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetType(TransactionType type)
        {
            Input.Type = new HexBigInteger((int)type);
            return this;
        }

        /// <summary>
        /// Sets the gas price for the transaction.
        /// </summary>
        /// <param name="gasPrice">The gas price to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetGasPrice(string gasPrice)
        {
            Input.GasPrice = BigInteger.Parse(gasPrice).ToHexBigInteger();
            return this;
        }

        /// <summary>
        /// Sets the chain ID for the transaction.
        /// </summary>
        /// <param name="chainId">The chain ID to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetChainId(string chainId)
        {
            Input.ChainId = BigInteger.Parse(chainId).ToHexBigInteger();
            return this;
        }

        /// <summary>
        /// Sets the nonce for the transaction.
        /// </summary>
        /// <param name="nonce">The nonce to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetNonce(string nonce)
        {
            Input.Nonce = BigInteger.Parse(nonce).ToHexBigInteger();
            return this;
        }

        /// <summary>
        /// Sets the arguments for the transaction.
        /// </summary>
        /// <param name="args">The arguments to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object.</returns>
        public Transaction SetArgs(params object[] args)
        {
            if (Utils.IsWebGLBuild())
            {
                this.FunctionArgs = args;
            }
            else
            {
                var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
                var contract = web3.Eth.GetContract(Contract.ABI, Contract.Address);
                var function = Utils.GetFunctionMatchSignature(contract, FunctionName, args);
                Input.Data = function.GetData(args);
            }
            return this;
        }

        /// <summary>
        /// Gets the gas price for the transaction asynchronously.
        /// </summary>
        /// <returns>The gas price for the transaction as a <see cref="BigInteger"/>.</returns>
        public async Task<BigInteger> GetGasPrice()
        {
            if (Utils.IsWebGLBuild())
            {
                var val = await Bridge.InvokeRoute<string>(GetTxBuilderRoute("getGasPrice"), Utils.ToJsonStringArray(Input, FunctionName, FunctionArgs));
                return BigInteger.Parse(val);
            }
            else
            {
                return await Utils.GetLegacyGasPriceAsync(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            }
        }

        /// <summary>
        /// Estimates the gas limit for the transaction asynchronously.
        /// </summary>
        /// <returns>The estimated gas limit for the transaction as a <see cref="BigInteger"/>.</returns>
        public async Task<BigInteger> EstimateGasLimit()
        {
            if (Utils.IsWebGLBuild())
            {
                var val = await Bridge.InvokeRoute<string>(GetTxBuilderRoute("estimateGasLimit"), Utils.ToJsonStringArray(Input, FunctionName, FunctionArgs));
                return BigInteger.Parse(val);
            }
            else
            {
                var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
                var gas = await web3.Eth.Transactions.EstimateGas.SendRequestAsync(Input);
                return gas.Value;
            }
        }

        /// <summary>
        /// Estimates the gas costs for the transaction asynchronously.
        /// </summary>
        /// <returns>The estimated gas costs for the transaction as a <see cref="GasCosts"/> struct.</returns>
        public async Task<GasCosts> EstimateGasCosts()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<GasCosts>(GetTxBuilderRoute("estimateGasCosts"), Utils.ToJsonStringArray(Input, FunctionName, FunctionArgs));
            }
            else
            {
                var gasLimit = await EstimateGasLimit();
                var gasPrice = await GetGasPrice();
                var gasCost = gasLimit * gasPrice;
                return new GasCosts { ether = gasCost.ToString().ToEth(18, false), wei = gasCost };
            }
        }

        /// <summary>
        /// Estimates and sets the gas limit for the transaction asynchronously.
        /// </summary>
        /// <param name="minimumGas">The minimum gas limit to be set.</param>
        /// <returns>The modified <see cref="Transaction"/> object with the updated gas limit.</returns>
        public async Task<Transaction> EstimateAndSetGasLimitAsync(string minimumGas = "100000")
        {
            var gasBigInt = await EstimateGasLimit();
            var minGasBigInt = BigInteger.Parse(minimumGas);
            Input.Gas = gasBigInt > minGasBigInt ? gasBigInt.ToHexBigInteger() : minGasBigInt.ToHexBigInteger();
            return this;
        }

        /// <summary>
        /// Simulates the transaction asynchronously.
        /// </summary>
        /// <returns>The result of the transaction simulation as a string.</returns>
        public async Task<string> Simulate()
        {
            if (Utils.IsWebGLBuild())
            {
                return JsonConvert.SerializeObject(await Bridge.InvokeRoute<object>(GetTxBuilderRoute("simulate"), Utils.ToJsonStringArray(Input, FunctionName, FunctionArgs)));
            }
            else
            {
                var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
                return await web3.Eth.Transactions.Call.SendRequestAsync(Input);
            }
        }

        /// <summary>
        /// Signs the transaction asynchronously, if the wallet supports it. Useful for smart wallet user op delayed broadcasting through thirdweb Engine. Otherwise not recommended.
        /// </summary>
        /// <returns>The signed transaction a string.</returns>
        public async Task<string> Sign()
        {
            if (Input.Value == null)
                Input.Value = new HexBigInteger(0);

            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(GetTxBuilderRoute("sign"), Utils.ToJsonStringArray(Input, FunctionName, FunctionArgs));
            }
            else
            {
                if (_sdk.Session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet && _sdk.Session.ActiveWallet.GetLocalAccount() != null)
                    return await _sdk.Session.ActiveWallet.GetLocalAccount().TransactionManager.SignTransactionAsync(Input);
                else
                    return await _sdk.Session.Request<string>("eth_signTransaction", Input);
            }
        }

        /// <summary>
        /// Sends the transaction asynchronously.
        /// </summary>
        /// <param name="gasless">Specifies whether to send the transaction as a gasless transaction. Default is null (uses gasless if set up).</param>
        /// <returns>The transaction hash as a string.</returns>
        public async Task<string> Send(bool? gasless = null)
        {
            if (Utils.IsWebGLBuild())
            {
                if (gasless == null || gasless == false)
                    return await Send();
                else
                    return await SendGasless();
            }
            else
            {
                if (Input.Gas == null)
                    await EstimateAndSetGasLimitAsync();
                if (Input.Value == null)
                    Input.Value = new HexBigInteger(0);
                bool isGaslessSetup = _sdk.Session.Options.gasless.HasValue && !string.IsNullOrEmpty(_sdk.Session.Options.gasless?.engine.relayerUrl);
                if (gasless != null && gasless.Value && !isGaslessSetup)
                    throw new UnityException("Gasless relayer transactions are not enabled. Please enable them in the SDK options.");
                bool sendGaslessly = gasless == null ? isGaslessSetup : gasless.Value;
                if (sendGaslessly)
                    return await SendGasless();
                else
                    return await Send();
            }
        }

        /// <summary>
        /// Sends the transaction and waits for the transaction result asynchronously.
        /// </summary>
        /// <param name="gasless">Specifies whether to send the transaction as a gasless transaction. Default is null.</param>
        /// <returns>The transaction result as a <see cref="TransactionResult"/> object.</returns>
        public async Task<TransactionResult> SendAndWaitForTransactionResult(bool? gasless = null)
        {
            if (Utils.IsWebGLBuild())
            {
                string action;
                if (gasless == null || gasless == false)
                    action = "execute";
                else
                    action = "executeGasless";

                return await Bridge.InvokeRoute<TransactionResult>(GetTxBuilderRoute(action), Utils.ToJsonStringArray(Input, FunctionName, FunctionArgs));
            }
            else
            {
                var txHash = await Send(gasless);
                return await WaitForTransactionResult(txHash, _sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            }
        }

        /// <summary>
        /// Waits for the transaction result asynchronously.
        /// </summary>
        /// <param name="txHash">The transaction hash to wait for.</param>
        /// <returns>The transaction result as a <see cref="TransactionResult"/> object.</returns>
        public static async Task<TransactionResult> WaitForTransactionResult(string txHash, BigInteger chainId, string clientId = null, string bundleId = null)
        {
            var receipt = await WaitForTransactionResultRaw(txHash, chainId);
            return receipt.ToTransactionResult();
        }

        /// <summary>
        /// Waits for the transaction result asynchronously.
        /// </summary>
        /// <param name="txHash">The transaction hash to wait for.</param>
        /// <returns>The transaction result as a <see cref="TransactionResult"/> object.</returns>
        public static async Task<TransactionReceipt> WaitForTransactionResultRaw(string txHash, BigInteger chainId, string clientId = null, string bundleId = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.WaitForTransactionResult(txHash);
            }
            else
            {
                var web3 = Utils.GetWeb3(chainId, clientId, bundleId);
                var receipt = await web3.TransactionReceiptPolling.PollForReceiptAsync(txHash);
                if (receipt.Failed())
                {
                    var reason = await web3.Eth.GetContractTransactionErrorReason.SendRequestAsync(txHash);
                    if (!string.IsNullOrEmpty(reason))
                        throw new UnityException($"Transaction {txHash} execution reverted: {reason}");
                }

                var userOpEvent = receipt.DecodeAllEvents<Thirdweb.Contracts.EntryPoint.ContractDefinition.UserOperationEventEventDTO>();
                if (userOpEvent != null && userOpEvent.Count > 0 && userOpEvent[0].Event.Success == false)
                {
                    var revertReasonEvent = receipt.DecodeAllEvents<Thirdweb.Contracts.EntryPoint.ContractDefinition.UserOperationRevertReasonEventDTO>();
                    if (revertReasonEvent != null && revertReasonEvent.Count > 0)
                    {
                        byte[] revertReason = revertReasonEvent[0].Event.RevertReason;
                        string revertReasonString = new FunctionCallDecoder().DecodeFunctionErrorMessage(revertReason.ByteArrayToHexString());
                        throw new Exception($"Transaction {txHash} execution silently reverted: {revertReasonString}");
                    }
                    else
                    {
                        throw new Exception($"Transaction {txHash} execution silently reverted with no reason string");
                    }
                }

                return receipt;
            }
        }

        private async Task<string> Send()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(GetTxBuilderRoute("send"), Utils.ToJsonStringArray(Input, FunctionName, FunctionArgs));
            }
            else
            {
                var supports1559 = Utils.Supports1559(_sdk.Session.ChainId.ToString());
                if (supports1559)
                {
                    if (Input.GasPrice == null)
                    {
                        var fees = await Utils.GetGasPriceAsync(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
                        if (Input.MaxFeePerGas == null)
                            Input.MaxFeePerGas = new HexBigInteger(fees.MaxFeePerGas);
                        if (Input.MaxPriorityFeePerGas == null)
                            Input.MaxPriorityFeePerGas = new HexBigInteger(fees.MaxPriorityFeePerGas);
                    }
                }
                else
                {
                    if (Input.MaxFeePerGas == null && Input.MaxPriorityFeePerGas == null)
                    {
                        ThirdwebDebug.Log("Using Legacy Gas Pricing");
                        Input.GasPrice = new HexBigInteger(await Utils.GetLegacyGasPriceAsync(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId));
                    }
                }

                string hash;
                if (_sdk.Session.ActiveWallet.GetSignerProvider() == WalletProvider.LocalWallet && _sdk.Session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet)
                {
                    hash = await _sdk.Session.Web3.Eth.TransactionManager.SendTransactionAsync(Input);
                }
                else
                {
                    var ethSendTx = new EthSendTransaction(_sdk.Session.Web3.Client);
                    hash = await ethSendTx.SendRequestAsync(Input);
                }
                ThirdwebDebug.Log($"Transaction hash: {hash}");
                return hash;
            }
        }

        private async Task<string> SendGasless()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(GetTxBuilderRoute("sendGasless"), Utils.ToJsonStringArray(Input, FunctionName, FunctionArgs));
            }
            else
            {
                string relayerUrl = _sdk.Session.Options.gasless?.engine.relayerUrl ?? throw new UnityException("Relayer URL not set in SDK options.");
                string forwarderAddress = _sdk.Session.Options.gasless?.engine.relayerForwarderAddress ?? "0xD04F98C88cE1054c90022EE34d566B9237a1203C";
                string forwarderDomain = _sdk.Session.Options.gasless?.engine.domainName ?? "GSNv2 Forwarder";
                string forwarderVersion = _sdk.Session.Options.gasless?.engine.domainVersion ?? "0.0.1";

                Input.Nonce = (
                    await TransactionManager.ThirdwebRead<MinimalForwarder.GetNonceFunction, MinimalForwarder.GetNonceOutputDTO>(
                        _sdk,
                        forwarderAddress,
                        new MinimalForwarder.GetNonceFunction() { From = Input.From }
                    )
                ).ReturnValue1.ToHexBigInteger();

                var request = new MinimalForwarder.ForwardRequest()
                {
                    From = Input.From,
                    To = Input.To,
                    Value = Input.Value,
                    Gas = Input.Gas.Value,
                    Nonce = Input.Nonce.Value,
                    Data = Input.Data
                };

                ThirdwebDebug.Log($"Forwarding request: {JsonConvert.SerializeObject(request)}");

                var signature = await EIP712.GenerateSignature_MinimalForwarder(
                    _sdk,
                    forwarderDomain,
                    forwarderVersion,
                    Input.ChainId?.Value ?? await _sdk.Wallet.GetChainId(),
                    forwarderAddress,
                    request
                );

                var postData = new RelayerRequest()
                {
                    Type = "forward",
                    Request = request,
                    Signature = signature,
                    ForwarderAddress = forwarderAddress,
                };

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
                    var queueId = JsonConvert.DeserializeObject<JObject>(req.downloadHandler.text)["result"]["queueId"].ToString();
                    ThirdwebDebug.Log($"Forwarded request to relayer with queue ID: {queueId}");
                    return await FetchTxHashFromQueueId(new Uri(relayerUrl).GetLeftPart(UriPartial.Authority), queueId);
                }
            }
        }

        private async Task<string> FetchTxHashFromQueueId(string engineUrl, string queueId)
        {
            string txHash = null;
            while (string.IsNullOrEmpty(txHash) && Application.isPlaying)
            {
                using UnityWebRequest req = UnityWebRequest.Get($"{engineUrl}/transaction/status/{queueId}");
                await new WaitForSeconds(1f);
                await req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    throw new UnityException($"Failed to fetch transaction hash from queue ID {queueId}.\nError: {req.downloadHandler.text}");
                }
                else
                {
                    txHash = JsonConvert.DeserializeObject<JObject>(req.downloadHandler.text)["result"]["transactionHash"].ToString();
                }
            }
            ThirdwebDebug.Log($"Transaction hash fetched from queue ID {queueId}: {txHash}");
            return txHash;
        }

        private string GetTxBuilderRoute(string action)
        {
            string route = Contract.ABI != null ? $"{Contract.Address}{Routable.subSeparator}{Contract.ABI}" : Contract.Address;
            return $"{route}{Routable.separator}tx{Routable.separator}{action}";
        }
    }

    [System.Serializable]
    public struct RelayerRequest
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("request")]
        public MinimalForwarder.ForwardRequest Request;

        [JsonProperty("signature")]
        public string Signature;

        [JsonProperty("forwarderAddress")]
        public string ForwarderAddress;
    }
}
