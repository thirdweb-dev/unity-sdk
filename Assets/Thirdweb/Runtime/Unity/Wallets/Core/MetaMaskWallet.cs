using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.HostWallet;
using Newtonsoft.Json;
using UnityEngine;

namespace Thirdweb.Unity
{
    public class MetaMaskWallet : IThirdwebWallet
    {
        public ThirdwebClient Client => _client;
        public ThirdwebAccountType AccountType => ThirdwebAccountType.ExternalAccount;

        private static ThirdwebClient _client;

        protected MetaMaskWallet() { }

        public static async Task<MetaMaskWallet> Create(ThirdwebClient client, BigInteger activeChainId)
        {
            _client = client;

            if (Application.platform != RuntimePlatform.WebGLPlayer || Application.isEditor)
            {
                throw new Exception("MetaMaskWallet is only available in WebGL Builds. Please use a different wallet provider on native platforms.");
            }

            var metaMaskInstance = WebGLMetaMask.Instance;

            if (metaMaskInstance.IsConnected() && !string.IsNullOrEmpty(metaMaskInstance.GetAddress()))
            {
                ThirdwebDebug.Log("MetaMask already initialized.");
                await EnsureCorrectNetwork(activeChainId);
                return new MetaMaskWallet();
            }

            if (metaMaskInstance.IsMetaMaskAvailable())
            {
                ThirdwebDebug.Log("MetaMask is available. Enabling Ethereum...");
                var isEnabled = await metaMaskInstance.EnableEthereumAsync();
                ThirdwebDebug.Log($"Ethereum enabled: {isEnabled}");
                if (isEnabled && !string.IsNullOrEmpty(metaMaskInstance.GetAddress()))
                {
                    ThirdwebDebug.Log("MetaMask initialized successfully.");
                    await EnsureCorrectNetwork(activeChainId);
                    return new MetaMaskWallet();
                }
                else
                {
                    throw new Exception("MetaMask initialization failed or address is empty.");
                }
            }
            else
            {
                throw new Exception("MetaMask is not available.");
            }
        }

        #region IThirdwebWallet

        public Task<string> EthSign(byte[] rawMessage)
        {
            throw new NotImplementedException("MetaMask does not support signing raw messages.");
        }

        public Task<string> EthSign(string message)
        {
            throw new NotImplementedException("MetaMask does not support signing messages.");
        }

        public Task<string> GetAddress()
        {
            return Task.FromResult(WebGLMetaMask.Instance.GetAddress());
        }

        public Task<bool> IsConnected()
        {
            return Task.FromResult(WebGLMetaMask.Instance.IsConnected());
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
                throw new ArgumentNullException(nameof(message), "Message to sign cannot be null or empty.");
            }

            var rpcRequest = new RpcRequest { Method = "personal_sign", Params = new object[] { message, WebGLMetaMask.Instance.GetAddress() } };
            return await WebGLMetaMask.Instance.RequestAsync<string>(rpcRequest);
        }

        public async Task<string> SendTransaction(ThirdwebTransactionInput transaction)
        {
            await EnsureCorrectNetwork(transaction.ChainId);

            var rpcRequest = new RpcRequest
            {
                Method = "eth_sendTransaction",
                Params = new object[]
                {
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
                }
            };
            return await WebGLMetaMask.Instance.RequestAsync<string>(rpcRequest);
        }

        public Task<string> SignTransaction(ThirdwebTransactionInput transaction)
        {
            throw new NotImplementedException("Offline transaction signing is not supported by MetaMask.");
        }

        public async Task<string> SignTypedDataV4(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json), "Json to sign cannot be null.");
            }

            var rpcRequest = new RpcRequest { Method = "eth_signTypedData_v4", Params = new object[] { await GetAddress(), json } };
            return await WebGLMetaMask.Instance.RequestAsync<string>(rpcRequest);
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

        public virtual async Task<string> Authenticate(
            string domain,
            BigInteger chainId,
            string authPayloadPath = "/auth/payload",
            string authLoginPath = "/auth/login",
            IThirdwebHttpClient httpClientOverride = null
        )
        {
            await EnsureCorrectNetwork(chainId);

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

        public Task<string> RecoverAddressFromEthSign(string message, string signature)
        {
            throw new NotImplementedException();
        }

        public Task<string> RecoverAddressFromPersonalSign(string message, string signature)
        {
            throw new NotImplementedException();
        }

        public Task<string> RecoverAddressFromTypedDataV4<T, TDomain>(T data, TypedData<TDomain> typedData, string signature)
            where TDomain : IDomain
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Network Switching

        public static async Task EnsureCorrectNetwork(BigInteger chainId)
        {
            if (WebGLMetaMask.Instance.GetActiveChainId() != chainId)
            {
                await AddNetwork(chainId);
                if (WebGLMetaMask.Instance.GetActiveChainId() == chainId)
                    return;
                await SwitchNetwork(chainId);
            }
        }

        private static async Task SwitchNetwork(BigInteger chainId)
        {
            var switchEthereumChainParameter = new SwitchEthereumChainParameter { ChainId = new HexBigInteger(chainId) };
            var rpcRequest = new RpcRequest { Method = "wallet_switchEthereumChain", Params = new object[] { switchEthereumChainParameter } };
            _ = await WebGLMetaMask.Instance.RequestAsync<string>(rpcRequest);
        }

        private static async Task AddNetwork(BigInteger chainId)
        {
            var twChainData = await Utils.FetchThirdwebChainDataAsync(_client, chainId);
            var addEthereumChainParameter = new AddEthereumChainParameter
            {
                ChainId = new HexBigInteger(chainId),
                BlockExplorerUrls = twChainData.Explorers.Select(e => e.Url).ToList(),
                ChainName = twChainData.Name,
                IconUrls = new List<string> { twChainData.Icon.Url },
                NativeCurrency = new NativeCurrency
                {
                    Name = twChainData.NativeCurrency.Name,
                    Symbol = twChainData.NativeCurrency.Symbol,
                    Decimals = (uint)twChainData.NativeCurrency.Decimals,
                },
                RpcUrls = new List<string> { $"https://{chainId}.rpc.thirdweb.com/" },
            };
            var rpcRequest = new RpcRequest { Method = "wallet_addEthereumChain", Params = new object[] { addEthereumChainParameter } };
            _ = await WebGLMetaMask.Instance.RequestAsync<string>(rpcRequest);
        }

        #endregion
    }
}
