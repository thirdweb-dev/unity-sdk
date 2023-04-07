using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;
using WalletConnectSharp.Unity.Models;
using WalletConnectSharp.Unity.Network;
using WalletConnectSharp.Unity.Utils;
using RotaryHeart.Lib.SerializableDictionary;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Core.Models.Ethereum.Types;
using System.Threading;

namespace WalletConnectSharp.Unity
{
    public enum Mode
    {
        Standalone,
        Android,
        IOS,
    }

    [System.Serializable]
    public class ModeUI : SerializableDictionaryBase<Mode, GameObject> { }

    [RequireComponent(typeof(NativeWebSocketTransport))]
    public class WalletConnect : BindableMonoBehavior
    {
        public const string SessionKey = "__WALLETCONNECT_SESSION__";

        public ModeUI modeUI;
        public GameObject walletConnectCanvas;
        public Dictionary<string, AppEntry> SupportedWallets { get; private set; }
        public AppEntry SelectedWallet { get; set; }
        public Wallets DefaultWallet;
        public event EventHandler ConnectionStarted;
        public int connectSessionRetryCount = 3;
        public WalletConnectUnitySession Session { get; private set; }

        [BindComponent]
        private NativeWebSocketTransport _transport;
        private bool initialized;
        private bool eventsSetup;

        public static WalletConnect Instance;

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }

            ClearSession();
            if (Session != null)
                Session = null;

