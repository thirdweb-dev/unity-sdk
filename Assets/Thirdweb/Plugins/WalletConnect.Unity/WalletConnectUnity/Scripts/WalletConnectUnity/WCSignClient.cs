using System.Threading.Tasks;
using UnityEngine;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Models;

namespace WalletConnect
{
    [RequireComponent(typeof(WalletConnectUnity))]
    public class WCSignClient : BindableMonoBehavior
    {
        private static WCSignClient _currentInstance;

        public static WCSignClient Instance => _currentInstance;

        private bool _initialized = false;

        [BindComponent]
        private WalletConnectUnity WalletConnectUnity;

        public WalletConnectSignClient SignClient { get; private set; }

        public bool ConnectOnAwake => WalletConnectUnity.ConnectOnAwake;
        public bool ConnectOnStart => WalletConnectUnity.ConnectOnStart;

        public override async void Awake()
        {
            base.Awake();

            if (_currentInstance == null || _currentInstance == this)
            {
                _currentInstance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
                return;
            }

            if (ConnectOnAwake)
            {
                await InitSignClient();
            }
        }

        private async void Start()
        {
            if (ConnectOnStart)
            {
                await InitSignClient();
            }
        }

        public async Task InitSignClient()
        {
            if (_initialized)
                return;

            _initialized = true;

            await WalletConnectUnity.InitCore();

            SignClient = await WalletConnectSignClient.Init(
                new SignClientOptions()
                {
                    BaseContext = WalletConnectUnity.BaseContext,
                    Core = WalletConnectUnity.Core,
                    Metadata = WalletConnectUnity.ClientMetadata,
                    Name = WalletConnectUnity.ProjectName,
                    ProjectId = WalletConnectUnity.ProjectId,
                    Storage = WalletConnectUnity.Core.Storage,
                }
            );
        }
    }
}
