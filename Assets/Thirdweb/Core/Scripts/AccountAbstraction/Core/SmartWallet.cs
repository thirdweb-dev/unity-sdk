using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client.RpcMessages;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Newtonsoft.Json;
using AccountContract = Thirdweb.Contracts.Account.ContractDefinition;
using EntryPointContract = Thirdweb.Contracts.EntryPoint.ContractDefinition;
using FactoryContract = Thirdweb.Contracts.AccountFactory.ContractDefinition;
using UnityEngine;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Thirdweb.Redcode.Awaiting;
using System.Threading;
using System.Collections.Concurrent;
using Thirdweb.Contracts.Account.ContractDefinition;

namespace Thirdweb.AccountAbstraction
{
    public class UserOperationHexified
    {
        public string sender { get; set; }
        public string nonce { get; set; }
        public string initCode { get; set; }
        public string callData { get; set; }
        public string callGasLimit { get; set; }
        public string verificationGasLimit { get; set; }
        public string preVerificationGas { get; set; }
        public string maxFeePerGas { get; set; }
        public string maxPriorityFeePerGas { get; set; }
        public string paymasterAndData { get; set; }
        public string signature { get; set; }
    }

    public class SmartWallet
    {
        private bool _deployed;
        private bool _deploying;
        private bool _initialized;

        public List<string> Accounts { get; internal set; }
        public string PersonalAddress { get; internal set; }
        public Web3 PersonalWeb3 { get; internal set; }
        public ThirdwebSDK.SmartWalletConfig Config { get; internal set; }
        public bool IsDeployed => _deployed;
        public bool IsDeploying => _deploying;

        public SmartWallet(Web3 personalWeb3, ThirdwebSDK.SmartWalletConfig config)
        {
            PersonalWeb3 = personalWeb3;
            Config = new ThirdwebSDK.SmartWalletConfig()
            {
                factoryAddress = config.factoryAddress,
                gasless = config.gasless,
                bundlerUrl = string.IsNullOrEmpty(config.bundlerUrl) ? $"https://{ThirdwebManager.Instance.SDK.session.CurrentChainData.chainName}.bundler.thirdweb.com" : config.bundlerUrl,
                paymasterUrl = string.IsNullOrEmpty(config.paymasterUrl) ? $"https://{ThirdwebManager.Instance.SDK.session.CurrentChainData.chainName}.bundler.thirdweb.com" : config.paymasterUrl,
                entryPointAddress = string.IsNullOrEmpty(config.entryPointAddress) ? Constants.DEFAULT_ENTRYPOINT_ADDRESS : config.entryPointAddress,
            };

            _deployed = false;
            _initialized = false;
            _deploying = false;
        }

        internal async Task<string> GetPersonalAddress()
        {
            var accounts = await PersonalWeb3.Eth.Accounts.SendRequestAsync();
            return Nethereum.Util.AddressUtil.Current.ConvertToChecksumAddress(accounts[0]);
        }

        internal async Task Initialize(string smartWalletOverride = null)
        {
            if (_initialized)
                return;

            PersonalAddress = await GetPersonalAddress();

            var predictedAccount =
                smartWalletOverride
                ?? (
                    await TransactionManager.ThirdwebRead<FactoryContract.GetAddressFunction, FactoryContract.GetAddressOutputDTO>(
                        Config.factoryAddress,
                        new FactoryContract.GetAddressFunction() { AdminSigner = PersonalAddress, Data = new byte[] { } }
                    )
                ).ReturnValue1;

            Accounts = new List<string>() { predictedAccount };

            await UpdateDeploymentStatus();

            _initialized = true;

            ThirdwebDebug.Log($"Initialized with Factory: {Config.factoryAddress}, AdminSigner: {PersonalAddress}, Predicted Account: {Accounts[0]}, Deployed: {_deployed}");
        }

        internal async Task UpdateDeploymentStatus()
        {
            var bytecode = await Utils.GetWeb3().Eth.GetCode.SendRequestAsync(Accounts[0]);
            _deployed = bytecode != "0x";
        }

