using UnityEngine;
using Thirdweb;

public class ThirdwebManager : MonoBehaviour
{
    [Header("SETTINGS")]
    public string chain = "goerli";

    public ThirdwebSDK SDK;

    public static ThirdwebManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

#if !UNITY_EDITOR
        SDK = new ThirdwebSDK(chain.ToLower());
#endif
    }

    public int GetChainID()
    {
        switch (chain)
        {
            case "mainnet":
            case "ethereum":
                return 1;
            case "goerli":
                return 5;
            case "polygon":
            case "matic":
                return 137;
            case "mumbai":
                return 80001;
            case "fantom":
                return 250;
            case "fantom-testnet":
                return 4002;
            case "avalanche":
                return 43114;
            case "avalanche-testnet":
            case "avalanche-fuji":
                return 43113;
            case "optimism":
                return 10;
            case "optimism-goerli":
                return 420;
            case "arbitrum":
                return 42161;
            case "arbitrum-goerli":
                return 421613;
            case "binance":
                return 56;
            case "binance-testnet":
                return 97;
            default:
                throw new UnityException($"Chain ID for chain {chain} unimplemented!");
        }
    }
}
