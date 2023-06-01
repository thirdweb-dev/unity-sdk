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

            bool isSigner = false;
            bool isAdmin = false;

            // TODO: Query eth_getCode w/ _personalWeb3

            var signers = await TransactionManager.ThirdwebRead<GetAccountsOfSignerFunction, GetAccountsOfSignerOutputDTO>(
                Config.factoryAddress,
                new GetAccountsOfSignerFunction() { Signer = PersonalAccount.Address }
            );
            isSigner = signers.Accounts.Count != 0 && !string.IsNullOrEmpty(signers.Accounts[0]) && signers.Accounts.Contains(Accounts[0]);

            if (!isSigner)
            {
                try
                {
                    var adminRoleOutput = await TransactionManager.ThirdwebRead<HasRoleFunction, HasRoleOutputDTO>(
                        Accounts[0],
                        new HasRoleFunction() { Role = Utils.AddressZero.HexToByteArray(), Account = PersonalAccount.Address }
                    );
                    isAdmin = adminRoleOutput.ReturnValue1;
                }
                catch (System.Exception e)
                {
                    Debug.Log("Could not check admin role, may not be deployed: " + e.Message + "\n" + e.StackTrace);
                }
            }

            _deployed = isSigner || isAdmin;

            Debug.Log("Initialized. " + (_deployed ? "Deployed." : "Not Deployed.") + " Is Signer: " + isSigner + ", Is Admin: " + isAdmin);
            _initialized = true;
        }

        internal async Task Deploy()
        {
            Debug.Log("Deploying...");

            if (_deployed)
            {
                Debug.Log("Already deployed.");
                return;
            }

            var fn = new CreateAccountFunction() { Admin = PersonalAccount.Address, Data = new byte[] { } };
            fn.FromAddress = PersonalAccount.Address;
            var deployHandler = _personalWeb3.Eth.GetContractTransactionHandler<CreateAccountFunction>();
            await deployHandler.SendRequestAndWaitForReceiptAsync(Config.factoryAddress, fn);

            var accounts = await TransactionManager.ThirdwebRead<GetAccountsOfSignerFunction, GetAccountsOfSignerOutputDTO>(
                Config.factoryAddress,
                new GetAccountsOfSignerFunction() { Signer = PersonalAccount.Address }
            );
            Accounts = accounts.Accounts;
            _deployed = true;
            Debug.Log("Deployed.");
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
                if (!_deployed)
                {
                    Debug.Log("Smart wallet not deployed, deploying...");
                    await Deploy();
                    Debug.Log("Smart wallet deployed.");
                }

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
            // Check deploy here
            var nonce = await TransactionManager.ThirdwebRead<GetNonceFunction, GetNonceOutputDTO>(Accounts[0], new GetNonceFunction() { });
            var signer = new EthereumMessageSigner();
            var signerKey = new EthECKey(PersonalAccount.PrivateKey);

            Debug.Log("Creating invalid user op for gas estimation");

            var invalidUserOp = new Thirdweb.Contracts.EntryPoint.ContractDefinition.UserOperation()
            {
                Sender = transactionInput.From,
                Nonce = nonce.ReturnValue1,
                InitCode = new byte[] { }, // TODO: change this to callData of CreateAccount fn
                CallData = transactionInput.Data.HexToByteArray(),
                CallGasLimit = transactionInput.Gas.Value,
                VerificationGasLimit = 100000,
                PreVerificationGas = 100000,
                MaxFeePerGas = 21, // TODO: Get from last block or check nethereum basic rpc
                MaxPriorityFeePerGas = 1, // TODO: Get from last block or check nethereum basic rpc
                PaymasterAndData = new byte[] { },
                Signature = new byte[] { }, // TODO: Make this 65 bytes if no paymaster, 1s not 0s
            };
            var invalidUserOpHash = await TransactionManager.ThirdwebRead<
                Thirdweb.Contracts.EntryPoint.ContractDefinition.GetUserOpHashFunction,
                Thirdweb.Contracts.EntryPoint.ContractDefinition.GetUserOpHashOutputDTO
            >(Config.entryPointAddress, new Contracts.EntryPoint.ContractDefinition.GetUserOpHashFunction() { UserOp = invalidUserOp });
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

            var gasEstimatesRequest = new RpcRequestMessage(requestMessage.Id, "eth_estimateUserOperationGas", new object[] { invalidUserOpHexified, Config.entryPointAddress });
            var gasEstimatesResult = await BundlerRequest(gasEstimatesRequest, bundlerUrl);
            var gasEstimates = JsonConvert.DeserializeObject<UserOperationGasEstimateResponse>(gasEstimatesResult.Result.ToString());

            Debug.Log("Hash and sign partial UserOp");

            var partialUserOp = new Thirdweb.Contracts.EntryPoint.ContractDefinition.UserOperation()
            {
                Sender = invalidUserOp.Sender,
                Nonce = invalidUserOp.Nonce,
                InitCode = invalidUserOp.InitCode,
                CallData = invalidUserOp.CallData,
                CallGasLimit = new HexBigInteger(gasEstimates.CallGasLimit).Value,
                VerificationGasLimit = new HexBigInteger(gasEstimates.VerificationGas).Value,
                PreVerificationGas = new HexBigInteger(gasEstimates.PreVerificationGas).Value,
                MaxFeePerGas = invalidUserOp.MaxFeePerGas,
                MaxPriorityFeePerGas = invalidUserOp.MaxPriorityFeePerGas,
                PaymasterAndData = invalidUserOp.PaymasterAndData,
                Signature = new byte[] { },
            };
            var partialUserOpHash = await TransactionManager.ThirdwebRead<
                Thirdweb.Contracts.EntryPoint.ContractDefinition.GetUserOpHashFunction,
                Thirdweb.Contracts.EntryPoint.ContractDefinition.GetUserOpHashOutputDTO
            >(Config.entryPointAddress, new Contracts.EntryPoint.ContractDefinition.GetUserOpHashFunction() { UserOp = partialUserOp });
            var partialUserOpHashSignature = signer.Sign(partialUserOpHash.ReturnValue1, signerKey);
            partialUserOp.Signature = partialUserOpHashSignature.HexStringToByteArray();

            Debug.Log("Encode and send valid UserOp to bundler");

            var validUserOperation = new UserOperation
            {
                Sender = partialUserOp.Sender,
                Nonce = partialUserOp.Nonce,
                InitCode = partialUserOp.InitCode,
                CallData = partialUserOp.CallData,
                CallGasLimit = partialUserOp.CallGasLimit,
                VerificationGasLimit = partialUserOp.VerificationGasLimit,
                PreVerificationGas = partialUserOp.PreVerificationGas,
                MaxFeePerGas = partialUserOp.MaxFeePerGas,
                MaxPriorityFeePerGas = partialUserOp.MaxPriorityFeePerGas,
                PaymasterAndData = partialUserOp.PaymasterAndData,
                Signature = partialUserOp.Signature,
            };
            Debug.Log("Valid UserOp: " + JsonConvert.SerializeObject(validUserOperation));
            var encodedValidUserOperation = EncodeUserOperation(validUserOperation);
            Debug.Log("Encoded UserOp: " + JsonConvert.SerializeObject(encodedValidUserOperation));
            var sendUserOpRequest = new RpcRequestMessage(requestMessage.Id, "eth_sendUserOperation", new object[] { encodedValidUserOperation, Config.entryPointAddress });
            var sendUserOpResult = await BundlerRequest(sendUserOpRequest, bundlerUrl);
            var userOpHash = sendUserOpResult.Result.ToString();

            string txHash = null;

            while (txHash == null && Application.isPlaying)
            {
                Debug.Log("Get transaction hash from UserOp hash: " + userOpHash);
                var userOpReceiptRequest = new RpcRequestMessage(requestMessage.Id, "eth_getUserOperationByHash", new object[] { userOpHash });
                var userOpReceiptResult = await BundlerRequest(userOpReceiptRequest, bundlerUrl);
                Debug.Log("UserOp receipt Raw: " + userOpReceiptResult.Result.ToString());
                var userOpReceipt = JsonConvert.DeserializeObject<UserOperationReceiptResponse>(userOpReceiptResult.Result.ToString());
                Debug.Log("UserOp receipt result list: " + JsonConvert.SerializeObject(userOpReceipt));
                txHash = userOpReceipt?.TransactionHash;
                Debug.Log("Transaction hash: " + txHash);
                await new WaitForSecondsRealtime(2f);
            }

            return new RpcResponseMessage(requestMessage.Id, txHash);
        }

        private async Task<RpcResponseMessage> BundlerRequest(RpcRequestMessage requestMessage, string bundlerUrl)
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
                Sender = userOperation.Sender,
                Nonce = userOperation.Nonce,
                InitCode = userOperation.InitCode.ByteArrayToHexString(),
                CallData = userOperation.CallData.ByteArrayToHexString(),
                CallGasLimit = userOperation.CallGasLimit,
                VerificationGasLimit = userOperation.VerificationGasLimit,
                PreVerificationGas = userOperation.PreVerificationGas,
                MaxFeePerGas = userOperation.MaxFeePerGas,
                MaxPriorityFeePerGas = userOperation.MaxPriorityFeePerGas,
                PaymasterAndData = userOperation.PaymasterAndData.ByteArrayToHexString(),
                Signature = userOperation.Signature.ByteArrayToHexString()
            };
        }

        [JsonObject]
        public class UserOperationHexified
        {
            [JsonProperty("sender")]
            public virtual string Sender { get; set; }

            [JsonProperty("nonce")]
            public virtual BigInteger Nonce { get; set; }

            [JsonProperty("initCode")]
            public virtual string InitCode { get; set; }

            [JsonProperty("callData")]
            public virtual string CallData { get; set; }

            [JsonProperty("callGasLimit")]
            public virtual BigInteger CallGasLimit { get; set; }

            [JsonProperty("verificationGasLimit")]
            public virtual BigInteger VerificationGasLimit { get; set; }

            [JsonProperty("preVerificationGas")]
            public virtual BigInteger PreVerificationGas { get; set; }

            [JsonProperty("maxFeePerGas")]
            public virtual BigInteger MaxFeePerGas { get; set; }

            [JsonProperty("maxPriorityFeePerGas")]
            public virtual BigInteger MaxPriorityFeePerGas { get; set; }

            [JsonProperty("paymasterAndData")]
            public virtual string PaymasterAndData { get; set; }

            [JsonProperty("signature")]
            public virtual string Signature { get; set; }
        }

        [JsonObject]
        public class UserOperationGasEstimateResponse
        {
            [JsonProperty("PreVerificationGas")]
            public virtual string PreVerificationGas { get; set; }

            [JsonProperty("VerificationGas")]
            public virtual string VerificationGas { get; set; }

            [JsonProperty("CallGasLimit")]
            public virtual string CallGasLimit { get; set; }
        }

        public class UserOperationReceiptResponse
        {
            [JsonProperty("entryPoint")]
            public string EntryPoint { get; set; }

            [JsonProperty("transactionHash")]
            public string TransactionHash { get; set; }

            [JsonProperty("blockHash")]
            public string BlockHash { get; set; }

            [JsonProperty("blockNumber")]
            public string BlockNumber { get; set; }
        }
    }
}