        internal async Task<TransactionResult> SetPermissionsForSigner(SignerPermissionRequest signerPermissionRequest, byte[] signature)
        {
            return await TransactionManager.ThirdwebWrite(Accounts[0], new SetPermissionsForSignerFunction() { Req = signerPermissionRequest, Signature = signature });
        }

        internal async Task ForceDeploy()
        {
            if (_deployed)
                return;

            var input = new TransactionInput("0x", Accounts[0], new HexBigInteger(0));
            var txHash = await Request(new RpcRequestMessage(1, "eth_sendTransaction", input));
            await Transaction.WaitForTransactionResult(txHash.Result.ToString());
            await UpdateDeploymentStatus();
        }

        internal async Task<bool> VerifySignature(byte[] hash, byte[] signature)
        {
            var verifyRes = await TransactionManager.ThirdwebRead<AccountContract.IsValidSignatureFunction, AccountContract.IsValidSignatureOutputDTO>(
                Accounts[0],
                new AccountContract.IsValidSignatureFunction() { Hash = hash, Signature = signature }
            );
            return verifyRes.MagicValue.ToHex(true) == new byte[] { 0x16, 0x26, 0xba, 0x7e }.ToHex(true);
        }

        internal async Task<(byte[] initCode, BigInteger gas)> GetInitCode()
        {
            if (_deployed)
                return (new byte[] { }, 0);

            var fn = new FactoryContract.CreateAccountFunction() { Admin = PersonalAddress, Data = new byte[] { } };
            var deployHandler = Utils.GetWeb3().Eth.GetContractTransactionHandler<FactoryContract.CreateAccountFunction>();
            var txInput = await deployHandler.CreateTransactionInputEstimatingGasAsync(Config.factoryAddress, fn);
            var data = Utils.HexConcat(Config.factoryAddress, txInput.Data);
            return (data.HexStringToByteArray(), txInput.Gas.Value);
        }

        internal async Task<RpcResponseMessage> Request(RpcRequestMessage requestMessage)
        {
            ThirdwebDebug.Log("Requesting: " + requestMessage.Method + "...");

            if (requestMessage.Method == "eth_sendTransaction")
            {
                return await CreateUserOpAndSend(requestMessage);
            }
            else if (requestMessage.Method == "eth_chainId")
            {
                try
                {
                    var chainId = await PersonalWeb3.Eth.ChainId.SendRequestAsync();
                    return new RpcResponseMessage(requestMessage.Id, chainId.HexValue);
                }
                catch
                {
                    return new RpcResponseMessage(requestMessage.Id, ThirdwebManager.Instance.SDK.session.CurrentChainData.chainId);
                }
            }
            else if (requestMessage.Method == "eth_estimateGas")
            {
                var web3 = Utils.GetWeb3();
                var parameters = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(requestMessage.RawParameters));
                var txInput = JsonConvert.DeserializeObject<TransactionInput>(JsonConvert.SerializeObject(parameters[0]));
                var result = await web3.Eth.Transactions.EstimateGas.SendRequestAsync(txInput);
                return new RpcResponseMessage(requestMessage.Id, result.HexValue);
            }
            else
            {
                throw new NotImplementedException("Method not supported: " + requestMessage.Method);
            }
        }

        private async Task<RpcResponseMessage> CreateUserOpAndSend(RpcRequestMessage requestMessage)
        {
            await new WaitUntil(() => !_deploying);

            await UpdateDeploymentStatus();
            if (!_deployed)
            {
                _deploying = true;
            }

            string apiKey = ThirdwebManager.Instance.SDK.session.Options.clientId;

            // Deserialize the transaction input from the request message

            var paramList = JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(requestMessage.RawParameters));
            var transactionInput = JsonConvert.DeserializeObject<TransactionInput>(JsonConvert.SerializeObject(paramList[0]));
            var dummySig = Constants.DUMMY_SIG;

            var (initCode, gas) = await GetInitCode();

            var executeFn = new AccountContract.ExecuteFunction
            {
                Target = transactionInput.To,
                Value = transactionInput.Value.Value,
                Calldata = transactionInput.Data.HexStringToByteArray(),
                FromAddress = Accounts[0]
            };
            var executeInput = executeFn.CreateTransactionInput(Accounts[0]);

            // Create the user operation and its safe (hexified) version

