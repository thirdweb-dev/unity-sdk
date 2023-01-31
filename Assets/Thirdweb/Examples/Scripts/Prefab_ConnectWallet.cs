using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using System;
using TMPro;

// Can be added to SDK
public enum Chain
{
    Ethereum,
    Goerli,
    Polygon,
    Mumbai,
    Arbitrum,
    ArbitrumGoerli,
    Avalanche,
    AvalancheFujiTestnet,
    BinanceSmartChainMainnet,
    BinanceSmartChainTestnet,
    Fantom,
    FantomTestnet,
    Optimism,
    OptimismGoerli,
}

// Can be added to SDK
[Serializable]
public enum Wallet
{
    MetaMask,
    CoinbaseWallet,
    WalletConnect,
    Injected,
    MagicAuth,
}

// Can be replaced with Serializeable Dictionary (asset)
[Serializable]
public struct WalletButton
{
    public Wallet wallet;
    public GameObject walletButton;
}

public class Prefab_ConnectWallet : MonoBehaviour
{
    [Header("SETTINGS")]
    public Chain chain;
    public List<Wallet> supportedWallets;

    [Header("UI - CONNECTING")]
    public GameObject connectButton;
    public Transform connectDropdown;
    public List<WalletButton> walletButtons;

    [Header("UI - CONNECTED")]
    public GameObject connectedButton;
    public Transform connectedDropdown;
    public TMP_Text connectInfoText;
    public TMP_Text walletAddressText;

    bool dropdownExpand;
    Coroutine dropdownRoutine;
    string address;

    ThirdwebSDK SDK;

    // SDK Initialization

    private void Awake()
    {
#if !UNITY_EDITOR
        SDK = new ThirdwebSDK(chain.ToString().ToLower());
#endif
    }

    // UI Initialization

    private void Start()
    {
        address = null;

        foreach (WalletButton wb in walletButtons)
            wb.walletButton.SetActive(supportedWallets.Contains(wb.wallet));

        connectDropdown.localScale = Vector3.zero;
        connectDropdown.gameObject.SetActive(false);

        connectedDropdown.localScale = Vector3.zero;
        connectedDropdown.gameObject.SetActive(false);

        connectButton.SetActive(true);
        connectedButton.SetActive(false);

        dropdownExpand = false;
    }

    // Connecting

    public async void OnConnect(Wallet _wallet)
    {
        WalletProvider _provider = GetWalletProvider(_wallet);
        int _chainID = GetChainID(chain);

        try
        {
            address = await SDK.wallet.Connect(
               new WalletConnection()
               {
                   provider = _provider,
                   chainId = _chainID
               });

            connectInfoText.text = $"{_wallet} ({chain})";
            walletAddressText.text = $"Connected As: {address}";

            connectButton.SetActive(false);
            connectedButton.SetActive(true);

            dropdownExpand = false;

            LogThirdweb($"Connected successfully to: {address}");
        }
        catch (Exception e)
        {
            LogThirdweb($"Error Connecting Wallet: ${e.Message}");
        }
    }

    // Disconnecting

    public async void OnDisconnect()
    {
        try
        {
            await SDK.wallet.Disconnect();
            address = null;

            connectDropdown.localScale = Vector3.zero;
            connectDropdown.gameObject.SetActive(false);

            connectedDropdown.localScale = Vector3.zero;
            connectedDropdown.gameObject.SetActive(false);

            connectButton.SetActive(true);
            connectedButton.SetActive(false);

            dropdownExpand = false;

            LogThirdweb($"Disconnected successfully.");

        }
        catch (Exception e)
        {
            LogThirdweb($"Error Disconnecting Wallet: ${e.Message}");
        }
    }

    // UI

    public void OnTryConnect(string _walletStr)
    {
        Wallet wallet;
        if (Enum.TryParse<Wallet>(_walletStr, out wallet))
            OnConnect(wallet);
        else
            LogThirdweb($"Did not find wallet: {_walletStr}");
    }

    public void OnClickDropdown()
    {
        dropdownExpand = !dropdownExpand;

        if (dropdownRoutine != null)
            StopCoroutine(dropdownRoutine);

        dropdownRoutine = StartCoroutine(DropdownRoutine(String.IsNullOrEmpty(address) ? connectDropdown : connectedDropdown));
    }

    IEnumerator DropdownRoutine(Transform _object)
    {
        _object.gameObject.SetActive(true);
        Vector3 targetScale = dropdownExpand ? Vector3.one : Vector3.zero;
        while (Math.Abs(_object.localScale.x - targetScale.x) > 0.01f)
        {
            _object.localScale = Vector3.Lerp(_object.localScale, targetScale, 10f * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        _object.localScale = targetScale;
        _object.gameObject.SetActive(dropdownExpand);
    }

    // Utility

    int GetChainID(Chain _chain)
    {
        switch (chain)
        {
            case Chain.Ethereum:
                return 1;
            case Chain.Goerli:
                return 5;
            case Chain.Polygon:
                return 137;
            case Chain.Mumbai:
                return 80001;
            case Chain.Arbitrum:
                return 42161;
            case Chain.ArbitrumGoerli:
                return 421613;
            case Chain.Avalanche:
                return 43114;
            case Chain.AvalancheFujiTestnet:
                return 43113;
            case Chain.BinanceSmartChainMainnet:
                return 56;
            case Chain.BinanceSmartChainTestnet:
                return 97;
            case Chain.Fantom:
                return 250;
            case Chain.FantomTestnet:
                return 4002;
            case Chain.Optimism:
                return 10;
            case Chain.OptimismGoerli:
                return 420;
            default:
                throw new UnityException($"Chain ID for chain {_chain} unimplemented!");
        }
    }

    WalletProvider GetWalletProvider(Wallet _wallet)
    {
        switch (_wallet)
        {
            case Wallet.MetaMask:
                return WalletProvider.MetaMask;
            case Wallet.CoinbaseWallet:
                return WalletProvider.CoinbaseWallet;
            case Wallet.WalletConnect:
                return WalletProvider.WalletConnect;
            case Wallet.Injected:
                return WalletProvider.Injected;
            case Wallet.MagicAuth:
                return WalletProvider.MagicAuth;
            default:
                throw new UnityException($"Wallet Provider for wallet {_wallet} unimplemented!");
        }
    }

    void LogThirdweb(string _message)
    {
        Debug.Log($"[Thirdweb] {_message}");
    }
}
