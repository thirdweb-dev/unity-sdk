using UnityEngine;
using Thirdweb;
using System.Collections.Generic;
using System.Numerics;

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
    public string chain = "goerli";

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

    [Tooltip("Thirdweb API Key (https://thirdweb.com/dashboard/). Used for Storage and Account Abstraction.")]
    public string clientId;

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

    [Tooltip("Magic Link API Key (https://dashboard.magic.link)")]
    public string magicLinkApiKey = null;

    [Tooltip("WalletConnect Project ID (https://cloud.walletconnect.com/app)")]
    public string walletConnectProjectId = null;

    [Tooltip("Paper Client ID (https://withpaper.com/dashboard)")]
    public string paperClientId = null;

    [Tooltip("Factory Contract Address")]
    public string factoryAddress;

    [Tooltip("Whether it should use a paymaster for gasless transactions or not")]
    public bool gasless;

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

    [Tooltip("Instantiates the Paper SDK for Native platforms.")]
    public GameObject PaperPrefab;

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

        ChainData currentChain = supportedChains.Find(x => x.identifier == chain);

        // Chain ID must be provided on native platforms.

        BigInteger chainId = -1;

        if (string.IsNullOrEmpty(currentChain.chainId))
            throw new UnityException("You must provide a Chain ID on native platforms!");

        if (!BigInteger.TryParse(currentChain.chainId, out chainId))
            throw new UnityException("The Chain ID must be a non-negative integer!");

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

        // Set up storage and gasless options (if any)

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
            appName = string.IsNullOrEmpty(appName) ? "thirdweb powered dApp" : appName,
            appDescription = string.IsNullOrEmpty(appDescription) ? "thirdweb powered dApp" : appDescription,
            appIcons = string.IsNullOrEmpty(appIcons[0]) ? new string[] { "https://thirdweb.com/favicon.ico" } : appIcons,
            appUrl = string.IsNullOrEmpty(appUrl) ? "https://thirdweb.com" : appUrl,
            magicLinkApiKey = string.IsNullOrEmpty(magicLinkApiKey) ? null : magicLinkApiKey,
            walletConnectProjectId = string.IsNullOrEmpty(walletConnectProjectId) ? "145769e410f16970a79ff77b2d89a1e0" : walletConnectProjectId,
            paperClientId = string.IsNullOrEmpty(paperClientId) ? null : paperClientId,
        };

        options.smartWalletConfig = string.IsNullOrEmpty(factoryAddress)
            ? null
            : new ThirdwebSDK.SmartWalletConfig()
            {
                factoryAddress = factoryAddress,
                gasless = gasless,
                bundlerUrl = string.IsNullOrEmpty(bundlerUrl) ? $"https://{currentChain.identifier}.bundler.thirdweb.com" : bundlerUrl,
                paymasterUrl = string.IsNullOrEmpty(paymasterUrl) ? $"https://{currentChain.identifier}.bundler.thirdweb.com" : paymasterUrl,
                entryPointAddress = string.IsNullOrEmpty(entryPointAddress) ? Thirdweb.AccountAbstraction.Constants.DEFAULT_ENTRYPOINT_ADDRESS : entryPointAddress,
            };

        options.clientId = string.IsNullOrEmpty(clientId) ? null : clientId;

        var supportedChainData = new List<ThirdwebChainData>();
        foreach (var chain in this.supportedChains)
        {
            string rpc = string.IsNullOrEmpty(chain.rpcOverride)
                ? (
                    string.IsNullOrEmpty(clientId)
                        ? $"https://{chain.identifier}.rpc.thirdweb.com/339d65590ba0fa79e4c8be0af33d64eda709e13652acb02c6be63f5a1fbef9c3"
                        : $"https://{chain.identifier}.rpc.thirdweb.com/${clientId}"
                )
                : chain.rpcOverride;

            if (rpc.Contains("thirdweb.com"))
                rpc = rpc.AppendBundleIdQueryParam();
            try
            {
                supportedChainData.Add(ThirdwebSession.FetchChainData(BigInteger.Parse(chain.chainId), rpc));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to fetch chain data for {chain.identifier} ({chain.chainId}) - {e}, skipping...");
                continue;
            }
        }

        options.supportedChains = supportedChainData.ToArray();

        SDK = new ThirdwebSDK(chainOrRPC, chainId, options);
    }
}
