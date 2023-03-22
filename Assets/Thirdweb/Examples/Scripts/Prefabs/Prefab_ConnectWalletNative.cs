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
    DeviceWalletNoPassword,
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
    public GameObject passwordPanel;
    public TMP_InputField passwordInputField;
    public Button passwordButton;

    // Connected
    public GameObject connectedButton;
    public GameObject connectedDropdown;
    public TMP_Text balanceText;
    public TMP_Text walletAddressText;
    public Image walletImage;
    public TMP_Text currentNetworkText;
    public Image currentNetworkImage;
    public Image chainImage;
    public GameObject exportButton;

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
        {
            if (supportedWallets[0] == WalletNative.DeviceWallet)
            {
                connectButton.GetComponent<Button>().onClick.AddListener(() => OpenPasswordPanel());
            }
            else
            {
                connectButton.GetComponent<Button>().onClick.AddListener(() => OnConnect(supportedWallets[0]));
            }
        }
        else
            connectButton.GetComponent<Button>().onClick.AddListener(() => OnClickDropdown());

        foreach (WalletButtonNative wb in walletButtons)
        {
            if (supportedWallets.Contains(wb.wallet))
            {
                wb.walletButton.gameObject.SetActive(true);

                if (wb.wallet == WalletNative.DeviceWallet)
                {
                    wb.walletButton.onClick.AddListener(() => OpenPasswordPanel());
                }
                else
                {
                    wb.walletButton.onClick.AddListener(() => OnConnect(wb.wallet, null));
                }
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

        passwordPanel.SetActive(false);
    }

    public void OpenPasswordPanel()
    {
        passwordPanel.SetActive(true);
        passwordButton.GetComponentInChildren<TMP_Text>().text = Utils.HasStoredAccount() ? "Unlock" : "Create";
        passwordButton.onClick.RemoveAllListeners();
        passwordButton.onClick.AddListener(() => OnConnect(WalletNative.DeviceWallet, passwordInputField.text));
    }

    // Connecting

    public async void OnConnect(WalletNative _wallet, string password = null)
    {
        try
        {
            exportButton.SetActive(_wallet == WalletNative.DeviceWallet || _wallet == WalletNative.DeviceWalletNoPassword);

            switch (_wallet)
            {
                case WalletNative.DeviceWallet:
                    address = await ThirdwebManager.Instance.SDK.wallet.Connect(new WalletConnection() { password = password });
                    break;
                case WalletNative.DeviceWalletNoPassword:
                    address = await ThirdwebManager.Instance.SDK.wallet.Connect();
                    break;
                case WalletNative.WalletConnect:
                    address = await ThirdwebManager.Instance.SDK.wallet.Connect(new WalletConnection() { provider = WalletProvider.WalletConnect });
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
        try
        {
            passwordPanel.SetActive(false);
            Chain _chain = ThirdwebManager.Instance.chain;
            CurrencyValue nativeBalance = await ThirdwebManager.Instance.SDK.wallet.GetBalance();
            balanceText.text = $"{nativeBalance.value.ToEth()} {nativeBalance.symbol}";
            walletAddressText.text = await Utils.GetENSName(address) ?? address.ShortenAddress();
            currentNetworkText.text = ThirdwebManager.Instance.supportedChainData[ThirdwebManager.Instance.chain].identifier;
            currentNetworkImage.sprite = networkSprites.Find(x => x.chain == _chain).sprite;
            connectButton.SetActive(false);
            connectedButton.SetActive(true);
            connectDropdown.SetActive(false);
            connectedDropdown.SetActive(false);
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
        passwordPanel.SetActive(false);
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
        print($"Copied your address to your clipboard! Address: {address}");
    }

    public void OnExportWallet()
    {
        Application.OpenURL(Utils.GetAccountPath()); // Doesn't work on iOS or > Android 6

        // Fallback
        string text = System.IO.File.ReadAllText(Utils.GetAccountPath());
        GUIUtility.systemCopyBuffer = text;
        print(
            "Copied your encrypted keystore to your clipboard! You may import it into an external wallet with your password.\n"
                + "If no password was provided upon the creation of this account, the password is your device unique ID."
        );
    }
}
