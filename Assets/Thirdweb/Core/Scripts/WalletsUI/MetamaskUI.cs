using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetaMask.Models;
using MetaMask.Transports;
using MetaMask.Transports.Unity;
using MetaMask.Unity;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using TMPro;

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

        public async Task<string> Connect()
        {
            OTPPanel.SetActive(false);

            _connected = false;
            _authorized = false;
            _exception = null;

            MetamaskCanvas.SetActive(true);

            MetaMaskUnity.Instance.Wallet.WalletConnectedHandler += OnWalletConnected;
            MetaMaskUnity.Instance.Wallet.WalletAuthorizedHandler += OnWalletAuthorized;

            MetaMaskUnity.Instance.Connect();

            await new WaitUntil(() => (_connected && _authorized) || _exception != null);

            MetaMaskUnity.Instance.Wallet.WalletAuthorizedHandler -= OnWalletConnected;
            MetaMaskUnity.Instance.Wallet.WalletAuthorizedHandler -= OnWalletAuthorized;

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

        public void ShowQR(string universalLink, string deepLink)
        {
            Debug.Log($"Universal Link: {universalLink}");
            Debug.Log($"Deep Link: {deepLink}");

            var qrCodeAsTexture2D = GenerateQRTexture(universalLink);
            QRCodeImage.sprite = Sprite.Create(qrCodeAsTexture2D, new Rect(0, 0, qrCodeAsTexture2D.width, qrCodeAsTexture2D.height), new Vector2(0.5f, 0.5f));
            DeepLinkButton.onClick.RemoveAllListeners();
            DeepLinkButton.onClick.AddListener(() => Application.OpenURL(deepLink));
            QRCodeImage.mainTexture.filterMode = FilterMode.Point;
        }

        private void OnWalletConnected(object sender, EventArgs e)
        {
            _connected = true;
        }

        private void OnWalletAuthorized(object sender, EventArgs e)
        {
            _authorized = true;
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

        public void OnMetaMaskConnectRequest(string universalLink, string deepLink)
        {
            ShowQR(universalLink, deepLink);
        }

        public void OnMetaMaskOTP(int otp)
        {
            OTPPanel.SetActive(true);
            OTPText.text = otp.ToString();
        }

        public void OnMetaMaskDisconnected()
        {
            _exception = new UnityException("User disconnected");
        }
    }
}
