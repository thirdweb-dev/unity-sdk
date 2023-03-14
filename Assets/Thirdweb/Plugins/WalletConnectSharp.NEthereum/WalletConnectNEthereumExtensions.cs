using System.Net.Http.Headers;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using WalletConnectSharp.Core;
using WalletConnectSharp.NEthereum.Account;
using WalletConnectSharp.NEthereum.Client;
using System.IO;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.NEthereum.Model;
using Nethereum.ABI.EIP712;
using System;
using System.Threading.Tasks;

namespace WalletConnectSharp.NEthereum
{
    /// <summary>
    /// The Web3 Builder allows you to create different kinds of NEthereum
    /// Web3 instances depending on use-case
    /// </summary>
    public class Web3Builder
    {
        private WalletConnectSession session;
        private IClient client;

        internal Web3Builder(WalletConnectSession session, IClient client)
        {
            this.session = session;
            this.client = client;
        }

        /// <summary>
        /// Build a Web3 instance using the WalletConnectAccount and WalletConnectTransactionManager. This will
        /// allow you to send any form of transactions directly on-chain (both legacy and EIP-1559)
        /// Signing transactions is limited by support from the connected wallet in the given WalletConnect session.
        /// To force signing transaction support, set allowEthSign to true. allowEthSign will fallback to eth_sign
        /// if eth_signTransaction is not supported by the wallet. This only supports legacy transactions
        /// <param name="useEthSignForTransactionSigning">Whether the eth_sign endpoint should be used if eth_signTransaction is not supported by the connected wallet</para>
        /// </summary>
        /// <returns>A new Web3 instance with an WalletConnectAccount attached representing the WalletConnect session</returns>
        public Web3 AsWalletAccount(bool useEthSignForTransactionSigning = false)
        {
            WalletConnectAccount account = new WalletConnectAccount(session, client, useEthSignForTransactionSigning);

            return new Web3(account, client);
        }

        /// <summary>
        /// Build a Web3 instance with no IAccount attached. This will route all RPC Requests through WalletConnect
        /// protocol. This is considered unmanaged since NEthereum will not automatically properly format transactions
        /// for you.
        /// </summary>
        /// <returns>A new Web3 instance with no account attached</returns>
        public Web3 AsUnmanagedAccount()
        {
            return new Web3(client);
        }
    }

    public static class WalletConnectNEthereumExtensions
    {
        /// <summary>
        /// Create a new Web3Builder instance for the given WalletConnectSession and connection options.
        /// Infura is used as the read client. You must specify your infura project ID. The Infura instance will
        /// only be used to make read calls (such as eth_call or eth_estimateGas), all other calls (eth_sendTransaction)
        /// will go through the WalletConnectSession instance given.
        /// </summary>
        /// <param name="session">The WalletConnectSession to use</param>
        /// <param name="infruaId">The project ID of the Infura instance to connect to for read calls</param>
        /// <param name="network">An optional network name to use. Used in the Infura URL</param>
        /// <param name="authenticationHeader">An optional authentication header to provide to the endpoint</param>
        /// <returns>A new Web3Builder that can be used to build the NEthereum Web3 object</returns>
        public static Web3Builder BuildWeb3(this WalletConnectSession session, string infruaId, string network = "mainnet", AuthenticationHeaderValue authenticationHeader = null)
        {
            var client = session.CreateProviderWithInfura(infruaId, network, authenticationHeader);

            return new Web3Builder(session, client);
        }

        /// <summary>
        /// Create a new Web3Builder instance for the given WalletConnectSession and connection options.
        /// A JSON-RPC endpoint will be used as the read client. The JSON-RPC endpoint given will only be used
        /// to make read calls (such as eth_call or eth_estimateGas), all other calls (eth_sendTransaction) will
        /// go through the WalletConnectSession instance given.
        /// </summary>
        /// <param name="session">The WalletConnectSession to use</param>
        /// <param name="url">The URL of the JSON-RPC endpoint (i.e geth)</param>
        /// <param name="authenticationHeader">An optional authentication header to provide to the endpoint</param>
        /// <returns>A new Web3Builder that can be used to build the NEthereum Web3 object</returns>
        public static Web3Builder BuildWeb3(this WalletConnectSession session, Uri url, AuthenticationHeaderValue authenticationHeader = null)
        {
            var client = session.CreateProvider(url, authenticationHeader);

            return new Web3Builder(session, client);
        }

        /// <summary>
        /// Create a new Web3Builder instance for the given WalletConnectSession and connection options.
        /// Another IClient instance is used as the read client. The IClient instance given will only be used to
        /// make read calls (such as eth_call or eth_estimateGas), all other calls (eth_sendTransaction) will go
        /// through the WalletConnectSession instance given.
        /// </summary>
        /// <param name="session">The WalletConnectSession to use</param>
        /// <param name="readClient">Any IClient instance to use as the read client</param>
        /// <returns>A new Web3Builder that can be used to build the NEthereum Web3 object</returns>
        public static Web3Builder BuildWeb3(this WalletConnectSession session, IClient readClient)
        {
            var client = session.CreateProvider(readClient);

            return new Web3Builder(session, client);
        }

