using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Core;
using ZXing;
using ZXing.QrCode;
using Nethereum.JsonRpc.Client;
using Nethereum.Unity.RpcModel;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Common.Utils;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Models;

namespace Thirdweb.Wallets
{
    public class WalletConnectUI : MonoBehaviour
    {
        public GameObject WalletConnectCanvas;
        public Image QRCodeImage;
        public Button DeepLinkButton;
        public string[] SupportedMethods = new string[] { "eth_sendTransaction", "personal_sign", "eth_signTypedData_v4" };

        public static WalletConnectUI Instance { get; private set; }

        private void Awake()
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
        }

        public async Task<(WalletConnectSignClient, string, string)> Connect(string walletConnectProjectId, System.Numerics.BigInteger chainId)
        {
            WalletConnectCanvas.SetActive(true);

            try
            {
                WalletConnectCore core = new WalletConnectCore(
                    new CoreOptions()
                    {
                        Name = ThirdwebManager.Instance.SDK.session.Options.wallet?.appName,
                        ProjectId = walletConnectProjectId,
                        BaseContext = "unity-game",
                        Storage = new InMemoryStorage() //new FileSystemStorage(Application.persistentDataPath + "/walletconnect.json"),
                    }
                );

                await core.Start();

                var client = await WalletConnectSignClient.Init(
                    new SignClientOptions()
                    {
                        BaseContext = "unity-game",
                        Core = core,
                        Metadata = new Metadata()
                        {
                            Name = ThirdwebManager.Instance.SDK.session.Options.wallet?.appName,
                            Description = ThirdwebManager.Instance.SDK.session.Options.wallet?.appDescription,
                            Icons = ThirdwebManager.Instance.SDK.session.Options.wallet?.appIcons,
                            Url = ThirdwebManager.Instance.SDK.session.Options.wallet?.appUrl
                        },
                        Name = core.Name,
                        ProjectId = core.ProjectId,
                        Storage = core.Storage,
                    }
                );

                var connectData = await client.Connect(
                    new ConnectOptions()
                    {
                        RequiredNamespaces = new RequiredNamespaces()
                        {
                            {
                                "eip155",
                                new ProposedNamespace()
                                {
                                    Methods = SupportedMethods,
                                    Chains = new[] { $"eip155:{chainId}" },
                                    Events = new[] { "chainChanged", "accountsChanged" }
                                }
                            }
                        }
                    }
                );

                Debug.Log($"URI: {connectData.Uri}");

                var qrCodeAsTexture2D = GenerateQRTexture(connectData.Uri);
                QRCodeImage.sprite = Sprite.Create(qrCodeAsTexture2D, new Rect(0, 0, qrCodeAsTexture2D.width, qrCodeAsTexture2D.height), new Vector2(0.5f, 0.5f));
                DeepLinkButton.onClick.RemoveAllListeners();
                DeepLinkButton.onClick.AddListener(() => Application.OpenURL(connectData.Uri));

                var sessionData = await connectData.Approval;

                string address = null;

                var selectedNamespace = sessionData.Namespaces["eip155"];
                if (selectedNamespace != null && selectedNamespace.Accounts.Length > 0)
                {
                    var currentSession = selectedNamespace.Accounts[0];
                    var parameters = currentSession.Split(':');
                    address = parameters[2];
                }

                WalletConnectCanvas.SetActive(false);
                return (client, address, sessionData.Topic);
            }
            catch (System.Exception)
            {
                WalletConnectCanvas.SetActive(false);
                throw;
            }
        }

        private static Texture2D GenerateQRTexture(string text)
        {
            Texture2D encoded = new Texture2D(256, 256);
            var color32 = EncodeToQR(text, encoded.width, encoded.height);
            encoded.SetPixels32(color32);
            encoded.Apply();
            return encoded;
        }

        private static Color32[] EncodeToQR(string textForEncoding, int width, int height)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions { Height = height, Width = width }
            };
            return writer.Write(textForEncoding);
        }
    }
}
