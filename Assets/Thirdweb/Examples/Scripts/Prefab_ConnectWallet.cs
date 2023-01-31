using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using System;
using TMPro;
using UnityEngine.UI;

public enum Wallet
{
    MetaMask,
    CoinbaseWallet,
    WalletConnect,
    MagicAuth,
}

[Serializable]
public struct WalletButton
{
    public Wallet wallet;
    public GameObject walletButton;
    public Sprite icon;
}

public class Prefab_ConnectWallet : MonoBehaviour
{
    [Header("SETTINGS")]
    public string chain = "goerli";
    public List<Wallet> supportedWallets = new List<Wallet> { Wallet.MetaMask, Wallet.CoinbaseWallet, Wallet.WalletConnect };

    [Header("UI - CONNECTING (DO NOT EDIT)")]
    public GameObject connectButton;
    public GameObject connectDropdown;
    public List<WalletButton> walletButtons;

    [Header("UI - CONNECTED (DO NOT EDIT)")]
    public GameObject connectedButton;
    public GameObject connectedDropdown;
    public TMP_Text connectInfoText;
    public TMP_Text walletAddressText;
    public Image dropdownIcon;

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

        if (supportedWallets.Count == 1)
            connectButton.GetComponent<Button>().onClick.AddListener(() => OnConnect(supportedWallets[0]));
        else
            connectButton.GetComponent<Button>().onClick.AddListener(() => OnClickDropdown());


        foreach (WalletButton wb in walletButtons)
        {
            if (supportedWallets.Contains(wb.wallet))
            {
                wb.walletButton.SetActive(true);
                wb.walletButton.GetComponent<Button>().onClick.AddListener(() => OnConnect(wb.wallet));
            }
            else
            {
                wb.walletButton.SetActive(false);
            }
        }

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
                   chainId = GetChainID(chain),
               });

            connectInfoText.text = chain;
            walletAddressText.text = address.ShortenAddress();

            connectButton.SetActive(false);
            connectedButton.SetActive(true);

            connectDropdown.SetActive(false);
            connectedDropdown.SetActive(false);

            dropdownIcon.sprite = walletButtons.Find(x => x.wallet == _wallet).icon;

            LogThirdweb($"Connected successfully to: {address}");
        }
        catch (Exception e)
        {
            LogThirdweb($"Error Connecting Wallet: {e.Message}");
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
            LogThirdweb($"Error Disconnecting Wallet: {e.Message}");
        }
    }

    // UI

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
            case Wallet.MagicAuth:
                return WalletProvider.MagicAuth;
            default:
                throw new UnityException($"Wallet Provider for wallet {_wallet} unimplemented!");
        }
    }

    int GetChainID(string _chain)
    {
        switch (_chain)
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
                throw new UnityException($"Chain ID for chain {_chain} unimplemented!");
        }
    }

    void LogThirdweb(string _message)
    {
        Debug.Log($"[Thirdweb] {_message}");
    }
}
