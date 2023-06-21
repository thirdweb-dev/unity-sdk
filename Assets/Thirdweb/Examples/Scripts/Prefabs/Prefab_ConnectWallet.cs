using System.Collections.Generic;
using UnityEngine;
using Thirdweb;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Numerics;

[Serializable]
public struct WalletButton
{
    public WalletProvider wallet;
    public Button walletButton;
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
    public List<WalletProvider> supportedWallets = new List<WalletProvider>() { WalletProvider.LocalWallet, WalletProvider.WalletConnect };

    [Header("CUSTOM CALLBACKS")]
    public UnityEvent OnConnectedCallback;
    public UnityEvent OnDisconnectedCallback;
    public UnityEvent OnSwitchNetworkCallback;
    public UnityEvent OnFailedConnectCallback;
    public UnityEvent OnFailedDisconnectCallback;
    public UnityEvent OnFailedSwitchNetworkCallback;

    [Header("UI ELEMENTS (DANGER ZONE)")]
    // Connecting
    public GameObject connectButton;
    public GameObject connectDropdown;
    public List<WalletButton> walletButtons;
    public GameObject passwordPanel;
    public TMP_InputField passwordInputField;
    public Button passwordButton;
    public GameObject emailPanel;
    public TMP_InputField emailInputField;
    public Button emailButton;

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
    public Button networkSwitchButton;
    public GameObject networkSwitchImage;
    public GameObject networkDropdown;
    public GameObject networkButtonPrefab;
    public List<NetworkSprite> networkSprites;

    string address;
    WalletProvider _wallet;
    bool connecting;

