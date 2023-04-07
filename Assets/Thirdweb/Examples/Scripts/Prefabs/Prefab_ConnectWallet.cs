using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public enum Wallet
{
    MetaMask,
    Injected,
    CoinbaseWallet,
    WalletConnect,
    MagicAuth,
    DeviceWallet
}

[Serializable]
public struct WalletButton
{
    public Wallet wallet;
    public GameObject walletButton;
    public Sprite icon;
}

[Serializable]
public struct NetworkSprite
{
    public string chain;
    public Sprite sprite;
}

public class Prefab_ConnectWallet : MonoBehaviour
{
    [Header("SETTINGS")]
    public List<Wallet> supportedWallets = new List<Wallet>() { Wallet.MetaMask, Wallet.Injected, Wallet.CoinbaseWallet, Wallet.WalletConnect, Wallet.MagicAuth, Wallet.DeviceWallet };
    public bool supportSwitchingNetwork = true;
    public List<string> supportedNetworks = new List<string>()
    {
        "ethereum",
        "goerli",
        "polygon",
        "mumbai",
        "fantom",
        "fantom-testnet",
        "avalanche",
        "avalanche-fuji",
        "optimism",
        "optimism-goerli",
        "arbitrum",
        "arbitrum-goerli",
        "binance",
        "binance-testnet",
    };

    [Header("CUSTOM CALLBACKS")]
    public UnityEvent OnConnectedCallback;
    public UnityEvent OnDisconnectedCallback;
    public UnityEvent OnSwitchNetworkCallback;
    public UnityEvent OnFailedConnectCallback;
    public UnityEvent OnFailedDisconnectCallback;
    public UnityEvent OnFailedSwitchNetworkCallback;

    [Header("UI ELEMENTS (DO NOT EDIT)")]
    // Connecting
    public GameObject connectButton;
    public GameObject connectDropdown;
    public List<WalletButton> walletButtons;

    // Connected
    public GameObject connectedButton;
    public GameObject connectedDropdown;
    public TMP_Text balanceText;
    public TMP_Text balanceText2;
    public TMP_Text walletAddressText;
    public TMP_Text walletAddressText2;
    public Image walletImage;
    public Image walletImage2;
    public TMP_Text currentNetworkText;
    public Image currentNetworkImage;
    public Image chainImage;

    // Network Switching
    public GameObject networkSwitchButton;
    public GameObject networkDropdown;
    public GameObject networkButtonPrefab;
    public List<NetworkSprite> networkSprites;

    string address;
    Wallet wallet;

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

        connectedButton.GetComponent<Button>().onClick.AddListener(() => OnClickDropdown());

        connectButton.SetActive(true);
        connectedButton.SetActive(false);

        connectDropdown.SetActive(false);
        connectedDropdown.SetActive(false);

