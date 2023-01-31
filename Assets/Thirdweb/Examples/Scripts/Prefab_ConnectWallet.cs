using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using System;
using TMPro;

// Can be added to SDK
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
    public string chain = "ethereum";
    public int chainID = 1;
    public List<Wallet> supportedWallets = new List<Wallet> { Wallet.MetaMask, Wallet.CoinbaseWallet, Wallet.Injected, Wallet.MagicAuth };

    [Header("UI - CONNECTING")]
    public GameObject connectButton;
    public GameObject connectDropdown;
    public List<WalletButton> walletButtons;

    [Header("UI - CONNECTED")]
    public GameObject connectedButton;
    public GameObject connectedDropdown;
    public TMP_Text connectInfoText;
    public TMP_Text walletAddressText;

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

        connectButton.SetActive(true);
        connectedButton.SetActive(false);

        connectDropdown.SetActive(false);
        connectedDropdown.SetActive(false);
    }

    // Connecting

    public async void OnConnect(Wallet _wallet)
    {
        try
        {
            address = await SDK.wallet.Connect(
               new WalletConnection()
               {
                   provider = GetWalletProvider(_wallet),
                   chainId = chainID
               });

            connectInfoText.text = $"{_wallet} ({chain})";
            walletAddressText.text = $"Connected As: {address.ShortenAddress()}";

            connectButton.SetActive(false);
            connectedButton.SetActive(true);

            connectDropdown.SetActive(false);
            connectedDropdown.SetActive(false);

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

            connectButton.SetActive(true);
            connectedButton.SetActive(false);

            connectDropdown.SetActive(false);
            connectedDropdown.SetActive(false);

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
        if (String.IsNullOrEmpty(address))
            connectDropdown.SetActive(!connectDropdown.activeInHierarchy);
        else
            connectedDropdown.SetActive(!connectedDropdown.activeInHierarchy);
    }

    // Utility

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
