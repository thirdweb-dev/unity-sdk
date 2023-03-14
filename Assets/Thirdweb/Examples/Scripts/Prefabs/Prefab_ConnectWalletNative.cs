using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Unity;

public enum WalletNative
{
    DeviceWallet,
    WalletConnect,
}

[Serializable]
public struct WalletButtonNative
{
    public WalletNative wallet;
    public Button walletButton;
    public Sprite icon;
}

[Serializable]
public struct NetworkSpriteNative
{
    public Chain chain;
    public Sprite sprite;
}

public class Prefab_ConnectWalletNative : MonoBehaviour
{
    [Header("SETTINGS")]
    public List<WalletNative> supportedWallets;

    [Header("CUSTOM CALLBACKS")]
    public UnityEvent OnConnectedCallback;
    public UnityEvent OnDisconnectedCallback;
    public UnityEvent OnFailedConnectCallback;
    public UnityEvent OnFailedDisconnectCallback;

    [Header("UI ELEMENTS (DO NOT EDIT)")]
    // Connecting
    public GameObject connectButton;
    public GameObject connectDropdown;
    public List<WalletButtonNative> walletButtons;

    // Connected
    public GameObject connectedButton;
    public GameObject connectedDropdown;
    public TMP_Text balanceText;
    public TMP_Text walletAddressText;
    public Image walletImage;
    public TMP_Text currentNetworkText;
    public Image currentNetworkImage;
    public Image chainImage;

    // Networks
    public List<NetworkSpriteNative> networkSprites;

    string address;
    WalletNative wallet;
    bool connecting;
    WCSessionData wcSessionData;

    // UI Initialization

    private void Start()
    {
        address = null;

        if (supportedWallets.Count == 1)
            connectButton.GetComponent<Button>().onClick.AddListener(() => OnConnect(supportedWallets[0]));
        else
            connectButton.GetComponent<Button>().onClick.AddListener(() => OnClickDropdown());

        foreach (WalletButtonNative wb in walletButtons)
        {
            if (supportedWallets.Contains(wb.wallet))
            {
                wb.walletButton.gameObject.SetActive(true);
                wb.walletButton.GetComponent<Button>().onClick.AddListener(() => OnConnect(wb.wallet));
            }
            else
            {
                wb.walletButton.gameObject.SetActive(false);
            }
        }

        connectedButton.GetComponent<Button>().onClick.AddListener(() => OnClickDropdown());

        connectButton.SetActive(true);
        connectedButton.SetActive(false);

        connectDropdown.SetActive(false);
        connectedDropdown.SetActive(false);
    }

    // Connecting

    public async void OnConnect(WalletNative _wallet)
    {
        try
        {
            Nethereum.Web3.Accounts.Account account = null;
            switch (_wallet)
            {
                case WalletNative.DeviceWallet:
                    account = Utils.GenerateAccount(ThirdwebManager.Instance.SDK.nativeSession.lastChainId);
                    address = await ThirdwebManager.Instance.SDK.wallet.Connect(null, account, null);
                    break;
                case WalletNative.WalletConnect:
                    wcSessionData = await WalletConnect.Instance.EnableWalletConnect();
                    address = await ThirdwebManager.Instance.SDK.wallet.Connect(null, null, wcSessionData);
                    break;
                default:
                    throw new UnityException("Unimplemented Method Of Native Wallet Connection");
            }
            wallet = _wallet;
            OnConnected();
            OnConnectedCallback?.Invoke();
            print($"Connected successfully to: {address}");
        }
        catch (Exception e)
        {
            OnDisconnect();
            OnFailedConnectCallback?.Invoke();
            print($"Error Connecting Wallet: {e.Message}");
        }
    }

    async void OnConnected()
    {
        // try
        // {
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
        walletImage.sprite = walletButtons.Find(x => x.wallet == wallet).icon;
        chainImage.sprite = networkSprites.Find(x => x.chain == _chain).sprite;
        // }
        // catch (Exception e)
        // {
        //     print($"Error Fetching Native Balance: {e.Message}");
        // }
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

    // UI

    public void OnClickDropdown()
    {
        if (String.IsNullOrEmpty(address))
            connectDropdown.SetActive(!connectDropdown.activeInHierarchy);
        else
            connectedDropdown.SetActive(!connectedDropdown.activeInHierarchy);
    }

    public void OnCopyAddress()
    {
        GUIUtility.systemCopyBuffer = address;
        Debugger.Instance.Log("Copied your address to your clipboard!", $"Address: {address}");
    }
}
