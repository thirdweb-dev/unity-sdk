using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using WalletConnectUnity.Core;
using System.Numerics;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models;
using System;
using Thirdweb.Redcode.Awaiting;

namespace Thirdweb.Wallets
{
    public class WalletConnectUI : MonoBehaviour
    {
        public GameObject WalletConnectCanvas;
        public Image QRCodeImage;
        public Button DeepLinkButton;
        public string[] SupportedMethods = new[] { "eth_sendTransaction", "personal_sign", "eth_signTypedData_v4" };

        public static WalletConnectUI Instance { get; private set; }

        protected Exception _exception;

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
            _exception = null;

            if (!WalletConnect.Instance.IsInitialized)
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

                var task = connectData.Approval;
                await new WaitUntil(() => task.IsCompleted || _exception != null);
                if (_exception != null)
                {
                    WalletConnectCanvas.SetActive(false);
                    throw _exception;
                }
            }
            WalletConnectCanvas.SetActive(false);
        }

        public virtual void Cancel()
        {
            _exception = new UnityException("User cancelled");
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