        /// <summary>
        /// Create a new NEtehereum IClient instance that uses Infura as the read client. You must specify your
        /// infura project ID. The Infura instance will only be used to make read calls (such as eth_call
        /// or eth_estimateGas), all other calls (eth_sendTransaction) will go through the WalletConnectSession
        /// instance given. The returned IClient instance can be used as a Provider in an NEthereum Web3 instance
        /// </summary>
        /// <param name="session">The WalletConnectSession to use</param>
        /// <param name="infruaId">The project ID of the Infura instance to connect to for read calls</param>
        /// <param name="network">An optional network name to use. Used in the Infura URL</param>
        /// <param name="authenticationHeader">An optional authentication header to provide to the endpoint</param>
        /// <returns>
        /// A new NEtehereum IClient instance that uses Infura as the read client and the WalletConnectSession
        /// for write client. The returned IClient instance can be used as a Provider in an NEthereum Web3 instance
        /// </returns>
        [Obsolete("Use BuildWeb3(infruaId).AsUnmanagedAccount() instead. Will be removed in v2")]
        public static IClient CreateProviderWithInfura(this WalletConnectSession session, string infruaId, string network = "mainnet", AuthenticationHeaderValue authenticationHeader = null)
        {
            var url = "https://" + network + ".infura.io/v3/" + infruaId;

            return CreateProvider(session, new Uri(url), authenticationHeader);
        }

        /// <summary>
        /// Create a new NEtehereum IClient instance that uses a JSON-RPC endpoint as the read client.
        /// The JSON-RPC endpoint given will only be used to make read calls (such as eth_call
        /// or eth_estimateGas), all other calls (eth_sendTransaction) will go through the WalletConnectSession
        /// instance given. The returned IClient instance can be used as a Provider in an NEthereum Web3 instance
        /// </summary>
        /// <param name="session">The WalletConnectSession to use</param>
        /// <param name="url">The URL of the JSON-RPC endpoint (i.e geth)</param>
        /// <param name="authenticationHeader">An optional authentication header to provide to the endpoint</param>
        /// <returns>
        /// A new NEtehereum IClient instance that uses a JSON-RPC endpoint as the read client and the
        /// WalletConnectSession for write client. The returned IClient instance can be used as a
        /// Provider in an NEthereum Web3 instance
        /// </returns>
        [Obsolete("Use BuildWeb3(url).AsUnmanagedAccount() instead. Will be removed in v2")]
        public static IClient CreateProvider(this WalletConnectSession session, Uri url, AuthenticationHeaderValue authenticationHeader = null)
        {
            return CreateProvider(session, new RpcClient(url, authenticationHeader));
        }

        /// <summary>
        /// Create a new NEtehereum IClient instance that uses another IClient instance as the read client.
        /// The IClient instance given will only be used to make read calls (such as eth_call
        /// or eth_estimateGas), all other calls (eth_sendTransaction) will go through the WalletConnectSession
        /// instance given. The returned IClient instance can be used as a Provider in an NEthereum Web3 instance
        /// </summary>
        /// <param name="session">The WalletConnectSession to use</param>
        /// <param name="readClient">Any IClient instance to use as the read client</param>
        /// <returns>
        /// A new NEtehereum IClient instance that uses another IClient instance as the read client and the
        /// WalletConnectSession for write client. The returned IClient instance can be used as a
        /// Provider in an NEthereum Web3 instance
        /// </returns>
        [Obsolete("Use BuildWeb3(readClient).AsUnmanagedAccount() instead. Will be removed in v2")]
        public static IClient CreateProvider(this WalletConnectSession session, IClient readClient)
        {
            if (!session.Connected)
            {
                throw new Exception("No connection has been made yet!");
            }

            return new FallbackProvider(new WalletConnectClient(session), readClient);
        }

        /// <summary>
        /// Submit an EIP-712 structure for signature using the built-in Nethereum serialization scheme.
        /// </summary>
        public static async Task<string> EthSignTypedData<T, TDomain>(this WalletConnectSession session, string address, T data, TypedData<TDomain> typedData)
            where TDomain : IDomain
        {
            if (session.Disconnected)
            {
                throw new IOException("Session stale! The session has been disconnected. This session cannot be reused.");
            }

            var request = new NEthSignTypedData<T, TDomain>(address, data, typedData);

            var response = await session.Send<NEthSignTypedData<T, TDomain>, EthResponse>(request);

            return response.Result;
        }
    }
}
