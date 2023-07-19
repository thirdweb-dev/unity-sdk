using System.IO;
using System.Threading.Tasks;
using UnityBinder;
using UnityEngine;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Storage;

namespace WalletConnect
{
    [RequireComponent(typeof(WCWebSocketBuilder))]
    public class WalletConnectUnity : BindableMonoBehavior
    {
        private static WalletConnectUnity _instance;

        public static WalletConnectUnity Instance => _instance;

        private bool _initialized = false;

        [BindComponent]
        private WCWebSocketBuilder _builder;
        public WalletConnectCore Core { get; private set; }

        protected override void Awake()
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
        }

        internal async Task InitCore(string projectName, string projectId, string baseContext)
        {
            if (_initialized)
                return;

            _initialized = true;

            if (File.Exists(Application.persistentDataPath + "/walletconnect.json"))
                File.Delete(Application.persistentDataPath + "/walletconnect.json");

            var storage = new FileSystemStorage(Application.persistentDataPath + "/walletconnect.json");

            if (_builder == null)
                _builder = GetComponent<WCWebSocketBuilder>();

            Core = new WalletConnectCore(
                new CoreOptions()
                {
                    Name = projectName,
                    ProjectId = projectId,
                    BaseContext = baseContext,
                    Storage = storage,
                    ConnectionBuilder = _builder,
                }
            );

            await Core.Start();
        }
    }
}
