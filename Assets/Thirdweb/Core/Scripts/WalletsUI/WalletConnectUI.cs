using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using WalletConnectUnity.Core;
using System.Numerics;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models;

namespace Thirdweb.Wallets
{
    public class WalletConnectUI : MonoBehaviour
    {
        public GameObject WalletConnectCanvas;
        public Image QRCodeImage;
        public Button DeepLinkButton;
        public string[] SupportedMethods = new[] { "eth_sendTransaction", "personal_sign", "eth_signTypedData_v4" };

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

        public virtual async Task Connect(string walletConnectProjectId, BigInteger chainId)
        {
            try
            {
                await WalletConnect.Instance.InitializeAsync();

                var sessionResumed = await WalletConnect.Instance.TryResumeSessionAsync();
                if (!sessionResumed)
                {
                    WalletConnectCanvas.SetActive(true);

                    var connectOptions = new ConnectOptions
                    {
                        RequiredNamespaces = new RequiredNamespaces
                        {
                            {
                                "eip155",
                                new ProposedNamespace
                                {
                                    Methods = SupportedMethods,
                                    Chains = new[] { $"eip155:{chainId}" },
                                    Events = new[] { "chainChanged", "accountsChanged" },
                                }
                            }
                        },
                    };

                    var connectData = await WalletConnect.Instance.ConnectAsync(connectOptions);

                    var qrCodeAsTexture2D = GenerateQRTexture(connectData.Uri);
                    QRCodeImage.sprite = Sprite.Create(qrCodeAsTexture2D, new Rect(0, 0, qrCodeAsTexture2D.width, qrCodeAsTexture2D.height), new UnityEngine.Vector2(0.5f, 0.5f));
                    QRCodeImage.mainTexture.filterMode = FilterMode.Point;

                    DeepLinkButton.onClick.RemoveAllListeners();
                    DeepLinkButton.onClick.AddListener(() => Application.OpenURL(connectData.Uri));

                    await connectData.Approval;
                }
                WalletConnectCanvas.SetActive(false);
            }
            catch
            {
                WalletConnectCanvas.SetActive(false);
                throw;
            }
        }

        public virtual Texture2D GenerateQRTexture(string text)
        {
            Texture2D encoded = new(256, 256);
            var color32 = EncodeToQR(text, encoded.width, encoded.height);
            encoded.SetPixels32(color32);
            encoded.Apply();
            return encoded;
        }

        public virtual Color32[] EncodeToQR(string textForEncoding, int width, int height)
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
