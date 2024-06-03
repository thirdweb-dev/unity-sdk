using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Signer;
using UnityEngine;
using System;
using System.Collections.Generic;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI.EIP712;
using Nethereum.Signer.EIP712;
using Newtonsoft.Json.Linq;
using Nethereum.Hex.HexTypes;
using System.Linq;
using UnityEngine.Networking;
using Thirdweb.Redcode.Awaiting;
using Newtonsoft.Json;

#pragma warning disable CS0618

namespace Thirdweb
{
    /// <summary>
    /// Connects and interacts with a wallet.
    /// </summary>
    public class Wallet : Routable
    {
        private readonly ThirdwebSDK _sdk;

        public Wallet(ThirdwebSDK sdk)
            : base($"sdk{subSeparator}wallet")
        {
            _sdk = sdk;
        }

        /// <summary>
        /// Connects a user's wallet via a given wallet provider.
        /// </summary>
        /// <param name="walletConnection">The wallet provider and optional parameters.</param>
        /// <returns>A task representing the connection result.</returns>
        public async Task<string> Connect(WalletConnection walletConnection)
        {
            string address = null;

            if (Utils.IsWebGLBuild())
            {
                address = await Bridge.Connect(walletConnection);
            }
            else
            {
                address = await _sdk.Session.Connect(walletConnection);
                Utils.TrackWalletAnalytics(
                    _sdk.Session.Options.clientId,
                    _sdk.Session.Options.bundleId,
                    "connectWallet",
                    "connect",
                    walletConnection.provider.ToString()[..1].ToLower() + walletConnection.provider.ToString()[1..],
                    address
                );
                return address;
            }

            try
            {
                await SwitchNetwork(walletConnection.chainId);
            }
            catch
            {
                // no-op
            }

            return address;
        }

        /// <summary>
        /// Disconnects the user's wallet.
        /// </summary>
        /// <returns>A task representing the disconnection process.</returns>
        public async Task Disconnect(bool endSession = false)
        {
            if (Utils.IsWebGLBuild())
            {
                await Bridge.Disconnect();
            }
            else
            {
                await _sdk.Session.Disconnect(endSession);
            }
        }

        /// <summary>
        /// Encrypts and exports the local wallet as a password-protected JSON keystore.
        /// </summary>
        /// <param name="password">The password used to encrypt the keystore (optional).</param>
        /// <returns>The exported JSON keystore as a string.</returns>
        public async Task<string> Export(string password)
        {
            password = string.IsNullOrEmpty(password) ? SystemInfo.deviceUniqueIdentifier : password;

            if (Utils.IsWebGLBuild())
            {
                return await Bridge.ExportWallet(password);
            }
            else
            {
                var localAccount = _sdk.Session.ActiveWallet.GetLocalAccount() ?? throw new Exception("No local account found");
                return await Utils.EncryptAndGenerateKeyStore(new EthECKey(localAccount.PrivateKey), password);
            }
        }

        /// <summary>
        /// Authenticates the user by signing a payload that can be used to securely identify users. See https://portal.thirdweb.com/auth.
        /// </summary>
        /// <param name="domain">The domain to authenticate to.</param>
        /// <returns>A string representing the server-side authentication result.</returns>
        public async Task<string> Authenticate(string domain, BigInteger chainId, string authPayloadPath = "/auth/payload", string authLoginPath = "/auth/login")
        {
            string payloadURL = domain + authPayloadPath;
            string loginURL = domain + authLoginPath;

            var payloadBodyRaw = new { address = await _sdk.Wallet.GetAddress(), chainId = chainId.ToString() };
            var payloadBody = JsonConvert.SerializeObject(payloadBodyRaw);

            using UnityWebRequest payloadRequest = UnityWebRequest.Post(payloadURL, "");
            payloadRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(payloadBody));
            payloadRequest.downloadHandler = new DownloadHandlerBuffer();
            payloadRequest.SetRequestHeader("Content-Type", "application/json");
            await payloadRequest.SendWebRequest();
            if (payloadRequest.result != UnityWebRequest.Result.Success)
            {
                throw new Exception("Error: " + payloadRequest.error + "\nResponse: " + payloadRequest.downloadHandler.text);
            }
            var payloadString = payloadRequest.downloadHandler.text;