            base.Awake();
        }

        public async Task<WCSessionData> EnableWalletConnect()
        {
            SetMode(true);
            var sessionData = await Connect(ThirdwebManager.Instance.SDK.nativeSession.lastChainId);
            SetMode(false);
            return sessionData;
        }

        public async void DisableWalletConnect()
        {
            try
            {
                await Session.Disconnect();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e.Message);
                Session = null;
            }
            ClearSession();
            SetMode(false);
        }

        public void SetMode(bool active)
        {
#if UNITY_ANDROID
            modeUI[Mode.Android].SetActive(active);
#elif UNITY_IOS
            modeUI[Mode.IOS].SetActive(active);
#else
            modeUI[Mode.Standalone].SetActive(active);
#endif
            walletConnectCanvas.SetActive(active);
        }

        public async Task<WCSessionData> Connect(int chainId)
        {
            SavedSession savedSession = null;
            if (PlayerPrefs.HasKey(SessionKey))
            {
                var json = PlayerPrefs.GetString(SessionKey);
                savedSession = JsonConvert.DeserializeObject<SavedSession>(json);
            }

            if (Session != null)
            {
                if (savedSession != null)
                {
                    if (Session.KeyData != savedSession.Key)
                    {
                        if (Session.SessionConnected)
                            await Session.Disconnect();
                        else if (Session.TransportConnected)
                            await Session.Transport.Close();
                    }
                    else if (!Session.Connected && !Session.Connecting)
                    {
                        StartCoroutine(SetupDefaultWallet());
                        SetupEvents();
                        return await CompleteConnect();
                    }
                    else
                    {
                        Debug.LogWarning("Already Connected");
                        return null;
                    }
                }
                else if (Session.SessionConnected)
                {
                    await Session.Disconnect();
                }
                else if (Session.TransportConnected)
                {
                    await Session.Transport.Close();
                }
                // else if (Session.Connecting)
                // {
                //     Debug.LogWarning("Session connecting...");
                //     return null;
                // }
            }

            if (savedSession != null)
            {
                Session = new WalletConnectUnitySession(savedSession, this, _transport);
            }
            else
            {
                Session = new WalletConnectUnitySession(
                    new ClientMeta()
                    {
                        Name = ThirdwebManager.Instance.SDK.nativeSession.options.wallet?.appName,
                        Description = ThirdwebManager.Instance.SDK.nativeSession.options.wallet?.appDescription,
                        URL = ThirdwebManager.Instance.SDK.nativeSession.options.wallet?.appUrl,
                        Icons = ThirdwebManager.Instance.SDK.nativeSession.options.wallet?.appIcons,
                    },
                    this,
                    null,
                    _transport,
                    null,
                    chainId
                );
            }

            StartCoroutine(SetupDefaultWallet());
            SetupEvents();
            return await CompleteConnect();
        }

        private void SetupEvents()
        {
            if (eventsSetup)
                return;

            eventsSetup = true;

            Session.OnSessionConnect += (sender, session) =>
            {
                Debug.LogWarning("[WalletConnect] Session Connected");
            };

#if UNITY_ANDROID || UNITY_IOS && !UNITY_EDITOR
            //Whenever we send a request to the Wallet, we want to open the Wallet app
            Session.OnSend += (sender, session) => OpenMobileWallet();
#endif
        }

        private async Task<WCSessionData> CompleteConnect()
        {
            Debug.LogWarning("Waiting for Wallet connection");

            if (ConnectionStarted != null)
            {
                ConnectionStarted(this, EventArgs.Empty);
            }

            int tries = 0;
            while (tries < connectSessionRetryCount)
            {
                try
                {
                    return await Session.SourceConnectSession();
                }
                catch (IOException e)
                {
                    tries++;
                    if (tries >= connectSessionRetryCount)
                        throw new IOException("Failed to request session connection after " + tries + " times.", e);
                }
            }

            throw new IOException("Failed to request session connection after " + tries + " times.");
        }

        private string FormatWalletName(string name)
        {
            return name.Replace('.', ' ').Replace('|', ' ').Replace(")", "").Replace("(", "").Replace("'", "").Replace(" ", "").Replace("1", "One").ToLower();
        }

        private IEnumerator SetupDefaultWallet()
        {
            yield return FetchWalletList(false);
            var wallet = SupportedWallets.Values.FirstOrDefault(a => FormatWalletName(a.name) == DefaultWallet.ToString().ToLower());
            SelectedWallet = wallet ?? SupportedWallets.Values.First();
            yield return DownloadImagesFor(wallet.id);
            Debug.Log("Setup default wallet " + wallet.name);
        }

        public IEnumerator FetchWalletList(bool downloadImages = true)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://registry.walletconnect.org/data/wallets.json"))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    var json = webRequest.downloadHandler.text;

                    SupportedWallets = JsonConvert.DeserializeObject<Dictionary<string, AppEntry>>(json);

                    if (downloadImages)
                    {
                        foreach (var id in SupportedWallets.Keys)
                        {
                            yield return DownloadImagesFor(id);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Error Getting Wallet Info: " + webRequest.error);
                }
            }
        }

        private IEnumerator DownloadImagesFor(string id, string[] sizes = null)
        {
            if (sizes == null)
                sizes = new string[] { "sm", "md", "lg" };

            var data = SupportedWallets[id];

            foreach (var size in sizes)
            {
                var url = "https://registry.walletconnect.org/logo/" + size + "/" + id + ".jpeg";

                using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return imageRequest.SendWebRequest();

                    if (imageRequest.result == UnityWebRequest.Result.Success)
                    {
                        var texture = ((DownloadHandlerTexture)imageRequest.downloadHandler).texture;
                        var sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);

                        if (size == "sm")
                        {
                            data.smallIcon = sprite;
                        }
                        else if (size == "md")
                        {
                            data.medimumIcon = sprite;
                        }
                        else if (size == "lg")
                        {
                            data.largeIcon = sprite;
                        }
                    }
                    else
                    {
                        Debug.Log("Error Getting Wallet Icon: " + imageRequest.error);
                    }
                }
            }
        }

        private async void OnApplicationPause(bool pauseStatus)
        {
            if (!initialized)
            {
                initialized = true;
                return;
            }

            if (pauseStatus)
                SaveSession();
            else if (PlayerPrefs.HasKey(SessionKey))
                await Connect(ThirdwebManager.Instance.SDK.nativeSession.lastChainId);
        }

        private void SaveSession()
        {
            if (Session == null || !Session.Connected)
                return;

            var session = Session.SaveSession();
            var json = JsonConvert.SerializeObject(session);
            PlayerPrefs.SetString(SessionKey, json);
        }

        public void OpenMobileWallet(AppEntry selectedWallet)
        {
            SelectedWallet = selectedWallet;
            OpenMobileWallet();
        }

        public void OpenDeepLink(AppEntry selectedWallet)
        {
            SelectedWallet = selectedWallet;
            OpenDeepLink();
        }

        public void OpenMobileWallet()
        {
#if UNITY_ANDROID
            var signingURL = Session.URI.Split('@')[0];

            Application.OpenURL(signingURL);
#elif UNITY_IOS
            if (SelectedWallet == null)
            {
                throw new NotImplementedException(
                    "You must use OpenMobileWallet(AppEntry) or set SelectedWallet on iOS!");
            }
            else
            {
                string url;
                string encodedConnect = WebUtility.UrlEncode(Session.URI);
                if (!string.IsNullOrWhiteSpace(SelectedWallet.mobile.universal))
                {
                    url = SelectedWallet.mobile.universal + "/wc?uri=" + encodedConnect;
                }
                else
                {
                    url = SelectedWallet.mobile.native + (SelectedWallet.mobile.native.EndsWith(":") ? "//" : "/") +
                          "wc?uri=" + encodedConnect;
                }

                var signingUrl = url.Split('?')[0];
                
                Debug.Log("Opening: " + signingUrl);
                Application.OpenURL(signingUrl);
            }
#else
            Debug.Log("Platform does not support deep linking");
            return;
#endif
        }

        public void OpenDeepLink()
        {
            if (!Session.ReadyForUserPrompt)
            {
                Debug.LogError("WalletConnect.Session not ready for a user prompt" + "\nWait for Session.ReadyForUserPrompt to be true");
                return;
            }

#if UNITY_ANDROID
            Debug.Log("[WalletConnect] Opening URL: " + Session.URI);
            Application.OpenURL(Session.URI);
#elif UNITY_IOS
            if (SelectedWallet == null)
            {
                throw new NotImplementedException(
                    "You must use OpenDeepLink(AppEntry) or set SelectedWallet on iOS!");
            }
            else
            {
                string url;
                string encodedConnect = WebUtility.UrlEncode(Session.URI);
                if (!string.IsNullOrWhiteSpace(SelectedWallet.mobile.universal))
                {
                    url = SelectedWallet.mobile.universal + "/wc?uri=" + encodedConnect;
                }
                else
                {
                    url = SelectedWallet.mobile.native + (SelectedWallet.mobile.native.EndsWith(":") ? "//" : "/") +
                          "wc?uri=" + encodedConnect;
                }
                
                Debug.Log("Opening: " + url);
                Application.OpenURL(url);
            }
#else
            Debug.Log("Platform does not support deep linking");
            return;
#endif
        }

        public void ClearSession()
        {
            if (PlayerPrefs.HasKey(SessionKey))
                PlayerPrefs.DeleteKey(SessionKey);
        }

        // Utility

        public async Task<string> WalletAddEthChain(EthChainData chainData)
        {
            var results = await Session.WalletAddEthChain(chainData);
            return results;
        }

        public async Task<string> WalletSwitchEthChain(EthChain ethChain)
        {
            var results = await Session.WalletSwitchEthChain(ethChain);
            return results;
        }

        public async Task<string> PersonalSign(string message, int addressIndex = 0)
        {
            var address = Session.Accounts[addressIndex];
            var results = await Session.EthPersonalSign(address, message);
            return results;
        }

        public async Task<string> SendTransaction(TransactionData transaction)
        {
            var results = await Session.EthSendTransaction(transaction);
            return results;
        }

        public async Task<string> SignTransaction(TransactionData transaction)
        {
            var results = await Session.EthSignTransaction(transaction);
            return results;
        }

        public async Task<string> SignTypedData<T>(T data, EIP712Domain eip712Domain, int addressIndex = 0)
        {
            var address = Session.Accounts[addressIndex];
            var results = await Session.EthSignTypedData(address, data, eip712Domain);
            return results;
        }
    }
}