            var gasPrices = await Utils.GetGasPriceAsync(ThirdwebManager.Instance.SDK.session.ChainId);

            var partialUserOp = new EntryPointContract.UserOperation()
            {
                Sender = Accounts[0],
                Nonce = await GetNonce(),
                InitCode = initCode,
                CallData = executeInput.Data.HexStringToByteArray(),
                CallGasLimit = 0,
                VerificationGasLimit = 0,
                PreVerificationGas = 0,
                MaxFeePerGas = gasPrices.MaxFeePerGas,
                MaxPriorityFeePerGas = gasPrices.MaxPriorityFeePerGas,
                PaymasterAndData = new byte[] { },
                Signature = dummySig.HexStringToByteArray(),
            };

            // Update paymaster data if any

            partialUserOp.PaymasterAndData = await GetPaymasterAndData(requestMessage.Id, partialUserOp.EncodeUserOperation(), apiKey);

            // Estimate gas

            var gasEstimates = await BundlerClient.EthEstimateUserOperationGas(Config.bundlerUrl, apiKey, requestMessage.Id, partialUserOp.EncodeUserOperation(), Config.entryPointAddress);
            partialUserOp.CallGasLimit = 50000 + new HexBigInteger(gasEstimates.CallGasLimit).Value;
            partialUserOp.VerificationGasLimit = new HexBigInteger(gasEstimates.VerificationGas).Value;
            partialUserOp.PreVerificationGas = new HexBigInteger(gasEstimates.PreVerificationGas).Value;

            // Update paymaster data if any

            partialUserOp.PaymasterAndData = await GetPaymasterAndData(requestMessage.Id, partialUserOp.EncodeUserOperation(), apiKey);

            // Hash, sign and encode the user operation

            partialUserOp.Signature = await partialUserOp.HashAndSignUserOp(Config.entryPointAddress);

            // Send the user operation

            ThirdwebDebug.Log("Valid UserOp: " + JsonConvert.SerializeObject(partialUserOp));
            ThirdwebDebug.Log("Valid Encoded UserOp: " + JsonConvert.SerializeObject(partialUserOp.EncodeUserOperation()));

            string userOpHash = null;

            if (new Uri(Config.bundlerUrl).Host.EndsWith("thirdweb.com"))
            {
                try
                {
                    ThirdwebDebug.Log("Compressing...");
                    var inflatorContract = ThirdwebManager.Instance.SDK.GetContract(
                        "0x564c7dC50f8293d070F490Fc31fEc3A0A091b9bB",
                        "[{\"inputs\": [{\"components\": [{\"internalType\": \"address\",\"name\": \"sender\",\"type\": \"address\"},{\"internalType\": \"uint256\",\"name\": \"nonce\",\"type\": \"uint256\"},{\"internalType\": \"bytes\",\"name\": \"initCode\",\"type\": \"bytes\"},{\"internalType\": \"bytes\",\"name\": \"callData\",\"type\": \"bytes\"},{\"internalType\": \"uint256\",\"name\": \"callGasLimit\",\"type\": \"uint256\"},{\"internalType\": \"uint256\",\"name\": \"verificationGasLimit\",\"type\": \"uint256\"},{\"internalType\": \"uint256\",\"name\": \"preVerificationGas\",\"type\": \"uint256\"},{\"internalType\": \"uint256\",\"name\": \"maxFeePerGas\",\"type\": \"uint256\"},{\"internalType\": \"uint256\",\"name\": \"maxPriorityFeePerGas\",\"type\": \"uint256\"},{\"internalType\": \"bytes\",\"name\": \"paymasterAndData\",\"type\": \"bytes\"},{\"internalType\": \"bytes\",\"name\": \"signature\",\"type\": \"bytes\"}],\"internalType\": \"struct UserOperation\",\"name\": \"op\",\"type\": \"tuple\"}],\"name\": \"compress\",\"outputs\": [{\"internalType\": \"bytes\",\"name\": \"compressed\",\"type\": \"bytes\"}],\"stateMutability\": \"pure\",\"type\": \"function\"},{\"inputs\": [{\"internalType\": \"bytes\",\"name\": \"compressed\",\"type\": \"bytes\"}],\"name\": \"inflate\",\"outputs\": [{\"components\": [{\"internalType\": \"address\",\"name\": \"sender\",\"type\": \"address\"},{\"internalType\": \"uint256\",\"name\": \"nonce\",\"type\": \"uint256\"},{\"internalType\": \"bytes\",\"name\": \"initCode\",\"type\": \"bytes\"},{\"internalType\": \"bytes\",\"name\": \"callData\",\"type\": \"bytes\"},{\"internalType\": \"uint256\",\"name\": \"callGasLimit\",\"type\": \"uint256\"},{\"internalType\": \"uint256\",\"name\": \"verificationGasLimit\",\"type\": \"uint256\"},{\"internalType\": \"uint256\",\"name\": \"preVerificationGas\",\"type\": \"uint256\"},{\"internalType\": \"uint256\",\"name\": \"maxFeePerGas\",\"type\": \"uint256\"},{\"internalType\": \"uint256\",\"name\": \"maxPriorityFeePerGas\",\"type\": \"uint256\"},{\"internalType\": \"bytes\",\"name\": \"paymasterAndData\",\"type\": \"bytes\"},{\"internalType\": \"bytes\",\"name\": \"signature\",\"type\": \"bytes\"}],\"internalType\": \"struct UserOperation\",\"name\": \"op\",\"type\": \"tuple\"}],\"stateMutability\": \"pure\",\"type\": \"function\"}]"
                    );
                    byte[] compressedUserOp = await inflatorContract.Read<byte[]>("compress", partialUserOp);
                    userOpHash = await BundlerClient.EthSendCompressedUserOperation(
                        Config.bundlerUrl,
                        apiKey,
                        requestMessage.Id,
                        compressedUserOp.ByteArrayToHexString(),
                        inflatorContract.address,
                        Config.entryPointAddress
                    );
                    ThirdwebDebug.Log("Compressed successfully");
                }
                catch (Exception e)
                {
                    ThirdwebDebug.LogWarning($"Compression failed, sending uncompressed. Error: {e.Message}");
                    userOpHash = null;
                }
            }

