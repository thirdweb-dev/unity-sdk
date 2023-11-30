using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Controllers;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Models.Relay;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using WalletConnectSharp.Storage;
using WalletConnectUnity.Models;
using WalletConnectSharp.Storage.Interfaces;
using WalletConnect.UnityBinder;

namespace WalletConnect
{
    [RequireComponent(typeof(WCWebSocketBuilder))]
    public class WalletConnectUnity : BindableMonoBehavior
    {
        private static WalletConnectUnity _instance;

        public static WalletConnectUnity Instance => _instance;

        public string ProjectName;
        public string ProjectId;
        public Metadata ClientMetadata;

        public bool ConnectOnAwake;
        public bool ConnectOnStart;
        public bool EnableCoreLogging;
        public string BaseContext = "unity-game";

        public bool AutoDownloadWalletData = true;
        public bool AutoDownloadWalletImages = false;
        public bool AlwaysUseDeeplink = false;
        public string DefaultWalletId = "c57ca95b47569778a828d19178114f4db188b89b763c899ba0be274e97267d96";

        public WCStorageType StorageType;

        public bool UseDeeplink => AlwaysUseDeeplink || Application.isMobilePlatform;

        public Wallet DefaultWallet
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DefaultWalletId))
                    return null;

                if (SupportedWallets.Count == 0)
                    throw new Exception("WalletConnectUnity.Instance.FetchWallets has not been invoked");

                return SupportedWallets[DefaultWalletId];
            }
            set => DefaultWalletId = value.Id;
        }

        [BindComponent]
        private WCWebSocketBuilder _builder;
        public WalletConnectCore Core { get; private set; }

        private TaskCompletionSource<bool> initTask;

        protected override async void Awake()
        {
            base.Awake();

            if (_instance == null || _instance == this)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
                return;
            }

            if (ConnectOnAwake)
            {
                await InitCore();
            }
        }

        // Start is called before the first frame update
        async void Start()
        {
            if (ConnectOnStart)
            {
                await InitCore();
            }
        }

        internal async Task InitCore()
        {
            if (initTask != null)
            {
                await initTask.Task;
                return;
            }

            if (Core != null)
                return;

            initTask = new TaskCompletionSource<bool>();

            if (AutoDownloadWalletData)
            {
                StartCoroutine(FetchWalletList(AutoDownloadWalletImages));
            }

            var storage = BuildStorage();

            if (EnableCoreLogging)
                WCLogger.Logger = new WCUnityLogger();
            try
            {
                WCLogger.Logger = new WCUnityLogger();

                if (_builder == null)
                    _builder = GetComponent<WCWebSocketBuilder>();

                Core = new WalletConnectCore(
                    new CoreOptions()
                    {
                        Name = ProjectName,
                        ProjectId = ProjectId,
                        BaseContext = BaseContext,
                        Storage = storage,
                        ConnectionBuilder = _builder,
                        //CryptoModule = crypto,
                    }
                );

                await Core.Start();

                initTask.SetResult(true);
            }
            catch (Exception e)
            {
                initTask.SetException(e);
            }
            finally
            {
                initTask.TrySetResult(false);
            }
        }

        public IKeyValueStorage BuildStorage()
        {
            switch (StorageType)
            {
                case WCStorageType.Disk:
                    var path = Application.persistentDataPath + "/walletconnect.json";
                    Debug.Log("Using storage location: " + path);
                    return new FileSystemStorage(Application.persistentDataPath + "/walletconnect.json");
                case WCStorageType.None:
                    return new InMemoryStorage();
                default:
                    throw new Exception("Invalid value");
            }
        }

        public async void OpenDefaultWallet()
        {
            if (DefaultWallet == null)
                throw new Exception("No default wallet set");

            await Task.Delay(TimeSpan.FromSeconds(2));

            DefaultWallet.OpenWallet();
        }

#if !UNITY_MONO
        [Preserve]
        void SetupAOT()
        {
            // Reference all required models
            // This is required so AOT code is generated for these generic functions
            var historyFactory = new JsonRpcHistoryFactory(null);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionPropose, SessionProposeResponse>().GetType().FullName);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionSettle, Boolean>().GetType().FullName);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionUpdate, Boolean>().GetType().FullName);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionExtend, Boolean>().GetType().FullName);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionDelete, Boolean>().GetType().FullName);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionPing, Boolean>().GetType().FullName);
            EventManager<string, GenericEvent<string>>.InstanceOf(null).PropagateEvent(null, null);
            throw new InvalidOperationException("This method is only for AOT code generation.");
        }
