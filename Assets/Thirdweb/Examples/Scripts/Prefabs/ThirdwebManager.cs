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
    public List<ChainData> supportedChainData = new List<ChainData>()
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

    [Header("STORAGE OPTIONS")]
    [Tooltip("IPFS Gateway Override")]
    public string storageIpfsGatewayUrl = "https://gateway.ipfscdn.io/ipfs/";

    [Header("OZ DEFENDER OPTIONS")]
    [Tooltip("Gasless Transaction Support")]
    public string relayerUrl = null;
    public string relayerForwarderAddress = null;

    public ThirdwebSDK SDK;

    public static ThirdwebManager Instance;

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
        if (!string.IsNullOrEmpty(relayerUrl) && !string.IsNullOrEmpty(relayerForwarderAddress))
        {
            options.gasless = new ThirdwebSDK.GaslessOptions()
            {
                openzeppelin = new ThirdwebSDK.OZDefenderOptions() { relayerUrl = this.relayerUrl, relayerForwarderAddress = this.relayerForwarderAddress, }
            };
        }

        SDK = new ThirdwebSDK(chainOrRPC, chainId, options);
    }

    public ChainData GetChainData(string chainIdentifier)
    {
        return supportedChainData.Find(x => x.identifier == chainIdentifier);
    }

    public ChainData GetCurrentChainData()
    {
        return supportedChainData.Find(x => x.identifier == chain);
    }

    public string GetCurrentChainIdentifier()
    {
        return chain;
    }
}
