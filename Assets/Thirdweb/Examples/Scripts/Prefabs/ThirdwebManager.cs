using UnityEngine;
using Thirdweb;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public enum Chain
{
    Ethereum,
    Goerli,
    Polygon,
    Mumbai,
    Fantom,
    FantomTestnet,
    Avalanche,
    AvalancheTestnet,
    Optimism,
    OptimismGoerli,
    Arbitrum,
    ArbitrumGoerli,
    Binance,
    BinanceTestnet
}

[System.Serializable]
public class ChainData
{
    public string identifier;
    public string chainId;
    public string rpcOverride;
}

[System.Serializable]
public class SupportedChainData : SerializableDictionaryBase<Chain, ChainData> { }

public class ThirdwebManager : MonoBehaviour
{
    [Header("REQUIRED SETTINGS")]
    [Tooltip("The chain to initialize the SDK with")]
    public Chain chain = Chain.Goerli;

    [Header("CHAIN DATA")]
    [Tooltip("Support any chain added to the Chain enum")]
    public SupportedChainData supportedChainData;

    [Header("STORAGE OPTIONS")]
    [Tooltip("IPFS Gateway Override")]
    public string storageIpfsGatewayUrl = null;

    [Header("OZ DEFENDER OPTIONS")]
    [Tooltip("Gasless Transaction Support")]
    public string relayerUrl = null;
    public string relayerForwarderAddress = null;

    public ThirdwebSDK SDK;

    public static ThirdwebManager Instance;

    private void Awake()
    {
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

        ChainData currentChain = supportedChainData[chain];

        if (!Utils.IsWebGLBuild() && string.IsNullOrEmpty(currentChain.chainId))
        {
            throw new UnityException("You must provide a Chain ID on native platforms!");
        }

        if (string.IsNullOrEmpty(currentChain.rpcOverride))
        {
            if (string.IsNullOrEmpty(currentChain.identifier))
                throw new UnityException("When not providing an RPC, you must provide a chain identifier!");
        }
        else
        {
            if (!currentChain.rpcOverride.StartsWith("https://"))
                throw new UnityException("RPC overrides must start with https:// !");
        }

        int chainId = int.Parse(currentChain.chainId);

        ThirdwebSDK.Options options = new ThirdwebSDK.Options();
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

        SDK = new ThirdwebSDK(currentChain.identifier, chainId, options);
    }
}
