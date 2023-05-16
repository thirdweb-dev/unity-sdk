using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Unity;

[Serializable]
public struct WalletButtonNative
{
    public WalletProvider wallet;
    public Button walletButton;
    public Sprite icon;
}

[Serializable]
public struct NetworkSpriteNative
{
    public string chain;
    public Sprite sprite;
}

public class Prefab_ConnectWalletNative : MonoBehaviour
{
    [Header("SETTINGS")]
    public List<WalletProvider> supportedWallets = new List<WalletProvider>() { WalletProvider.LocalWallet, WalletProvider.WalletConnectV1 };

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
    public TMP_Text balanceText2;
    public TMP_Text walletAddressText;
    public TMP_Text walletAddressText2;
    public Image walletImage;
    public Image walletImage2;
    public TMP_Text currentNetworkText;
    public Image currentNetworkImage;
    public Image chainImage;
    public GameObject exportButton;

    // Networks
    public List<NetworkSpriteNative> networkSprites;

    string address;
    WalletProvider wallet;
    bool connecting;
    WCSessionData wcSessionData;

    // UI Initialization

    private void Start()
    {
        address = null;

        if (supportedWallets.Count == 1)
        {
            if (supportedWallets[0] == WalletProvider.LocalWallet)
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

                if (wb.wallet == WalletProvider.LocalWallet)
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
        passwordButton.GetComponentInChildren<TMP_Text>().text = Utils.HasStoredAccount() ? "Unlock" : "Create wallet";
        passwordButton.onClick.RemoveAllListeners();
        passwordButton.onClick.AddListener(() => OnConnect(WalletProvider.LocalWallet, passwordInputField.text));
    }

    // Connecting

    public async void OnConnect(WalletProvider _wallet, string password = null, string email = "0xfirekeeper@gmail.com")
    {
        try
        {
            exportButton.SetActive(_wallet == WalletProvider.LocalWallet);

            address = await ThirdwebManager.Instance.SDK.wallet.Connect(new WalletConnection(_wallet, ThirdwebManager.Instance.GetCurrentChainID(), password, null, email));

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
            string _chain = ThirdwebManager.Instance.chain;
            CurrencyValue nativeBalance = await ThirdwebManager.Instance.SDK.wallet.GetBalance();
            balanceText.text = $"{nativeBalance.value.ToEth()} {nativeBalance.symbol}";
            balanceText2.text = balanceText.text;
            walletAddressText.text = await Utils.GetENSName(address) ?? address.ShortenAddress();
            walletAddressText2.text = walletAddressText.text;
            currentNetworkText.text = ThirdwebManager.Instance.GetCurrentChainIdentifier();
            currentNetworkImage.sprite = networkSprites.Find(x => x.chain == _chain).sprite;
            connectButton.SetActive(false);
            connectedButton.SetActive(true);
            connectDropdown.SetActive(false);
            connectedDropdown.SetActive(false);
            walletImage.sprite = walletButtons.Find(x => x.wallet == wallet).icon;
            walletImage2.sprite = walletImage.sprite;
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