#endif

        public void FetchWallets(bool downloadImages = true)
        {
            StartCoroutine(FetchWalletList(downloadImages));
        }

        private IEnumerator FetchWalletList(bool downloadImages = true, Action callback = null)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://registry.walletconnect.org/data/wallets.json"))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Error Getting Wallet Info: " + webRequest.error);
                }
                else
                {
                    var json = webRequest.downloadHandler.text;

                    SupportedWallets = JsonConvert.DeserializeObject<Dictionary<string, Wallet>>(json);

                    if (downloadImages)
                    {
                        yield return WaitForAll(SupportedWallets.Keys.Select(DownloadImagesFor));
                    }
                }
            }

            if (callback != null)
                callback();
        }

        public void DownloadWalletImages()
        {
            StartCoroutine(DownloadAllWalletImages());
        }

        private IEnumerator DownloadAllWalletImages()
        {
            yield return WaitForAll(SupportedWallets.Keys.Select(DownloadImagesFor));
        }

        private IEnumerator WaitForAll(IEnumerable<IEnumerator> coroutines)
        {
            int tally = 0;

            foreach (IEnumerator c in coroutines)
            {
                StartCoroutine(RunCoroutine(c));
            }

            while (tally > 0)
            {
                yield return null;
            }

            IEnumerator RunCoroutine(IEnumerator c)
            {
                tally++;
                yield return StartCoroutine(c);
                tally--;
            }
        }

        private IEnumerator DownloadImagesFor(string id)
        {
            Dictionary<string, Action<Wallet, Sprite>> sizeMapping = new Dictionary<string, Action<Wallet, Sprite>>()
            {
                { "sm", (wallet, sprite) => wallet.Images.SmallIcon = sprite },
                { "md", (wallet, sprite) => wallet.Images.MediumIcon = sprite },
                { "lg", (wallet, sprite) => wallet.Images.LargeIcon = sprite }
            };

            var data = SupportedWallets[id];

            foreach (var size in sizeMapping.Keys)
            {
                var url = "https://registry.walletconnect.org/logo/" + size + "/" + id + ".jpeg";

                using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return imageRequest.SendWebRequest();

                    if (imageRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Error Getting Wallet Icon: " + imageRequest.error);
                    }
                    else
                    {
                        var texture = ((DownloadHandlerTexture)imageRequest.downloadHandler).texture;
                        var sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);

                        sizeMapping[size](data, sprite);
                    }
                }
            }
        }

        public void FindAndSetDefaultWallet(string walletName, bool ignoreCase = true, bool tryFetchWallets = true)
        {
            if (SupportedWallets.Count == 0)
            {
                if (!tryFetchWallets)
                    throw new Exception("No wallets to search");

                StartCoroutine(FetchWalletList(callback: () => FindAndSetDefaultWallet(walletName, tryFetchWallets: false)));
                return;
            }

            if (ignoreCase)
                walletName = walletName.ToLower();

            var defaultWalletId = SupportedWallets.FirstOrDefault(t => (ignoreCase ? t.Value.Name.ToLower() : t.Value.Name) == walletName).Key;

            if (defaultWalletId != null)
                DefaultWalletId = defaultWalletId;
            else
                throw new KeyNotFoundException($"No wallet by the name of {walletName} could be found");
        }

        public void FindAndSetDefaultWallet(Metadata peer, bool tryFetchWallets = true)
        {
            if (SupportedWallets.Count == 0)
            {
                if (!tryFetchWallets)
                    throw new Exception("No wallets to search");

                StartCoroutine(FetchWalletList(callback: () => FindAndSetDefaultWallet(peer, tryFetchWallets: false)));
                return;
            }

            var nativeUrl = peer.Redirect.Native.Replace("//", "");

            var defaultWalletId = SupportedWallets.FirstOrDefault(t => t.Value.Mobile.NativeProtocol == nativeUrl || t.Value.Desktop.NativeProtocol == nativeUrl).Key;

            if (defaultWalletId != null)
                DefaultWalletId = defaultWalletId;
            else
                throw new KeyNotFoundException($"No wallet by the name of {peer.Name} could be found");
        }

        public Dictionary<string, Wallet> SupportedWallets { get; set; }
    }
}
