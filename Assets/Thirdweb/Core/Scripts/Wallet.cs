using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Signer;
using UnityEngine;
using System;
using Nethereum.Siwe.Core;
using System.Collections.Generic;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.ABI.EIP712;
using Nethereum.Signer.EIP712;
using Newtonsoft.Json.Linq;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using System.Linq;

namespace Thirdweb
{
    /// <summary>
    /// Connects and interacts with a wallet.
    /// </summary>
    public class Wallet : Routable
    {
        public Wallet()
            : base($"sdk{subSeparator}wallet") { }

        /// <summary>
        /// Connects a user's wallet via a given wallet provider.
        /// </summary>
        /// <param name="walletConnection">The wallet provider and optional parameters.</param>
        /// <returns>A task representing the connection result.</returns>
        public async Task<string> Connect(WalletConnection walletConnection)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.Connect(walletConnection);
            }
            else
            {
                string address = await ThirdwebManager.Instance.SDK.session.Connect(walletConnection);
                Utils.TrackWalletAnalytics(
                    ThirdwebManager.Instance.SDK.session.Options.clientId,
                    "connectWallet",
                    "connect",
                    walletConnection.provider.ToString()[..1].ToLower() + walletConnection.provider.ToString()[1..],
                    address
                );
                return address;
            }
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
                await ThirdwebManager.Instance.SDK.session.Disconnect(endSession);
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
                var localAccount = ThirdwebManager.Instance.SDK.session.ActiveWallet.GetLocalAccount() ?? throw new Exception("No local account found");
                return Utils.EncryptAndGenerateKeyStore(new EthECKey(localAccount.PrivateKey), password);
            }
        }

        /// <summary>
        /// Authenticates the user by signing a payload that can be used to securely identify users. See https://portal.thirdweb.com/auth.
        /// </summary>
        /// <param name="domain">The domain to authenticate to.</param>
        /// <returns>A task representing the authentication result.</returns>
        public async Task<LoginPayload> Authenticate(string domain)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<LoginPayload>($"auth{subSeparator}login", Utils.ToJsonStringArray(domain));
            }
            else
            {
                var siwe = ThirdwebManager.Instance.SDK.session.SiweSession;
                var siweMsg = new SiweMessage()
                {
                    Resources = null,
                    Uri = null,
                    Statement = "Please ensure that the domain above matches the URL of the current website.",
                    Address = await GetAddress(),
                    Domain = domain,
                    ChainId = (await GetChainId()).ToString(),
                    Version = "1",
                    Nonce = null,
                    IssuedAt = null,
                    ExpirationTime = null,
                    NotBefore = null,
                    RequestId = null
                };
                siweMsg.SetIssuedAtNow();
                siweMsg.SetExpirationTime(DateTime.UtcNow.AddSeconds(60 * 5));
                siweMsg.SetNotBefore(DateTime.UtcNow);
                siweMsg = siwe.AssignNewNonce(siweMsg);

                var resourcesString = siweMsg.Resources != null ? "\nResources:" + string.Join("", siweMsg.Resources.Select(r => $"\n- {r}")) : string.Empty;
                var finalMsg =
                    $"{siweMsg.Domain} wants you to sign in with your Ethereum account:"
                    + $"\n{siweMsg.Address}\n\n"
                    + $"{(string.IsNullOrEmpty(siweMsg.Statement) ? "" : $"{siweMsg.Statement}\n")}"
                    + $"{(string.IsNullOrEmpty(siweMsg.Uri) ? "" : $"\nURI: {siweMsg.Uri}")}"
                    + $"\nVersion: {siweMsg.Version}"
                    + $"\nChain ID: {siweMsg.ChainId}"
                    + $"\nNonce: {siweMsg.Nonce}"
                    + $"\nIssued At: {siweMsg.IssuedAt}"
                    + $"{(string.IsNullOrEmpty(siweMsg.ExpirationTime) ? "" : $"\nExpiration Time: {siweMsg.ExpirationTime}")}"
                    + $"{(string.IsNullOrEmpty(siweMsg.NotBefore) ? "" : $"\nNot Before: {siweMsg.NotBefore}")}"
                    + $"{(string.IsNullOrEmpty(siweMsg.RequestId) ? "" : $"\nRequest ID: {siweMsg.RequestId}")}"
                    + resourcesString;
                var signature = await Sign(finalMsg);

                return new LoginPayload()
                {
                    signature = signature,
                    payload = new LoginPayloadData()
                    {
                        Domain = siweMsg.Domain,
                        Address = siweMsg.Address,
                        Statement = siweMsg.Statement,
                        Uri = siweMsg.Uri,
                        Version = siweMsg.Version,
                        ChainId = siweMsg.ChainId,
                        Nonce = siweMsg.Nonce,
                        IssuedAt = siweMsg.IssuedAt,
                        ExpirationTime = siweMsg.ExpirationTime,
                        InvalidBefore = siweMsg.NotBefore,
                        Resources = siweMsg.Resources,
                    }
                };
            }
        }

        /// <summary>
        /// Verifies the authenticity of a login payload.
        /// </summary>
        /// <param name="payload">The login payload to verify.</param>
        /// <returns>The verification result as a string.</returns>
        public async Task<string> Verify(LoginPayload payload)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>($"auth{subSeparator}verify", Utils.ToJsonStringArray(payload));
            }
            else
            {
                var siwe = ThirdwebManager.Instance.SDK.session.SiweSession;
                var siweMessage = new SiweMessage()
                {
                    Domain = payload.payload.Domain,
                    Address = payload.payload.Address,
                    Statement = payload.payload.Statement,
                    Uri = payload.payload.Uri,
                    Version = payload.payload.Version,
                    ChainId = payload.payload.ChainId,
                    Nonce = payload.payload.Nonce,
                    IssuedAt = payload.payload.IssuedAt,
                    ExpirationTime = payload.payload.ExpirationTime,
                    NotBefore = payload.payload.InvalidBefore,
                    Resources = payload.payload.Resources,
                    RequestId = null
                };
                var signature = payload.signature;
                var validUser = await siwe.IsUserAddressRegistered(siweMessage);
                var resourcesString = siweMessage.Resources != null ? "\nResources:" + string.Join("", siweMessage.Resources.Select(r => $"\n- {r}")) : string.Empty;
                var msg =
                    $"{siweMessage.Domain} wants you to sign in with your Ethereum account:"
                    + $"\n{siweMessage.Address}\n\n"
                    + $"{(string.IsNullOrEmpty(siweMessage.Statement) ? "" : $"{siweMessage.Statement}\n")}"
                    + $"{(string.IsNullOrEmpty(siweMessage.Uri) ? "" : $"\nURI: {siweMessage.Uri}")}"
                    + $"\nVersion: {siweMessage.Version}"
                    + $"\nChain ID: {siweMessage.ChainId}"
                    + $"\nNonce: {siweMessage.Nonce}"
                    + $"\nIssued At: {siweMessage.IssuedAt}"
                    + $"{(string.IsNullOrEmpty(siweMessage.ExpirationTime) ? "" : $"\nExpiration Time: {siweMessage.ExpirationTime}")}"
                    + $"{(string.IsNullOrEmpty(siweMessage.NotBefore) ? "" : $"\nNot Before: {siweMessage.NotBefore}")}"
                    + $"{(string.IsNullOrEmpty(siweMessage.RequestId) ? "" : $"\nRequest ID: {siweMessage.RequestId}")}"
                    + resourcesString;
                if (validUser)
                {
                    string recoveredAddress = await RecoverAddress(msg, signature);
                    if (recoveredAddress == siweMessage.Address)
                    {
                        if (siwe.IsMessageTheSameAsSessionStored(siweMessage))
                        {
                            if (siwe.HasMessageDateStartedAndNotExpired(siweMessage))
                            {
                                return siweMessage.Address;
                            }
                            else
                            {
                                return "Expired";
                            }
                        }
                        else
                        {
                            return "Invalid Session";
                        }
                    }
                    else
                    {
                        return "Invalid Signature";
                    }
                }
                else
                {
                    return "Invalid User";
                }
            }
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
                    Contract contract = ThirdwebManager.Instance.SDK.GetContract(currencyAddress);
                    return await contract.ERC20.Balance();
                }
                else
                {
                    string address = await GetAddress();
                    HexBigInteger balance = await Utils.GetWeb3().Eth.GetBalance.SendRequestAsync(address);
                    var nativeCurrency = ThirdwebManager.Instance.SDK.session.CurrentChainData.nativeCurrency;
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

                return await ThirdwebManager.Instance.SDK.session.ActiveWallet.GetAddress();
            }
        }

        /// <summary>
        /// Gets the connected embedded wallet email if any.
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

                return await ThirdwebManager.Instance.SDK.session.ActiveWallet.GetEmail();
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

                return await ThirdwebManager.Instance.SDK.session.ActiveWallet.GetSignerAddress();
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
                    return await ThirdwebManager.Instance.SDK.session.ActiveWallet.IsConnected();
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
                return await Bridge.InvokeRoute<BigInteger>(getRoute("getChainId"), new string[] { });
            }
            else
            {
                var hexChainId = await ThirdwebManager.Instance.SDK.session.Request<string>("eth_chainId");
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
                await ThirdwebManager.Instance.SDK.session.EnsureCorrectNetwork(chainId);
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
                    Contract contract = ThirdwebManager.Instance.SDK.GetContract(currencyAddress);
                    return await contract.ERC20.Transfer(to, amount);
                }
                else
                {
                    var txHash = await ThirdwebManager.Instance.SDK.session.Web3.Eth.GetEtherTransferService().TransferEtherAsync(to, decimal.Parse(amount));
                    return await Transaction.WaitForTransactionResult(txHash);
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
                if (ThirdwebManager.Instance.SDK.session.ActiveWallet.GetProvider() == WalletProvider.SmartWallet && ThirdwebManager.Instance.SDK.session.Options.smartWalletConfig.Value.deployOnSign)
                {
                    var sw = ThirdwebManager.Instance.SDK.session.ActiveWallet as Wallets.ThirdwebSmartWallet;
                    if (!sw.SmartWallet.IsDeployed && !sw.SmartWallet.IsDeploying)
                    {
                        ThirdwebDebug.Log("SmartWallet not deployed, deploying before signing...");
                        await sw.SmartWallet.ForceDeploy();
                    }
                }

                return await ThirdwebManager.Instance.SDK.session.Request<string>("personal_sign", message, await GetSignerAddress());
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

            if (ThirdwebManager.Instance.SDK.session.ActiveWallet.GetProvider() == WalletProvider.SmartWallet && ThirdwebManager.Instance.SDK.session.Options.smartWalletConfig.Value.deployOnSign)
            {
                var sw = ThirdwebManager.Instance.SDK.session.ActiveWallet as Wallets.ThirdwebSmartWallet;
                if (!sw.SmartWallet.IsDeployed && !sw.SmartWallet.IsDeploying)
                {
                    ThirdwebDebug.Log("SmartWallet not deployed, deploying before signing...");
                    await sw.SmartWallet.ForceDeploy();
                }
            }

            if (ThirdwebManager.Instance.SDK.session.ActiveWallet.GetLocalAccount() != null)
            {
                var signer = new Eip712TypedDataSigner();
                var key = new EthECKey(ThirdwebManager.Instance.SDK.session.ActiveWallet.GetLocalAccount().PrivateKey);
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
                return await ThirdwebManager.Instance.SDK.session.Request<string>("eth_signTypedData_v4", await GetSignerAddress(), safeJson);
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
                if (ThirdwebManager.Instance.SDK.session.ActiveWallet.GetProvider() == WalletProvider.SmartWallet)
                {
                    var sw = ThirdwebManager.Instance.SDK.session.ActiveWallet as Wallets.ThirdwebSmartWallet;
                    bool isSigValid = await sw.SmartWallet.VerifySignature(signer.HashPrefixedMessage(System.Text.Encoding.UTF8.GetBytes(message)), signature.HexStringToByteArray());
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
                if (ThirdwebManager.Instance.SDK.session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet)
                    throw new UnityException("This functionality is only available for SmartWallets.");
                var smartWallet = ThirdwebManager.Instance.SDK.session.ActiveWallet as Wallets.ThirdwebSmartWallet;
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
                string signature = await EIP712.GenerateSignature_SmartAccount("Account", "1", await GetChainId(), await GetAddress(), request);
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
                if (ThirdwebManager.Instance.SDK.session.ActiveWallet.GetProvider() != WalletProvider.SmartWallet)
                    throw new UnityException("This functionality is only available for SmartWallets.");
                var smartWallet = ThirdwebManager.Instance.SDK.session.ActiveWallet as Wallets.ThirdwebSmartWallet;
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
                string signature = await EIP712.GenerateSignature_SmartAccount("Account", "1", await GetChainId(), await GetAddress(), request);
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
                var smartWallet = ThirdwebManager.Instance.SDK.session.ActiveWallet as Wallets.ThirdwebSmartWallet;
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
                string signature = await EIP712.GenerateSignature_SmartAccount("Account", "1", await GetChainId(), await GetAddress(), request);
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
                var smartWallet = ThirdwebManager.Instance.SDK.session.ActiveWallet as Wallets.ThirdwebSmartWallet;
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
                string signature = await EIP712.GenerateSignature_SmartAccount("Account", "1", await GetChainId(), await GetAddress(), request);
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
                string address = await GetAddress();
                var raw = await TransactionManager.ThirdwebRead<Contracts.Account.ContractDefinition.GetAllActiveSignersFunction, Contracts.Account.ContractDefinition.GetAllActiveSignersOutputDTO>(
                    address,
                    new Contracts.Account.ContractDefinition.GetAllActiveSignersFunction()
                );
                var signers = new List<SignerWithPermissions>();
                foreach (var rawSigner in raw.Signers)
                {
                    bool? isAdmin;
                    try
                    {
                        isAdmin = (
                            await TransactionManager.ThirdwebRead<Contracts.Account.ContractDefinition.IsAdminFunction, Contracts.Account.ContractDefinition.IsAdminOutputDTO>(
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
        /// Sends a raw transaction from the connected wallet.
        /// </summary>
        /// <param name="transactionRequest">The transaction request object containing transaction details.</param>
        /// <returns>The result of the transaction as a TransactionResult object.</returns>
        public async Task<TransactionResult> SendRawTransaction(TransactionRequest transactionRequest)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("sendRawTransaction"), Utils.ToJsonStringArray(transactionRequest));
            }
            else
            {
                var input = new Nethereum.RPC.Eth.DTOs.TransactionInput(
                    transactionRequest.data,
                    transactionRequest.to,
                    transactionRequest.from,
                    new HexBigInteger(BigInteger.Parse(transactionRequest.gasLimit)),
                    new HexBigInteger(BigInteger.Parse(transactionRequest.gasPrice)),
                    new HexBigInteger(transactionRequest.value)
                );
                var receipt = await ThirdwebManager.Instance.SDK.session.Web3.Eth.TransactionManager.SendTransactionAndWaitForReceiptAsync(input);
                return receipt.ToTransactionResult();
            }
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
        /// <param name="authOptions">The authentication options if using embedded wallets.</param>
        /// <param name="smartWalletAccountOverride">Optionally choose to connect to a smart account the personal wallet is not an admin of.</param>
        /// <returns>A new instance of the <see cref="WalletConnection"/> class.</returns>
        public WalletConnection(
            WalletProvider provider,
            BigInteger chainId,
            string password = null,
            string email = null,
            WalletProvider personalWallet = WalletProvider.LocalWallet,
            AuthOptions authOptions = null,
            string smartWalletAccountOverride = null
        )
        {
            this.provider = provider;
            this.chainId = chainId;
            this.password = password;
            this.email = email;
            this.personalWallet = personalWallet;
            this.authOptions = authOptions ?? new AuthOptions(authProvider: AuthProvider.EmailOTP, jwtOrPayload: null, encryptionKey: null);
            this.smartWalletAccountOverride = smartWalletAccountOverride;
        }
    }

    /// <summary>
    /// Embedded Wallet Authentication Options.
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
        EmbeddedWallet
    }

    /// <summary>
    /// Represents the available auth providers for Embedded Wallet.
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
    }
}
