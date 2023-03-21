using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Signer;
using Nethereum.Web3;
using UnityEngine;
using System;
using WalletConnectSharp.Unity;
using WalletConnectSharp.NEthereum;
using Nethereum.Siwe.Core;
using System.Collections.Generic;
using Nethereum.Web3.Accounts;

//using WalletConnectSharp.NEthereum;

namespace Thirdweb
{
    /// <summary>
    /// Connect and Interact with a Wallet.
    /// </summary>
    public class Wallet : Routable
    {
        public Wallet() : base($"sdk{subSeparator}wallet") { }

        /// <summary>
        /// Connect a user's wallet via a given wallet provider
        /// </summary>
        /// <param name="walletConnection">The wallet provider and chainId to connect to. Defaults to the injected browser extension.</param>
        public async Task<string> Connect(WalletConnection? walletConnection = null)
        {
            if (Utils.IsWebGLBuild())
            {
                var connection = walletConnection ?? new WalletConnection() { provider = WalletProvider.Injected, };
                return await Bridge.Connect(connection);
            }
            else
            {
                ThirdwebSDK.NativeSession oldSession = ThirdwebManager.Instance.SDK.nativeSession;

                if (walletConnection == null)
                {
                    Account noPassAcc = Utils.UnlockOrGenerateAccount(oldSession.lastChainId, null, null);
                    ThirdwebManager.Instance.SDK.nativeSession = new ThirdwebSDK.NativeSession(
                        oldSession.lastChainId,
                        oldSession.lastRPC,
                        noPassAcc,
                        new Web3(noPassAcc, oldSession.lastRPC),
                        oldSession.options,
                        oldSession.siweSession
                    );
                    return noPassAcc.Address;
                }
                else
                {
                    if (walletConnection?.provider?.ToString() == "walletConnect")
                    {
                        await WalletConnect.Instance.EnableWalletConnect();

                        ThirdwebManager.Instance.SDK.nativeSession = new ThirdwebSDK.NativeSession(
                            oldSession.lastChainId,
                            oldSession.lastRPC,
                            null,
                            WalletConnect.Instance.Session.BuildWeb3(new Uri(oldSession.lastRPC)).AsWalletAccount(true),
                            oldSession.options,
                            oldSession.siweSession
                        );
                        return Nethereum.Util.AddressUtil.Current.ConvertToChecksumAddress(WalletConnect.Instance.Session.Accounts[0]);
                    }
                    else if (walletConnection?.password != null)
                    {
                        Account acc = Utils.UnlockOrGenerateAccount(oldSession.lastChainId, walletConnection?.password, null);
                        ThirdwebManager.Instance.SDK.nativeSession = new ThirdwebSDK.NativeSession(
                            oldSession.lastChainId,
                            oldSession.lastRPC,
                            acc,
                            new Web3(acc, oldSession.lastRPC),
                            oldSession.options,
                            oldSession.siweSession
                        );
                        return acc.Address;
                    }
                    else if (walletConnection?.privateKey != null)
                    {
                        Account acc = Utils.UnlockOrGenerateAccount(oldSession.lastChainId, null, walletConnection?.privateKey);
                        ThirdwebManager.Instance.SDK.nativeSession = new ThirdwebSDK.NativeSession(
                            oldSession.lastChainId,
                            oldSession.lastRPC,
                            acc,
                            new Web3(acc, oldSession.lastRPC),
                            oldSession.options,
                            oldSession.siweSession
                        );
                        return acc.Address;
                    }
                    else
                    {
                        throw new UnityException("This wallet connection method is not supported on this platform!");
                    }
                }
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
                ThirdwebSDK.NativeSession oldSession = ThirdwebManager.Instance.SDK.nativeSession;

                if (Utils.ActiveWalletConnectSession())
                {
                    WalletConnect.Instance.DisableWalletConnect();
                }

                ThirdwebManager.Instance.SDK.nativeSession = new ThirdwebSDK.NativeSession(
                    oldSession.lastChainId,
                    oldSession.lastRPC,
                    null,
                    new Web3(oldSession.lastRPC),
                    oldSession.options,
                    oldSession.siweSession
                );
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
                var siwe = ThirdwebManager.Instance.SDK.nativeSession.siweSession;
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
                var siwe = ThirdwebManager.Instance.SDK.nativeSession.siweSession;
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
                if (currencyAddress != Utils.NativeTokenAddress)
                {
                    Contract contract = ThirdwebManager.Instance.SDK.GetContract(currencyAddress);
                    return await contract.ERC20.Balance();
                }
                else
                {
                    var balance = await ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetBalance.SendRequestAsync(await ThirdwebManager.Instance.SDK.wallet.GetAddress());
                    return new CurrencyValue("Ether", "ETH", "18", balance.Value.ToString(), balance.Value.ToString().ToEth()); // TODO: Get actual name/symbol
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
                if (Utils.ActiveWalletConnectSession())
                {
                    return Nethereum.Util.AddressUtil.Current.ConvertToChecksumAddress(WalletConnect.Instance.Session.Accounts[0]);
                }
                else if (ThirdwebManager.Instance.SDK.nativeSession.account != null)
                {
                    return ThirdwebManager.Instance.SDK.nativeSession.account.Address;
                }
                else
                {
                    throw new UnityException("No Account Connected!");
                }
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
                return ThirdwebManager.Instance.SDK.nativeSession.account != null || Utils.ActiveWalletConnectSession();
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
                int chainId = (int)(await ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.ChainId.SendRequestAsync()).Value;
                ThirdwebManager.Instance.SDK.nativeSession.lastChainId = chainId;
                return chainId;
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
                    var receipt = await ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(to, decimal.Parse(amount));
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
                if (Utils.ActiveWalletConnectSession())
                {
                    return await WalletConnect.Instance.PersonalSign(message);
                }
                else if (ThirdwebManager.Instance.SDK.nativeSession.account != null)
                {
                    var signer = new EthereumMessageSigner();
                    return signer.EncodeUTF8AndSign(message, new EthECKey(ThirdwebManager.Instance.SDK.nativeSession.account.PrivateKey));
                }
                else
                {
                    throw new UnityException("No Account Connected!");
                }
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
                return addressRecovered; // TODO: Check viability
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
                Nethereum.RPC.Eth.DTOs.TransactionInput input = new Nethereum.RPC.Eth.DTOs.TransactionInput(
                    transactionRequest.data,
                    transactionRequest.to,
                    transactionRequest.value,
                    new Nethereum.Hex.HexTypes.HexBigInteger(BigInteger.Parse(transactionRequest.gasLimit)),
                    new Nethereum.Hex.HexTypes.HexBigInteger(BigInteger.Parse(transactionRequest.gasPrice))
                );
                var receipt = await ThirdwebManager.Instance.SDK.nativeSession.web3.TransactionManager.SendTransactionAndWaitForReceiptAsync(input);
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

    public struct WalletConnection
    {
        public WalletProvider provider;
        public int chainId;
        public string password;
        public string privateKey;
    }

    public class WalletProvider
    {
        private WalletProvider(string value)
        {
            Value = value;
        }

        public static string Value { get; private set; }

        public static WalletProvider MetaMask
        {
            get { return new WalletProvider("metamask"); }
        }
        public static WalletProvider CoinbaseWallet
        {
            get { return new WalletProvider("coinbaseWallet"); }
        }
        public static WalletProvider WalletConnect
        {
            get { return new WalletProvider("walletConnect"); }
        }
        public static WalletProvider Injected
        {
            get { return new WalletProvider("injected"); }
        }
        public static WalletProvider MagicAuth
        {
            get { return new WalletProvider("magicAuth"); }
        }
        public static WalletProvider DeviceWallet
        {
            get { return new WalletProvider("deviceWallet"); }
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
