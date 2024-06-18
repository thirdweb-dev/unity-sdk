using UnityEngine;
using System.Collections.Generic;

namespace Thirdweb.Unity
{
    public class ThirdwebManager : MonoBehaviour
    {
        [field: SerializeField, Header("Client Settings")]
        private string ClientId { get; set; }

        [field: SerializeField]
        private string BundleId { get; set; }

        [field: SerializeField]
        private bool InitializeOnAwake { get; set; } = true;

        [field: SerializeField]
        private bool ShowDebugLogs { get; set; } = true;

        public ThirdwebClient Client { get; private set; }

        public static ThirdwebManager Instance { get; private set; }

        private const string THIRDWEB_UNITY_SDK_VERSION = "5.0.0-beta.1";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            ThirdwebDebug.IsEnabled = ShowDebugLogs;

            if (InitializeOnAwake)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(BundleId))
            {
                ThirdwebDebug.LogError("ClientId and BundleId must be set in order to initialize ThirdwebManager.");
                return;
            }

            Client = ThirdwebClient.Create(
                clientId: ClientId,
                bundleId: BundleId,
                httpClient: new UnityThirdwebHttpClient(),
                headers: new Dictionary<string, string>
                {
                    { "x-sdk-name", Application.platform == RuntimePlatform.WebGLPlayer ? "UnitySDK_WebGL" : "UnitySDK" },
                    { "x-sdk-os", Application.platform.ToString() },
                    { "x-sdk-platform", "unity" },
                    { "x-sdk-version", THIRDWEB_UNITY_SDK_VERSION },
                    { "x-client-id", ClientId },
                    { "x-bundle-id", BundleId }
                }
            );

            ThirdwebDebug.Log("ThirdwebManager initialized.");
        }
    }
}
