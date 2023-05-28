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
using Nethereum.RPC.Eth.Transactions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexTypes;

//using WalletConnectSharp.NEthereum;

namespace Thirdweb
{
    /// <summary>
    /// Connect and Interact with a Wallet.
    /// </summary>
    public class Wallet : Routable
    {
        public Wallet()
            : base($"sdk{subSeparator}wallet") { }

        /// <summary>
        /// Connect a user's wallet via a given wallet provider
        /// </summary>
        /// <param name="walletConnection">The wallet provider and optional parameters.</param>
        public async Task<string> Connect(WalletConnection walletConnection)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.Connect(walletConnection);
            }
            else
            {
                return await ThirdwebManager.Instance.SDK.session.Connect(walletConnection.provider, walletConnection.password, walletConnection.email);
            }
        }

        /// <summary>
        /// Disconnect the user's wallet
        /// </summary>
        public async Task Disconnect()
        {
            if (Utils.IsWebGLBuild())
            {
                await Bridge.Disconnect();
            }
            else
            {
                ThirdwebManager.Instance.SDK.session.Disconnect();
            }
        }

        /// <summary>
        /// Authenticate the user by signing a payload that can be used to securely identify users. See https://portal.thirdweb.com/auth
        /// </summary>
        /// <param name="domain">The domain to authenticate to</param>
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
                    Resources = new List<string>(),
                    Uri = $"https://{domain}",
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

                var finalMsg = SiweMessageStringBuilder.BuildMessage(siweMsg);
                var signature = await Sign(finalMsg);

                return new LoginPayload()
                {
                    signature = signature,
                    payload = new LoginPayloadData()
                    {
                        domain = siweMsg.Domain,
                        address = siweMsg.Address,
                        statement = siweMsg.Statement,
                        uri = siweMsg.Uri,
                        version = siweMsg.Version,
                        chain_id = siweMsg.ChainId,
                        nonce = siweMsg.Nonce,
                        issued_at = siweMsg.IssuedAt,
                        expiration_time = siweMsg.ExpirationTime,
                        invalid_before = siweMsg.NotBefore,
                        resources = siweMsg.Resources,
                    }
                };
            }
        }

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
                    Domain = payload.payload.domain,
                    Address = payload.payload.address,
                    Statement = payload.payload.statement,
                    Uri = payload.payload.uri,
                    Version = payload.payload.version,
                    ChainId = payload.payload.chain_id,
                    Nonce = payload.payload.nonce,
                    IssuedAt = payload.payload.issued_at,
                    ExpirationTime = payload.payload.expiration_time,
                    NotBefore = payload.payload.invalid_before,
                    Resources = payload.payload.resources,
                    RequestId = null
                };
                var signature = payload.signature;
                var validUser = await siwe.IsUserAddressRegistered(siweMessage);
                if (validUser)
                {
                    if (await siwe.IsMessageSignatureValid(siweMessage, signature))
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
        /// Get the balance of the connected wallet
        /// </summary>
        /// <param name="currencyAddress">Optional address of the currency to check balance of</param>
        public async Task<CurrencyValue> GetBalance(string currencyAddress = Utils.NativeTokenAddress)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<CurrencyValue>(getRoute("balance"), Utils.ToJsonStringArray(currencyAddress));
            }
            else
            {
                if (!await IsConnected())
                    throw new Exception("No account connected!");

                if (currencyAddress != Utils.NativeTokenAddress)
                {
                    Contract contract = ThirdwebManager.Instance.SDK.GetContract(currencyAddress);
                    return await contract.ERC20.Balance();
                }
                else
                {
                    var balance = await ThirdwebManager.Instance.SDK.session.Web3.Eth.GetBalance.SendRequestAsync(await GetAddress());
                    var nativeCurrency = ThirdwebManager.Instance.SDK.session.CurrentChainData.nativeCurrency;
                    return new CurrencyValue(nativeCurrency.name, nativeCurrency.symbol, nativeCurrency.decimals.ToString(), balance.Value.ToString(), balance.Value.ToString().ToEth());
                }
            }
        }

        /// <summary>
        /// Get the connected wallet address
        /// </summary>
        public async Task<string> GetAddress()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("getAddress"), new string[] { });
            }
            else
            {
                return await ThirdwebManager.Instance.SDK.session.Request<string>("eth_accounts");
            }
        }

        /// <summary>
        /// Check if a wallet is connected
        /// </summary>
        public async Task<bool> IsConnected()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<bool>(getRoute("isConnected"), new string[] { });
            }
            else
            {
                return ThirdwebManager.Instance.SDK.session.IsConnected;
            }
        }

        /// <summary>
        /// Get the connected chainId
        /// </summary>
        public async Task<int> GetChainId()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<int>(getRoute("getChainId"), new string[] { });
            }
            else
            {
                var hexChainId = await ThirdwebManager.Instance.SDK.session.Request<string>("eth_chainId");
                return (int)hexChainId.HexToBigInteger(false);
            }
        }

        /// <summary>
        /// Prompt the connected wallet to switch to the giiven chainId
        /// </summary>
        public async Task SwitchNetwork(int chainId)
        {
            if (Utils.IsWebGLBuild())
            {
                await Bridge.SwitchNetwork(chainId);
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Transfer currency to a given address
        /// </summary>
        public async Task<TransactionResult> Transfer(string to, string amount, string currencyAddress = Utils.NativeTokenAddress)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, amount, currencyAddress));
            }
            else
            {
                if (currencyAddress != Utils.NativeTokenAddress)
                {
                    Contract contract = ThirdwebManager.Instance.SDK.GetContract(currencyAddress);
                    return await contract.ERC20.Transfer(to, amount);
                }
                else
                {
                    var receipt = await ThirdwebManager.Instance.SDK.session.Web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(to, decimal.Parse(amount));
                    return receipt.ToTransactionResult();
                }
            }
        }

        /// <summary>
        /// Prompt the connected wallet to sign the given message
        /// </summary>
        public async Task<string> Sign(string message)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("sign"), Utils.ToJsonStringArray(message));
            }
            else
            {
                if (ThirdwebManager.Instance.SDK.session.WalletProvider == WalletProvider.LocalWallet)
                {
                    var signer = new EthereumMessageSigner();
                    return signer.EncodeUTF8AndSign(message, new EthECKey(ThirdwebManager.Instance.SDK.session.LocalAccount.PrivateKey));
                }
                else
                {
                    try
                    {
                        return await ThirdwebManager.Instance.SDK.session.Request<string>("personal_sign", await GetAddress(), message);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning(e.Message);
                        return await ThirdwebManager.Instance.SDK.session.Request<string>("eth_sign", await GetAddress(), message);
                    }
                }
            }
        }

        public async Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
            where TDomain : IDomain
        {
            if (ThirdwebManager.Instance.SDK.session.WalletProvider == WalletProvider.LocalWallet)
            {
                var signer = new Eip712TypedDataSigner();
                var key = new EthECKey(ThirdwebManager.Instance.SDK.session.LocalAccount.PrivateKey);
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
                    property.Value = property.Value.ToString();

                string safeJson = jsonObject.ToString();
                return await ThirdwebManager.Instance.SDK.session.Request<string>("eth_signTypedData_v4", await GetAddress(), safeJson);
            }
        }

        /// <summary>
        /// Recover the original wallet address that signed a message
        /// </summary>
        public async Task<string> RecoverAddress(string message, string signature)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("recoverAddress"), Utils.ToJsonStringArray(message, signature));
            }
            else
            {
                var signer = new EthereumMessageSigner();
                var addressRecovered = signer.EncodeUTF8AndEcRecover(message, signature);
                return addressRecovered;
            }
        }

        /// <summary>
        /// Send a raw transaction from the connected wallet
        /// </summary>
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
                    new Nethereum.Hex.HexTypes.HexBigInteger(BigInteger.Parse(transactionRequest.gasLimit)),
                    new Nethereum.Hex.HexTypes.HexBigInteger(BigInteger.Parse(transactionRequest.gasPrice)),
                    new Nethereum.Hex.HexTypes.HexBigInteger(transactionRequest.value)
                );
                var receipt = await ThirdwebManager.Instance.SDK.session.Web3.Eth.TransactionManager.SendTransactionAndWaitForReceiptAsync(input);
                return receipt.ToTransactionResult();
            }
        }

        /// <summary>
        /// Prompt the user to fund their wallet using one of the thirdweb pay providers (defaults to Coinbase Pay).
        /// </summary>
        /// <param name="options">The options like wallet address to fund, on which chain, etc</param>
        public async Task FundWallet(FundWalletOptions options)
        {
            if (Utils.IsWebGLBuild())
            {
                if (options.address == null)
                {
                    options.address = await GetAddress();
                }
                await Bridge.FundWallet(options);
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }
    }

    public class WalletConnection
    {
        public WalletProvider provider;
        public int chainId;
        public string password;
        public string email;

        public WalletConnection(WalletProvider provider = WalletProvider.LocalWallet, int chainId = 1, string password = null, string email = null)
        {
            this.provider = provider;
            this.chainId = chainId;
            this.password = password;
            this.email = email;
        }
    }

    public enum WalletProvider
    {
        Metamask,
        Coinbase,
        WalletConnectV1,
        Injected,
        MagicLink,
        LocalWallet,
        SmartWallet
    }
}
