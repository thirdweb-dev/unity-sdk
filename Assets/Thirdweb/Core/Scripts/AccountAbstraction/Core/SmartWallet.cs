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
using Org.BouncyCastle.Utilities.Encoders;
using System.Linq;
using Nethereum.Util;
using Nethereum.RLP;

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
        private bool _approved;
        private bool _approving;

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
                erc20PaymasterAddress = config.erc20PaymasterAddress,
                erc20TokenAddress = config.erc20TokenAddress,
                bundlerUrl = string.IsNullOrEmpty(config.bundlerUrl) ? $"https://{ThirdwebManager.Instance.SDK.session.CurrentChainData.chainName}.bundler.thirdweb.com" : config.bundlerUrl,
                paymasterUrl = string.IsNullOrEmpty(config.paymasterUrl) ? $"https://{ThirdwebManager.Instance.SDK.session.CurrentChainData.chainName}.bundler.thirdweb.com" : config.paymasterUrl,
                entryPointAddress = string.IsNullOrEmpty(config.entryPointAddress) ? Constants.DEFAULT_ENTRYPOINT_ADDRESS : config.entryPointAddress,
            };

            _deployed = false;
            _initialized = false;
            _deploying = false;
            _approved = false;
            _approving = false;
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

            var executeFn = new AccountContract.ExecuteFunction
            {
                Target = transactionInput.To,
                Value = transactionInput.Value.Value,
                Calldata = transactionInput.Data.HexStringToByteArray(),
                FromAddress = Accounts[0]
            };
            var executeInput = executeFn.CreateTransactionInput(Accounts[0]);

            // Approve ERC20 tokens if any

            if (!string.IsNullOrEmpty(Config.erc20PaymasterAddress) && !_approved && !_approving)
            {
                try
                {
                    _approving = true;
                    var tokenContract = ThirdwebManager.Instance.SDK.GetContract(Config.erc20TokenAddress);
                    var approvedAmount = await tokenContract.ERC20.AllowanceOf(Accounts[0], Config.erc20PaymasterAddress);
                    if (BigInteger.Parse(approvedAmount.value) == 0)
                    {
                        ThirdwebDebug.Log($"Approving tokens for ERC20Paymaster spending");
                        _deploying = false;
                        await tokenContract.ERC20.SetAllowance(Config.erc20PaymasterAddress, (BigInteger.Pow(2, 96) - 1).ToString().ToEth());
                    }
                    _approved = true;
                    _approving = false;
                    await UpdateDeploymentStatus();
                }
                catch (Exception e)
                {
                    _approving = false;
                    _approved = false;
                    throw new Exception($"Approving tokens for ERC20Paymaster spending failed: {e.Message}");
                }
            }

            // Create the user operation and its safe (hexified) version

            var (initCode, gas) = await GetInitCode();

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

            if (Config.gasless && (string.IsNullOrEmpty(Config.erc20PaymasterAddress) || _approving)) // TODO: remove once simulation with minimal ERC20 amounts is in
                partialUserOp.PaymasterAndData = await GetPaymasterAndData(requestMessage.Id, partialUserOp.EncodeUserOperation(), apiKey);

            // Estimate gas

            var gasEstimates = await BundlerClient.EthEstimateUserOperationGas(Config.bundlerUrl, apiKey, requestMessage.Id, partialUserOp.EncodeUserOperation(), Config.entryPointAddress);
            partialUserOp.CallGasLimit = 50000 + new HexBigInteger(gasEstimates.CallGasLimit).Value;
            partialUserOp.VerificationGasLimit = string.IsNullOrEmpty(Config.erc20PaymasterAddress)
                ? new HexBigInteger(gasEstimates.VerificationGas).Value
                : new HexBigInteger(gasEstimates.VerificationGas).Value * 3;
            partialUserOp.PreVerificationGas = new HexBigInteger(gasEstimates.PreVerificationGas).Value;

            // Update paymaster data if any

            partialUserOp.PaymasterAndData = await GetPaymasterAndData(requestMessage.Id, partialUserOp.EncodeUserOperation(), apiKey);

            // Hash, sign and encode the user operation

            partialUserOp.Signature = await partialUserOp.HashAndSignUserOp(Config.entryPointAddress);

            // Send the user operation

            ThirdwebDebug.Log("Valid UserOp: " + JsonConvert.SerializeObject(partialUserOp));
            ThirdwebDebug.Log("Valid Encoded UserOp: " + JsonConvert.SerializeObject(partialUserOp.EncodeUserOperation()));
            var userOpHash = await BundlerClient.EthSendUserOperation(Config.bundlerUrl, apiKey, requestMessage.Id, partialUserOp.EncodeUserOperation(), Config.entryPointAddress);
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
            if (!string.IsNullOrEmpty(Config.erc20PaymasterAddress) && !_approving)
            {
                return Config.erc20PaymasterAddress.HexToByteArray();
            }
            else if (Config.gasless)
            {
                var paymasterAndData = await BundlerClient.PMSponsorUserOperation(Config.paymasterUrl, apiKey, requestId, userOp, Config.entryPointAddress);
                return paymasterAndData.paymasterAndData.HexToByteArray();
            }
            else
            {
                return new byte[] { };
            }
        }

        // private async Task<BigInteger> CalculateTokenAmount(UserOperationHexified userOp)
        // {
        //     string abi =
        //         "[{\"inputs\":[{\"internalType\":\"contract IERC20Metadata\",\"name\":\"_token\",\"type\":\"address\"},{\"internalType\":\"contract IEntryPoint\",\"name\":\"_entryPoint\",\"type\":\"address\"},{\"internalType\":\"contract IOracle\",\"name\":\"_tokenOracle\",\"type\":\"address\"},{\"internalType\":\"contract IOracle\",\"name\":\"_nativeAssetOracle\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"_owner\",\"type\":\"address\"}],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"uint32\",\"name\":\"priceMarkup\",\"type\":\"uint32\"},{\"indexed\":false,\"internalType\":\"uint32\",\"name\":\"updateThreshold\",\"type\":\"uint32\"}],\"name\":\"ConfigUpdated\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"previousOwner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"newOwner\",\"type\":\"address\"}],\"name\":\"OwnershipTransferred\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"user\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"actualTokenNeeded\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"actualGasCost\",\"type\":\"uint256\"}],\"name\":\"UserOperationSponsored\",\"type\":\"event\"},{\"inputs\":[],\"name\":\"REFUND_POSTOP_COST\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint32\",\"name\":\"unstakeDelaySec\",\"type\":\"uint32\"}],\"name\":\"addStake\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"deposit\",\"outputs\":[],\"stateMutability\":\"payable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"entryPoint\",\"outputs\":[{\"internalType\":\"contract IEntryPoint\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getDeposit\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"nativeAssetOracle\",\"outputs\":[{\"internalType\":\"contract IOracle\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"owner\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"enum IPaymaster.PostOpMode\",\"name\":\"mode\",\"type\":\"uint8\"},{\"internalType\":\"bytes\",\"name\":\"context\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"actualGasCost\",\"type\":\"uint256\"}],\"name\":\"postOp\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"previousPrice\",\"outputs\":[{\"internalType\":\"uint192\",\"name\":\"\",\"type\":\"uint192\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"priceDenominator\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"priceMarkup\",\"outputs\":[{\"internalType\":\"uint32\",\"name\":\"\",\"type\":\"uint32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"priceUpdateThreshold\",\"outputs\":[{\"internalType\":\"uint32\",\"name\":\"\",\"type\":\"uint32\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"renounceOwnership\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"token\",\"outputs\":[{\"internalType\":\"contract IERC20\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"tokenDecimals\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"tokenOracle\",\"outputs\":[{\"internalType\":\"contract IOracle\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"newOwner\",\"type\":\"address\"}],\"name\":\"transferOwnership\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"unlockStake\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint32\",\"name\":\"_priceMarkup\",\"type\":\"uint32\"},{\"internalType\":\"uint32\",\"name\":\"_updateThreshold\",\"type\":\"uint32\"}],\"name\":\"updateConfig\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"updatePrice\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"address\",\"name\":\"sender\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"nonce\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"initCode\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"callData\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"callGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"verificationGasLimit\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"preVerificationGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxPriorityFeePerGas\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"paymasterAndData\",\"type\":\"bytes\"},{\"internalType\":\"bytes\",\"name\":\"signature\",\"type\":\"bytes\"}],\"internalType\":\"struct UserOperation\",\"name\":\"userOp\",\"type\":\"tuple\"},{\"internalType\":\"bytes32\",\"name\":\"userOpHash\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"maxCost\",\"type\":\"uint256\"}],\"name\":\"validatePaymasterUserOp\",\"outputs\":[{\"internalType\":\"bytes\",\"name\":\"context\",\"type\":\"bytes\"},{\"internalType\":\"uint256\",\"name\":\"validationData\",\"type\":\"uint256\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address payable\",\"name\":\"withdrawAddress\",\"type\":\"address\"}],\"name\":\"withdrawStake\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address payable\",\"name\":\"withdrawAddress\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"withdrawTo\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"amount\",\"type\":\"uint256\"}],\"name\":\"withdrawToken\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"}]";
        //     var contract = Utils.GetWeb3().Eth.GetContract(abi, Config.erc20PaymasterAddress);
        //     var priceMarkup = await contract.GetFunction("priceMarkup").CallAsync<BigInteger>();
        //     var cachedPrice = await contract.GetFunction("previousPrice").CallAsync<BigInteger>();
        //     if (cachedPrice == BigInteger.Zero)
        //     {
        //         throw new Exception("ERC20Paymaster: no previous price set");
        //     }

        //     var preVerificationGas = new HexBigInteger(userOp.preVerificationGas).Value;
        //     var verificationGasLimit = new HexBigInteger(userOp.verificationGasLimit).Value;
        //     var callGasLimit = new HexBigInteger(userOp.callGasLimit).Value;
        //     var maxFeePerGas = new HexBigInteger(userOp.maxFeePerGas).Value;

        //     var requiredPreFund = (preVerificationGas + verificationGasLimit * 3 + callGasLimit) * maxFeePerGas;
        //     var tokenAmount = (requiredPreFund + maxFeePerGas * 40000) * priceMarkup * cachedPrice / BigInteger.Pow(10, 18) / 1000000; // 1e6 is the priceDenominator constant

        //     return tokenAmount;
        // }
    }
}
