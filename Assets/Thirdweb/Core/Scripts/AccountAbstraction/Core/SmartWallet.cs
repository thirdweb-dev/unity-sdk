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
using Thirdweb.Contracts.Account.ContractDefinition;
using Thirdweb.Wallets;
using Nethereum.Signer;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

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
        public IThirdwebWallet PersonalWallet { get; internal set; }
        public bool IsDeployed => _deployed;
        public bool IsDeploying => _deploying;

        private readonly ThirdwebSDK _sdk;
        private bool IsZkSync => _sdk.Session.ChainId == 300 || _sdk.Session.ChainId == 324;

        public SmartWallet(IThirdwebWallet personalWallet, ThirdwebSDK sdk)
        {
            PersonalWallet = personalWallet;
            _sdk = sdk;
            _deployed = false;
            _initialized = false;
            _deploying = false;
            _approved = false;
            _approving = false;
        }

        internal async Task<string> GetPersonalAddress()
        {
            return await PersonalWallet.GetAddress();
        }

        internal async Task Initialize(string smartWalletOverride = null)
        {
            if (_initialized)
                return;

            if (IsZkSync)
            {
                Accounts = new List<string>() { await GetPersonalAddress() };
                _initialized = true;
                return;
            }

            var predictedAccount =
                smartWalletOverride
                ?? (
                    await TransactionManager.ThirdwebRead<FactoryContract.GetAddressFunction, FactoryContract.GetAddressOutputDTO>(
                        _sdk,
                        _sdk.Session.Options.smartWalletConfig?.factoryAddress,
                        new FactoryContract.GetAddressFunction() { AdminSigner = await GetPersonalAddress(), Data = new byte[] { } }
                    )
                ).ReturnValue1;

            Accounts = new List<string>() { predictedAccount };

            await UpdateDeploymentStatus();

            _initialized = true;

            ThirdwebDebug.Log(
                $"Initialized with Factory: {_sdk.Session.Options.smartWalletConfig?.factoryAddress}, AdminSigner: {await GetPersonalAddress()}, Predicted Account: {Accounts[0]}, Deployed: {_deployed}"
            );
        }

        internal async Task UpdateDeploymentStatus()
        {
            if (IsZkSync)
            {
                _deployed = true;
                return;
            }

            var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            var bytecode = await web3.Eth.GetCode.SendRequestAsync(Accounts[0]);
            _deployed = bytecode != "0x";
        }

        internal async Task<TransactionResult> SetPermissionsForSigner(SignerPermissionRequest signerPermissionRequest, byte[] signature)
        {
            if (IsZkSync)
            {
                throw new NotImplementedException("SetPermissionsForSigner is not supported on zkSync");
            }
            return await TransactionManager.ThirdwebWrite(_sdk, Accounts[0], new SetPermissionsForSignerFunction() { Req = signerPermissionRequest, Signature = signature });
        }

        internal async Task ForceDeploy()
        {
            if (IsZkSync)
            {
                return;
            }

            if (_deployed)
                return;

            var input = new TransactionInput("0x", Accounts[0], new HexBigInteger(0));
            var txHash = await Request(new RpcRequestMessage(1, "eth_sendTransaction", input));
            await Transaction.WaitForTransactionResult(txHash.Result.ToString(), _sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            await UpdateDeploymentStatus();
        }

        internal async Task<bool> VerifySignature(byte[] hash, byte[] signature)
        {
            if (IsZkSync)
            {
                throw new NotImplementedException("VerifySignature is not supported on zkSync");
            }

            try
            {
                var verifyRes = await TransactionManager.ThirdwebRead<AccountContract.IsValidSignatureFunction, AccountContract.IsValidSignatureOutputDTO>(
                    _sdk,
                    Accounts[0],
                    new AccountContract.IsValidSignatureFunction() { Hash = hash, Signature = signature }
                );
                return verifyRes.MagicValue.ToHex(true) == new byte[] { 0x16, 0x26, 0xba, 0x7e }.ToHex(true);
            }
            catch (Exception e)
            {
                ThirdwebDebug.LogWarning("isValidSignature call failed: " + e.Message);
                return false;
            }
        }

        internal async Task<(byte[] initCode, BigInteger gas)> GetInitCode()
        {
            if (IsZkSync)
            {
                throw new NotImplementedException("GetInitCode is not supported on zkSync");
            }

            if (_deployed)
                return (new byte[] { }, 0);

            var fn = new FactoryContract.CreateAccountFunction() { Admin = await GetPersonalAddress(), Data = new byte[] { } };
            var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
            var deployHandler = web3.Eth.GetContractTransactionHandler<FactoryContract.CreateAccountFunction>();
            var txInput = await deployHandler.CreateTransactionInputEstimatingGasAsync(_sdk.Session.Options.smartWalletConfig?.factoryAddress, fn);
            var data = Utils.HexConcat(_sdk.Session.Options.smartWalletConfig?.factoryAddress, txInput.Data);
            return (data.HexStringToByteArray(), txInput.Gas.Value);
        }

        internal async Task<RpcResponseMessage> Request(RpcRequestMessage requestMessage)
        {
            ThirdwebDebug.Log("Requesting: " + requestMessage.Method + "...");

            if (requestMessage.Method == "eth_signTransaction")
            {
                if (IsZkSync)
                {
                    throw new NotImplementedException("eth_signTransaction is not supported on zkSync");
                }

                var parameters = JsonConvert.DeserializeObject<object[]>(JsonConvert.SerializeObject(requestMessage.RawParameters));
                var txInput = JsonConvert.DeserializeObject<TransactionInput>(JsonConvert.SerializeObject(parameters[0]));
                var partialUserOp = await SignTransactionAsUserOp(txInput, requestMessage.Id);
                return new RpcResponseMessage(requestMessage.Id, JsonConvert.SerializeObject(EncodeUserOperation(partialUserOp)));
            }
            else if (requestMessage.Method == "eth_sendTransaction")
            {
                if (IsZkSync)
                {
                    var paramList = JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(requestMessage.RawParameters));
                    var transactionInput = JsonConvert.DeserializeObject<TransactionInput>(JsonConvert.SerializeObject(paramList[0]));
                    var hash = await SendZkSyncAATransaction(transactionInput);
                    return new RpcResponseMessage(requestMessage.Id, hash);
                }
                return await CreateUserOpAndSend(requestMessage);
            }
            else if (requestMessage.Method == "eth_chainId")
            {
                try
                {
                    var chainId = await (await PersonalWallet.GetWeb3()).Eth.ChainId.SendRequestAsync();
                    return new RpcResponseMessage(requestMessage.Id, chainId.HexValue);
                }
                catch
                {
                    return new RpcResponseMessage(requestMessage.Id, _sdk.Session.CurrentChainData.chainId);
                }
            }
            else if (requestMessage.Method == "eth_estimateGas")
            {
                var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
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

        private async Task<string> SendZkSyncAATransaction(TransactionInput transactionInput)
        {
            var transaction = new Transaction(_sdk, transactionInput);
            var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);

            if (transactionInput.Nonce == null)
            {
                var nonce = await web3.Client.SendRequestAsync<HexBigInteger>(method: "eth_getTransactionCount", route: null, paramList: new object[] { Accounts[0], "latest" });
                _ = transaction.SetNonce(nonce.Value.ToString());
            }

            var feeData = await web3.Client.SendRequestAsync<JToken>(method: "zks_estimateFee", route: null, paramList: new object[] { transactionInput, "latest" });
            var maxFee = feeData["max_fee_per_gas"].ToObject<HexBigInteger>().Value * 10 / 5;
            var maxPriorityFee = feeData["max_priority_fee_per_gas"].ToObject<HexBigInteger>().Value * 10 / 5;
            var gasPerPubData = feeData["gas_per_pubdata_limit"].ToObject<HexBigInteger>().Value;
            var gasLimit = feeData["gas_limit"].ToObject<HexBigInteger>().Value * 10 / 5;

            if (_sdk.Session.Options.smartWalletConfig?.gasless == true)
            {
                var pmDataResult = await BundlerClient.ZkPaymasterData(
                    _sdk.Session.Options.smartWalletConfig?.paymasterUrl,
                    _sdk.Session.Options.clientId,
                    _sdk.Session.Options.bundleId,
                    1,
                    transactionInput
                );

                var zkTx = new AccountAbstraction.ZkSyncAATransaction
                {
                    TxType = 0x71,
                    From = new HexBigInteger(transaction.Input.From).Value,
                    To = new HexBigInteger(transaction.Input.To).Value,
                    GasLimit = gasLimit,
                    GasPerPubdataByteLimit = gasPerPubData,
                    MaxFeePerGas = maxFee,
                    MaxPriorityFeePerGas = maxPriorityFee,
                    Paymaster = new HexBigInteger(pmDataResult.paymaster).Value,
                    Nonce = transaction.Input.Nonce.Value,
                    Value = transaction.Input.Value?.Value ?? 0,
                    Data = transaction.Input.Data?.HexToByteArray() ?? new byte[0],
                    FactoryDeps = new List<byte[]>(),
                    PaymasterInput = pmDataResult.paymasterInput?.HexToByteArray() ?? new byte[0]
                };

                var zkTxSigned = await EIP712.GenerateSignature_ZkSyncTransaction(_sdk, "zkSync", "2", _sdk.Session.ChainId, zkTx);

                // Match bundler ZkTransactionInput type without recreating
                var zkBroadcastResult = await BundlerClient.ZkBroadcastTransaction(
                    _sdk.Session.Options.smartWalletConfig?.paymasterUrl,
                    _sdk.Session.Options.clientId,
                    _sdk.Session.Options.bundleId,
                    1,
                    new
                    {
                        nonce = zkTx.Nonce.ToString(),
                        from = zkTx.From,
                        to = zkTx.To,
                        gas = zkTx.GasLimit.ToString(),
                        gasPrice = string.Empty,
                        value = zkTx.Value.ToString(),
                        data = Utils.ByteArrayToHexString(zkTx.Data),
                        maxFeePerGas = zkTx.MaxFeePerGas.ToString(),
                        maxPriorityFeePerGas = zkTx.MaxPriorityFeePerGas.ToString(),
                        chainId = _sdk.Session.ChainId.ToString(),
                        signedTransaction = zkTxSigned,
                        paymaster = pmDataResult.paymaster,
                    }
                );
                return zkBroadcastResult.transactionHash;
            }
            else
            {
                throw new NotImplementedException("ZkSync Smart Wallet transactions are not supported without gasless mode");
            }
        }

        private async Task<EntryPointContract.UserOperation> SignTransactionAsUserOp(TransactionInput transactionInput, object requestId = null)
        {
            requestId ??= SmartWalletClient.GenerateRpcId();

            string apiKey = _sdk.Session.Options.clientId;
            string bundleId = _sdk.Session.Options.bundleId;

            // Create the user operation and its safe (hexified) version

            var executeFn = new AccountContract.ExecuteFunction
            {
                Target = transactionInput.To,
                Value = transactionInput.Value.Value,
                Calldata = transactionInput.Data.HexStringToByteArray(),
                FromAddress = Accounts[0]
            };
            var executeInput = executeFn.CreateTransactionInput(Accounts[0]);

            var (initCode, gas) = await GetInitCode();

            BigInteger maxFee;
            BigInteger maxPriorityFee;
            if (Utils.IsThirdwebRequest(_sdk.Session.Options.smartWalletConfig?.bundlerUrl))
            {
                var fees = await BundlerClient.ThirdwebGetUserOperationGasPrice(_sdk.Session.Options.smartWalletConfig?.bundlerUrl, apiKey, bundleId, requestId);
                maxFee = new HexBigInteger(fees.maxFeePerGas).Value;
                maxPriorityFee = new HexBigInteger(fees.maxPriorityFeePerGas).Value;
            }
            else
            {
                var fees = await Utils.GetGasPriceAsync(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
                maxFee = fees.MaxFeePerGas;
                maxPriorityFee = fees.MaxPriorityFeePerGas;
            }

            var partialUserOp = new EntryPointContract.UserOperation()
            {
                Sender = Accounts[0],
                Nonce = await GetNonce(),
                InitCode = initCode,
                CallData = executeInput.Data.HexStringToByteArray(),
                CallGasLimit = 0,
                VerificationGasLimit = 0,
                PreVerificationGas = 0,
                MaxFeePerGas = maxFee,
                MaxPriorityFeePerGas = maxPriorityFee,
                PaymasterAndData = new byte[] { },
                Signature = Constants.DUMMY_SIG.HexStringToByteArray(),
            };

            // Update paymaster data if any

            partialUserOp.PaymasterAndData = await GetPaymasterAndData(requestId, EncodeUserOperation(partialUserOp), apiKey, bundleId);

            // Estimate gas

            var gasEstimates = await BundlerClient.EthEstimateUserOperationGas(
                _sdk.Session.Options.smartWalletConfig?.bundlerUrl,
                apiKey,
                bundleId,
                requestId,
                EncodeUserOperation(partialUserOp),
                _sdk.Session.Options.smartWalletConfig?.entryPointAddress
            );
            partialUserOp.CallGasLimit = 50000 + new HexBigInteger(gasEstimates.CallGasLimit).Value;
            partialUserOp.VerificationGasLimit = string.IsNullOrEmpty(_sdk.Session.Options.smartWalletConfig?.erc20PaymasterAddress)
                ? new HexBigInteger(gasEstimates.VerificationGas).Value
                : new HexBigInteger(gasEstimates.VerificationGas).Value * 3;
            partialUserOp.PreVerificationGas = new HexBigInteger(gasEstimates.PreVerificationGas).Value;

            // Update paymaster data if any

            partialUserOp.PaymasterAndData = await GetPaymasterAndData(requestId, EncodeUserOperation(partialUserOp), apiKey, bundleId);

            // Hash, sign and encode the user operation

            partialUserOp.Signature = await HashAndSignUserOp(partialUserOp, _sdk.Session.Options.smartWalletConfig?.entryPointAddress);

            return partialUserOp;
        }

        private async Task<RpcResponseMessage> CreateUserOpAndSend(RpcRequestMessage requestMessage)
        {
            await new WaitUntil(() => !_deploying);

            await UpdateDeploymentStatus();
            if (!_deployed)
            {
                _deploying = true;
            }

            string apiKey = _sdk.Session.Options.clientId;
            string bundleId = _sdk.Session.Options.bundleId;

            // Deserialize the transaction input from the request message

            var paramList = JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(requestMessage.RawParameters));
            var transactionInput = JsonConvert.DeserializeObject<TransactionInput>(JsonConvert.SerializeObject(paramList[0]));

            // Approve ERC20 tokens if any

            if (!string.IsNullOrEmpty(_sdk.Session.Options.smartWalletConfig?.erc20PaymasterAddress) && !_approved && !_approving)
            {
                try
                {
                    _approving = true;
                    var tokenContract = _sdk.GetContract(_sdk.Session.Options.smartWalletConfig?.erc20TokenAddress);
                    var approvedAmount = await tokenContract.ERC20.AllowanceOf(Accounts[0], _sdk.Session.Options.smartWalletConfig?.erc20PaymasterAddress);
                    if (BigInteger.Parse(approvedAmount.value) == 0)
                    {
                        ThirdwebDebug.Log($"Approving tokens for ERC20Paymaster spending");
                        _deploying = false;
                        await tokenContract.ERC20.SetAllowance(_sdk.Session.Options.smartWalletConfig?.erc20PaymasterAddress, (BigInteger.Pow(2, 96) - 1).ToString().ToEth());
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

            // Create and sign the user operation

            var partialUserOp = await SignTransactionAsUserOp(transactionInput, requestMessage.Id);

            // Send the user operation

            ThirdwebDebug.Log("Valid UserOp: " + JsonConvert.SerializeObject(partialUserOp));
            ThirdwebDebug.Log("Valid Encoded UserOp: " + JsonConvert.SerializeObject(EncodeUserOperation(partialUserOp)));
            var userOpHash = await BundlerClient.EthSendUserOperation(
                _sdk.Session.Options.smartWalletConfig?.bundlerUrl,
                apiKey,
                bundleId,
                requestMessage.Id,
                EncodeUserOperation(partialUserOp),
                _sdk.Session.Options.smartWalletConfig?.entryPointAddress
            );
            ThirdwebDebug.Log("UserOp Hash: " + userOpHash);

            // Wait for the transaction to be mined

            string txHash = null;
            while (txHash == null)
            {
                var userOpReceipt = await BundlerClient.EthGetUserOperationReceipt(_sdk.Session.Options.smartWalletConfig?.bundlerUrl, apiKey, bundleId, requestMessage.Id, userOpHash);
                txHash = userOpReceipt?.receipt?.TransactionHash;
                await new WaitForSecondsRealtime(1f);
            }
            ThirdwebDebug.Log("Tx Hash: " + txHash);

            // Check if successful deployment

            if (!_deployed)
            {
                await UpdateDeploymentStatus();
            }

            _deploying = false;

            return new RpcResponseMessage(requestMessage.Id, txHash);
        }

        private async Task<BigInteger> GetNonce()
        {
            var nonce = await TransactionManager.ThirdwebRead<EntryPointContract.GetNonceFunction, EntryPointContract.GetNonceOutputDTO>(
                _sdk,
                _sdk.Session.Options.smartWalletConfig?.entryPointAddress,
                new EntryPointContract.GetNonceFunction() { Sender = Accounts[0], Key = GetRandomInt192() }
            );
            return nonce.Nonce;
        }

        private async Task<byte[]> GetPaymasterAndData(object requestId, UserOperationHexified userOp, string apiKey, string bundleId)
        {
            if (!string.IsNullOrEmpty(_sdk.Session.Options.smartWalletConfig?.erc20PaymasterAddress) && !_approving)
            {
                return _sdk.Session.Options.smartWalletConfig?.erc20PaymasterAddress.HexToByteArray();
            }
            else if (_sdk.Session.Options.smartWalletConfig?.gasless == true)
            {
                var paymasterAndData = await BundlerClient.PMSponsorUserOperation(
                    _sdk.Session.Options.smartWalletConfig?.paymasterUrl,
                    apiKey,
                    bundleId,
                    requestId,
                    userOp,
                    _sdk.Session.Options.smartWalletConfig?.entryPointAddress
                );
                return paymasterAndData.paymasterAndData.HexToByteArray();
            }
            else
            {
                return new byte[] { };
            }
        }

        private BigInteger GetRandomInt192()
        {
            byte[] randomBytes = GetRandomBytes(24); // 192 bits = 24 bytes
            BigInteger randomInt = new(randomBytes);
            randomInt = BigInteger.Abs(randomInt) % (BigInteger.One << 192);
            return randomInt;
        }

        private byte[] GetRandomBytes(int byteCount)
        {
            using (RNGCryptoServiceProvider rng = new())
            {
                byte[] randomBytes = new byte[byteCount];
                rng.GetBytes(randomBytes);
                return randomBytes;
            }
        }

        private async Task<byte[]> HashAndSignUserOp(EntryPointContract.UserOperation userOp, string entryPoint)
        {
            var userOpHash = await TransactionManager.ThirdwebRead<EntryPointContract.GetUserOpHashFunction, EntryPointContract.GetUserOpHashOutputDTO>(
                _sdk,
                entryPoint,
                new EntryPointContract.GetUserOpHashFunction() { UserOp = userOp }
            );

            var smartWallet = _sdk.Session.ActiveWallet;
            if (smartWallet.GetLocalAccount() != null)
            {
                var localWallet = smartWallet.GetLocalAccount();
                return new EthereumMessageSigner().Sign(userOpHash.ReturnValue1, new EthECKey(localWallet.PrivateKey)).HexStringToByteArray();
            }
            else
            {
                var sig = await _sdk.Session.Request<string>("personal_sign", userOpHash.ReturnValue1.ByteArrayToHexString(), await _sdk.Wallet.GetSignerAddress());
                return sig.HexStringToByteArray();
            }
        }

        private UserOperationHexified EncodeUserOperation(EntryPointContract.UserOperation userOperation)
        {
            return new UserOperationHexified()
            {
                sender = userOperation.Sender,
                nonce = userOperation.Nonce.ToHexBigInteger().HexValue,
                initCode = userOperation.InitCode.ByteArrayToHexString(),
                callData = userOperation.CallData.ByteArrayToHexString(),
                callGasLimit = userOperation.CallGasLimit.ToHexBigInteger().HexValue,
                verificationGasLimit = userOperation.VerificationGasLimit.ToHexBigInteger().HexValue,
                preVerificationGas = userOperation.PreVerificationGas.ToHexBigInteger().HexValue,
                maxFeePerGas = userOperation.MaxFeePerGas.ToHexBigInteger().HexValue,
                maxPriorityFeePerGas = userOperation.MaxPriorityFeePerGas.ToHexBigInteger().HexValue,
                paymasterAndData = userOperation.PaymasterAndData.ByteArrayToHexString(),
                signature = userOperation.Signature.ByteArrayToHexString()
            };
        }
    }
}
