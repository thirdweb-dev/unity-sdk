using UnityEngine;
using Thirdweb;
using System.Collections.Generic;

[System.Serializable]
public enum Chain
{
    Ethereum = 1,
    Goerli = 5,
    Polygon = 137,
    Mumbai = 80001,
    Fantom = 250,
    FantomTestnet = 4002,
    Avalanche = 43114,
    AvalancheTestnet = 43113,
    Optimism = 10,
    OptimismGoerli = 420,
    Arbitrum = 42161,
    ArbitrumGoerli = 421613,
    Binance = 56,
    BinanceTestnet = 97
}

public class ThirdwebManager : MonoBehaviour
{
    [Header("REQUIRED SETTINGS")]
    [Tooltip("The chain to initialize the SDK with")]
    public Chain chain = Chain.Goerli;

    [Header("OPTIONAL SETTINGS")]
    [Tooltip("Supported by all platforms")]
    public string rpcOverride = "";
    [Tooltip("Supported by native platforms")]
    public int chainIdOverride = -1;

    private string API_KEY = "339d65590ba0fa79e4c8be0af33d64eda709e13652acb02c6be63f5a1fbef9c3";

    public Dictionary<Chain, string> chainIdentifiers = new Dictionary<Chain, string>
    {
        {Chain.Ethereum, "ethereum"},
        {Chain.Goerli, "goerli"},
        {Chain.Polygon, "polygon"},
        {Chain.Mumbai, "mumbai"},
        {Chain.Fantom, "fantom"},
        {Chain.FantomTestnet, "fantom-testnet"},
        {Chain.Avalanche, "avalanche"},
        {Chain.AvalancheTestnet, "avalanche-testnet"},
        {Chain.Optimism, "optimism"},
        {Chain.OptimismGoerli, "optimism-goerli"},
        {Chain.Arbitrum, "arbitrum"},
        {Chain.ArbitrumGoerli, "arbitrum-goerli"},
        {Chain.Binance, "binance"},
        {Chain.BinanceTestnet, "binance-testnet"},
    };

    public ThirdwebSDK SDK;

    public static ThirdwebManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);


        if (!Utils.IsWebGLBuild() && rpcOverride.StartsWith("https://") && chainIdOverride == -1)
        {
            throw new UnityException("To use custom RPC overrides on native platforms, please provide the corresponding Chain ID Override!");
        }
        else
        {
            string rpc = rpcOverride.StartsWith("https://") ? rpcOverride : $"https://{chainIdentifiers[chain]}.rpc.thirdweb.com/{API_KEY}";
            int chainId = chainIdOverride == -1 ? (int)chain : chainIdOverride;
            SDK = new ThirdwebSDK(rpc, chainId);
        }

    }
}
