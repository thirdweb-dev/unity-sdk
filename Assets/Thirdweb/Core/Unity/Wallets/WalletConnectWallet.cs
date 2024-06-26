using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Nethereum.ABI.EIP712;
using Newtonsoft.Json;
using UnityEngine;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectUnity.Core;
using WalletConnectUnity.Modal;
using Thirdweb.Redcode.Awaiting;
using System.Linq;
using WalletConnectUnity.Nethereum;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb.Unity
{
    public class WalletConnectWallet : IThirdwebWallet
    {
        #region Core

        public ThirdwebAccountType AccountType => ThirdwebAccountType.ExternalAccount;

        protected ThirdwebClient _client;

        protected Exception _exception;
        protected bool _isConnected;
        protected string[] _supportedChains;
        protected string[] _includedWalletIds;

        private WalletConnectServiceCore _walletConnectService;

        protected WalletConnectWallet(ThirdwebClient client)
        {
            _client = client;
        }

        public static Task<WalletConnectWallet> Create(ThirdwebClient client)
        {
            return Task.FromResult(new WalletConnectWallet(client));
        }

        public Task<string> GetAddress()
        {
            return Task.FromResult(WalletConnect.Instance.SignClient.AddressProvider.CurrentAddress().Address.ToChecksumAddress());
        }

        public Task<string> EthSign(byte[] rawMessage)
        {
            if (rawMessage == null)
            {
                throw new ArgumentNullException(nameof(rawMessage), "Message to sign cannot be null.");
            }

            throw new InvalidOperationException("EthSign is not supported by external wallets.");
        }

        public Task<string> EthSign(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "Message to sign cannot be null.");
            }

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

            var json = typedData.ToJson(data);
            return SignTypedDataV4(json);
        }

        public Task<string> SignTransaction(ThirdwebTransactionInput transaction)
        {
            throw new InvalidOperationException("Offline signing is not supported by external wallets.");
        }

        public async Task<string> SendTransaction(ThirdwebTransactionInput transaction)
        {
            var task = _walletConnectService.SendTransactionAsync(
                new TransactionInput()
                {
                    Nonce = transaction.Nonce,
                    From = transaction.From,
                    To = transaction.To,
                    Gas = transaction.Gas,
                    GasPrice = transaction.GasPrice,
                    Value = transaction.Value,
                    Data = transaction.Data,
                    MaxFeePerGas = transaction.MaxFeePerGas,
                    MaxPriorityFeePerGas = transaction.MaxPriorityFeePerGas,
                    ChainId = transaction.ChainId,
                }
            );
            SessionRequestDeeplink();
            return await task as string;
        }

        public Task<bool> IsConnected()
        {
            return Task.FromResult(WalletConnect.Instance.IsConnected);
        }

        public async Task Disconnect()
        {
            await WalletConnect.Instance.DisconnectAsync();
        }

        public virtual async Task<string> Authenticate(
            string domain,
            BigInteger chainId,
            string authPayloadPath = "/auth/payload",
            string authLoginPath = "/auth/login",
            IThirdwebHttpClient httpClientOverride = null
        )
        {
            var payloadURL = domain + authPayloadPath;
            var loginURL = domain + authLoginPath;

            var payloadBodyRaw = new { address = await GetAddress(), chainId = chainId.ToString() };
            var payloadBody = JsonConvert.SerializeObject(payloadBodyRaw);

            var httpClient = httpClientOverride ?? _client.HttpClient;

            var payloadContent = new StringContent(payloadBody, Encoding.UTF8, "application/json");
            var payloadResponse = await httpClient.PostAsync(payloadURL, payloadContent);
            _ = payloadResponse.EnsureSuccessStatusCode();
            var payloadString = await payloadResponse.Content.ReadAsStringAsync();

            var loginBodyRaw = JsonConvert.DeserializeObject<LoginPayload>(payloadString);
            var payloadToSign = Utils.GenerateSIWE(loginBodyRaw.payload);

            loginBodyRaw.signature = await PersonalSign(payloadToSign);
            var loginBody = JsonConvert.SerializeObject(new { payload = loginBodyRaw });

            var loginContent = new StringContent(loginBody, Encoding.UTF8, "application/json");
            var loginResponse = await httpClient.PostAsync(loginURL, loginContent);
            _ = loginResponse.EnsureSuccessStatusCode();
            var responseString = await loginResponse.Content.ReadAsStringAsync();
            return responseString;
        }

        #endregion

        #region UI

        public async Task<string> Connect(BigInteger initialChainId, BigInteger[] supportedChains)
        {
            var eip155ChainsSupported = new string[] { };
            if (supportedChains != null)
                eip155ChainsSupported = eip155ChainsSupported.Concat(supportedChains.Select(x => $"eip155:{x}")).ToArray();

            _exception = null;
            _isConnected = false;
            _supportedChains = eip155ChainsSupported;

            WalletConnectModal.Ready += OnReady;
            WalletConnect.Instance.ActiveSessionChanged += OnActiveSessionChanged;
            WalletConnect.Instance.SessionDisconnected += OnSessionDisconnected;

            if (WalletConnect.Instance.IsInitialized)
                CreateNewSession();

            await new WaitUntil(() => _isConnected || _exception != null);

            WalletConnectModal.Ready -= OnReady;
            WalletConnect.Instance.ActiveSessionChanged -= OnActiveSessionChanged;
            WalletConnect.Instance.SessionDisconnected -= OnSessionDisconnected;

            if (_exception != null)
            {
                throw _exception;
            }
            else
            {
                await WalletConnect.Instance.SignClient.AddressProvider.SetDefaultChainIdAsync($"eip155:{initialChainId}");
                _walletConnectService = new WalletConnectServiceCore(WalletConnect.Instance.SignClient);
                return await GetAddress();
            }
        }

        protected void OnSessionDisconnected(object sender, EventArgs e)
        {
            _isConnected = false;
        }

        protected void OnActiveSessionChanged(object sender, SessionStruct sessionStruct)
        {
            if (!string.IsNullOrEmpty(sessionStruct.Topic))
            {
                _isConnected = true;
            }
            else
            {
                _isConnected = false;
            }
        }

        protected async void OnReady(object sender, ModalReadyEventArgs args)
        {
            try
            {
                if (args.SessionResumed)
                {
                    // Session exists
                    await WalletConnect.Instance.DisconnectAsync();
                }

                CreateNewSession();
            }
            catch (Exception e)
            {
                _exception = e;
            }
        }

        protected void CreateNewSession()
        {
            try
            {
                var optionalNamespaces = new Dictionary<string, ProposedNamespace>
                {
                    {
                        "eip155",
                        new ProposedNamespace
                        {
                            Methods = new[] { "eth_sendTransaction", "personal_sign", "eth_signTypedData_v4", "wallet_switchEthereumChain", "wallet_addEthereumChain" },
                            Chains = _supportedChains,
                            Events = new[] { "chainChanged", "accountsChanged" },
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
