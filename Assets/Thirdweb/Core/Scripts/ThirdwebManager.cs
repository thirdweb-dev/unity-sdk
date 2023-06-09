using UnityEngine;
using Thirdweb;
using System.Collections.Generic;

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
    [Header("REQUIRED SETTINGS")]
    [Tooltip("The chain to initialize the SDK with")]
    public string chain = "goerli";

    [Header("CHAIN DATA")]
    [Tooltip("Support any chain by adding it to this list from the inspector")]
    public List<ChainData> supportedChains = new List<ChainData>()
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
    };

    [Header("APP METADATA")]
    public string appName = "Thirdweb Game";
    public string appDescription = "Thirdweb Game Demo";
    public string[] appIcons = new string[] { "https://thirdweb.com/favicon.ico" };
    public string appUrl = "https://thirdweb.com";

    [Header("STORAGE OPTIONS")]
    [Tooltip("IPFS Gateway Override")]
    public string storageIpfsGatewayUrl = "https://gateway.ipfscdn.io/ipfs/";

    [Header("OZ DEFENDER OPTIONS")]
    [Tooltip("Autotask URL")]
    public string relayerUrl = null;

    [Tooltip("Forwarders can be found here https://github.com/thirdweb-dev/ozdefender-autotask")]
    public string forwarderAddress = null;

    [Tooltip("Forwarder Domain Override (Defaults to GSNv2 Forwarder if left empty)")]
    public string forwarderDomainOverride = null;

    [Tooltip("Forwarder Version (Defaults to 0.0.1 if left empty)")]
    public string forwaderVersionOverride = null;

    [Header("MAGIC LINK OPTIONS")]
    [Tooltip("Magic Link API Key (https://dashboard.magic.link)")]
    public string magicLinkApiKey = null;

    [Header("SMART WALLET OPTIONS")]
    [Tooltip("Factory Contract Address")]
    public string factoryAddress;

    [Tooltip("Thirdweb API Key (https://thirdweb.com/dashboard/api-keys)")]
    public string thirdwebApiKey;

    [Tooltip("Whether it should use a paymaster for gasless transactions or not")]
    public bool gasless;

    [Tooltip("Optional - If you want to use a custom relayer, you can provide the URL here")]
    public string bundlerUrl;

    [Tooltip("Optional - If you want to use a custom paymaster, you can provide the URL here")]
    public string paymasterUrl;

    [Tooltip("Optional - If you want to use a custom entry point, you can provide the contract address here")]
    public string entryPointAddress;

    [Header("NATIVE PREFABS (DANGER ZONE)")]
    [Tooltip("Instantiates the WalletConnect SDK for Native platforms.")]
    public GameObject WalletConnectPrefab;

    [Tooltip("Instantiates the Metamask SDK for Native platforms.")]
    public GameObject MetamaskPrefab;

    public ThirdwebSDK SDK;

    public static ThirdwebManager Instance { get; private set; }

    private void Awake()
    {
        // Single persistent instance at all times.

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Debug.LogWarning("Two ThirdwebManager instances were found, removing this one.");
            Destroy(this.gameObject);
            return;
        }

        // Inspector chain data dictionary.

        ChainData currentChain = GetChainData(chain);

        // Chain ID must be provided on native platforms.

        int chainId = -1;

        if (!Utils.IsWebGLBuild())
        {
            if (string.IsNullOrEmpty(currentChain.chainId))
                throw new UnityException("You must provide a Chain ID on native platforms!");

            if (!int.TryParse(currentChain.chainId, out chainId))
                throw new UnityException("The Chain ID must be a non-negative integer!");
        }

        // Must provide a proper chain identifier (https://thirdweb.com/dashboard/rpc) or RPC override.

        string chainOrRPC = null;

        if (!string.IsNullOrEmpty(currentChain.rpcOverride))
        {
            if (!currentChain.rpcOverride.StartsWith("https://"))
                throw new UnityException("RPC overrides must start with https:// !");
            else
                chainOrRPC = currentChain.rpcOverride;
        }
        else
        {
            if (string.IsNullOrEmpty(currentChain.identifier))
                throw new UnityException("When not providing an RPC, you must provide a chain identifier!");
            else
                chainOrRPC = currentChain.identifier;
        }

        // Set up storage and gasless options (if an)

        var options = new ThirdwebSDK.Options();

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
        options.wallet = new ThirdwebSDK.WalletOptions()
        {
            appName = string.IsNullOrEmpty(appName) ? "Thirdweb Game" : appName,
            appDescription = string.IsNullOrEmpty(appDescription) ? "Thirdweb Game Demo" : appDescription,
            appIcons = appIcons.Length == 0 ? new string[] { "https://thirdweb.com/favicon.ico" } : appIcons,
            appUrl = string.IsNullOrEmpty(appUrl) ? "https://thirdweb.com" : appUrl,
            magicLinkApiKey = string.IsNullOrEmpty(magicLinkApiKey) ? null : magicLinkApiKey,
        };

        options.smartWalletConfig =
            string.IsNullOrEmpty(factoryAddress) || string.IsNullOrEmpty(thirdwebApiKey)
                ? null
                : new ThirdwebSDK.SmartWalletConfig()
                {
                    factoryAddress = factoryAddress,
                    thirdwebApiKey = thirdwebApiKey,
                    gasless = gasless,
                    bundlerUrl = bundlerUrl,
                    paymasterUrl = paymasterUrl,
                    entryPointAddress = entryPointAddress
                };

        SDK = new ThirdwebSDK(chainOrRPC, chainId, options);
    }

    public ChainData GetChainData(string chainIdentifier)
    {
        return supportedChains.Find(x => x.identifier == chainIdentifier);
    }

    public ChainData GetCurrentChainData()
    {
        return supportedChains.Find(x => x.identifier == chain);
    }

    public int GetCurrentChainID()
    {
        return int.Parse(GetCurrentChainData().chainId);
    }

    public string GetCurrentChainIdentifier()
    {
        return chain;
    }
}