            if (userOpHash == null)
            {
                userOpHash = await BundlerClient.EthSendUserOperation(Config.bundlerUrl, apiKey, requestMessage.Id, partialUserOp.EncodeUserOperation(), Config.entryPointAddress);
                ThirdwebDebug.Log("UserOp Hash: " + userOpHash);
            }

            // Wait for the transaction to be mined

            string txHash = null;
            while (txHash == null)
            {
                var userOpReceipt = await BundlerClient.EthGetUserOperationReceipt(Config.bundlerUrl, apiKey, requestMessage.Id, userOpHash);
                txHash = userOpReceipt?.receipt?.TransactionHash;
                await new WaitForSecondsRealtime(1f);
            }
            ThirdwebDebug.Log("Tx Hash: " + txHash);

            // Check if successful

            if (!_deployed)
            {
                var receipt = await Transaction.WaitForTransactionResultRaw(txHash);
                var decodedEvents = receipt.DecodeAllEvents<EntryPointContract.UserOperationEventEventDTO>();
                if (decodedEvents[0].Event.Success == false)
                {
                    throw new Exception($"Transaction {txHash} execution reverted");
                }
                else
                {
                    ThirdwebDebug.Log("Transaction successful");
                    _deployed = true;
                }
            }

            _deploying = false;

            return new RpcResponseMessage(requestMessage.Id, txHash);
        }

        private async Task<BigInteger> GetNonce()
        {
            var nonce = await TransactionManager.ThirdwebRead<EntryPointContract.GetNonceFunction, EntryPointContract.GetNonceOutputDTO>(
                Config.entryPointAddress,
                new EntryPointContract.GetNonceFunction() { Sender = Accounts[0], Key = UserOpUtils.GetRandomInt192() }
            );
            return nonce.Nonce;
        }

        private async Task<byte[]> GetPaymasterAndData(object requestId, UserOperationHexified userOp, string apiKey)
        {
            return Config.gasless
                ? (await BundlerClient.PMSponsorUserOperation(Config.paymasterUrl, apiKey, requestId, userOp, Config.entryPointAddress)).paymasterAndData.HexStringToByteArray()
                : new byte[] { };
        }
    }
}
