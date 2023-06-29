using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RotaryHeart.Lib.SerializableDictionary;
using Thirdweb;
using TMPro;

[System.Serializable]
public class WalletProviderUI
{
    public GameObject objectToShow;
    public Button connectButton;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
}

[System.Serializable]
public class WalletProviderUIDictionary : SerializableDictionaryBase<WalletProvider, WalletProviderUI> { }

public class Prefab_ConnectWalletButton : MonoBehaviour
{
    [Header("SUPPORTED WALLETS")]
    public List<WalletProvider> SupportedWallets = new List<WalletProvider>()
    {
        WalletProvider.Paper,
        WalletProvider.Injected,
        WalletProvider.Metamask,
        WalletProvider.Coinbase,
        WalletProvider.WalletConnect,
        WalletProvider.SmartWallet,
        WalletProvider.LocalWallet,
    };

    [Header("UI ELEMENTS (DANGER ZONE)")]
    public Button ConnectButton;
    public GameObject ConnectPanel;
    public GameObject OrGameObject;
    public WalletProviderUIDictionary SupportedWalletsUI;

    private void Awake()
    {
        ConnectPanel.SetActive(false);

        ConnectButton.onClick.AddListener(() => ToggleConnectPanel(true));

        if (SupportedWallets == null || SupportedWalletsUI.Count == 0)
            throw new UnityException("Please add at least one supported wallet!");

        foreach (var walletUI in SupportedWalletsUI)
        {
            walletUI.Value.objectToShow.SetActive(SupportedWallets.Contains(walletUI.Key));
            walletUI.Value.connectButton.onClick.AddListener(() => ValidateConnection(walletUI.Key));
        }

        bool usingEmailWallet = SupportedWallets.Contains(WalletProvider.MagicLink) || SupportedWallets.Contains(WalletProvider.Paper);
        bool usingNormalWallet =
            SupportedWallets.Contains(WalletProvider.Metamask)
            || SupportedWallets.Contains(WalletProvider.Coinbase)
            || SupportedWallets.Contains(WalletProvider.WalletConnect)
            || SupportedWallets.Contains(WalletProvider.Injected)
            || SupportedWallets.Contains(WalletProvider.SmartWallet);

        OrGameObject.SetActive(usingEmailWallet && usingNormalWallet);
    }

    public void ToggleConnectPanel(bool active)
    {
        ConnectPanel.SetActive(active);
    }

    private void ValidateConnection(WalletProvider walletProvider)
    {
        Debug.Log("ShowSecondaryPanel: " + walletProvider);

        string email = null;
        string password = null;
        WalletProvider personalWallet = WalletProvider.LocalWallet;

        switch (walletProvider)
        {
            case WalletProvider.Paper:
            case WalletProvider.MagicLink:
                if (SupportedWalletsUI[walletProvider].emailInput == null || string.IsNullOrEmpty(SupportedWalletsUI[walletProvider].emailInput.text))
                {
                    Debug.LogWarning("Could not connect, no email provided!");
                    ConnectPanel.SetActive(false);
                    return;
                }
                else
                {
                    email = SupportedWalletsUI[walletProvider].emailInput.text;
                    break;
                }
            case WalletProvider.LocalWallet:
                if (SupportedWalletsUI[walletProvider].passwordInput != null)
                {
                    password = string.IsNullOrEmpty(SupportedWalletsUI[walletProvider].passwordInput.text) ? null : SupportedWalletsUI[walletProvider].passwordInput.text;
                }
                break;
        }

        ConnectWallet(walletProvider, password, email, personalWallet);
    }

    private async void ConnectWallet(WalletProvider walletProvider, string password, string email, WalletProvider personalWallet)
    {
        Debug.Log($"Connecting to Wallet Provider: {walletProvider}...");

        try
        {
            string address = await ThirdwebManager.Instance.SDK.wallet.Connect(new WalletConnection(walletProvider, ThirdwebManager.Instance.SDK.session.ChainId, password, email, personalWallet));
            ShowConnectedState(address);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not connect to Wallet Provider: {walletProvider}! {e}");
            ConnectPanel.SetActive(false);
        }
    }

    private void ShowConnectedState(string address)
    {
        Debug.Log($"Connected to: {address}");

        ConnectPanel.SetActive(false);
    }
}