        networkSwitchButton.SetActive(supportSwitchingNetwork);
        networkDropdown.SetActive(false);
    }

    // Connecting

    public async void OnConnect(Wallet _wallet)
    {
        try
        {
            address = await ThirdwebManager.Instance.SDK.wallet.Connect(
                new WalletConnection() { provider = GetWalletProvider(_wallet), chainId = int.Parse(ThirdwebManager.Instance.GetCurrentChainData().chainId), }
            );

            wallet = _wallet;
            OnConnected();
            OnConnectedCallback?.Invoke();
            print($"Connected successfully to: {address}");
        }
        catch (Exception e)
        {
            OnFailedConnectCallback?.Invoke();
            print($"Error Connecting Wallet: {e.Message}");
        }
    }

    async void OnConnected()
    {
        try
        {
            CurrencyValue nativeBalance = await ThirdwebManager.Instance.SDK.wallet.GetBalance();
            balanceText.text = $"{nativeBalance.value.ToEth()} {nativeBalance.symbol}";
            balanceText2.text = balanceText.text;
            walletAddressText.text = await Utils.GetENSName(address) ?? address.ShortenAddress();
            walletAddressText2.text = walletAddressText.text;
        }
        catch (Exception e)
        {
            print($"Error Fetching Native Balance: {e.Message}");
        }
        finally
        {
            string _chain = ThirdwebManager.Instance.chain;
            currentNetworkText.text = ThirdwebManager.Instance.GetCurrentChainIdentifier();
            currentNetworkImage.sprite = networkSprites.Find(x => x.chain == _chain).sprite;
            connectButton.SetActive(false);
            connectedButton.SetActive(true);
            connectDropdown.SetActive(false);
            connectedDropdown.SetActive(false);
            networkDropdown.SetActive(false);
            walletImage.sprite = walletButtons.Find(x => x.wallet == wallet).icon;
            walletImage2.sprite = walletImage.sprite;
            chainImage.sprite = networkSprites.Find(x => x.chain == _chain).sprite;
        }
    }

    // Disconnecting

    public async void OnDisconnect()
    {
        try
        {
            await ThirdwebManager.Instance.SDK.wallet.Disconnect();
            OnDisconnected();
            OnDisconnectedCallback?.Invoke();
            print($"Disconnected successfully.");
        }
        catch (Exception e)
        {
            OnFailedDisconnectCallback?.Invoke();
            print($"Error Disconnecting Wallet: {e.Message}");
        }
    }

    void OnDisconnected()
    {
        address = null;
        connectButton.SetActive(true);
        connectedButton.SetActive(false);
        connectDropdown.SetActive(false);
        connectedDropdown.SetActive(false);
    }

    // Switching Network

    public async void OnSwitchNetwork(string _chain)
    {
        try
        {
            ThirdwebManager.Instance.chain = _chain;
            await ThirdwebManager.Instance.SDK.wallet.SwitchNetwork(int.Parse(ThirdwebManager.Instance.GetCurrentChainData().chainId));
            OnConnected();
            OnSwitchNetworkCallback?.Invoke();
            print($"Switched Network Successfully: {_chain}");
        }
        catch (Exception e)
        {
            OnFailedSwitchNetworkCallback?.Invoke();
            print($"Error Switching Network: {e.Message}");
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

    public void OnClickNetworkSwitch()
    {
        if (networkDropdown.activeInHierarchy)
        {
            networkDropdown.SetActive(false);
            return;
        }

        networkDropdown.SetActive(true);

        foreach (Transform child in networkDropdown.transform)
            Destroy(child.gameObject);

        foreach (ChainData chainData in ThirdwebManager.Instance.supportedChainData)
        {
            if (chainData.identifier == ThirdwebManager.Instance.chain || !supportedNetworks.Contains(chainData.identifier))
                continue;

            GameObject networkButton = Instantiate(networkButtonPrefab, networkDropdown.transform);
            networkButton.GetComponent<Button>().onClick.RemoveAllListeners();
            networkButton.GetComponent<Button>().onClick.AddListener(() => OnSwitchNetwork(chainData.identifier));
            networkButton.transform.Find("Text_Network").GetComponent<TMP_Text>().text = chainData.identifier;
            networkButton.transform.Find("Icon_Network").GetComponent<Image>().sprite = networkSprites.Find(x => x.chain == chainData.identifier).sprite;
        }
    }

    public void OnCopyAddress()
    {
        GUIUtility.systemCopyBuffer = address;
        print($"Copied your address to your clipboard! Address: {address}");
    }

    // Utility

    WalletProvider GetWalletProvider(Wallet _wallet)
    {
        switch (_wallet)
        {
            case Wallet.MetaMask:
                return WalletProvider.MetaMask;
            case Wallet.Injected:
                return WalletProvider.Injected;
            case Wallet.CoinbaseWallet:
                return WalletProvider.CoinbaseWallet;
            case Wallet.WalletConnect:
                return WalletProvider.WalletConnect;
            case Wallet.MagicAuth:
                return WalletProvider.MagicAuth;
            case Wallet.DeviceWallet:
                return WalletProvider.DeviceWallet;
            default:
                throw new UnityException($"Wallet Provider for wallet {_wallet} unimplemented!");
        }
    }
}
