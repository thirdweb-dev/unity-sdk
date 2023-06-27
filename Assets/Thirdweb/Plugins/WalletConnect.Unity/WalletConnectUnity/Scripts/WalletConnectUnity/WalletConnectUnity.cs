using System.Threading.Tasks;
using UnityEngine;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Storage;

namespace WalletConnect
{
    public class WalletConnectUnity : BindableMonoBehavior
    {
        private static WalletConnectUnity _instance;

        public static WalletConnectUnity Instance => _instance;

        public string ProjectName;
        public string ProjectId;
        public Metadata ClientMetadata;

        public bool ConnectOnAwake;
        public bool ConnectOnStart;
        public string BaseContext = "unity-game";

        private bool _initialized = false;
        public WalletConnectCore Core { get; private set; }

        public override async void Awake()
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
            if (_initialized)
                return;

            _initialized = true;

            var storage = new FileSystemStorage(Application.persistentDataPath + "/walletconnect.json");

            Core = new WalletConnectCore(
                new CoreOptions()
                {
                    Name = ProjectName,
                    ProjectId = ProjectId,
                    BaseContext = BaseContext,
                    Storage = storage,
                }
            );

            await Core.Start();
        }
    }
}
