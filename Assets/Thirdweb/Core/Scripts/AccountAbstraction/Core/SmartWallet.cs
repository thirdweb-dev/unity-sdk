using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client.RpcMessages;
using Nethereum.RPC.Eth;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Thirdweb.Contracts.Account.ContractDefinition;
using Thirdweb.Contracts.AccountFactory.ContractDefinition;
using static Thirdweb.ThirdwebSDK;
using UnityEngine;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Thirdweb.AccountAbstraction
{
    public class SmartWallet
    {
        public List<string> Accounts { get; internal set; }
        public Account PersonalAccount { get; internal set; }
        public SmartWalletConfig Config { get; internal set; }

        private bool _initialized;
        private bool _deployed;
        private Web3 _personalWeb3;
        private HttpClient _httpClient;

        private readonly string DEFAULT_ENTRYPOINT_ADDRESS = "0x5FF137D4b0FDCD49DcA30c7CF57E578a026d2789"; // v0.6
        private readonly string DUMMY_PAYMASTER_AND_DATA_HEX =
            "0x0101010101010101010101010101010101010101000000000000000000000000000000000000000000000000000001010101010100000000000000000000000000000000000000000000000000000000000000000101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101010101";

        public SmartWallet(Account personalAccount, SmartWalletConfig config)
        {
            PersonalAccount = personalAccount;
            Config = config;
            _deployed = false;
            _initialized = false;
            _personalWeb3 = new Web3(PersonalAccount, ThirdwebManager.Instance.SDK.session.RPC);
            _httpClient = new HttpClient();
        }

        internal async Task Initialize()
        {
            if (_initialized)
            {
                Debug.Log("Already initialized.");
                return;
            }
            Debug.Log("Initializing... Factory: " + Config.factoryAddress + ", Admin: " + PersonalAccount.Address);
            var predictedAccount = await TransactionManager.ThirdwebRead<GetAddressFunction, GetAddressOutputDTO>(
                Config.factoryAddress,
                new GetAddressFunction() { AdminSigner = PersonalAccount.Address }
            );
            Accounts = new List<string>() { predictedAccount.ReturnValue1 };
            Debug.Log("Predicted account: " + Accounts[0]);
            var bytecode = await _personalWeb3.Eth.GetCode.SendRequestAsync(Accounts[0]);
            _deployed = bytecode != "0x";
            Debug.Log("Initialized. " + (_deployed ? "Deployed." : "Not Deployed."));
            _initialized = true;
        }

        internal async Task<byte[]> GetInitCode()
        {
            if (_deployed)
            {
                Debug.Log("Already deployed.");
                return new byte[] { };
            }

            var fn = new CreateAccountFunction() { Admin = PersonalAccount.Address, Data = new byte[] { } };
            fn.FromAddress = PersonalAccount.Address;
            var deployHandler = _personalWeb3.Eth.GetContractTransactionHandler<CreateAccountFunction>();
            var txInput = await deployHandler.CreateTransactionInputEstimatingGasAsync(Config.factoryAddress, fn);
            var data = txInput.Data;
            return data.HexStringToByteArray();
        }

        internal async Task<RpcResponseMessage> Request(RpcRequestMessage requestMessage)
        {
            Debug.Log("Requesting: " + requestMessage.Method + "...");

            if (requestMessage.Method == "eth_chainId")
            {
                var response = JToken.FromObject(new HexBigInteger(PersonalAccount.ChainId.Value).HexValue);
                return new RpcResponseMessage(requestMessage.Id, response);
            }
            else if (requestMessage.Method == "eth_sendTransaction")
            {
                return await CreateUserOpAndSend(requestMessage);
            }
            else
            {
                throw new Exception("Method not supported: " + requestMessage.Method);
            }
        }

        private async Task<RpcResponseMessage> CreateUserOpAndSend(RpcRequestMessage requestMessage)
        {
            Debug.Log("Deserialize the transaction input from the request message");

            var paramList = JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(requestMessage.RawParameters));
            var transactionInput = JsonConvert.DeserializeObject<TransactionInput>(JsonConvert.SerializeObject(paramList[0]));
            string bundlerUrl = string.IsNullOrEmpty(Config.bundlerUrl) ? $"https://{ThirdwebManager.Instance.SDK.session.CurrentChainData.chainName}.bundler.thirdweb.com" : Config.bundlerUrl;
            string paymasterUrl = string.IsNullOrEmpty(Config.paymasterUrl) ? $"https://{ThirdwebManager.Instance.SDK.session.CurrentChainData.chainName}.bundler.thirdweb.com" : Config.paymasterUrl;
            string entryPoint = string.IsNullOrEmpty(Config.entryPointAddress) ? DEFAULT_ENTRYPOINT_ADDRESS : Config.entryPointAddress;
            var nonce = await TransactionManager.ThirdwebRead<GetNonceFunction, GetNonceOutputDTO>(Accounts[0], new GetNonceFunction() { });
            var signer = new EthereumMessageSigner();
            var signerKey = new EthECKey(PersonalAccount.PrivateKey);
            Block latestBlock = await Utils.GetBlockByNumber(await Utils.GetLatestBlockNumber());
            var dummySig = new byte[65];
            for (int i = 0; i < dummySig.Length; i++)
                dummySig[i] = 0x01;

            Debug.Log("Creating invalid user op for gas estimation");

            var invalidUserOp = new Thirdweb.Contracts.EntryPoint.ContractDefinition.UserOperation()
            {
                Sender = Accounts[0],
                Nonce = nonce.ReturnValue1,
                InitCode = await GetInitCode(),
                CallData = transactionInput.Data.HexStringToByteArray(),
                CallGasLimit = transactionInput.Gas.Value,
                VerificationGasLimit = 100000,
                PreVerificationGas = 100000,
                MaxFeePerGas = latestBlock.BaseFeePerGas,
                MaxPriorityFeePerGas = 2,
                PaymasterAndData = Config.gasless ? DUMMY_PAYMASTER_AND_DATA_HEX.HexStringToByteArray() : new byte[] { },
                Signature = dummySig,
            };
            var invalidUserOpHash = await TransactionManager.ThirdwebRead<
                Thirdweb.Contracts.EntryPoint.ContractDefinition.GetUserOpHashFunction,
                Thirdweb.Contracts.EntryPoint.ContractDefinition.GetUserOpHashOutputDTO
            >(entryPoint, new Contracts.EntryPoint.ContractDefinition.GetUserOpHashFunction() { UserOp = invalidUserOp });
            var invalidUserUpSignature = signer.Sign(invalidUserOpHash.ReturnValue1, signerKey);
            invalidUserOp.Signature = invalidUserUpSignature.HexStringToByteArray();
            var invalidUserOpHexified = EncodeUserOperation(
                new UserOperation()
                {
                    Sender = invalidUserOp.Sender,
                    Nonce = invalidUserOp.Nonce,
                    InitCode = invalidUserOp.InitCode,
                    CallData = invalidUserOp.CallData,
                    CallGasLimit = invalidUserOp.CallGasLimit,
                    VerificationGasLimit = invalidUserOp.VerificationGasLimit,
                    PreVerificationGas = invalidUserOp.PreVerificationGas,
                    MaxFeePerGas = invalidUserOp.MaxFeePerGas,
                    MaxPriorityFeePerGas = invalidUserOp.MaxPriorityFeePerGas,
                    PaymasterAndData = invalidUserOp.PaymasterAndData,
                    Signature = invalidUserOp.Signature,
                }
            );

            Debug.Log("Estimate gas with invalid UserOp");

            if (Config.gasless)
            {
                var gasEstimatesRequest = new RpcRequestMessage(requestMessage.Id, "pm_sponsorUserOperation", new object[] { invalidUserOpHexified, entryPoint });
                var gasEstimatesResult = await InnerRpcRequest(gasEstimatesRequest, paymasterUrl);
                if (gasEstimatesResult.Result == null)
                    throw new Exception("Failed to estimate gas: " + gasEstimatesResult.Error.Message);
                var gasEstimates = JsonConvert.DeserializeObject<PMSponsorOperationResponse>(gasEstimatesResult.Result.ToString());

                invalidUserOp.CallGasLimit = new HexBigInteger(gasEstimates.callGasLimit).Value;
                invalidUserOp.VerificationGasLimit = new HexBigInteger(gasEstimates.verificationGas).Value;
                invalidUserOp.PreVerificationGas = new HexBigInteger(gasEstimates.preVerificationGas).Value;
                invalidUserOp.PaymasterAndData = gasEstimates.paymasterAndData.HexStringToByteArray();
            }
            else
            {
                var gasEstimatesRequest = new RpcRequestMessage(requestMessage.Id, "eth_estimateUserOperationGas", new object[] { invalidUserOpHexified, entryPoint });
                var gasEstimatesResult = await InnerRpcRequest(gasEstimatesRequest, bundlerUrl);
                if (gasEstimatesResult.Result == null)
                    throw new Exception("Failed to estimate gas: " + gasEstimatesResult.Error.Message);
                var gasEstimates = JsonConvert.DeserializeObject<UserOperationGasEstimateResponse>(gasEstimatesResult.Result.ToString());

                invalidUserOp.CallGasLimit = new HexBigInteger(gasEstimates.CallGasLimit).Value;
                invalidUserOp.VerificationGasLimit = new HexBigInteger(gasEstimates.VerificationGas).Value;
                invalidUserOp.PreVerificationGas = new HexBigInteger(gasEstimates.PreVerificationGas).Value;
                invalidUserOp.PaymasterAndData = new byte[] { };
            }

            Debug.Log("Hash and sign partial UserOp");

            var unsignedValidUserOp = new Thirdweb.Contracts.EntryPoint.ContractDefinition.UserOperation()
            {
                Sender = invalidUserOp.Sender,
                Nonce = invalidUserOp.Nonce,
                InitCode = invalidUserOp.InitCode,
                CallData = invalidUserOp.CallData,
                CallGasLimit = invalidUserOp.CallGasLimit,
                VerificationGasLimit = invalidUserOp.VerificationGasLimit,
                PreVerificationGas = invalidUserOp.PreVerificationGas,
                MaxFeePerGas = invalidUserOp.MaxFeePerGas,
                MaxPriorityFeePerGas = invalidUserOp.MaxPriorityFeePerGas,
                PaymasterAndData = invalidUserOp.PaymasterAndData,
                Signature = invalidUserOp.Signature,
            };
            var partialUserOpHash = await TransactionManager.ThirdwebRead<
                Thirdweb.Contracts.EntryPoint.ContractDefinition.GetUserOpHashFunction,
                Thirdweb.Contracts.EntryPoint.ContractDefinition.GetUserOpHashOutputDTO
            >(entryPoint, new Contracts.EntryPoint.ContractDefinition.GetUserOpHashFunction() { UserOp = unsignedValidUserOp });
            var partialUserOpHashSignature = signer.Sign(partialUserOpHash.ReturnValue1, signerKey);
            unsignedValidUserOp.Signature = partialUserOpHashSignature.HexStringToByteArray();

            Debug.Log("Encode and send valid UserOp to bundler");

            var validUserOperation = new UserOperation
            {
                Sender = unsignedValidUserOp.Sender,
                Nonce = unsignedValidUserOp.Nonce,
                InitCode = unsignedValidUserOp.InitCode,
                CallData = unsignedValidUserOp.CallData,
                CallGasLimit = unsignedValidUserOp.CallGasLimit,
                VerificationGasLimit = unsignedValidUserOp.VerificationGasLimit,
                PreVerificationGas = unsignedValidUserOp.PreVerificationGas,
                MaxFeePerGas = unsignedValidUserOp.MaxFeePerGas,
                MaxPriorityFeePerGas = unsignedValidUserOp.MaxPriorityFeePerGas,
                PaymasterAndData = unsignedValidUserOp.PaymasterAndData,
                Signature = unsignedValidUserOp.Signature,
            };
            Debug.Log("Valid UserOp: " + JsonConvert.SerializeObject(validUserOperation));
            var encodedValidUserOperation = EncodeUserOperation(validUserOperation);
            Debug.Log("Encoded UserOp: " + JsonConvert.SerializeObject(encodedValidUserOperation));
            var sendUserOpRequest = new RpcRequestMessage(requestMessage.Id, "eth_sendUserOperation", new object[] { encodedValidUserOperation, entryPoint });
            var sendUserOpResult = await InnerRpcRequest(sendUserOpRequest, bundlerUrl);
            if (sendUserOpResult.Result == null)
                throw new Exception("Failed to send UserOp: " + sendUserOpResult.Error.Message);
            var userOpHash = sendUserOpResult.Result.ToString();

            _deployed = true;

            string txHash = null;

            while (txHash == null && Application.isPlaying)
            {
                Debug.Log("Get transaction hash from UserOp hash: " + userOpHash);
                var getUserOpRequest = new RpcRequestMessage(requestMessage.Id, "eth_getUserOperationByHash", new object[] { userOpHash });
                var getUserOpResult = await InnerRpcRequest(getUserOpRequest, bundlerUrl);
                var getUserOpResponse = JsonConvert.DeserializeObject<GetUserOperationResponse>(getUserOpResult.Result.ToString());
                txHash = getUserOpResponse?.transactionHash;
                Debug.Log("Transaction hash: " + txHash);
                await new WaitForSecondsRealtime(2f);
            }

            Debug.Log("Checking if success...");
            var receipt = await _personalWeb3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            var decodedEvents = receipt.DecodeAllEvents<Thirdweb.Contracts.EntryPoint.ContractDefinition.UserOperationEventEventDTO>();
            if (decodedEvents[0].Event.Success == false)
            {
                Debug.Log("Transaction not successful, checking reason...");
                var reason = await _personalWeb3.Eth.GetContractTransactionErrorReason.SendRequestAsync(txHash);
                throw new Exception($"Transaction {txHash} reverted with reason: {reason}");
            }

            return new RpcResponseMessage(requestMessage.Id, txHash);
        }

        private async Task<RpcResponseMessage> InnerRpcRequest(RpcRequestMessage requestMessage, string bundlerUrl)
        {
            string requestMessageJson = JsonConvert.SerializeObject(requestMessage);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, bundlerUrl);
            httpRequestMessage.Content = new StringContent(requestMessageJson, System.Text.Encoding.UTF8, "application/json");
            httpRequestMessage.Headers.Add("x-api-key", Config.thirdwebApiKey);

            var httpResponse = await _httpClient.SendAsync(httpRequestMessage);

            if (!httpResponse.IsSuccessStatusCode)
                throw new Exception(
                    "Failed to send UserOperation to the bundler. Error: " + httpResponse.StatusCode + " - " + httpResponse.ReasonPhrase + " - " + await httpResponse.Content.ReadAsStringAsync()
                );

            var httpResponseJson = await httpResponse.Content.ReadAsStringAsync();
            var rpcResponseMessage = JsonConvert.DeserializeObject<RpcResponseMessage>(httpResponseJson);
            return rpcResponseMessage;
        }

        private UserOperationHexified EncodeUserOperation(UserOperation userOperation)
        {
            return new UserOperationHexified()
            {
                sender = userOperation.Sender,
                nonce = userOperation.Nonce,
                initCode = userOperation.InitCode.ByteArrayToHexString(),
                callData = userOperation.CallData.ByteArrayToHexString(),
                callGasLimit = userOperation.CallGasLimit,
                verificationGasLimit = userOperation.VerificationGasLimit,
                preVerificationGas = userOperation.PreVerificationGas,
                maxFeePerGas = userOperation.MaxFeePerGas,
                maxPriorityFeePerGas = userOperation.MaxPriorityFeePerGas,
                paymasterAndData = userOperation.PaymasterAndData.ByteArrayToHexString(),
                signature = userOperation.Signature.ByteArrayToHexString()
            };
        }

        public class UserOperationHexified
        {
            public string sender { get; set; }
            public BigInteger nonce { get; set; }
            public string initCode { get; set; }
            public string callData { get; set; }
            public BigInteger callGasLimit { get; set; }
            public BigInteger verificationGasLimit { get; set; }
            public BigInteger preVerificationGas { get; set; }
            public BigInteger maxFeePerGas { get; set; }
            public BigInteger maxPriorityFeePerGas { get; set; }
            public string paymasterAndData { get; set; }
            public string signature { get; set; }
        }

        public class PMSponsorOperationResponse
        {
            public string paymasterAndData { get; set; }
            public string preVerificationGas { get; set; }
            public string verificationGas { get; set; }
            public string callGasLimit { get; set; }
        }

        public class UserOperationGasEstimateResponse
        {
            public string PreVerificationGas { get; set; }
            public string VerificationGas { get; set; }
            public string CallGasLimit { get; set; }
        }

        public class GetUserOperationResponse
        {
            public string entryPoint { get; set; }
            public string transactionHash { get; set; }
            public string blockHash { get; set; }
            public string blockNumber { get; set; }
        }
    }
}
