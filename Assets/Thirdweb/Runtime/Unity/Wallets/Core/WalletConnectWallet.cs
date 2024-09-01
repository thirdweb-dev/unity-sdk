using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Nethereum.ABI.EIP712;
using Newtonsoft.Json;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectUnity.Core;
using WalletConnectUnity.Modal;
using System.Linq;
using WalletConnectUnity.Nethereum;
using Nethereum.RPC.Eth.DTOs;
using WalletConnectUnity.Core.Evm;
using Nethereum.Hex.HexTypes;
using UnityEngine;

namespace Thirdweb.Unity
{
    public class WalletConnectWallet : IThirdwebWallet
    {
        public ThirdwebClient Client => _client;

        public ThirdwebAccountType AccountType => ThirdwebAccountType.ExternalAccount;

        protected ThirdwebClient _client;

        protected static Exception _exception;
        protected static bool _isConnected;
        protected static string[] _supportedChains;
        protected static string[] _includedWalletIds;

        private static WalletConnectServiceCore _walletConnectService;

        protected WalletConnectWallet(ThirdwebClient client)
        {
            _client = client;
        }

        public async static Task<WalletConnectWallet> Create(ThirdwebClient client, BigInteger initialChainId, BigInteger[] supportedChains)
        {
            var eip155ChainsSupported = new string[] { };
            if (supportedChains != null)
                eip155ChainsSupported = eip155ChainsSupported.Concat(supportedChains.Select(x => $"eip155:{x}")).ToArray();

            _exception = null;
            _isConnected = false;
            _supportedChains = eip155ChainsSupported;

            if (WalletConnect.Instance != null && WalletConnect.Instance.IsConnected)
            {
                try
                {
                    await WalletConnect.Instance.DisconnectAsync();
                }
                catch
                {
                    // no-op
                }
                await Task.Delay(100);
            }

            CreateNewSession(eip155ChainsSupported);

            while (!WalletConnect.Instance.IsConnected && _exception == null)
            {
                await Task.Delay(100);
            }

            if (_exception != null)
            {
                throw _exception;
            }
            else
            {
                try
                {
                    var data = new WalletSwitchEthereumChain(new HexBigInteger(initialChainId).HexValue);
                    await WalletConnect.Instance.RequestAsync<WalletSwitchEthereumChain, string>(data);
                    await Task.Delay(5000); // wait for chain switch to take effect
                    await WalletConnect.Instance.SignClient.AddressProvider.SetDefaultChainIdAsync($"eip155:{initialChainId}");
                }
                catch (Exception e)
                {
                    ThirdwebDebug.LogWarning($"Failed to ensure wallet is on active chain: {e.Message}");
                }
                _walletConnectService = new WalletConnectServiceCore(WalletConnect.Instance.SignClient);
            }

            return new WalletConnectWallet(client);
        }

        public async Task EnsureCorrectNetwork(BigInteger chainId)
        {
            var chainInfo = await Utils.GetChainMetadata(_client, chainId);
            var wcChainInfo = new EthereumChain()
            {
                chainIdHex = new HexBigInteger(chainInfo.ChainId).HexValue,
                name = chainInfo.Name,
                nativeCurrency = new Currency(chainInfo.NativeCurrency.Name, chainInfo.NativeCurrency.Symbol, chainInfo.NativeCurrency.Decimals),
                rpcUrls = new string[] { $"https://{chainInfo.ChainId}.rpc.thirdweb.com" },
                blockExplorerUrls = chainInfo.Explorers == null || chainInfo.Explorers.Count == 0 ? null : new string[] { chainInfo.Explorers[0].Url },
                chainIdDecimal = chainInfo.ChainId.ToString(),
            };
            var request = new WalletAddEthereumChain(wcChainInfo);
            try
            {
                await WalletConnect.Instance.RequestAsync<WalletAddEthereumChain, string>(request);
            }
            catch
            {
                // no-op
            }
            var data = new WalletSwitchEthereumChain(new HexBigInteger(chainId).HexValue);
            await WalletConnect.Instance.RequestAsync<WalletSwitchEthereumChain, string>(data);
            await Task.Delay(5000); // wait for chain switch to take effect
            await WalletConnect.Instance.SignClient.AddressProvider.SetDefaultChainIdAsync($"eip155:{chainId}");
        }

        #region IThirdwebWallet

        public Task<string> GetAddress()
        {
            return Task.FromResult(WalletConnect.Instance.SignClient.AddressProvider.CurrentAddress(WalletConnect.Instance.ActiveChainId).Address.ToChecksumAddress());
        }

