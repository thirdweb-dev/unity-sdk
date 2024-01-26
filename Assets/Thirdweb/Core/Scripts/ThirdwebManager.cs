using UnityEngine;
using System.Collections.Generic;
using System.Numerics;

namespace Thirdweb
{
    [System.Serializable]
    public class ChainData
    {
        public string identifier;
        public string chainId;
        public string rpcOverride;

        public ChainData(string identifier, string chainId, string rpcOverride)
        {
            this.identifier = identifier;
            this.chainId = chainId;
            this.rpcOverride = rpcOverride;
        }
    }

    public class ThirdwebManager : MonoBehaviour
    {
        [Tooltip("The chain to initialize the SDK with")]
        public string activeChain = "goerli";

        [Tooltip("Support any chain by adding it to this list from the inspector")]
        public List<ChainData> supportedChains =
            new()
            {
                new ChainData("ethereum", "1", null),
                new ChainData("goerli", "5", null),
                new ChainData("polygon", "137", null),
                new ChainData("mumbai", "80001", null),
                new ChainData("fantom", "250", null),
                new ChainData("fantom-testnet", "4002", null),
                new ChainData("avalanche", "43114", null),
                new ChainData("avalanche-fuji", "43113", null),
                new ChainData("optimism", "10", null),
                new ChainData("optimism-goerli", "420", null),
                new ChainData("arbitrum", "42161", null),
                new ChainData("arbitrum-goerli", "421613", null),
                new ChainData("binance", "56", null),
                new ChainData("binance-testnet", "97", null),
                new ChainData("sepolia", "11155111", null),
            };

        [Tooltip("Thirdweb Client ID (https://thirdweb.com/create-api-key/). Used for default thirdweb services such as RPC, Storage and Account Abstraction.")]
        public string clientId;

        [Tooltip("Whether the SDK should initialize on awake or not")]
        public bool initializeOnAwake = true;

        [Tooltip("Whether to show thirdweb sdk debug logs")]
        public bool showDebugLogs = true;

        [Tooltip("Optional Bundle ID override for thirdweb services")]
        public string bundleIdOverride = null;

        [Tooltip("General Thirdweb Settings")]
        public ThirdwebConfig thirdwebConfig;

        [Tooltip("The name of your app")]
        public string appName = null;

        [Tooltip("The description of your app")]
        public string appDescription = null;

        [Tooltip("Favicons for your app")]
        public string[] appIcons = new string[] { };

        [Tooltip("The url of your app")]
        public string appUrl = null;

        [Tooltip("IPFS Gateway Override")]
        public string storageIpfsGatewayUrl = null;

        [Tooltip("Autotask URL")]
        public string relayerUrl = null;

        [Tooltip("Forwarders can be found here https://github.com/thirdweb-dev/ozdefender-autotask")]
        public string forwarderAddress = null;

        [Tooltip("Forwarder Domain Override (Defaults to GSNv2 Forwarder if left empty)")]
        public string forwarderDomainOverride = null;

        [Tooltip("Forwarder Version (Defaults to 0.0.1 if left empty)")]
        public string forwaderVersionOverride = null;

        [Tooltip("WalletConnect Project ID (https://cloud.walletconnect.com/app)")]
        public string walletConnectProjectId = null;

        [Tooltip("Wallets to show in the WalletConnect Modal (https://walletconnect.com/explorer)")]
        public string[] walletConnectExplorerRecommendedWalletIds = new string[] { };

        [Tooltip("Factory Contract Address")]
        public string factoryAddress;

        [Tooltip("Whether it should use a paymaster for gasless transactions or not")]
        public bool gasless;

        [Tooltip("Indicates whether to deploy the smart wallet upon signing any type of message.")]
        public bool deployOnSign;

        [Tooltip("Optional - If you want to use a custom relayer, you can provide the URL here")]
        public string bundlerUrl;

        [Tooltip("Optional - If you want to use a custom paymaster, you can provide the URL here")]
        public string paymasterUrl;

