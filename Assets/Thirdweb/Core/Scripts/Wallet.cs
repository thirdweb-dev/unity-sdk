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
using WalletConnectSharp.Core.Models.Ethereum;
using link.magic.unity.sdk;
using Nethereum.RPC;
using MetaMask.Unity;
using MetaMask.NEthereum;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.EIP712;
using Nethereum.Signer.EIP712;
using Newtonsoft.Json;

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
                ThirdwebSDK.NativeSession oldSession = ThirdwebManager.Instance.SDK.nativeSession;

                switch (walletConnection.provider)
                {
                    case WalletProvider.LocalWallet:
                        Account acc = Utils.UnlockOrGenerateLocalAccount(oldSession.lastChainId, walletConnection.password);
                        ThirdwebManager.Instance.SDK.nativeSession = new ThirdwebSDK.NativeSession(
                            walletConnection.provider,
                            oldSession.lastChainId,
                            oldSession.lastRPC,
                            acc,
                            new Web3(acc, oldSession.lastRPC),
                            oldSession.options,
                            oldSession.siweSession
                        );
                        break;
                    case WalletProvider.WalletConnectV1:
                        if (WalletConnect.Instance == null)
                        {
                            GameObject.Instantiate(ThirdwebManager.Instance.WalletConnectPrefab);
                            await new WaitForSeconds(0.5f);
                        }

                        WalletConnect.Instance.Initialize();

                        await WalletConnect.Instance.EnableWalletConnect();

                        ThirdwebManager.Instance.SDK.nativeSession = new ThirdwebSDK.NativeSession(
                            walletConnection.provider,
                            oldSession.lastChainId,
                            oldSession.lastRPC,
                            null,
                            WalletConnect.Instance.Session.BuildWeb3(new Uri(oldSession.lastRPC)).AsWalletAccount(true),
                            oldSession.options,
                            oldSession.siweSession
                        );

                        try
                        {
                            await WalletConnect.Instance.WalletSwitchEthChain(new EthChain() { chainId = ThirdwebManager.Instance.SDK.currentChainData.chainId });
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning("Switching chain error, attempting to add chain: " + e.Message);
                            try
                            {
                                await WalletConnect.Instance.WalletAddEthChain(ThirdwebManager.Instance.SDK.currentChainData);
                                await WalletConnect.Instance.WalletSwitchEthChain(new EthChain() { chainId = ThirdwebManager.Instance.SDK.currentChainData.chainId });
                            }
                            catch (System.Exception f)
                            {
                                Debug.LogWarning("Adding chain error: " + f.Message);
                            }
                        }
                        break;
                    case WalletProvider.MagicLink:
                        if (MagicUnity.Instance == null)
                        {
                            GameObject.Instantiate(ThirdwebManager.Instance.MagicAuthPrefab);
                            await new WaitForSeconds(0.5f);
                        }

                        if (ThirdwebManager.Instance.SDK.nativeSession.options.wallet?.magicLinkApiKey == null)
                            throw new UnityException("MagicLink API Key is not set!");

                        MagicUnity.Instance.Initialize(
                            ThirdwebManager.Instance.SDK.nativeSession.options.wallet?.magicLinkApiKey,
                            new link.magic.unity.sdk.Relayer.CustomNodeConfiguration(oldSession.lastRPC, oldSession.lastChainId)
                        );

                        await MagicUnity.Instance.EnableMagicAuth(walletConnection.email);

                        ThirdwebManager.Instance.SDK.nativeSession = new ThirdwebSDK.NativeSession(
                            walletConnection.provider,
                            oldSession.lastChainId,
                            oldSession.lastRPC,
                            null,
                            new Web3(Magic.Instance.Provider),
                            oldSession.options,
                            oldSession.siweSession
                        );
                        break;
                    case WalletProvider.MetaMask:
                        if (MetaMaskUnity.Instance == null)
                        {
                            GameObject.Instantiate(ThirdwebManager.Instance.MetamaskPrefab);
                            MetaMaskUnity.Instance.Initialize();
                            await new WaitForSeconds(1f);
                        }

                        MetaMaskUnity.Instance.Connect();

                        bool connected = false;
                        MetaMaskUnity.Instance.Wallet.WalletAuthorized += (sender, e) =>
                        {
                            ThirdwebManager.Instance.SDK.nativeSession = new ThirdwebSDK.NativeSession(
                                walletConnection.provider,
                                oldSession.lastChainId,
                                oldSession.lastRPC,
                                null,
                                MetaMaskUnity.Instance.Wallet.CreateWeb3(),
                                oldSession.options,
                                oldSession.siweSession
                            );
                            connected = true;
                        };

                        await new WaitUntil(() => connected);

                        try
                        {
                            await MetaMaskUnity.Instance.WalletSwitchEthChain(new EthChain() { chainId = ThirdwebManager.Instance.SDK.currentChainData.chainId });
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning("Switching chain error, attempting to add chain: " + e.Message);
                            try
                            {
                                await MetaMaskUnity.Instance.WalletAddEthChain(ThirdwebManager.Instance.SDK.currentChainData);
                                await MetaMaskUnity.Instance.WalletSwitchEthChain(new EthChain() { chainId = ThirdwebManager.Instance.SDK.currentChainData.chainId });
                            }
                            catch (System.Exception f)
                            {
                                Debug.LogWarning("Adding chain error: " + f.Message);
                            }
                        }

                        break;
                    default:
                        throw new UnityException("This wallet connection method is not supported on this platform!");
                }

                return await GetAddress();
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

                switch (oldSession.provider)
                {
                    case WalletProvider.WalletConnectV1:
                        WalletConnect.Instance.DisableWalletConnect();
                        break;
                    case WalletProvider.MagicLink:
                        MagicUnity.Instance.DisableMagicAuth();
                        break;
                    case WalletProvider.MetaMask:
                        MetaMaskUnity.Instance.Wallet.Disconnect();
                        break;
                    default:
                        break;
                }

                ThirdwebManager.Instance.SDK.nativeSession = new ThirdwebSDK.NativeSession(
                    WalletProvider.LocalWallet,
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
                if (!await IsConnected())
                    throw new Exception("No account connected!");

                if (currencyAddress != Utils.NativeTokenAddress)
                {
                    Contract contract = ThirdwebManager.Instance.SDK.GetContract(currencyAddress);
                    return await contract.ERC20.Balance();
                }
                else
                {
                    var balance = await ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetBalance.SendRequestAsync(await ThirdwebManager.Instance.SDK.wallet.GetAddress());
                    var nativeCurrency = ThirdwebManager.Instance.SDK.currentChainData.nativeCurrency;
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
                string address = null;

                switch (ThirdwebManager.Instance.SDK.nativeSession.provider)
                {
                    case WalletProvider.LocalWallet:
                        address = ThirdwebManager.Instance.SDK.nativeSession.account.Address;
                        break;
                    case WalletProvider.WalletConnectV1:
                        address = WalletConnect.Instance.Session.Accounts[0];
                        break;
                    case WalletProvider.MagicLink:
                        address = await MagicUnity.Instance.GetAddress();
                        break;
                    case WalletProvider.MetaMask:
                        address = MetaMaskUnity.Instance.Wallet.SelectedAddress;
                        break;
                    default:
                        throw new UnityException("No Account Connected!");
                }

                return Nethereum.Util.AddressUtil.Current.ConvertToChecksumAddress(address);
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
                return ThirdwebManager.Instance.SDK.nativeSession.account != null || ThirdwebManager.Instance.SDK.nativeSession.provider != WalletProvider.LocalWallet;
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
                switch (ThirdwebManager.Instance.SDK.nativeSession.provider)
                {
                    case WalletProvider.LocalWallet:
                        var signer = new EthereumMessageSigner();
                        return signer.EncodeUTF8AndSign(message, new EthECKey(ThirdwebManager.Instance.SDK.nativeSession.account.PrivateKey));
                    case WalletProvider.WalletConnectV1:
                        return await WalletConnect.Instance.PersonalSign(message);
                    case WalletProvider.MagicLink:
                        return await MagicUnity.Instance.PersonalSign(message);
                    case WalletProvider.MetaMask:
                        return await MetaMaskUnity.Instance.PersonalSign(message);
                    default:
                        throw new UnityException("Invalid Wallet Provider!");
                }
            }
        }

        public async Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
            where TDomain : IDomain
        {
            switch (ThirdwebManager.Instance.SDK.nativeSession.provider)
            {
                case WalletProvider.LocalWallet:
                    var signer = new Eip712TypedDataSigner();
                    var key = new EthECKey(ThirdwebManager.Instance.SDK.nativeSession.account.PrivateKey);
                    return signer.SignTypedDataV4(data, typedData, key);
                case WalletProvider.WalletConnectV1:
                    return await WalletConnect.Instance.SignTypedDataV4(data, typedData);
                case WalletProvider.MagicLink:
                    return await MagicUnity.Instance.SignTypedDataV4(data, typedData);
                case WalletProvider.MetaMask:
                    return await MetaMaskUnity.Instance.SignTypedDataV4(data, typedData);
                default:
                    throw new UnityException("Invalid Wallet Provider!");
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
        MetaMask,
        CoinbaseWallet,
        WalletConnectV1,
        Injected,
        MagicLink,
        LocalWallet,
        SmartWallet
    }
}