    public string Address
    {
        get { return address; }
    }

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
            else if (supportedWallets[0] == WalletProvider.MagicLink)
            {
                connectButton.GetComponent<Button>().onClick.AddListener(() => OpenEmailPanel(WalletProvider.MagicLink));
            }
            else if (supportedWallets[0] == WalletProvider.Paper)
            {
                connectButton.GetComponent<Button>().onClick.AddListener(() => OpenEmailPanel(WalletProvider.Paper));
            }
            else
            {
                connectButton.GetComponent<Button>().onClick.AddListener(() => OnConnect(supportedWallets[0]));
            }
        }
        else
            connectButton.GetComponent<Button>().onClick.AddListener(() => OnClickDropdown());

        foreach (WalletButton wb in walletButtons)
        {
            if (supportedWallets.Contains(wb.wallet))
            {
                wb.walletButton.gameObject.SetActive(true);

                if (wb.wallet == WalletProvider.LocalWallet)
                {
                    wb.walletButton.onClick.AddListener(() => OpenPasswordPanel());
                }
                else if (wb.wallet == WalletProvider.MagicLink)
                {
                    wb.walletButton.onClick.AddListener(() => OpenEmailPanel(WalletProvider.MagicLink));
                }
                else if (wb.wallet == WalletProvider.Paper)
                {
                    wb.walletButton.onClick.AddListener(() => OpenEmailPanel(WalletProvider.Paper));
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

        bool multipleNetworks = ThirdwebManager.Instance.supportedChains.Count > 1;
        networkSwitchButton.GetComponent<Image>().raycastTarget = multipleNetworks;
        networkSwitchImage.SetActive(multipleNetworks);
        networkDropdown.SetActive(false);

        passwordPanel.SetActive(false);
        emailPanel.SetActive(false);
    }

    public void OpenPasswordPanel()
    {
        passwordPanel.SetActive(true);
        passwordButton.GetComponentInChildren<TMP_Text>().text = Utils.HasStoredAccount() ? "Unlock" : "Create wallet";
        passwordButton.onClick.RemoveAllListeners();
        passwordButton.onClick.AddListener(() => OnConnect(WalletProvider.LocalWallet, passwordInputField.text));
    }

    public void OpenEmailPanel(WalletProvider wallet)
    {
        emailPanel.SetActive(true);
        emailButton.onClick.RemoveAllListeners();
        emailButton.onClick.AddListener(() => OnConnect(wallet, null, emailInputField.text));
    }

    // Connecting

    public async void OnConnect(WalletProvider wallet, string password = null, string email = "joe@biden.com", WalletProvider personalWallet = WalletProvider.LocalWallet)
    {
        try
        {
            ChainData currentChain = ThirdwebManager.Instance.supportedChains.Find(x => x.identifier == ThirdwebManager.Instance.chain);

            address = await ThirdwebManager.Instance.SDK.wallet.Connect(new WalletConnection(wallet, BigInteger.Parse(currentChain.chainId), password, email, personalWallet));

            if (wallet == WalletProvider.LocalWallet || (wallet == WalletProvider.SmartWallet && personalWallet == WalletProvider.LocalWallet))
            {
                exportButton.SetActive(true);
                exportButton.GetComponent<Button>().onClick.RemoveAllListeners();
                exportButton.GetComponent<Button>().onClick.AddListener(() => OnExportWallet(password));
            }
            else
            {
                exportButton.SetActive(false);
            }

            _wallet = wallet;
            OnConnected();
            OnConnectedCallback?.Invoke();
            Debug.Log($"Connected successfully to: {address}");
        }
        catch (Exception e)
        {
            OnDisconnect();
            OnFailedConnectCallback?.Invoke();
            Debug.LogWarning($"Error Connecting Wallet: {e}");
        }
    }

    async void OnConnected()
    {
        try
        {
            passwordPanel.SetActive(false);
            emailPanel.SetActive(false);
            string _chain = ThirdwebManager.Instance.chain;
            CurrencyValue nativeBalance = await ThirdwebManager.Instance.SDK.wallet.GetBalance();
            balanceText.text = $"{nativeBalance.value.ToEth()} {nativeBalance.symbol}";
            balanceText2.text = balanceText.text;
            walletAddressText.text = await Utils.GetENSName(address) ?? address.ShortenAddress();
            walletAddressText2.text = walletAddressText.text;
            currentNetworkText.text = ThirdwebManager.Instance.chain;
            currentNetworkImage.sprite = networkSprites.Find(x => x.chain == _chain).sprite;
            connectButton.SetActive(false);
            connectedButton.SetActive(true);
            connectDropdown.SetActive(false);
            connectedDropdown.SetActive(false);
            networkDropdown.SetActive(false);
            walletImage.sprite = walletButtons.Find(x => x.wallet == _wallet).icon;
            walletImage2.sprite = walletImage.sprite;
            chainImage.sprite = networkSprites.Find(x => x.chain == _chain).sprite;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error Fetching Native Balance: {e}");
        }
    }

    // Switching Network

    public async void OnSwitchNetwork(string _chain)
    {
        try
        {
            ThirdwebManager.Instance.chain = _chain;
            ChainData currentChain = ThirdwebManager.Instance.supportedChains.Find(x => x.identifier == ThirdwebManager.Instance.chain);
            await ThirdwebManager.Instance.SDK.wallet.SwitchNetwork(int.Parse(currentChain.chainId));
            OnConnected();
            OnSwitchNetworkCallback?.Invoke();
            Debug.Log($"Switched Network Successfully: {_chain}");
        }
        catch (Exception e)
        {
            OnFailedSwitchNetworkCallback?.Invoke();
            Debug.LogWarning($"Error Switching Network: {e.Message}");
        }
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

        foreach (ChainData chainData in ThirdwebManager.Instance.supportedChains)
        {
            if (chainData.identifier == ThirdwebManager.Instance.chain || !ThirdwebManager.Instance.supportedChains.Contains(chainData))
                continue;

            GameObject networkButton = Instantiate(networkButtonPrefab, networkDropdown.transform);
            networkButton.GetComponent<Button>().onClick.RemoveAllListeners();
            networkButton.GetComponent<Button>().onClick.AddListener(() => OnSwitchNetwork(chainData.identifier));
            networkButton.transform.Find("Text_Network").GetComponent<TMP_Text>().text = chainData.identifier;
            networkButton.transform.Find("Icon_Network").GetComponent<Image>().sprite = networkSprites.Find(x => x.chain == chainData.identifier).sprite;
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
            Debug.Log($"Disconnected successfully.");
        }
        catch (Exception e)
        {
            OnFailedDisconnectCallback?.Invoke();
            Debug.LogWarning($"Error Disconnecting Wallet: {e.Message}");
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
        emailPanel.SetActive(false);
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
        Debugger.Instance.Log("Copied Address", $"Connected Address: {address}");
        GUIUtility.systemCopyBuffer = address;
    }

    public async void OnExportWallet(string password)
    {
        string text = await ThirdwebManager.Instance.SDK.wallet.Export(password);
        Debugger.Instance.Log("Exported Wallet", text);
        GUIUtility.systemCopyBuffer = text;
    }
}
