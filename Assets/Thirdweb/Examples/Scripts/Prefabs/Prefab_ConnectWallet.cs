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

[Serializable]
public struct NetworkSprite
{
    public Chain chain;
    public Sprite sprite;
}

public class Prefab_ConnectWallet : MonoBehaviour
{
    [Header("SETTINGS")]
    public List<Wallet> supportedWallets = new List<Wallet> { Wallet.MetaMask, Wallet.CoinbaseWallet, Wallet.WalletConnect };
    public bool supportSwitchingNetwork = false;

    [Header("UI ELEMENTS (DO NOT EDIT)")]
    // Connecting
    public GameObject connectButton;
    public GameObject connectDropdown;
    public List<WalletButton> walletButtons;
    // Connected
    public GameObject connectedButton;
    public GameObject connectedDropdown;
    public TMP_Text balanceText;
    public TMP_Text walletAddressText;
    public Image walletImage;
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
               new WalletConnection()
               {
                   provider = GetWalletProvider(_wallet),
                   chainId = (int)ThirdwebManager.Instance.chain,
               });

            wallet = _wallet;
            OnConnected();
            print($"Connected successfully to: {address}");
        }
        catch (Exception e)
        {
            print($"Error Connecting Wallet: {e.Message}");
        }
    }

    async void OnConnected()
    {
        try
        {
            Chain _chain = ThirdwebManager.Instance.chain;
            CurrencyValue nativeBalance = await ThirdwebManager.Instance.SDK.wallet.GetBalance();
            balanceText.text = $"{nativeBalance.value.ToEth()} {nativeBalance.symbol}";
            walletAddressText.text = address.ShortenAddress();
            currentNetworkText.text = ThirdwebManager.Instance.chainIdentifiers[_chain];
            currentNetworkImage.sprite = networkSprites.Find(x => x.chain == _chain).sprite;
            connectButton.SetActive(false);
            connectedButton.SetActive(true);
            connectDropdown.SetActive(false);
            connectedDropdown.SetActive(false);
            networkDropdown.SetActive(false);
            walletImage.sprite = walletButtons.Find(x => x.wallet == wallet).icon;
            chainImage.sprite = networkSprites.Find(x => x.chain == _chain).sprite;
        }
        catch (Exception e)
        {
            print($"Error Fetching Native Balance: {e.Message}");
        }

    }

    // Disconnecting

    public async void OnDisconnect()
    {
        try
        {
            await ThirdwebManager.Instance.SDK.wallet.Disconnect();
            OnDisconnected();
            print($"Disconnected successfully.");

        }
        catch (Exception e)
        {
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

    public async void OnSwitchNetwork(Chain _chain)
    {

        try
        {
            ThirdwebManager.Instance.chain = _chain;
            await ThirdwebManager.Instance.SDK.wallet.SwitchNetwork((int)_chain);
            OnConnected();
            print($"Switched Network Successfully: {_chain}");

        }
        catch (Exception e)
        {
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

        foreach (Chain chain in Enum.GetValues(typeof(Chain)))
        {
            if (chain == ThirdwebManager.Instance.chain || !ThirdwebManager.Instance.supportedNetworks.Contains(chain))
                continue;

            GameObject networkButton = Instantiate(networkButtonPrefab, networkDropdown.transform);
            networkButton.GetComponent<Button>().onClick.RemoveAllListeners();
            networkButton.GetComponent<Button>().onClick.AddListener(() => OnSwitchNetwork(chain));
            networkButton.transform.Find("Text_Network").GetComponent<TMP_Text>().text = ThirdwebManager.Instance.chainIdentifiers[chain];
            networkButton.transform.Find("Icon_Network").GetComponent<Image>().sprite = networkSprites.Find(x => x.chain == chain).sprite;
        }
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
}
