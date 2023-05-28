using System;
using System.Collections;

using MetaMask.Transports.Unity.UI;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetaMask.Unity.Samples
{
    public class VisualController : MonoBehaviour
    {
        public GameObject MetamaskCanvas;
        public Button ConnectButton;
        public GameObject welcomeScreen;
        public GameObject mainScreen;
        public Button DeeplinkButton;
        public TextMeshProUGUI HeaderText;
        public TextMeshProUGUI DescriptionText;
        public Sprite loadingSprite;
        public RawImage QRCodeImage;
        private bool scale;

        private void Start()
        {
            MetaMaskUnity.Instance.OnConnectionAttempted += ShowUI;
            MetaMaskUnity.Instance.OnDisconnectionAttempted += HideUI;
            ConnectButton.interactable = true;
            this.DeeplinkButton.interactable = Application.isMobilePlatform;
            DeeplinkButton.gameObject.SetActive(Application.isMobilePlatform);
        }

        private void ShowUI(object sender, EventArgs e)
        {
            OpenWelcomeScreen();
            MetaMaskUnity.Instance.Wallet.WalletAuthorized += OnWalletAuthorized;
            MetaMaskUnity.Instance.Wallet.WalletConnected += OnWalletConnected;
            MetaMaskUnity.Instance.Wallet.WalletDisconnected += OnWalletDisconnected;
            MetaMaskUnity.Instance.Wallet.WalletReady += OnWalletReady;
            MetamaskCanvas.SetActive(true);
        }

        private void HideUI(object sender, EventArgs e)
        {
            MetaMaskUnity.Instance.Wallet.WalletAuthorized -= OnWalletAuthorized;
            MetaMaskUnity.Instance.Wallet.WalletConnected -= OnWalletConnected;
            MetaMaskUnity.Instance.Wallet.WalletDisconnected -= OnWalletDisconnected;
            MetaMaskUnity.Instance.Wallet.WalletReady -= OnWalletReady;
            MetamaskCanvas.SetActive(false);
        }

        public void OpenWelcomeScreen()
        {
            DeeplinkButton.gameObject.SetActive(Application.isMobilePlatform);
            this.welcomeScreen.SetActive(true);
            this.mainScreen.SetActive(false);
        }

        private void OnWalletAuthorized(object sender, EventArgs e)
        {
            HideUI(null, null);
        }

        private void OnWalletReady(object sender, EventArgs e)
        {
            WalletStartVisuals();
        }

        public void OpenMainScreen()
        {
            this.welcomeScreen.SetActive(false);
            this.mainScreen.SetActive(true);
        }

        private void OnWalletConnected(object sender, EventArgs e)
        {
            WalletStartVisuals();
        }

        private void OnWalletDisconnected(object sender, EventArgs e)
        {
            WalletStopVisuals();
        }

        private void WalletStartVisuals()
        {
            this.HeaderText.text = "Wallet Ready";
            this.DescriptionText.text = "";
        }

        private void WalletStopVisuals()
        {
            this.HeaderText.text = "Connect Wallet";
            this.DescriptionText.text = "Scan the QR in your MetaMask app";
        }
    }
}