            var loginBodyRaw = JsonConvert.DeserializeObject<LoginPayload>(payloadString);
            var resourcesString = loginBodyRaw.payload.Resources != null ? "\nResources:" + string.Join("", loginBodyRaw.payload.Resources.Select(r => $"\n- {r}")) : string.Empty;
            var payloadToSign =
                $"{loginBodyRaw.payload.Domain} wants you to sign in with your Ethereum account:"
                + $"\n{loginBodyRaw.payload.Address}\n\n"
                + $"{(string.IsNullOrEmpty(loginBodyRaw.payload.Statement) ? "" : $"{loginBodyRaw.payload.Statement}\n")}"
                + $"{(string.IsNullOrEmpty(loginBodyRaw.payload.Uri) ? "" : $"\nURI: {loginBodyRaw.payload.Uri}")}"
                + $"\nVersion: {loginBodyRaw.payload.Version}"
                + $"\nChain ID: {loginBodyRaw.payload.ChainId}"
                + $"\nNonce: {loginBodyRaw.payload.Nonce}"
                + $"\nIssued At: {loginBodyRaw.payload.IssuedAt}"
                + $"{(string.IsNullOrEmpty(loginBodyRaw.payload.ExpirationTime) ? "" : $"\nExpiration Time: {loginBodyRaw.payload.ExpirationTime}")}"
                + $"{(string.IsNullOrEmpty(loginBodyRaw.payload.InvalidBefore) ? "" : $"\nNot Before: {loginBodyRaw.payload.InvalidBefore}")}"
                + resourcesString;

            loginBodyRaw.signature = await _sdk.Wallet.Sign(payloadToSign);
            var loginBody = JsonConvert.SerializeObject(new { payload = loginBodyRaw });

