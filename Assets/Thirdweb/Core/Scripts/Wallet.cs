using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;
using System;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Unity;
using WalletConnectSharp.NEthereum.Account;
using WalletConnectSharp.NEthereum;

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
        /// <param name="walletConnection">The wallet provider and chainId to connect to. Defaults to the injected browser extension.</param>
        public async Task<string> Connect(WalletConnection? walletConnection = null, Account account = null, WCSessionData wcSessionData = null)
        {
            if (Utils.IsWebGLBuild())
            {
                var connection = walletConnection ?? new WalletConnection() { provider = WalletProvider.Injected, };
                ;
                return await Bridge.Connect(connection);
            }
            else
            {
                ThirdwebSDK.NativeSession newNativeSession = new ThirdwebSDK.NativeSession();
                if (account != null)
                {
                    newNativeSession.lastRPC = ThirdwebManager.Instance.SDK.nativeSession.lastRPC;
                    newNativeSession.lastChainId = ThirdwebManager.Instance.SDK.nativeSession.lastChainId;
                    newNativeSession.account = account;
                    newNativeSession.web3 = new Web3(account, newNativeSession.lastRPC);
                    ThirdwebManager.Instance.SDK.nativeSession = newNativeSession;
                    return account.Address;
                }
                else if (wcSessionData != null)
                {
                    newNativeSession.lastRPC = ThirdwebManager.Instance.SDK.nativeSession.lastRPC;
                    newNativeSession.lastChainId = ThirdwebManager.Instance.SDK.nativeSession.lastChainId;
                    newNativeSession.account = null;
                    newNativeSession.web3 = WalletConnect.Instance.Session.BuildWeb3(new Uri(newNativeSession.lastRPC)).AsWalletAccount(true);
                    ThirdwebManager.Instance.SDK.nativeSession = newNativeSession;
                    return WalletConnect.ActiveSession.Accounts[0];
                }
                else
                {
                    newNativeSession.lastRPC = ThirdwebManager.Instance.SDK.nativeSession.lastRPC;
                    newNativeSession.lastChainId = ThirdwebManager.Instance.SDK.nativeSession.lastChainId;
                    newNativeSession.account = Utils.GenerateAccount(newNativeSession.lastChainId, null, null); // TODO: Allow custom private keys/passwords
                    Debug.Log("account" + account.Address);
                    Debug.Log("lastrpc" + newNativeSession.lastRPC);
                    newNativeSession.web3 = new Web3(account, newNativeSession.lastRPC);
                    ThirdwebManager.Instance.SDK.nativeSession = newNativeSession;
                    return account.Address;
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
                if (Utils.ActiveWalletConnectSession())
                {
                    WalletConnect.Instance.DisableWalletConnect();
                }
                ThirdwebSDK.NativeSession newNativeSession = new ThirdwebSDK.NativeSession();
                newNativeSession.lastRPC = ThirdwebManager.Instance.SDK.nativeSession.lastRPC;
                newNativeSession.lastChainId = ThirdwebManager.Instance.SDK.nativeSession.lastChainId;
                newNativeSession.account = null;
                newNativeSession.web3 = new Web3(newNativeSession.lastRPC); // fallback
                ThirdwebManager.Instance.SDK.nativeSession = newNativeSession;
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
                throw new UnityException("This functionality is not yet available on your current platform.");
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
                    return WalletConnect.Instance.Session.Accounts[0];
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

        public override string ToString()
        {
            return Value;
        }
    }
}