        public Task<string> EthSign(byte[] rawMessage)
        {
            throw new InvalidOperationException("EthSign is not supported by external wallets.");
        }

        public Task<string> EthSign(string message)
        {
            throw new InvalidOperationException("EthSign is not supported by external wallets.");
        }

        public Task<string> PersonalSign(byte[] rawMessage)
        {
            if (rawMessage == null)
            {
                throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
            }

            var message = Encoding.UTF8.GetString(rawMessage);
            return PersonalSign(message);
        }

        public async Task<string> PersonalSign(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
            }

            var task = _walletConnectService.PersonalSignAsync(message);
            SessionRequestDeeplink();
            return await task as string;
        }

        public async Task<string> SignTypedDataV4(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json), "Json to sign cannot be null.");
            }

            var task = _walletConnectService.EthSignTypedDataV4Async(json);
            SessionRequestDeeplink();
            return await task as string;
        }

        public Task<string> SignTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData)
            where TDomain : IDomain
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Data to sign cannot be null.");
            }

            if (typedData == null)
            {
                throw new ArgumentNullException(nameof(typedData), "Typed data to sign cannot be null.");
            }

            var safeJson = Utils.ToJsonExternalWalletFriendly(typedData, data);
            return SignTypedDataV4(safeJson);
        }

        public Task<string> SignTransaction(ThirdwebTransactionInput transaction)
        {
            throw new NotImplementedException("Raw transaction signing is not supported.");
        }

        public async Task<string> SendTransaction(ThirdwebTransactionInput transaction)
        {
            Debug.Log("DEFAULT CHAIN ID: " + WalletConnect.Instance.SignClient.AddressProvider.DefaultChainId);
            var task = _walletConnectService.SendTransactionAsync(
                new TransactionInput()
                {
                    Nonce = transaction.Nonce,
                    From = await GetAddress(),
                    To = transaction.To,
                    Gas = transaction.Gas,
                    GasPrice = transaction.GasPrice,
                    Value = transaction.Value,
                    Data = transaction.Data,
                    MaxFeePerGas = transaction.MaxFeePerGas,
                    MaxPriorityFeePerGas = transaction.MaxPriorityFeePerGas,
                    ChainId = new HexBigInteger(WalletConnect.Instance.SignClient.AddressProvider.DefaultChainId),
                }
            );
            SessionRequestDeeplink();
            return await task as string;
        }

        public async Task<ThirdwebTransactionReceipt> ExecuteTransaction(ThirdwebTransactionInput transaction)
        {
            var hash = await SendTransaction(transaction);
            return await ThirdwebTransaction.WaitForTransactionReceipt(client: _client, chainId: WebGLMetaMask.Instance.GetActiveChainId(), txHash: hash);
        }

        public Task<bool> IsConnected()
        {
            return Task.FromResult(WalletConnect.Instance.IsConnected);
        }

        public async Task Disconnect()
        {
            await WalletConnect.Instance.DisconnectAsync();
        }

        public Task<string> RecoverAddressFromEthSign(string message, string signature)
        {
            throw new NotImplementedException();
        }

        public Task<string> RecoverAddressFromPersonalSign(string message, string signature)
        {
            var signer = new Nethereum.Signer.EthereumMessageSigner();
            var addressRecovered = signer.EncodeUTF8AndEcRecover(message, signature);
            return Task.FromResult(addressRecovered);
        }

        public Task<string> RecoverAddressFromTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData, string signature)
            where TDomain : IDomain
        {
            throw new NotImplementedException();
        }

        #endregion

        #region UI

        protected static void CreateNewSession(string[] supportedChains)
        {
            try
            {
                var optionalNamespaces = new Dictionary<string, ProposedNamespace>()
                {
                    {
                        "eip155",
                        new ProposedNamespace
                        {
                            Methods = new[] { "eth_sendTransaction", "personal_sign", "eth_signTypedData_v4", "wallet_switchEthereumChain", "wallet_addEthereumChain" },
                            Chains = supportedChains,
                            Events = new string[] { "chainChanged", "accountsChanged" },
                        }
                    }
                };

                var connectOptions = new ConnectOptions { OptionalNamespaces = optionalNamespaces, };

                // Open modal
                WalletConnectModal.Open(new WalletConnectModalOptions { ConnectOptions = connectOptions, IncludedWalletIds = _includedWalletIds });
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        #endregion

        private void SessionRequestDeeplink()
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            var activeSessionTopic = WalletConnect.Instance.ActiveSession.Topic;
            WalletConnect.Instance.Linker.OpenSessionRequestDeepLinkAfterMessageFromSession(activeSessionTopic);
#endif
        }
    }
}
