using System;

using MetaMask.Models;

using UnityEngine;

namespace MetaMask.Transports.Unity.UI
{

    public class MetaMaskUnityUIHandler : MonoBehaviour, IMetaMaskUnityTransportListener
    {

        #region Events

        /// <summary>Occurs when the application's open state changes.</summary>
        public event EventHandler OpenStateChanged;

        #endregion

        #region Fields

        /// <summary>The CanvasGroup component attached to the root GameObject.</summary>
        [SerializeField]
        protected CanvasGroup canvasGroup;
        /// <summary>Gets or sets a value indicating whether the dropdown is open.</summary>
        /// <value>true if the dropdown is open; otherwise, false.</value>
        [SerializeField]
        protected bool isOpen = false;

        /// <summary>The QR image.</summary>
        [SerializeField]
        protected MetaMaskUnityUIQRImage qrImage;

        #endregion

        #region Properties

        /// <summary>Gets a value indicating whether the UI is open.</summary>
        /// <returns>true if the UI is open; otherwise, false.</returns>
        public virtual bool IsOpen => this.isOpen;

        #endregion

        #region Protected Methods
        /// <summary>Returns whether a UI is opened.</summary>
        protected virtual void OnOpenStateChanged()
        {
            this.canvasGroup.interactable = this.isOpen;
            this.canvasGroup.blocksRaycasts = this.isOpen;
            this.canvasGroup.alpha = this.isOpen ? 1f : 0f;

            OpenStateChanged?.Invoke(this, null);
        }

        #endregion

        #region Public Methods

        /// <summary>Opens the clipboard.</summary>
        public virtual void Open()
        {
            SetOpen(true);
        }

        /// <summary>Closes the window.</summary>
        public virtual void Close()
        {
            SetOpen(false);
        }

        /// <summary>Toggles the open state of the menu.</summary>
        public virtual void ToggleOpen()
        {
            SetOpen(!this.isOpen);
        }

        /// <summary>Sets the open state of the menu.</summary>
        /// <param name="open">Whether the menu is open.</param>
        public void SetOpen(bool open)
        {
            if (this.isOpen == open)
            {
                return;
            }
            this.isOpen = open;
            OnOpenStateChanged();
        }

        /// <summary>Called when the MetaMask client wants to connect to the application.</summary>
        /// <param name="url">The URL to connect to.</param>
        public void OnMetaMaskConnectRequest(string url)
        {
            this.qrImage.ShowQR(url);
        }

        /// <summary>Handles a MetaMask request.</summary>
        /// <param name="id">The request ID.</param>
        /// <param name="request">The request.</param>
        public void OnMetaMaskRequest(string id, MetaMaskEthereumRequest request)
        {
        }

        /// <summary>Called when the MetaMask client encounters an error.</summary>
        /// <param name="error">The error that occurred.</param>
        public void OnMetaMaskFailure(Exception error)
        {
        }

        /// <summary>Called when the MetaMask login was successful.</summary>
        public void OnMetaMaskSuccess()
        {
        }

        #endregion

    }

}