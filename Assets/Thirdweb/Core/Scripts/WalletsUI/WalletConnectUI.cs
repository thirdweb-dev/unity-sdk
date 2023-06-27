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

namespace Thirdweb.Wallets
{
    public class WalletConnectUI : MonoBehaviour
    {
        public GameObject WalletConnectCanvas;
        public Image QRCodeImage;
        public Button DeepLinkButton;

        public static WalletConnectUI Instance;

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

        public async Task<WalletConnectSignClient> Connect(string walletConnectProjectId, System.Numerics.BigInteger chainId)
        {
            WalletConnectCanvas.SetActive(true);

            var dappOptions = new SignClientOptions()
            {
                ProjectId = walletConnectProjectId,
                Metadata = new Metadata()
                {
                    Description = ThirdwebManager.Instance.SDK.session.Options.wallet?.appDescription,
                    Icons = ThirdwebManager.Instance.SDK.session.Options.wallet?.appIcons,
                    Name = ThirdwebManager.Instance.SDK.session.Options.wallet?.appName,
                    Url = ThirdwebManager.Instance.SDK.session.Options.wallet?.appUrl
                },
                // Storage = new InMemoryStorage()
            };

            WalletConnectSignClient dappClient = await WalletConnectSignClient.Init(dappOptions);

            var dappConnectOptions = new ConnectOptions()
            {
                RequiredNamespaces = new RequiredNamespaces()
                {
                    {
                        "eip155",
                        new RequiredNamespace()
                        {
                            Methods = new[]
                            {
                                "eth_sendTransaction",
                                // "eth_sendRawTransaction",
                                "personal_sign",
                                "eth_signTypedData_v4",
                                "eth_accounts",
                                "eth_chainId",
                                "eth_getBalance"
                            },
                            Chains = new[] { $"eip155:{chainId}" },
                            Events = new[] { "chainChanged", "accountsChanged", }
                        }
                    }
                }
            };

            var connectData = await dappClient.Connect(dappConnectOptions);

            Debug.Log($"URI: {connectData.Uri}");

            var qrCodeAsTexture2D = GenerateQRTexture(connectData.Uri);
            QRCodeImage.sprite = Sprite.Create(qrCodeAsTexture2D, new Rect(0, 0, qrCodeAsTexture2D.width, qrCodeAsTexture2D.height), new Vector2(0.5f, 0.5f));
            DeepLinkButton.onClick.RemoveAllListeners();
            DeepLinkButton.onClick.AddListener(() => Application.OpenURL(connectData.Uri));

            await connectData.Approval;

            WalletConnectCanvas.SetActive(false);

            return dappClient;
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
