using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetaMask;
using MetaMask.Models;
using MetaMask.Transports;
using MetaMask.Transports.Unity;
using MetaMask.Unity;
using MetaMask.Unity.Models;
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
        public TMP_Text OTPHeaderText;

        public MetaMaskWallet Wallet => MetaMaskSDK.Instance.Wallet;

        public static MetamaskUI Instance;

        protected bool _connected;
        protected bool _authorized;
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

        // Core

        public virtual async Task<string> Connect(WalletConnection walletConnection)
        {
            OTPPanel.SetActive(false);
            MetaMaskWallet.Source = "Thirdweb";

            _connected = false;
            _authorized = false;
            _exception = null;

            await new WaitForSeconds(0.5f);

            MetamaskCanvas.SetActive(true);

            MetaMaskSDK.Instance.Events.WalletConnected += OnWalletConnected;
            MetaMaskSDK.Instance.Events.WalletAuthorized += OnWalletAuthorized;

            // setup config
            var appName = ThirdwebManager.Instance.SDK.Session.Options.wallet?.appName;
            var appUrl = ThirdwebManager.Instance.SDK.Session.Options.wallet?.appUrl;
            var currentChainData = ThirdwebManager.Instance.SDK.Session.CurrentChainData;
            var appIconUrl = ThirdwebManager.Instance.SDK.Session.Options.wallet?.appIcons[0];

            var sdk = MetaMaskSDK.SDKInstance;

            sdk.dappName = appName;
            sdk.dappIconUrl = appIconUrl;
            sdk.dappUrl = appUrl;
            sdk.debugLogging = ThirdwebManager.Instance.showDebugLogs;
            
            sdk.useDefaultChain = true;
            sdk.defaultChain = ChainId.Other;
            sdk.DefaultChainInfo = new ChainInfo()
            {
                BlockExplorerUrls = currentChainData.blockExplorerUrls,
                ChainId = currentChainData.chainId,
                ChainName = currentChainData.chainName,
                IconUrls = currentChainData.iconUrls,
                NativeCurrency = new Blockchains.NativeCurrency()
                {
                    Decimals = currentChainData.nativeCurrency.decimals,
                    Name = currentChainData.nativeCurrency.name,
                    Symbol = currentChainData.nativeCurrency.symbol
                },
                RpcUrls = currentChainData.rpcUrls
            };
            sdk.rpcUrl = new List<MetaMaskUnityRpcUrlConfig>(currentChainData.rpcUrls.Select(url =>
                new MetaMaskUnityRpcUrlConfig()
                {
                    ChainId = currentChainData.chainId,
                    RpcUrl = url
                }));
            
            // Connect() will now auto initialize SDK
            sdk.Connect();

            await new WaitUntil(() => (_connected && _authorized) || _exception != null);

            MetaMaskSDK.Instance.Events.WalletConnected -= OnWalletConnected;
            MetaMaskSDK.Instance.Events.WalletAuthorized -= OnWalletAuthorized;

            OTPPanel.SetActive(false);

            MetamaskCanvas.SetActive(false);

            if (_exception != null)
            {
                MetaMaskSDK.Instance.Disconnect(true);
                throw _exception;
            }

            return Wallet.SelectedAddress;
        }

        public virtual void Cancel()
        {
            _exception = new UnityException("User cancelled");
        }

        // QR

        public virtual void ShowQR(string universalLink, string deepLink)
        {
            var qrCodeAsTexture2D = GenerateQRTexture(universalLink);
            QRCodeImage.sprite = Sprite.Create(qrCodeAsTexture2D, new Rect(0, 0, qrCodeAsTexture2D.width, qrCodeAsTexture2D.height), new Vector2(0.5f, 0.5f));
            DeepLinkButton.onClick.RemoveAllListeners();
            DeepLinkButton.onClick.AddListener(() => Application.OpenURL(universalLink));
            QRCodeImage.mainTexture.filterMode = FilterMode.Point;
        }

        public virtual Texture2D GenerateQRTexture(string text)
        {
            Texture2D encoded = new Texture2D(256, 256);
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

        // Top level Event Listeners

        public virtual void OnWalletConnected(object sender, EventArgs e)
        {
            _connected = true;
        }

        public virtual void OnWalletAuthorized(object sender, EventArgs e)
        {
            _authorized = true;
        }

        // IMetaMaskUnityTransportListener

        public virtual void OnMetaMaskConnectRequest(string universalLink, string deepLink)
        {
            ShowQR(universalLink, deepLink);
        }

        public virtual void OnMetaMaskRequest(string id, MetaMaskEthereumRequest request)
        {
            return;
        }

        public virtual void OnMetaMaskFailure(Exception error)
        {
            _exception = error;
        }

        public virtual void OnMetaMaskSuccess()
        {
            _connected = true;
            _authorized = true;
        }

        public virtual void OnMetaMaskOTP(int otp)
        {
            OTPPanel.SetActive(true);

            var shouldShowOtpCode = DateTime.Now - Wallet.LastActive >= TimeSpan.FromHours(1);

            // They simply need to press resume in the app
            OTPText.gameObject.SetActive(shouldShowOtpCode);

            if (shouldShowOtpCode)
            {
                OTPText.text = otp.ToString();
            }
            else
            {
                OTPHeaderText.text = "Open the MetaMask app to continue with your session.";
            }
        }

        public virtual void OnMetaMaskDisconnected()
        {
            var isMobile = Wallet.Transport.ConnectionMode == TransportMode.Deeplink;
            if (!isMobile || !Wallet.HasSession)
            {
                _exception = new UnityException("User disconnected");
            }
        }
    }
}