            using UnityWebRequest loginRequest = UnityWebRequest.Post(loginURL, "");
            loginRequest.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(loginBody));
            loginRequest.downloadHandler = new DownloadHandlerBuffer();
            loginRequest.SetRequestHeader("Content-Type", "application/json");
            await loginRequest.SendWebRequest();
            if (loginRequest.result != UnityWebRequest.Result.Success)
            {
                throw new Exception("Error: " + loginRequest.error + "\nResponse: " + loginRequest.downloadHandler.text);
            }
            var responseString = loginRequest.downloadHandler.text;
            return responseString;
        }

        /// <summary>
        /// Gets the balance of the connected wallet.
        /// </summary>
        /// <param name="currencyAddress">Optional address of the currency to check balance of.</param>
        /// <returns>The balance of the wallet as a CurrencyValue object.</returns>
        public async Task<CurrencyValue> GetBalance(string currencyAddress = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<CurrencyValue>(getRoute("balance"), Utils.ToJsonStringArray(currencyAddress));
            }
            else
            {
                if (!await IsConnected())
                    throw new Exception("No account connected!");

                if (currencyAddress != null)
                {
                    Contract contract = _sdk.GetContract(currencyAddress);
                    return await contract.ERC20.Balance();
                }
                else
                {
                    string address = await GetAddress();
                    var web3 = Utils.GetWeb3(_sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
                    HexBigInteger balance = await web3.Eth.GetBalance.SendRequestAsync(address);
                    var nativeCurrency = _sdk.Session.CurrentChainData.nativeCurrency;
                    return new CurrencyValue(nativeCurrency.name, nativeCurrency.symbol, nativeCurrency.decimals.ToString(), balance.Value.ToString(), balance.Value.ToString().ToEth());
                }
            }
        }

        /// <summary>
        /// Gets the connected wallet address.
        /// </summary>
        /// <returns>The address of the connected wallet as a string.</returns>
        public async Task<string> GetAddress()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("getAddress"), new string[] { });
            }
            else
            {
                if (!await IsConnected())
                    throw new Exception("No account connected!");

                return await _sdk.Session.ActiveWallet.GetAddress();
            }
        }

        /// <summary>
        /// Gets the connected In App Wallet email if any.
        /// </summary>
        public async Task<string> GetEmail()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.GetEmail();
            }
            else
            {
                if (!await IsConnected())
                    throw new Exception("No account connected!");

                return await _sdk.Session.ActiveWallet.GetEmail();
            }
        }

        /// <summary>
        /// Gets the address of the signer associated with the connected wallet.
        /// </summary>
        /// <returns>The address of the signer as a string.</returns>
        public async Task<string> GetSignerAddress()
        {
            if (Utils.IsWebGLBuild())
            {
                try
                {
                    var signer = await Bridge.GetSigner();
                    if (string.IsNullOrEmpty(signer))
                        return await GetAddress();
                    else
                        return signer;
                }
                catch
                {
                    return await GetAddress();
                }
            }
            else
            {
                if (!await IsConnected())
                    throw new Exception("No account connected!");

                return await _sdk.Session.ActiveWallet.GetSignerAddress();
            }
        }

        /// <summary>
        /// Checks if a wallet is connected.
        /// </summary>
        /// <returns>True if a wallet is connected, false otherwise.</returns>
        public async Task<bool> IsConnected()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<bool>(getRoute("isConnected"), new string[] { });
            }
            else
            {
                try
                {
                    return await _sdk.Session.ActiveWallet.IsConnected();
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the connected chainId.
        /// </summary>
        /// <returns>The connected chainId as an integer.</returns>
        public async Task<BigInteger> GetChainId()
        {
            if (Utils.IsWebGLBuild())
            {
                var val = await Bridge.InvokeRoute<string>(getRoute("getChainId"), new string[] { });
                return BigInteger.Parse(val);
            }
            else
            {
                var hexChainId = await _sdk.Session.Request<string>("eth_chainId");
                return hexChainId.HexToBigInteger(false);
            }
        }

        /// <summary>
        /// Prompts the connected wallet to switch to the given chainId.
        /// </summary>
        /// <param name="chainId">The chainId to switch to.</param>
        /// <returns>A task representing the switching process.</returns>
        public async Task SwitchNetwork(BigInteger chainId)
        {
            if (!await IsConnected())
                throw new Exception("No account connected!");

            if (Utils.IsWebGLBuild())
            {
                await Bridge.SwitchNetwork(chainId.ToString());
            }
            else
            {
                await _sdk.Session.EnsureCorrectNetwork(chainId);
            }
        }

        /// <summary>
        /// Transfers currency to a given address.
        /// </summary>
        /// <param name="to">The address to transfer the currency to.</param>
        /// <param name="amount">The amount of currency to transfer.</param>
        /// <param name="currencyAddress">Optional address of the currency to transfer (defaults to native token address).</param>
        /// <returns>The result of the transfer as a TransactionResult object.</returns>
        public async Task<TransactionResult> Transfer(string to, string amount, string currencyAddress = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, amount, currencyAddress));
            }
            else
            {
                if (currencyAddress != null)
                {
                    Contract contract = _sdk.GetContract(currencyAddress);
                    return await contract.ERC20.Transfer(to, amount);
                }
                else
                {
                    var txRequest = new TransactionRequest()
                    {
                        from = await GetAddress(),
                        to = to,
                        data = "0x",
                        value = amount.ToWei(),
                    };
                    return await ExecuteRawTransaction(txRequest);
                }
            }
        }

        /// <summary>
        /// Prompts the connected wallet to sign the given message.
        /// </summary>
        /// <param name="message">The message to sign.</param>
        /// <returns>The signature of the message as a string.</returns>
        public async Task<string> Sign(string message)
        {
            if (!await IsConnected())
                throw new Exception("No account connected!");

            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("sign"), Utils.ToJsonStringArray(message));
            }
            else
            {
                if (_sdk.Session.ActiveWallet.GetProvider() == WalletProvider.SmartWallet && _sdk.Session.ChainId != 300 && _sdk.Session.ChainId != 324)
                {
                    var sw = _sdk.Session.ActiveWallet as Wallets.ThirdwebSmartWallet;
                    if (!sw.SmartWallet.IsDeployed && !sw.SmartWallet.IsDeploying)
                    {
                        ThirdwebDebug.Log("SmartWallet not deployed, deploying before signing...");
                        await sw.SmartWallet.ForceDeploy();
                    }
                    if (sw.SmartWallet.IsDeployed)
                    {
                        byte[] originalMsgHash = System.Text.Encoding.UTF8.GetBytes(message).HashPrefixedMessage();
                        string swAddress = await GetAddress();
                        bool factorySupports712;
                        string signature = null;
                        try
                        {
                            // if this fails it's a pre 712 factory
                            await TransactionManager.ThirdwebRead<Contracts.Account.ContractDefinition.GetMessageHashFunction, Contracts.Account.ContractDefinition.GetMessageHashOutputDTO>(
                                _sdk,
                                swAddress,
                                new Contracts.Account.ContractDefinition.GetMessageHashFunction() { Hash = originalMsgHash }
                            );
                            factorySupports712 = true;
                        }
                        catch
                        {
                            factorySupports712 = false;
                        }

                        if (factorySupports712)
                            signature = await EIP712.GenerateSignature_SmartAccount_AccountMessage(_sdk, "Account", "1", await GetChainId(), swAddress, originalMsgHash);
                        else
                            signature = await _sdk.Session.Request<string>("personal_sign", message, await GetSignerAddress());

                        bool isValid = await RecoverAddress(message, signature) == swAddress;
                        if (isValid)
                            return signature;
                        else
                            throw new Exception("Unable to verify signature on smart account, please make sure the smart account is deployed and the signature is valid.");
                    }
                    else
                    {
                        throw new Exception("Smart account could not be deployed, unable to sign message.");
                    }
                }

                return await _sdk.Session.Request<string>("personal_sign", message, await GetSignerAddress());
            }
        }

        /// <summary>
        /// Signs a typed data object using EIP-712 signature.
        /// </summary>
        /// <typeparam name="T">The type of the data to sign.</typeparam>
        /// <typeparam name="TDomain">The type of the domain object.</typeparam>
        /// <param name="data">The data object to sign.</param>
        /// <param name="typedData">The typed data object that defines the domain and message schema.</param>
        /// <returns>The signature of the typed data as a string.</returns>
        public async Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
            where TDomain : IDomain
        {
            if (!await IsConnected())
                throw new Exception("No account connected!");

            if (Utils.IsWebGLBuild())
            {
                var domainType = typedData.Domain.GetType();
                var domain = new
                {
                    name = domainType.GetProperty("Name").GetValue(typedData.Domain).ToString(),
                    version = domainType.GetProperty("Version").GetValue(typedData.Domain).ToString(),
                    chainId = domainType.GetProperty("ChainId").GetValue(typedData.Domain).ToString(),
                    verifyingContract = domainType.GetProperty("VerifyingContract").GetValue(typedData.Domain).ToString()
                };

                var types = new Dictionary<string, object>();
                foreach (var type in typedData.Types)
                {
                    if (type.Key.Contains("EIP712Domain"))
                        continue;

                    types.Add(type.Key, type.Value);
                }

                var message = new Dictionary<string, object>();
                foreach (var member in data.GetType().GetProperties())
                {
                    string n = char.ToLower(member.Name[0]) + member.Name.Substring(1);
                    object v = member.GetValue(data);
                    // hexify bytes to avoid base64 json serialization, mostly useful for bytes32 uid
                    if (member.PropertyType == typeof(byte[]))
                        v = Utils.ToBytes32HexString((byte[])v);
                    message.Add(n, v);
                }
                var result = await Bridge.InvokeRoute<JToken>(getRoute("signTypedData"), Utils.ToJsonStringArray(domain, types, message));
                return result["signature"].Value<string>();
            }
            else
            {
                if (_sdk.Session.ActiveWallet.GetLocalAccount() != null)
                {
                    var signer = new Eip712TypedDataSigner();
                    var key = new EthECKey(_sdk.Session.ActiveWallet.GetLocalAccount().PrivateKey);
                    return signer.SignTypedDataV4(data, typedData, key);
                }
                else
                {
                    var json = typedData.ToJson(data);
                    var jsonObject = JObject.Parse(json);

                    var uidToken = jsonObject.SelectToken("$.message.uid");
                    if (uidToken != null)
                    {
                        var uidBase64 = uidToken.Value<string>();
                        var uidBytes = Convert.FromBase64String(uidBase64);
                        var uidHex = uidBytes.ByteArrayToHexString();
                        uidToken.Replace(uidHex);
                    }

                    if (_sdk.Session.ActiveWallet.GetProvider() == WalletProvider.SmartWallet)
                    {
                        // Zk AA
                        if (_sdk.Session.ChainId == 300 || _sdk.Session.ChainId == 324)
                        {
                            var hashToken = jsonObject.SelectToken("$.message.data");
                            if (hashToken != null)
                            {
                                var hashBase64 = hashToken.Value<string>();
                                var hashBytes = Convert.FromBase64String(hashBase64);
                                var hashHex = hashBytes.ByteArrayToHexString();
                                hashToken.Replace(hashHex);
                            }
                            // set factory deps to 0x
                            var factoryDepsToken = jsonObject.SelectToken("$.message.factoryDeps");
                            if (factoryDepsToken != null)
                            {
                                factoryDepsToken.Replace(new JArray());
                            }
                            var paymasterInputToken = jsonObject.SelectToken("$.message.paymasterInput");
                            if (paymasterInputToken != null)
                            {
                                var paymasterInputBase64 = paymasterInputToken.Value<string>();
                                var paymasterInputBytes = Convert.FromBase64String(paymasterInputBase64);
                                var paymasterInputHex = paymasterInputBytes.ByteArrayToHexString();
                                paymasterInputToken.Replace(paymasterInputHex);
                            }
                        }
                        // Normal AA
                        else
                        {
                            var hashToken = jsonObject.SelectToken("$.message.message");
                            if (hashToken != null)
                            {
                                var hashBase64 = hashToken.Value<string>();
                                var hashBytes = Convert.FromBase64String(hashBase64);
                                var hashHex = hashBytes.ByteArrayToHexString();
                                hashToken.Replace(hashHex);
                            }
                        }
                    }

                    var messageObject = jsonObject.GetValue("message") as JObject;
                    foreach (var property in messageObject.Properties())
                    {
                        if (property.Value.Type == JTokenType.Array)
                        {
                            continue;
                        }
                        else
                        {
                            property.Value = property.Value.ToString();
                        }
                    }

                    string safeJson = jsonObject.ToString();
                    return await _sdk.Session.Request<string>("eth_signTypedData_v4", await GetSignerAddress(), safeJson);
                }
            }
        }

        /// <summary>
        /// Recovers the original wallet address that signed a message.
        /// </summary>
        /// <param name="message">The message that was signed.</param>
        /// <param name="signature">The signature of the message.</param>
        /// <returns>The recovered wallet address as a string.</returns>
        public async Task<string> RecoverAddress(string message, string signature)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("recoverAddress"), Utils.ToJsonStringArray(message, signature));
            }
            else
            {
                var signer = new EthereumMessageSigner();
                if (_sdk.Session.ActiveWallet.GetProvider() == WalletProvider.SmartWallet && _sdk.Session.ChainId != 300 && _sdk.Session.ChainId != 324)
                {
                    var sw = _sdk.Session.ActiveWallet as Wallets.ThirdwebSmartWallet;
                    bool isSigValid = await sw.SmartWallet.VerifySignature(message.HashPrefixedMessage().HexStringToByteArray(), signature.HexStringToByteArray());
                    if (isSigValid)
                    {
                        return await GetAddress();
                    }
                }
                var addressRecovered = signer.EncodeUTF8AndEcRecover(message, signature);
                return addressRecovered;
            }
        }

        /// <summary>
        /// Smart Wallet only: Add an admin to the connected smart account.
        /// </summary>
        /// <param name="admin">Address of the admin to add.</param>
        /// <returns>The result of the transaction as a TransactionResult object.</returns>
        /// <exception cref="UnityException"></exception>
        public async Task<TransactionResult> AddAdmin(string admin)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.SmartWalletAddAdmin<TransactionResult>(admin);
            }
            else
            {
                if (_sdk.Session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet)
                    throw new UnityException("This functionality is only available for SmartWallets.");

                var smartWallet = _sdk.Session.ActiveWallet as Wallets.ThirdwebSmartWallet;
                var request = new Contracts.Account.ContractDefinition.SignerPermissionRequest()
                {
                    Signer = admin,
                    IsAdmin = 1,
                    ApprovedTargets = new List<string>(),
                    NativeTokenLimitPerTransaction = 0,
                    PermissionStartTimestamp = Utils.GetUnixTimeStampNow() - 3600,
                    PermissionEndTimestamp = Utils.GetUnixTimeStampIn10Years(),
                    ReqValidityStartTimestamp = Utils.GetUnixTimeStampNow() - 3600,
                    ReqValidityEndTimestamp = Utils.GetUnixTimeStampIn10Years(),
                    Uid = Guid.NewGuid().ToByteArray()
                };
                string signature = await EIP712.GenerateSignature_SmartAccount(_sdk, "Account", "1", await GetChainId(), await GetAddress(), request);
                return await smartWallet.SmartWallet.SetPermissionsForSigner(request, signature.HexToByteArray());
            }
        }

        /// <summary>
        /// Smart Wallet only: Remove an admin from the connected smart account.
        /// </summary>
        /// <param name="admin">Address of the admin to remove.</param>
        /// <returns>The result of the transaction as a TransactionResult object.</returns>
        /// <exception cref="UnityException"></exception>
        public async Task<TransactionResult> RemoveAdmin(string admin)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.SmartWalletRemoveAdmin<TransactionResult>(admin);
            }
            else
            {
                if (_sdk.Session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet)
                    throw new UnityException("This functionality is only available for SmartWallets.");

                var smartWallet = _sdk.Session.ActiveWallet as Wallets.ThirdwebSmartWallet;
                var request = new Contracts.Account.ContractDefinition.SignerPermissionRequest()
                {
                    Signer = admin,
                    IsAdmin = 2,
                    ApprovedTargets = new List<string>(),
                    NativeTokenLimitPerTransaction = 0,
                    PermissionStartTimestamp = Utils.GetUnixTimeStampNow() - 3600,
                    PermissionEndTimestamp = Utils.GetUnixTimeStampIn10Years(),
                    ReqValidityStartTimestamp = Utils.GetUnixTimeStampNow() - 3600,
                    ReqValidityEndTimestamp = Utils.GetUnixTimeStampIn10Years(),
                    Uid = Guid.NewGuid().ToByteArray()
                };
                string signature = await EIP712.GenerateSignature_SmartAccount(_sdk, "Account", "1", await GetChainId(), await GetAddress(), request);
                return await smartWallet.SmartWallet.SetPermissionsForSigner(request, signature.HexToByteArray());
            }
        }

        /// <summary>
        /// Smart Wallet only: Create a new signer for the connected smart account.
        /// </summary>
        /// <param name="signerAddress">Address of the wallet to add as a signer for the connected smart account.</param>
        /// <param name="approvedTargets">List of contract addresses that the signer is approved to interact with.</param>
        /// <param name="nativeTokenLimitPerTransactionInWei">The maximum amount of native token that can be transferred in a single transaction.</param>
        /// <param name="permissionStartTimestamp">UNIX timestamp of when the signer's permissions start.</param>
        /// <param name="permissionEndTimestamp">UNIX timestamp of when the signer's permissions end.</param>
        /// <param name="reqValidityStartTimestamp">UNIX timestamp of when the signer's permissions request validity starts.</param>
        /// <param name="reqValidityEndTimestamp">UNIX timestamp of when the signer's permissions request validity ends.</param>
        /// <returns>The result of the transaction as a TransactionResult object.</returns>
        public async Task<TransactionResult> CreateSessionKey(
            string signerAddress,
            List<string> approvedTargets,
            string nativeTokenLimitPerTransactionInWei,
            string permissionStartTimestamp,
            string permissionEndTimestamp,
            string reqValidityStartTimestamp,
            string reqValidityEndTimestamp
        )
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.SmartWalletCreateSessionKey<TransactionResult>(
                    Utils.ToJson(
                        new
                        {
                            signerAddress,
                            approvedCallTargets = approvedTargets,
                            nativeTokenLimitPerTransactionInWei,
                            startDate = permissionStartTimestamp,
                            expirationDate = permissionEndTimestamp,
                            reqValidityStartTimestamp,
                            reqValidityEndTimestamp
                        }
                    )
                );
            }
            else
            {
                if (_sdk.Session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet)
                    throw new UnityException("This functionality is only available for SmartWallets.");

                var smartWallet = _sdk.Session.ActiveWallet as Wallets.ThirdwebSmartWallet;
                var request = new Contracts.Account.ContractDefinition.SignerPermissionRequest()
                {
                    Signer = signerAddress,
                    IsAdmin = 0,
                    ApprovedTargets = approvedTargets,
                    NativeTokenLimitPerTransaction = BigInteger.Parse(nativeTokenLimitPerTransactionInWei),
                    PermissionStartTimestamp = BigInteger.Parse(permissionStartTimestamp),
                    PermissionEndTimestamp = BigInteger.Parse(permissionEndTimestamp),
                    ReqValidityStartTimestamp = BigInteger.Parse(reqValidityStartTimestamp),
                    ReqValidityEndTimestamp = BigInteger.Parse(reqValidityEndTimestamp),
                    Uid = Guid.NewGuid().ToByteArray()
                };
                string signature = await EIP712.GenerateSignature_SmartAccount(_sdk, "Account", "1", await GetChainId(), await GetAddress(), request);
                return await smartWallet.SmartWallet.SetPermissionsForSigner(request, signature.HexToByteArray());
            }
        }

        /// <summary>
        /// Smart Wallet only: Revoke a signer for the connected smart account.
        /// </summary>
        /// <param name="signerAddress">Address of the signer to revoke.</param>
        /// <returns>The result of the transaction as a TransactionResult object.</returns>
        public async Task<TransactionResult> RevokeSessionKey(string signerAddress)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.SmartWalletRevokeSessionKey<TransactionResult>(signerAddress);
            }
            else
            {
                if (_sdk.Session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet)
                    throw new UnityException("This functionality is only available for SmartWallets.");

                var smartWallet = _sdk.Session.ActiveWallet as Wallets.ThirdwebSmartWallet;
                var request = new Contracts.Account.ContractDefinition.SignerPermissionRequest()
                {
                    Signer = signerAddress,
                    IsAdmin = 0,
                    ApprovedTargets = new List<string>(),
                    NativeTokenLimitPerTransaction = 0,
                    PermissionStartTimestamp = 0,
                    PermissionEndTimestamp = 0,
                    ReqValidityStartTimestamp = 0,
                    ReqValidityEndTimestamp = Utils.GetUnixTimeStampIn10Years(),
                    Uid = Guid.NewGuid().ToByteArray()
                };
                string signature = await EIP712.GenerateSignature_SmartAccount(_sdk, "Account", "1", await GetChainId(), await GetAddress(), request);
                return await smartWallet.SmartWallet.SetPermissionsForSigner(request, signature.HexToByteArray());
            }
        }

        /// <summary>
        /// Smart Wallet only: Get all active signers for the connected smart account.
        /// </summary>
        /// <returns>A list of SignerWithPermissions objects.</returns>
        public async Task<List<SignerWithPermissions>> GetAllActiveSigners()
        {
            if (Utils.IsWebGLBuild())
            {
                var activeSigners = await Bridge.SmartWalletGetAllActiveSigners<List<SignerWithPermissions>>();
                for (int i = 0; i < activeSigners.Count; i++)
                {
                    var signer = activeSigners[i];
                    signer.permissions.startDate = signer.permissions.startDate == "0" ? "0" : Utils.JSDateToUnixTimestamp(signer.permissions.startDate);
                    signer.permissions.expirationDate = signer.permissions.expirationDate == "0" ? "0" : Utils.JSDateToUnixTimestamp(signer.permissions.expirationDate);
                    activeSigners[i] = signer;
                }
                return activeSigners;
            }
            else
            {
                if (_sdk.Session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet)
                    throw new UnityException("This functionality is only available for SmartWallets.");

                string address = await GetAddress();

                var rawSigners = await TransactionManager.ThirdwebRead<
                    Contracts.Account.ContractDefinition.GetAllActiveSignersFunction,
                    Contracts.Account.ContractDefinition.GetAllActiveSignersOutputDTO
                >(_sdk, address, new Contracts.Account.ContractDefinition.GetAllActiveSignersFunction());
                var allSigners = rawSigners.Signers;

                var rawAdmins = await TransactionManager.ThirdwebRead<Contracts.Account.ContractDefinition.GetAllAdminsFunction, Contracts.Account.ContractDefinition.GetAllAdminsOutputDTO>(
                    _sdk,
                    address,
                    new Contracts.Account.ContractDefinition.GetAllAdminsFunction()
                );
                foreach (var admin in rawAdmins.ReturnValue1)
                {
                    allSigners.Add(
                        new Contracts.Account.ContractDefinition.SignerPermissions()
                        {
                            Signer = admin,
                            ApprovedTargets = new List<string>() { Utils.AddressZero },
                            NativeTokenLimitPerTransaction = BigInteger.Zero,
                            StartTimestamp = 0,
                            EndTimestamp = Utils.GetUnixTimeStampIn10Years()
                        }
                    );
                }

                var signers = new List<SignerWithPermissions>();
                foreach (var rawSigner in allSigners)
                {
                    bool? isAdmin;
                    try
                    {
                        isAdmin = (
                            await TransactionManager.ThirdwebRead<Contracts.Account.ContractDefinition.IsAdminFunction, Contracts.Account.ContractDefinition.IsAdminOutputDTO>(
                                _sdk,
                                address,
                                new Contracts.Account.ContractDefinition.IsAdminFunction() { Account = rawSigner.Signer }
                            )
                        ).ReturnValue1;
                    }
                    catch
                    {
                        isAdmin = null;
                    }

                    signers.Add(
                        new SignerWithPermissions()
                        {
                            isAdmin = isAdmin,
                            signer = rawSigner.Signer,
                            permissions = new SignerPermissions()
                            {
                                approvedCallTargets = rawSigner.ApprovedTargets,
                                nativeTokenLimitPerTransaction = rawSigner.NativeTokenLimitPerTransaction.ToString(),
                                startDate = rawSigner.StartTimestamp.ToString(),
                                expirationDate = rawSigner.EndTimestamp.ToString(),
                            }
                        }
                    );
                }
                return signers;
            }
        }

        /// <summary>
        /// Smart Wallet only: check if the account is deployed.
        /// </summary>
        /// <returns>True if the account is deployed, false otherwise.</returns>
        public async Task<bool> IsDeployed()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.SmartWalletIsDeployed();
            }
            else
            {
                if (_sdk.Session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet)
                    throw new UnityException("This functionality is only available for SmartWallets.");

                var smartWallet = _sdk.Session.ActiveWallet as Wallets.ThirdwebSmartWallet;
                return smartWallet.SmartWallet.IsDeployed;
            }
        }

        /// <summary>
        /// Sends a raw transaction from the connected wallet.
        /// </summary>
        /// <param name="transactionRequest">The transaction request object containing transaction details.</param>
        /// <returns>The transaction hash.</returns>
        public async Task<string> SendRawTransaction(TransactionRequest transactionRequest)
        {
            if (Utils.IsWebGLBuild())
            {
                transactionRequest.gasPrice = null;
                var res = await Bridge.InvokeRoute<JObject>(getRoute("sendRawTransaction"), Utils.ToJsonStringArray(transactionRequest));
                return res["hash"].Value<string>();
            }
            else
            {
                if (string.IsNullOrEmpty(transactionRequest.to))
                    throw new UnityException("Please specify a to address.");

                transactionRequest.from ??= await GetAddress();

                var input = new Nethereum.RPC.Eth.DTOs.TransactionInput(
                    string.IsNullOrEmpty(transactionRequest.data) ? null : transactionRequest.data,
                    transactionRequest.to,
                    transactionRequest.from,
                    string.IsNullOrEmpty(transactionRequest.gasLimit) ? null : new HexBigInteger(BigInteger.Parse(transactionRequest.gasLimit)),
                    string.IsNullOrEmpty(transactionRequest.gasPrice) ? null : new HexBigInteger(BigInteger.Parse(transactionRequest.gasPrice)),
                    string.IsNullOrEmpty(transactionRequest.value) ? new HexBigInteger(0) : new HexBigInteger(BigInteger.Parse(transactionRequest.value))
                );

                var tx = new Transaction(_sdk, input);
                return await tx.Send();
            }
        }

        /// <summary>
        /// Sends a raw transaction from the connected wallet and waits for the transaction to be mined.
        /// </summary>
        /// <param name="transactionRequest">The transaction request object containing transaction details.</param>
        /// <returns>The result of the transaction as a TransactionResult object.</returns>
        public async Task<TransactionResult> ExecuteRawTransaction(TransactionRequest transactionRequest)
        {
            var hash = await SendRawTransaction(transactionRequest);
            return await Transaction.WaitForTransactionResult(hash, _sdk.Session.ChainId, _sdk.Session.Options.clientId, _sdk.Session.Options.bundleId);
        }

        /// <summary>
        /// Prompts the user to fund their wallet using one of the Thirdweb pay providers (defaults to Coinbase Pay).
        /// </summary>
        /// <param name="options">The options for funding the wallet.</param>
        /// <returns>A task representing the funding process.</returns>
        public async Task FundWallet(FundWalletOptions options)
        {
            if (Utils.IsWebGLBuild())
            {
                options.address ??= await GetAddress();
                await Bridge.FundWallet(options);
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }
    }

    /// <summary>
    /// Represents the connection details for a wallet.
    /// </summary>
    [System.Serializable]
    public class WalletConnection
    {
        public WalletProvider provider;
        public BigInteger chainId;
        public string password;
        public string email;
        public string phoneNumber;
        public WalletProvider personalWallet;
        public AuthOptions authOptions;
        public string smartWalletAccountOverride;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletConnection"/> class with the specified parameters.
        /// </summary>
        /// <param name="provider">The wallet provider to connect to.</param>
        /// <param name="chainId">The chain ID.</param>
        /// <param name="password">Optional wallet encryption password</param>
        /// <param name="email">The email to login with if using email based providers.</param>
        /// <param name="personalWallet">The personal wallet provider if using smart wallets.</param>
        /// <param name="authOptions">The authentication options if using in app wallets.</param>
        /// <param name="smartWalletAccountOverride">Optionally choose to connect to a smart account the personal wallet is not an admin of.</param>
        /// <returns>A new instance of the <see cref="WalletConnection"/> class.</returns>
        public WalletConnection(
            WalletProvider provider,
            BigInteger chainId,
            string password = null,
            string email = null,
            string phoneNumber = null,
            WalletProvider personalWallet = WalletProvider.LocalWallet,
            AuthOptions authOptions = null,
            string smartWalletAccountOverride = null
        )
        {
            this.provider = provider;
            this.chainId = chainId;
            this.password = password;
            this.email = email;
            this.phoneNumber = phoneNumber;
            this.personalWallet = personalWallet;
            this.authOptions = authOptions ?? new AuthOptions(authProvider: AuthProvider.EmailOTP, jwtOrPayload: null, encryptionKey: null);
            this.smartWalletAccountOverride = smartWalletAccountOverride;
        }
    }

    /// <summary>
    /// In App Wallet Authentication Options.
    /// </summary>
    [System.Serializable]
    public class AuthOptions
    {
        public AuthProvider authProvider;
        public string jwtOrPayload;
        public string encryptionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthOptions"/> class with the specified parameters.
        /// </summary>
        /// <param name="authProvider">The authentication provider to use.</param>
        /// <param name="jwtOrPayload">Used for custom JWT or AuthEndpoint methods, pass JWT or auth payload respectively.</param>
        /// <param name="encryptionKey">Used for custom JWT or AuthEndpoint methods, developer-manaed recovery encryption key.</param>
        /// <returns>A new instance of the <see cref="AuthOptions"/> class.</returns>
        public AuthOptions(AuthProvider authProvider, string jwtOrPayload = null, string encryptionKey = null)
        {
            this.authProvider = authProvider;
            this.jwtOrPayload = jwtOrPayload;
            this.encryptionKey = encryptionKey;
        }
    }

    /// <summary>
    /// Represents the available wallet providers.
    /// </summary>
    [System.Serializable]
    public enum WalletProvider
    {
        Metamask,
        Coinbase,
        WalletConnect,
        Injected,
        LocalWallet,
        SmartWallet,
        Hyperplay,
        InAppWallet
    }

    /// <summary>
    /// Represents the available auth providers for In App Wallet.
    /// </summary>
    [System.Serializable]
    public enum AuthProvider
    {
        /// <summary>
        /// Email OTP Flow.
        /// </summary>
        EmailOTP,

        /// <summary>
        /// Google OAuth2 Flow.
        /// </summary>
        Google,

        /// <summary>
        /// Apple OAuth2 Flow.
        /// </summary>
        Apple,

        /// <summary>
        /// Facebook OAuth2 Flow.
        /// </summary>
        Facebook,

        /// <summary>
        /// JWT-Based Authentication Flow, checks JWT against developer-set JWKS URI.
        /// </summary>
        JWT,

        /// <summary>
        /// Custom Authentication Flow, checks payload against developer-set Auth Endpoint.
        /// </summary>
        AuthEndpoint,

        /// <summary>
        /// Phone Number OTP Flow.
        /// </summary>
        PhoneOTP
    }
}
