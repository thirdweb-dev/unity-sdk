using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.Web3;
using Newtonsoft.Json;
using UnityEngine;
using MinimalForwarder = Thirdweb.Contracts.Forwarder.ContractDefinition;
using UnityEngine.Networking;

namespace Thirdweb
{
    public enum TransactionType
    {
        LegacyTransaction = -1,
        LegacyChainTransaction = -2,
        Legacy = 0,
        EIP1559 = 2
    }

    public struct GasCosts
    {
        public string ether;
        public BigInteger wei;
    }

    public class Transaction
    {
        private readonly Contract contract;

        public TransactionInput Input { get; private set; }

        public Transaction(Contract contract, TransactionInput txInput)
        {
            this.contract = contract;
            this.Input = txInput;
        }

        public Transaction SetMaxPriorityFeePerGas(string maxPriorityFeePerGas)
        {
            Input.MaxPriorityFeePerGas = BigInteger.Parse(maxPriorityFeePerGas).ToHexBigInteger();
            return this;
        }

        public Transaction SetMaxFeePerGas(string maxFeePerGas)
        {
            Input.MaxFeePerGas = BigInteger.Parse(maxFeePerGas).ToHexBigInteger();
            return this;
        }

        public Transaction SetData(string data)
        {
            Input.Data = data;
            return this;
        }

        public Transaction SetValue(string ethValue)
        {
            Input.Value = BigInteger.Parse(ethValue.ToWei()).ToHexBigInteger();
            return this;
        }

        public Transaction SetFrom(string from)
        {
            Input.From = from;
            return this;
        }

        public Transaction SetGas(string gas)
        {
            Input.Gas = BigInteger.Parse(gas).ToHexBigInteger();
            return this;
        }

        public Transaction SetTo(string to)
        {
            Input.To = to;
            return this;
        }

        public Transaction SetType(TransactionType type)
        {
            Input.Type = new HexBigInteger((int)type);
            return this;
        }

        public Transaction SetGasPrice(string gasPrice)
        {
            Input.GasPrice = BigInteger.Parse(gasPrice).ToHexBigInteger();
            return this;
        }

        public Transaction SetChainId(string chainId)
        {
            Input.ChainId = BigInteger.Parse(chainId).ToHexBigInteger();
            return this;
        }

        public Transaction SetNonce(string nonce)
        {
            Input.Nonce = BigInteger.Parse(nonce).ToHexBigInteger();
            return this;
        }

        public Transaction SetArgs(params object[] args)
        {
            var web3 = new Web3(ThirdwebManager.Instance.SDK.session.RPC);
            var function = web3.Eth.GetContract(contract.abi, contract.address).GetFunction(Input.To);
            Input.Data = function.GetData(args);
            return this;
        }

        public async Task<BigInteger> GetGasPrice()
        {
            var web3 = new Web3(ThirdwebManager.Instance.SDK.session.RPC);
            var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();
            var maxGasPrice = BigInteger.Parse("300000000000"); // 300 Gwei in Wei
            var extraTip = gasPrice.Value / 10; // +10%
            var txGasPrice = gasPrice.Value + extraTip;
            return txGasPrice > maxGasPrice ? maxGasPrice : txGasPrice;
        }

        public async Task<BigInteger> EstimateGasLimit()
        {
            var gasEstimator = new Web3(ThirdwebManager.Instance.SDK.session.RPC);
            var gas = await gasEstimator.Eth.Transactions.EstimateGas.SendRequestAsync(Input);
            var defaultGas = BigInteger.Parse("100000");
            return gas.Value < defaultGas ? defaultGas : gas;
        }

        public async Task<GasCosts> EstimateGasCosts()
        {
            var gasLimit = await EstimateGasLimit();
            var gasPrice = await GetGasPrice();
            var gasCost = gasLimit * gasPrice;

            return new GasCosts { ether = gasPrice.ToString().ToEth(18, false), wei = gasCost };
        }

        public async Task<Transaction> EstimateAndSetGasAsync()
        {
            var gasBigInt = await EstimateGasLimit();
            Input.Gas = gasBigInt.ToHexBigInteger();
            return this;
        }

        public async Task<string> Send(bool? gasless = null)
        {
            bool isGaslessSetup = ThirdwebManager.Instance.SDK.session.Options.gasless.HasValue && ThirdwebManager.Instance.SDK.session.Options.gasless.Value.openzeppelin.HasValue;
            if (gasless != null && gasless.Value && !isGaslessSetup)
                throw new UnityException("Gasless transactions are not enabled. Please enable them in the SDK options.");

            bool sendGaslessly = gasless == null ? isGaslessSetup : gasless.Value;
            if (sendGaslessly)
                return await SendGasless();
            else
                return await Send();
        }

        public async Task<TransactionResult> SendAndWaitForTransactionResult(bool? gasless = null)
        {
            var txHash = await Send(gasless);
            return await WaitForTransactionResult(txHash);
        }

        public static async Task<TransactionResult> WaitForTransactionResult(string txHash)
        {
            var receiptPoller = new Web3(ThirdwebManager.Instance.SDK.session.RPC);
            var receipt = await receiptPoller.TransactionReceiptPolling.PollForReceiptAsync(txHash);
            return receipt.ToTransactionResult();
        }

        private async Task<string> Send()
        {
            if (Utils.IsWebGLBuild())
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
                // string route = contract.abi != null ? $"{contract.address}{Routable.subSeparator}{contract.abi}" : contract.address;
                // string sendRoute = $"{route}{Routable.separator}send";
                // return await Bridge.InvokeRoute<string>(sendRoute, new string[] { JsonConvert.SerializeObject(Input) });
            }
            else
            {
                var ethSendTx = new EthSendTransaction(ThirdwebManager.Instance.SDK.session.Web3.Client);
                return await ethSendTx.SendRequestAsync(Input);
            }
        }

        private async Task<string> SendGasless()
        {
            if (Utils.IsWebGLBuild())
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
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

                var signature = await EIP712.GenerateSignature_MinimalForwarder(forwarderDomain, forwarderVersion, Input.ChainId, forwarderAddress, request);

                var postData = new RelayerRequest(request, signature, forwarderAddress);

                using (UnityWebRequest req = UnityWebRequest.Post(relayerUrl, ""))
                {
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
        }
    }
}
