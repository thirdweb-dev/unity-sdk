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
        private readonly Contract contract;
        private readonly string fnName;
        private object[] fnArgs;

        /// <summary>
        /// Gets the transaction input.
        /// </summary>
        public TransactionInput Input { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class.
        /// </summary>
        /// <param name="contract">The contract associated with the transaction.</param>
        /// <param name="txInput">The transaction input.</param>
        public Transaction(Contract contract, TransactionInput txInput, string fnName, object[] fnArgs)
        {
            this.contract = contract;
            this.Input = txInput;
            this.fnName = fnName;
            this.fnArgs = fnArgs;
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
                this.fnArgs = args;
            }
            else
            {
                var web3 = Utils.GetWeb3();
                var function = web3.Eth.GetContract(contract.abi, contract.address).GetFunction(Input.To);
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
                return await Bridge.InvokeRoute<BigInteger>(GetTxBuilderRoute("getGasPrice"), Utils.ToJsonStringArray(Input, fnName, fnArgs));
            }
            else
            {
                var web3 = Utils.GetWeb3();
                var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();
                var maxGasPrice = BigInteger.Parse("300000000000"); // 300 Gwei in Wei
                var extraTip = gasPrice.Value / 10; // +10%
                var txGasPrice = gasPrice.Value + extraTip;
                return txGasPrice > maxGasPrice ? maxGasPrice : txGasPrice;
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
                return await Bridge.InvokeRoute<BigInteger>(GetTxBuilderRoute("estimateGasLimit"), Utils.ToJsonStringArray(Input, fnName, fnArgs));
            }
            else
            {
                var gasEstimator = Utils.GetWeb3();
                var gas = await gasEstimator.Eth.Transactions.EstimateGas.SendRequestAsync(Input);
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
                return await Bridge.InvokeRoute<GasCosts>(GetTxBuilderRoute("estimateGasCosts"), Utils.ToJsonStringArray(Input, fnName, fnArgs));
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
                return JsonConvert.SerializeObject(await Bridge.InvokeRoute<object>(GetTxBuilderRoute("simulate"), Utils.ToJsonStringArray(Input, fnName, fnArgs)));
            }
            else
            {
                var web3 = Utils.GetWeb3();
                return await web3.Eth.Transactions.Call.SendRequestAsync(Input);
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
                bool isGaslessSetup = ThirdwebManager.Instance.SDK.session.Options.gasless.HasValue && ThirdwebManager.Instance.SDK.session.Options.gasless.Value.openzeppelin.HasValue;
                if (gasless != null && gasless.Value && !isGaslessSetup)
                    throw new UnityException("Gasless transactions are not enabled. Please enable them in the SDK options.");
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

                return await Bridge.InvokeRoute<TransactionResult>(GetTxBuilderRoute(action), Utils.ToJsonStringArray(Input, fnName, fnArgs));
            }
            else
            {
                var txHash = await Send(gasless);
                return await WaitForTransactionResult(txHash);
            }
        }

        /// <summary>
        /// Waits for the transaction result asynchronously.
        /// </summary>
        /// <param name="txHash">The transaction hash to wait for.</param>
        /// <returns>The transaction result as a <see cref="TransactionResult"/> object.</returns>
        public static async Task<TransactionResult> WaitForTransactionResult(string txHash)
        {
            var receipt = await WaitForTransactionResultRaw(txHash);
            return receipt.ToTransactionResult();
        }

        /// <summary>
        /// Waits for the transaction result asynchronously.
        /// </summary>
        /// <param name="txHash">The transaction hash to wait for.</param>
        /// <returns>The transaction result as a <see cref="TransactionResult"/> object.</returns>
        public static async Task<TransactionReceipt> WaitForTransactionResultRaw(string txHash)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.WaitForTransactionResult(txHash);
            }
            else
            {
                var web3 = Utils.GetWeb3();
                var receipt = await web3.TransactionReceiptPolling.PollForReceiptAsync(txHash);
                if (receipt.Failed())
                {
                    var reason = await web3.Eth.GetContractTransactionErrorReason.SendRequestAsync(txHash);
                    if (!string.IsNullOrEmpty(reason))
                        throw new UnityException($"Transaction failed: {reason}");
                }
                return receipt;
            }
        }

        private async Task<string> Send()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(GetTxBuilderRoute("send"), Utils.ToJsonStringArray(Input, fnName, fnArgs));
            }
            else
            {
                if (
                    ThirdwebManager.Instance.SDK.session.ActiveWallet.GetSignerProvider() == WalletProvider.LocalWallet
                    && ThirdwebManager.Instance.SDK.session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet
                )
                {
                    return await ThirdwebManager.Instance.SDK.session.Web3.Eth.TransactionManager.SendTransactionAsync(Input);
                }
                else
                {
                    var ethSendTx = new EthSendTransaction(ThirdwebManager.Instance.SDK.session.Web3.Client);
                    return await ethSendTx.SendRequestAsync(Input);
                }
            }
        }

        private async Task<string> SendGasless()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(GetTxBuilderRoute("sendGasless"), Utils.ToJsonStringArray(Input, fnName, fnArgs));
            }
            else
            {
                string relayerUrl = ThirdwebManager.Instance.SDK.session.Options.gasless.Value.openzeppelin?.relayerUrl;
                string forwarderAddress = ThirdwebManager.Instance.SDK.session.Options.gasless.Value.openzeppelin?.relayerForwarderAddress;
                string forwarderDomain = ThirdwebManager.Instance.SDK.session.Options.gasless.Value.openzeppelin?.domainName;
                string forwarderVersion = ThirdwebManager.Instance.SDK.session.Options.gasless.Value.openzeppelin?.domainVersion;

                Input.Nonce = (
                    await TransactionManager.ThirdwebRead<MinimalForwarder.GetNonceFunction, MinimalForwarder.GetNonceOutputDTO>(
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

                var signature = await EIP712.GenerateSignature_MinimalForwarder(
                    forwarderDomain,
                    forwarderVersion,
                    Input.ChainId?.Value ?? await ThirdwebManager.Instance.SDK.wallet.GetChainId(),
                    forwarderAddress,
                    request
                );

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
                    return result.txHash;
                }
            }
        }

        private string GetTxBuilderRoute(string action)
        {
            string route = contract.abi != null ? $"{contract.address}{Routable.subSeparator}{contract.abi}" : contract.address;
            return $"{route}{Routable.separator}tx{Routable.separator}{action}";
        }
    }
}
