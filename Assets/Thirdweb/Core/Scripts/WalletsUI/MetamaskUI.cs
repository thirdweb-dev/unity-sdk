using System;
using System.Threading.Tasks;
using MetaMask.Models;
using MetaMask.Transports.Unity;
using MetaMask.Unity;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using TMPro;
using Thirdweb.Redcode.Awaiting;

namespace Thirdweb.Wallets
{
    public class MetamaskUI : MonoBehaviour, IMetaMaskUnityTransportListener
    {
        public GameObject MetamaskCanvas;
        public Image QRCodeImage;
        public Button DeepLinkButton;
        public GameObject OTPPanel;
        public TMP_Text OTPText;

        public static MetamaskUI Instance;

        private bool _connected;
        private bool _authorized;
        private Exception _exception;

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

        // Core

        public async Task<string> Connect()
        {
            OTPPanel.SetActive(false);

            _connected = false;
            _authorized = false;
            _exception = null;

            await new WaitForSeconds(0.5f);

            MetamaskCanvas.SetActive(true);

            MetaMaskUnity.Instance.Events.WalletConnected += OnWalletConnected;
            MetaMaskUnity.Instance.Events.WalletAuthorized += OnWalletAuthorized;

            MetaMaskUnity.Instance.Connect();

            await new WaitUntil(() => (_connected && _authorized) || _exception != null);

            MetaMaskUnity.Instance.Events.WalletConnected -= OnWalletConnected;
            MetaMaskUnity.Instance.Events.WalletAuthorized -= OnWalletAuthorized;

            OTPPanel.SetActive(false);

            MetamaskCanvas.SetActive(false);

            if (_exception != null)
            {
                MetaMaskUnity.Instance.Disconnect(true);
                throw _exception;
            }

            return MetaMaskUnity.Instance.Wallet.SelectedAddress;
        }

        public void Cancel()
        {
            _exception = new UnityException("User cancelled");
        }

        // QR

        private void ShowQR(string universalLink, string deepLink)
        {
            var qrCodeAsTexture2D = GenerateQRTexture(universalLink);
            QRCodeImage.sprite = Sprite.Create(qrCodeAsTexture2D, new Rect(0, 0, qrCodeAsTexture2D.width, qrCodeAsTexture2D.height), new Vector2(0.5f, 0.5f));
            DeepLinkButton.onClick.RemoveAllListeners();
            DeepLinkButton.onClick.AddListener(() => Application.OpenURL(universalLink));
            QRCodeImage.mainTexture.filterMode = FilterMode.Point;
        }

        private Texture2D GenerateQRTexture(string text)
        {
            Texture2D encoded = new Texture2D(256, 256);
            var color32 = EncodeToQR(text, encoded.width, encoded.height);
            encoded.SetPixels32(color32);
            encoded.Apply();
            return encoded;
        }

        private Color32[] EncodeToQR(string textForEncoding, int width, int height)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions { Height = height, Width = width }
            };
            return writer.Write(textForEncoding);
        }

        // Top level Event Listeners

        private void OnWalletConnected(object sender, EventArgs e)
        {
            _connected = true;
        }

        private void OnWalletAuthorized(object sender, EventArgs e)
        {
            _authorized = true;
        }

        // IMetaMaskUnityTransportListener

        public void OnMetaMaskConnectRequest(string universalLink, string deepLink)
        {
            ShowQR(universalLink, deepLink);
        }

        public void OnMetaMaskRequest(string id, MetaMaskEthereumRequest request)
        {
            return;
        }

        public void OnMetaMaskFailure(Exception error)
        {
            _exception = error;
        }

        public void OnMetaMaskSuccess()
        {
            _connected = true;
            _authorized = true;
        }

        public void OnMetaMaskOTP(int otp)
        {
            OTPPanel.SetActive(true);
            OTPText.text = otp.ToString();
        }

        public void OnMetaMaskDisconnected()
        {
            if (!MetaMaskUnity.Instance.Wallet.Transport.IsMobile || !MetaMaskUnity.Instance.Wallet.HasSession)
            {
                _exception = new UnityException("User disconnected");
            }
        }
    }
}