        [Tooltip("Optional - If you want to use a custom entry point, you can provide the contract address here")]
        public string entryPointAddress;

        [Tooltip("Instantiates the WalletConnect SDK for Native platforms.")]
        public GameObject WalletConnectPrefab;

        [Tooltip("Instantiates the Metamask SDK for Native platforms.")]
        public GameObject MetamaskPrefab;

        [Tooltip("Instantiates the EmbeddedWallet SDK for Native platforms.")]
        public GameObject EmbeddedWalletPrefab;

        public ThirdwebSDK SDK;

        public static ThirdwebManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                ThirdwebDebug.LogWarning("Two ThirdwebManager instances were found, removing this one.");
                Destroy(this.gameObject);
                return;
            }

            if (initializeOnAwake)
                Initialize(activeChain);
        }

        public void Initialize(string chainIdentifier)
        {
            // Pass supported chains with replaced RPCs

            var options = new ThirdwebSDK.Options();

            // Set up Client ID and Bundle ID

            options.clientId = string.IsNullOrEmpty(clientId) ? null : clientId;
            options.bundleId = string.IsNullOrEmpty(bundleIdOverride) ? Application.identifier.ToLower() : bundleIdOverride;

            // Set up supported chains

            activeChain = chainIdentifier;
            string activeChainId = null;
            string activeChainRpc = null;

            var supportedChainData = new List<ThirdwebChainData>();
            foreach (var chainData in this.supportedChains)
            {
                if (string.IsNullOrEmpty(chainData.identifier))
                    throw new UnityException($"You must provide a valid chain identifier! See https://thirdweb.com/dashboard/rpc for a list of supported chains.");

                if (string.IsNullOrEmpty(chainData.chainId) || !BigInteger.TryParse(chainData.chainId, out _))
                    throw new UnityException($"Could not add {chainData.identifier} to supported chains, you must provide a valid chain ID!");

                if (!string.IsNullOrEmpty(chainData.rpcOverride) && !chainData.rpcOverride.StartsWith("https://"))
                    throw new UnityException($"Could not add {chainData.identifier} to supported chains, RPC overrides must start with https:// or be left empty to use thirdweb RPCs!");

                string rpc = string.IsNullOrEmpty(chainData.rpcOverride)
                    ? (string.IsNullOrEmpty(clientId) ? $"https://{chainData.chainId}.rpc.thirdweb.com/" : $"https://{chainData.chainId}.rpc.thirdweb.com/{clientId}")
                    : chainData.rpcOverride;

                if (new System.Uri(rpc).Host.EndsWith(".thirdweb.com"))
                    rpc = rpc.AppendBundleIdQueryParam();

                if (chainData.identifier == activeChain)
                {
                    activeChainId = chainData.chainId;
                    activeChainRpc = rpc;
                }

                try
                {
                    supportedChainData.Add(ThirdwebSession.FetchChainData(BigInteger.Parse(chainData.chainId), rpc));
                }
                catch (System.Exception e)
                {
                    ThirdwebDebug.LogWarning($"Failed to fetch chain data for {chainData.identifier} ({chainData.chainId}) - {e}, skipping...");
                    continue;
                }
            }

            options.supportedChains = supportedChainData.ToArray();

            // Set up storage and gasless options (if any)

            if (!string.IsNullOrEmpty(storageIpfsGatewayUrl))
            {
                options.storage = new ThirdwebSDK.StorageOptions() { ipfsGatewayUrl = storageIpfsGatewayUrl };
            }
            if (!string.IsNullOrEmpty(relayerUrl) && !string.IsNullOrEmpty(forwarderAddress))
            {
                options.gasless = new ThirdwebSDK.GaslessOptions()
                {
                    openzeppelin = new ThirdwebSDK.OZDefenderOptions()
                    {
                        relayerUrl = this.relayerUrl,
                        relayerForwarderAddress = this.forwarderAddress,
                        domainName = string.IsNullOrEmpty(this.forwarderDomainOverride) ? "GSNv2 Forwarder" : this.forwarderDomainOverride,
                        domainVersion = string.IsNullOrEmpty(this.forwaderVersionOverride) ? "0.0.1" : this.forwaderVersionOverride
                    }
                };
            }

            // Set up wallet data

            thirdwebConfig = Resources.Load<ThirdwebConfig>("ThirdwebConfig");

            options.wallet = new ThirdwebSDK.WalletOptions()
            {
                appName = string.IsNullOrEmpty(appName) ? "thirdweb powered dApp" : appName,
                appDescription = string.IsNullOrEmpty(appDescription) ? "thirdweb powered dApp" : appDescription,
                appIcons = (appIcons == null || appIcons.Length == 0 || string.IsNullOrEmpty(appIcons[0])) ? new string[] { "https://thirdweb.com/favicon.ico" } : appIcons,
                appUrl = string.IsNullOrEmpty(appUrl) ? "https://thirdweb.com" : appUrl,
                walletConnectProjectId = string.IsNullOrEmpty(walletConnectProjectId) ? "145769e410f16970a79ff77b2d89a1e0" : walletConnectProjectId,
                walletConnectExplorerRecommendedWalletIds =
                    (walletConnectExplorerRecommendedWalletIds == null || walletConnectExplorerRecommendedWalletIds.Length == 0 || string.IsNullOrEmpty(walletConnectExplorerRecommendedWalletIds[0]))
                        ? new string[]
                        {
                            "c57ca95b47569778a828d19178114f4db188b89b763c899ba0be274e97267d96", // metamask
                            "4622a2b2d6af1c9844944291e5e7351a6aa24cd7b23099efac1b2fd875da31a0", // trustwallet
                            "225affb176778569276e484e1b92637ad061b01e13a048b35a9d280c3b58970f", // safe
                            "1ae92b26df02f0abca6304df07debccd18262fdf5fe82daa81593582dac9a369", // rainbow
                            "a797aa35c0fadbfc1a53e7f675162ed5226968b44a19ee3d24385c64d1d3c393", // phantom
                            "c03dfee351b6fcc421b4494ea33b9d4b92a984f87aa76d1663bb28705e95034a", // uniswap
                            "ecc4036f814562b41a5268adc86270fba1365471402006302e70169465b7ac18", // zerion
                            "ef333840daf915aafdc4a004525502d6d49d77bd9c65e0642dbaefb3c2893bef", // imtoken
                            "bc949c5d968ae81310268bf9193f9c9fb7bb4e1283e1284af8f2bd4992535fd6", // argent
                            "74f8092562bd79675e276d8b2062a83601a4106d30202f2d509195e30e19673d", // spot
                            "afbd95522f4041c71dd4f1a065f971fd32372865b416f95a0b1db759ae33f2a7", // omni
                            "f2436c67184f158d1beda5df53298ee84abfc367581e4505134b5bcf5f46697d", // crypto.com
                            "20459438007b75f4f4acb98bf29aa3b800550309646d375da5fd4aac6c2a2c66", // tokenpocket
                            "8837dd9413b1d9b585ee937d27a816590248386d9dbf59f5cd3422dbbb65683e", // robinhood wallet
                            "85db431492aa2e8672e93f4ea7acf10c88b97b867b0d373107af63dc4880f041", // frontier
                            "84b43e8ddfcd18e5fcb5d21e7277733f9cccef76f7d92c836d0e481db0c70c04", // blockchain.com
                            "0b415a746fb9ee99cce155c2ceca0c6f6061b1dbca2d722b3ba16381d0562150", // safepal
                            "38f5d18bd8522c244bdd70cb4a68e0e718865155811c043f052fb9f1c51de662", // bitkeep
                            "9414d5a85c8f4eabc1b5b15ebe0cd399e1a2a9d35643ab0ad22a6e4a32f596f0", // zengo
                            "c286eebc742a537cd1d6818363e9dc53b21759a1e8e5d9b263d0c03ec7703576", // 1inch
                            "8a0ee50d1f22f6651afcae7eb4253e52a3310b90af5daef78a8c4929a9bb99d4", // binance defi wallet
                            "e9ff15be73584489ca4a66f64d32c4537711797e30b6660dbcb71ea72a42b1f4", // exodus
                            "19177a98252e07ddfc9af2083ba8e07ef627cb6103467ffebb3f8f4205fd7927", // ledger live
                            "f5b4eeb6015d66be3f5940a895cbaa49ef3439e518cd771270e6b553b48f31d2", // mew wallet
                            "138f51c8d00ac7b9ac9d8dc75344d096a7dfe370a568aa167eabc0a21830ed98", // alpha wallet
                            "47bb07617af518642f3413a201ec5859faa63acb1dd175ca95085d35d38afb83", // keyring pro
                            "76a3d548a08cf402f5c7d021f24fd2881d767084b387a5325df88bc3d4b6f21b", // lobstr wallet
                            "dceb063851b1833cbb209e3717a0a0b06bf3fb500fe9db8cd3a553e4b1d02137", // onto
                            "7674bb4e353bf52886768a3ddc2a4562ce2f4191c80831291218ebd90f5f5e26", // math wallet
                            "8308656f4548bb81b3508afe355cfbb7f0cb6253d1cc7f998080601f838ecee3", // unstoppable domains
                            "031f0187049b7f96c6f039d1c9c8138ff7a17fd75d38b34350c7182232cc29aa", // obvious
                            "5864e2ced7c293ed18ac35e0db085c09ed567d67346ccb6f58a0327a75137489", // fireblocks
                            "2c81da3add65899baeac53758a07e652eea46dbb5195b8074772c62a77bbf568", // ambire wallet
                            "802a2041afdaf4c7e41a2903e98df333c8835897532699ad370f829390c6900f", // infinity wallet
                            "7424d97904535b14fe34f09f63d8ca66935546f798758dabd5b26c2309f2b1f9", // bridge wallet
                            "dd43441a6368ec9046540c46c5fdc58f79926d17ce61a176444568ca7c970dcd", // internet money wallet
                            "c482dfe368d4f004479977fd88e80dc9e81107f3245d706811581a6dfe69c534", // now wallet
                            "107bb20463699c4e614d3a2fb7b961e66f48774cb8f6d6c1aee789853280972c", // bitcoin.com wallet
                            "053ac0ac602e0969736941cf5aa07a3af57396d4601cb521a173a626e1015fb1", // au wallet
                            "2a3c89040ac3b723a1972a33a125b1db11e258a6975d3a61252cd64e6ea5ea01", // coin98 super app
                            "b956da9052132e3dabdcd78feb596d5194c99b7345d8c4bd7a47cabdcb69a25f", // abc wallet
                        }
                        : walletConnectExplorerRecommendedWalletIds,
                customScheme = string.IsNullOrEmpty(thirdwebConfig.customScheme) ? null : thirdwebConfig.customScheme,
            };

            options.smartWalletConfig = string.IsNullOrEmpty(factoryAddress)
                ? null
                : new ThirdwebSDK.SmartWalletConfig()
                {
                    factoryAddress = factoryAddress,
                    gasless = gasless,
                    deployOnSign = deployOnSign,
                    bundlerUrl = string.IsNullOrEmpty(bundlerUrl) ? $"https://{activeChainId}.bundler.thirdweb.com" : bundlerUrl,
                    paymasterUrl = string.IsNullOrEmpty(paymasterUrl) ? $"https://{activeChainId}.bundler.thirdweb.com" : paymasterUrl,
                    entryPointAddress = string.IsNullOrEmpty(entryPointAddress) ? Thirdweb.AccountAbstraction.Constants.DEFAULT_ENTRYPOINT_ADDRESS : entryPointAddress,
                };

            // Pass active chain rpc and chainId

            SDK = new ThirdwebSDK(activeChainRpc, BigInteger.Parse(activeChainId), options);
        }
    }
}
