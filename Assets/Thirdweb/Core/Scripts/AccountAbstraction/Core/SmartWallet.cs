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
        public List<string> Accounts { get; internal set; }
        public string PersonalAddress { get; internal set; }
        public Web3 PersonalWeb3 { get; internal set; }
        public ThirdwebSDK.SmartWalletConfig Config { get; internal set; }

        private bool _initialized;
        private bool _deployed;

        public SmartWallet(Web3 personalWeb3, ThirdwebSDK.SmartWalletConfig config)
        {
            PersonalWeb3 = personalWeb3;
            Config = new ThirdwebSDK.SmartWalletConfig()
            {
                factoryAddress = config.factoryAddress,
                thirdwebApiKey = config.thirdwebApiKey,
                gasless = config.gasless,
                bundlerUrl = string.IsNullOrEmpty(config.bundlerUrl) ? $"https://{ThirdwebManager.Instance.SDK.session.CurrentChainData.chainName}.bundler.thirdweb.com" : config.bundlerUrl,
                paymasterUrl = string.IsNullOrEmpty(config.paymasterUrl) ? $"https://{ThirdwebManager.Instance.SDK.session.CurrentChainData.chainName}.bundler.thirdweb.com" : config.paymasterUrl,
                entryPointAddress = string.IsNullOrEmpty(config.entryPointAddress) ? Constants.DEFAULT_ENTRYPOINT_ADDRESS : config.entryPointAddress,
            };

            _deployed = false;
            _initialized = false;
        }

        internal async Task<string> GetPersonalAddress()
        {
            var accounts = await PersonalWeb3.Eth.Accounts.SendRequestAsync();
            return Nethereum.Util.AddressUtil.Current.ConvertToChecksumAddress(accounts[0]);
        }

        internal async Task Initialize()
        {
            if (_initialized)
                return;

            PersonalAddress = await GetPersonalAddress();

            var predictedAccount = await TransactionManager.ThirdwebRead<FactoryContract.GetAddressFunction, FactoryContract.GetAddressOutputDTO>(
                Config.factoryAddress,
                new FactoryContract.GetAddressFunction() { AdminSigner = PersonalAddress, Data = new byte[] { } }
            );

            Accounts = new List<string>() { predictedAccount.ReturnValue1 };

            await UpdateDeploymentStatus();

            _initialized = true;

            Debug.Log($"Initialized with Factory: {Config.factoryAddress}, AdminSigner: {PersonalAddress}, Predicted Account: {Accounts[0]}, Deployed: {_deployed}");
        }

        internal async Task UpdateDeploymentStatus()
        {
            var bytecode = await new Web3(ThirdwebManager.Instance.SDK.session.RPC).Eth.GetCode.SendRequestAsync(Accounts[0]);
            _deployed = bytecode != "0x";
        }

        internal async Task<(byte[] initCode, BigInteger gas)> GetInitCode()
        {
            if (_deployed)
                return (new byte[] { }, 0);

            var fn = new FactoryContract.CreateAccountFunction() { Admin = PersonalAddress, Data = new byte[] { } };
            var deployHandler = new Web3(ThirdwebManager.Instance.SDK.session.RPC).Eth.GetContractTransactionHandler<FactoryContract.CreateAccountFunction>();
            var txInput = await deployHandler.CreateTransactionInputEstimatingGasAsync(Config.factoryAddress, fn);
            var data = Utils.HexConcat(Config.factoryAddress, txInput.Data);
            return (data.HexStringToByteArray(), txInput.Gas.Value);
        }

        internal async Task<RpcResponseMessage> Request(RpcRequestMessage requestMessage)
        {
            Debug.Log("Requesting: " + requestMessage.Method + "...");

            if (requestMessage.Method == "eth_chainId")
            {
                var chainId = await PersonalWeb3.Eth.ChainId.SendRequestAsync();
                return new RpcResponseMessage(requestMessage.Id, chainId.HexValue);
            }
            else if (requestMessage.Method == "eth_sendTransaction")
            {
                return await CreateUserOpAndSend(requestMessage);
            }
            else
            {
                throw new NotImplementedException("Method not supported: " + requestMessage.Method);
            }
        }

        private async Task<RpcResponseMessage> CreateUserOpAndSend(RpcRequestMessage requestMessage)
        {
            // Deserialize the transaction input from the request message

            var paramList = JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(requestMessage.RawParameters));
            var transactionInput = JsonConvert.DeserializeObject<TransactionInput>(JsonConvert.SerializeObject(paramList[0]));
            var latestBlock = await Utils.GetBlockByNumber(await Utils.GetLatestBlockNumber());
            var dummySig = new byte[Constants.DUMMY_SIG_LENGTH];
            for (int i = 0; i < Constants.DUMMY_SIG_LENGTH; i++)
                dummySig[i] = 0x01;

            await UpdateDeploymentStatus();
            var initData = await GetInitCode();

            var executeFn = new AccountContract.ExecuteFunction()
            {
                Target = transactionInput.To,
                Value = transactionInput.Value.Value,
                Calldata = transactionInput.Data.HexStringToByteArray(),
            };
            executeFn.FromAddress = Accounts[0];
            var executeInput = executeFn.CreateTransactionInput(Accounts[0]);

            // Create the user operation and its safe (hexified) version

            var partialUserOp = new EntryPointContract.UserOperation()
            {
                Sender = Accounts[0],
                Nonce = await GetNonce(),
                InitCode = initData.initCode,
                CallData = executeInput.Data.HexStringToByteArray(),
                CallGasLimit = transactionInput.Gas.Value,
                VerificationGasLimit = 100000 + initData.gas,
                PreVerificationGas = 21000,
                MaxFeePerGas = latestBlock.BaseFeePerGas.Value * 2 + BigInteger.Parse("1500000000"),
                MaxPriorityFeePerGas = BigInteger.Parse("1500000000"),
                PaymasterAndData = Constants.DUMMY_PAYMASTER_AND_DATA_HEX.HexStringToByteArray(),
                Signature = dummySig,
            };
            partialUserOp.PreVerificationGas = partialUserOp.CalcPreVerificationGas();
            var partialUserOpHexified = partialUserOp.EncodeUserOperation();

            // Update paymaster data if any

            partialUserOp.PaymasterAndData = await GetPaymasterAndData(requestMessage.Id, partialUserOpHexified);

            // Hash, sign and encode the user operation

            partialUserOp.Signature = await partialUserOp.HashAndSignUserOp(Config.entryPointAddress);
            partialUserOpHexified = partialUserOp.EncodeUserOperation();

            // Send the user operation

            Debug.Log("Valid UserOp: " + JsonConvert.SerializeObject(partialUserOp));
            Debug.Log("Valid Encoded UserOp: " + JsonConvert.SerializeObject(partialUserOpHexified));
            var userOpHash = await BundlerClient.EthSendUserOperation(Config.bundlerUrl, Config.thirdwebApiKey, requestMessage.Id, partialUserOpHexified, Config.entryPointAddress);
            Debug.Log("UserOp Hash: " + userOpHash);

            // Wait for the transaction to be mined

            string txHash = null;
            while (txHash == null && Application.isPlaying)
            {
                var getUserOpResponse = await BundlerClient.EthGetUserOperationByHash(Config.bundlerUrl, Config.thirdwebApiKey, requestMessage.Id, userOpHash);
                txHash = getUserOpResponse?.transactionHash;
                await new WaitForSecondsRealtime(5f);
            }
            Debug.Log("Tx Hash: " + txHash);

            // Check if successful

            var receipt = await new Web3(ThirdwebManager.Instance.SDK.session.RPC).Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            var decodedEvents = receipt.DecodeAllEvents<EntryPointContract.UserOperationEventEventDTO>();
            if (decodedEvents[0].Event.Success == false)
            {
                Debug.Log("Transaction not successful, checking reason...");
                var reason = await new Web3(ThirdwebManager.Instance.SDK.session.RPC).Eth.GetContractTransactionErrorReason.SendRequestAsync(txHash);
                throw new Exception($"Transaction {txHash} reverted with reason: {reason}");
            }
            else
            {
                Debug.Log("Transaction successful");
                _deployed = true;
            }

            return new RpcResponseMessage(requestMessage.Id, txHash);
        }

        private async Task<BigInteger> GetNonce()
        {
            var nonce = await TransactionManager.ThirdwebRead<AccountContract.GetNonceFunction, AccountContract.GetNonceOutputDTO>(Accounts[0], new AccountContract.GetNonceFunction() { });
            return nonce.ReturnValue1;
        }

        private async Task<byte[]> GetPaymasterAndData(object requestId, UserOperationHexified userOp)
        {
            return Config.gasless
                ? (await BundlerClient.PMSponsorUserOperation(Config.paymasterUrl, Config.thirdwebApiKey, requestId, userOp, Config.entryPointAddress)).paymasterAndData.HexStringToByteArray()
                : new byte[] { };
        }
    }
}
