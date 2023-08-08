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

namespace Thirdweb.Wallets
{
    public class MetamaskUI : MonoBehaviour, IMetaMaskUnityTransportListener
    {
        public GameObject MetamaskCanvas;
        public Image QRCodeImage;
        public Button DeepLinkButton;

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
            _connected = false;
            _authorized = false;
            _exception = null;

            MetamaskCanvas.SetActive(true);

            MetaMaskUnity.Instance.Wallet.WalletConnected += OnWalletConnected;
            MetaMaskUnity.Instance.Wallet.WalletAuthorized += OnWalletAuthorized;

            MetaMaskUnity.Instance.Connect();

            await new WaitUntil(() => (_connected && _authorized) || _exception != null);

            MetaMaskUnity.Instance.Wallet.WalletConnected -= OnWalletConnected;
            MetaMaskUnity.Instance.Wallet.WalletAuthorized -= OnWalletAuthorized;

            MetamaskCanvas.SetActive(false);

            if (_exception != null)
            {
                MetaMaskUnity.Instance.Disconnect();
                throw _exception;
            }

            return MetaMaskUnity.Instance.Wallet.SelectedAddress;
        }

        public void Cancel()
        {
            _exception = new UnityException("User cancelled");
        }

        public void ShowQR(string url)
        {
            Debug.Log($"URI: {url}");

            var qrCodeAsTexture2D = GenerateQRTexture(url);
            QRCodeImage.sprite = Sprite.Create(qrCodeAsTexture2D, new Rect(0, 0, qrCodeAsTexture2D.width, qrCodeAsTexture2D.height), new Vector2(0.5f, 0.5f));
            DeepLinkButton.onClick.RemoveAllListeners();
            DeepLinkButton.onClick.AddListener(() => Application.OpenURL(url));
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
            var encoded = new Texture2D(256, 256);
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

        public void OnMetaMaskConnectRequest(string url)
        {
            ShowQR(url);
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
            return;
        }
    }
}
