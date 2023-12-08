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
            var latestBlock = await Blocks.GetBlock(await Blocks.GetLatestBlockNumber());
            var dummySig = new byte[Constants.DUMMY_SIG_LENGTH];
            for (int i = 0; i < Constants.DUMMY_SIG_LENGTH; i++)
                dummySig[i] = 0x01;

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

            var partialUserOp = new EntryPointContract.UserOperation()
            {
                Sender = Accounts[0],
                Nonce = await GetNonce(),
                InitCode = initCode,
                CallData = executeInput.Data.HexStringToByteArray(),
                CallGasLimit = 50000 + (transactionInput.Gas != null ? (transactionInput.Gas.Value < 21000 ? 100000 : transactionInput.Gas.Value) : 100000),
                VerificationGasLimit = 100000 + gas,
                PreVerificationGas = 21000,
                MaxFeePerGas = latestBlock.BaseFeePerGas.Value * 2 + BigInteger.Parse("1500000000"),
                MaxPriorityFeePerGas = BigInteger.Parse("1500000000"),
                PaymasterAndData = Constants.DUMMY_PAYMASTER_AND_DATA_HEX.HexStringToByteArray(),
                Signature = dummySig,
            };
            partialUserOp.PreVerificationGas = partialUserOp.CalcPreVerificationGas();
            var partialUserOpHexified = partialUserOp.EncodeUserOperation();

            // Update paymaster data if any

            partialUserOp.PaymasterAndData = await GetPaymasterAndData(requestMessage.Id, partialUserOpHexified, apiKey);

            // Hash, sign and encode the user operation

            partialUserOp.Signature = await partialUserOp.HashAndSignUserOp(Config.entryPointAddress);
            partialUserOpHexified = partialUserOp.EncodeUserOperation();

            // Send the user operation

            ThirdwebDebug.Log("Valid UserOp: " + JsonConvert.SerializeObject(partialUserOp));
            ThirdwebDebug.Log("Valid Encoded UserOp: " + JsonConvert.SerializeObject(partialUserOpHexified));
            var userOpHash = await BundlerClient.EthSendUserOperation(Config.bundlerUrl, apiKey, requestMessage.Id, partialUserOpHexified, Config.entryPointAddress);
            ThirdwebDebug.Log("UserOp Hash: " + userOpHash);

            // Wait for the transaction to be mined

            string txHash = null;
            while (txHash == null)
            {
                var getUserOpResponse = await BundlerClient.EthGetUserOperationByHash(Config.bundlerUrl, apiKey, requestMessage.Id, userOpHash);
                txHash = getUserOpResponse?.transactionHash;
                await new WaitForSecondsRealtime(2f);
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
