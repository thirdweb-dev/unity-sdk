using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using UnityEngine;

namespace Thirdweb
{
    /// <summary>
    /// The entry point for the thirdweb SDK.
    /// </summary>
    public class ThirdwebSDK
    {
        /// <summary>
        /// Options for the thirdweb SDK.
        /// </summary>
        [System.Serializable]
        public struct Options
        {
            public GaslessOptions? gasless;
            public StorageOptions? storage;
            public WalletOptions? wallet;
        }

        /// <summary>
        /// Wallet configuration options.
        /// </summary>
        [System.Serializable]
        public struct WalletOptions
        {
            public string appName; // the app name that will show in different wallet providers
            public Dictionary<string, object> extras; // extra data to pass to the wallet provider
        }

        /// <summary>
        /// Storage configuration options.
        /// </summary>
        [System.Serializable]
        public struct StorageOptions
        {
            public string ipfsGatewayUrl; // override the default ipfs gateway, should end in /ipfs/
        }

        /// <summary>
        /// Gasless configuration options.
        /// </summary>
        [System.Serializable]
        public struct GaslessOptions
        {
            public OZDefenderOptions? openzeppelin;
            public BiconomyOptions? biconomy;
            public bool experimentalChainlessSupport;
        }

        /// <summary>
        /// OpenZeppelin Defender Gasless configuration options.
        /// </summary>
        [System.Serializable]
        public struct OZDefenderOptions
        {
            public string relayerUrl;
            public string relayerForwarderAddress;
        }

        /// <summary>
        /// Biconomy Gasless configuration options.
        /// </summary>
        [System.Serializable]
        public struct BiconomyOptions
        {
            public string apiId;
            public string apiKey;
        }

        private string chainOrRPC;

        /// <summary>
        /// Connect and Interact with a user's wallet
        /// </summary>
        public Wallet wallet;

        /// <summary>
        /// Deploy new contracts
        /// </summary>
        public Deployer deployer;

        public Web3 web3;

        public Account account;

        /// <summary>
        /// Create an instance of the thirdweb SDK. Requires a webGL browser context.
        /// </summary>
        /// <param name="chainOrRPC">The chain name or RPC url to connect to</param>
        /// <param name="options">Configuration options</param>
        public ThirdwebSDK(string chainOrRPC, int chainId = -1, Options options = new Options())
        {
            if (!chainOrRPC.StartsWith("https://"))
                throw new UnityException("Invalid RPC URL!");

            this.chainOrRPC = chainOrRPC;
            this.wallet = new Wallet();
            this.deployer = new Deployer();
            Bridge.Initialize(chainOrRPC, options);

            if (!Utils.IsWebGLBuild())
            {
                if (chainId == -1)
                    throw new UnityException("Chain ID override required for native platforms!");

                var path = Application.persistentDataPath + "/account.json";
                var keyStoreService = new Nethereum.KeyStore.KeyStoreScryptService();
                var password = SystemInfo.deviceUniqueIdentifier;

                if (File.Exists(path))
                {
                    var encryptedJson = File.ReadAllText(path);
                    var key = keyStoreService.DecryptKeyStoreFromJson(password, encryptedJson);
                    this.account = new Account(key, chainId);
                }
                else
                {
                    var scryptParams = new Nethereum.KeyStore.Model.ScryptParams
                    {
                        Dklen = 32,
                        N = 262144,
                        R = 1,
                        P = 8
                    };
                    var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
                    var keyStore = keyStoreService.EncryptAndGenerateKeyStore(
                        password,
                        ecKey.GetPrivateKeyAsBytes(),
                        ecKey.GetPublicAddress(),
                        scryptParams
                    );
                    var json = keyStoreService.SerializeKeyStoreToJson(keyStore);
                    File.WriteAllText(path, json);
                    this.account = new Account(ecKey, chainId);
                }

                this.web3 = new Web3(this.account, chainOrRPC);
                Debug.Log($"Connected to RPC {chainOrRPC} (Chain ID: {chainId}) with account {account.Address}");
            }
        }

        /// <summary>
        /// Get an instance of a deployed contract.
        /// </summary>
        /// <param name="address">The contract address</param>
        /// <param name="abi">Optionally pass the ABI for contracts that cannot be auto resolved. Expected format for the ABI is escaped JSON string</param>
        /// <returns>A contract instance</returns>
        public Contract GetContract(string address, string abi = null)
        {
            return new Contract(this.chainOrRPC, address, abi);
        }
    }
}
