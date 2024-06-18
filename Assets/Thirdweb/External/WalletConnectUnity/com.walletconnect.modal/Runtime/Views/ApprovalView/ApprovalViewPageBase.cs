using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Views
{
    public abstract class ApprovalViewPageBase : WCTabPage
    {
        [SerializeField] private Image _walletIconImage;
        [SerializeField] private Button _copyLinkButton;

        [SerializeField] private Sprite _fallbackWalletIconSprite;

        private RemoteSprite<Image> _walletIconRemoteSprite;

        protected string Uri { get; private set; }
        protected WCModal Modal { get; private set; }

        protected Wallet Wallet { get; private set; }

        public virtual async Task InitializeAsync(
            Wallet wallet,
            WCModal modal,
            RemoteSprite<Image> remoteWalletIcon,
            CancellationToken cancellationToken)
        {
            Modal = modal;
            Wallet = wallet;

            var connectedData = await WalletConnectModal.ConnectionController.GetConnectionDataAsync(cancellationToken);
            Uri = connectedData.Uri;

            if (remoteWalletIcon != null)
            {
                _walletIconRemoteSprite = remoteWalletIcon;
                _walletIconRemoteSprite.SubscribeImage(_walletIconImage);
            }
            else
            {
                _walletIconImage.sprite = _fallbackWalletIconSprite;
            }

            EnableCopyLink();
        }

        public override void Disable()
        {
            base.Disable();

            DisableCopyLink();

            _walletIconRemoteSprite?.UnsubscribeImage(_walletIconImage);
        }

        private void EnableCopyLink()
        {
            if (_copyLinkButton.gameObject.activeSelf) return;

            _copyLinkButton.gameObject.SetActive(true);
            _copyLinkButton.onClick.AddListener(OnCopyToClipboardClicked);
        }

        private void DisableCopyLink()
        {
            if (!_copyLinkButton.gameObject.activeSelf) return;

            _copyLinkButton.gameObject.SetActive(false);
            _copyLinkButton.onClick.RemoveListener(OnCopyToClipboardClicked);
        }

        private void OnCopyToClipboardClicked()
        {
            Modal.Header.Snackbar.Show(WCSnackbar.Type.Success, "Link copied");
            GUIUtility.systemCopyBuffer = Uri;
        }
    }
}