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
        private Coroutine visualCoroutine;

        private async void Start()
        {
            ConnectButton.interactable = false;
            MetaMaskUnity.Instance.OnConnectionAttempted += ShowUI;
            MetaMaskUnity.Instance.OnDisconnectionAttempted += HideUI;

            await new WaitForSecondsRealtime(4f);
            if (Application.isPlaying)
                ConnectButton.interactable = true;
        }

        // Start is called before the first frame update
        private void ShowUI(object sender, EventArgs e)
        {
            MetamaskCanvas.SetActive(true);
            MetaMaskUnity.Instance.Wallet.WalletAuthorized += OnWalletAuthorized;
            MetaMaskUnity.Instance.Wallet.WalletConnected += OnWalletConnected;
            MetaMaskUnity.Instance.Wallet.WalletDisconnected += OnWalletDisconnected;
            MetaMaskUnity.Instance.Wallet.WalletPaused += OnWalletPaused;
            MetaMaskUnity.Instance.Wallet.WalletReady += OnWalletReady;
            if (Application.isMobilePlatform && MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable())
            {
                MetaMaskUnity.Instance.Wallet.WalletConnected += ConnectedFeedback;
            }
        }

        private void HideUI(object sender, EventArgs e)
        {
            MetamaskCanvas.SetActive(false);

            MetaMaskUnity.Instance.Wallet.WalletAuthorized -= OnWalletAuthorized;
            MetaMaskUnity.Instance.Wallet.WalletConnected -= OnWalletConnected;
            MetaMaskUnity.Instance.Wallet.WalletDisconnected -= OnWalletDisconnected;
            MetaMaskUnity.Instance.Wallet.WalletPaused -= OnWalletPaused;
            MetaMaskUnity.Instance.Wallet.WalletReady -= OnWalletReady;
            if (Application.isMobilePlatform && MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable())
            {
                MetaMaskUnity.Instance.Wallet.WalletConnected -= ConnectedFeedback;
            }
        }

        private void OnWalletAuthorized(object sender, EventArgs e)
        {
            MetamaskCanvas.SetActive(false);
        }

        private void OnWalletReady(object sender, EventArgs e)
        {
            WalletStartVisuals();
        }

        private void OnWalletPaused(object sender, EventArgs e)
        {
            if (
                Application.platform == RuntimePlatform.Android && MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable()
                || Application.platform == RuntimePlatform.IPhonePlayer && MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable()
            )
            {
                MetaMaskUnity.Instance.Wallet.Dispose();
                OpenWelcomeScreen();
            }
            WalletStopVisuals();
        }

        public void OpenMainScreen()
        {
            StopAllCoroutines();
            if (
                Application.platform == RuntimePlatform.Android && MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable()
                || Application.platform == RuntimePlatform.IPhonePlayer && MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable()
            )
            {
                DeeplinkButton.gameObject.SetActive(true);
                visualCoroutine = StartCoroutine(DeeplinkFeedback());
            }
            else
            {
                DeeplinkButton.gameObject.SetActive(false);
            }
            this.welcomeScreen.SetActive(false);
            this.mainScreen.SetActive(true);
        }

        public void OpenWelcomeScreen()
        {
            if (
                Application.platform == RuntimePlatform.Android && MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable()
                || Application.platform == RuntimePlatform.IPhonePlayer && MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable()
            )
            {
                DeeplinkButton.gameObject.SetActive(true);
            }
            else
            {
                DeeplinkButton.gameObject.SetActive(false);
            }
            this.welcomeScreen.SetActive(true);
            this.mainScreen.SetActive(false);
        }

        private void OnWalletConnected(object sender, EventArgs e)
        {
            WalletStartVisuals();
        }

        private void OnWalletDisconnected(object sender, EventArgs e)
        {
            WalletStopVisuals();
            OpenWelcomeScreen();
        }

        private void WalletStartVisuals()
        {
            this.HeaderText.text = "Wallet Ready";
            this.DescriptionText.text = "";
            if (
                MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable() && Application.platform == RuntimePlatform.Android
                || MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable() && Application.platform == RuntimePlatform.IPhonePlayer
            )
            {
                this.DeeplinkButton.interactable = true;
            }
            else
            {
                this.DeeplinkButton.gameObject.SetActive(false);
            }
        }

        private void WalletStopVisuals()
        {
            this.HeaderText.text = "Connect Wallet";
            this.DescriptionText.text = "Scan the QR in your MetaMask app";
            if (
                MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable() && Application.platform == RuntimePlatform.Android
                || MetaMaskUnityUITransport.DefaultInstance.IsDeeplinkAvailable() && Application.platform == RuntimePlatform.IPhonePlayer
            )
            {
                this.DeeplinkButton.interactable = false;
            }
            else
            {
                this.DeeplinkButton.gameObject.SetActive(false);
            }
        }

        private void OnSignSend(object sender, EventArgs e)
        {
            ModalData modalData = new ModalData();
            modalData.headerText = "Sign Sent";
            modalData.bodyText = "Sign has been sent to your wallet, please ensure you have the application open on your device";
            UIModalManager.Instance.OpenModal(modalData);
        }

        private void OnTransactionSent(object sender, EventArgs e)
        {
            ModalData modalData = new ModalData();
            modalData.headerText = "Transaction Sent";
            modalData.bodyText = "Transaction Sent has been sent to your wallet, please ensure you have the application open on your device";
            UIModalManager.Instance.OpenModal(modalData);
        }

        private void OnTransactionResult(object sender, MetaMaskEthereumRequestResultEventArgs e)
        {
            ModalData modalData = new ModalData();
            modalData.type = ModalData.ModalType.Transaction;
            modalData.headerText = "Result Received";
            modalData.bodyText = string.Format("<b>Method Name:</b><br> {0} <br> <br> <b>Transaction Details:</b><br>{1}", e.Request.Method, e.Result.ToString());
            UIModalManager.Instance.OpenModal(modalData);
        }

        #region Coroutine

        private void ConnectedFeedback(object sender, EventArgs e)
        {
            StopAllCoroutines();
        }

        private IEnumerator DeeplinkFeedback()
        {
            yield return new WaitForSeconds(4f);
            OpenWelcomeScreen();
        }

        #endregion
    }
}
