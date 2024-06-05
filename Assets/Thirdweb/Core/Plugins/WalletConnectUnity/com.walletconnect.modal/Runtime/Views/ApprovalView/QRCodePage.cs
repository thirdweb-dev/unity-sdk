using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;
using DeviceType = WalletConnectUnity.Core.Utils.DeviceType;

namespace WalletConnectUnity.Modal.Views
{
    public class QRCodePage : ApprovalViewPageBase
    {
        [SerializeField]
        private RectTransform _viewRoot;

        [Space]
        [SerializeField]
        private GameObject _walletIconRoot;

        [SerializeField]
        private GameObject _fallbackWalletIconRoot;

        [Space]
        [SerializeField]
        private RectTransform _qrCodeRoot;

        [SerializeField]
        private RawImage _qrCodeRawImage;

        [SerializeField]
        private float _qrCodePadding = 20f;

        public override async Task InitializeAsync(Wallet wallet, WCModal modal, RemoteSprite<Image> remoteWalletIcon, CancellationToken cancellationToken)
        {
            if (DeviceUtils.GetDeviceType() is DeviceType.Phone)
            {
                // On phones don't show any icons above the QR code may be too small
                _walletIconRoot.SetActive(false);
                _fallbackWalletIconRoot.SetActive(false);
            }
            else
            {
                var validWallet = remoteWalletIcon != null;
                _walletIconRoot.SetActive(validWallet);
                _fallbackWalletIconRoot.SetActive(!validWallet);
            }

            await base.InitializeAsync(wallet, modal, remoteWalletIcon, cancellationToken);
            var texture = QRCode.EncodeTexture(Uri);

            WCLoadingAnimator.Instance.Unsubscribe(_qrCodeRawImage);

            _qrCodeRawImage.texture = texture;
        }

        public override float GetPageHeight()
        {
            ResizeQrCode();
            return base.GetPageHeight();
        }

        public void ResizeQrCode()
        {
            var oldQrCodeHeight = _qrCodeRoot.sizeDelta.y;
            Vector2 newSizeDelta;

            if (DeviceUtils.GetDeviceType() is DeviceType.Phone && Screen.orientation is ScreenOrientation.LandscapeLeft or ScreenOrientation.LandscapeRight)
            {
                var screenHeight = Modal.RootRectTransform.sizeDelta.y;
                var workingHeight = screenHeight * Modal.MobileMaxHeightPercent - Modal.Header.RectTransform.sizeDelta.y;

                var qrCodeSize = workingHeight - _qrCodePadding * 2;
                qrCodeSize *= 0.85f; // leave some space for the other elements to hint at scrolling
                newSizeDelta = new Vector2(qrCodeSize, qrCodeSize);
            }
            else
            {
                // Stretch the QR code to the full width of the view, but keep the aspect ratio and padding
                var qrCodeSize = _viewRoot.rect.width - _qrCodePadding * 2;
                newSizeDelta = new Vector2(qrCodeSize, qrCodeSize);
            }

            _qrCodeRoot.sizeDelta = newSizeDelta;

            // Resize the container to fit the QR code
            var heightDelta = newSizeDelta.y - oldQrCodeHeight;
            (transform as RectTransform)!.sizeDelta += new Vector2(0, heightDelta);
        }

        public override void Disable()
        {
            base.Disable();
            _qrCodeRawImage.texture = null;
        }
    }
}
