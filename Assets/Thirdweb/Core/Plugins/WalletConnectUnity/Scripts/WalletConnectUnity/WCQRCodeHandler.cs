using System;
using System.Collections;
using QRCoder;
using QRCoder.Unity;
using WalletConnect.UnityBinder;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectUnity.Utils;

namespace WalletConnect
{
    public class WCQRCodeHandler : BindableMonoBehavior
    {
        [Inject]
        private WCSignClient _signClient;

        /// <summary>
        /// The image component we'll place the QR code texture into.
        /// </summary>
        public Image QRCodeImage;

        public GameObject loader;

        [Serializable]
        public class SignClientConnectEvent : UnityEvent { }

        [Serializable]
        public class SignClientConnectEventArgs : UnityEvent<ConnectedData> { }

        [Serializable]
        public class SignClientAuthorizedEvent : UnityEvent { }

        [Serializable]
        public class SignClientAuthorizedEventArgs : UnityEvent<SessionStruct> { }

        [SerializeField]
        private SignClientConnectEvent onSignClientReady = new SignClientConnectEvent();

        [SerializeField]
        private SignClientConnectEventArgs onSignClientReadyWithArgs = new SignClientConnectEventArgs();

        [SerializeField]
        private SignClientAuthorizedEvent onSignClientAuthorized = new SignClientAuthorizedEvent();

        [SerializeField]
        private SignClientAuthorizedEventArgs onSignClientAuthorizedWithArgs = new SignClientAuthorizedEventArgs();

        private ConnectedData currentConnectData;

        protected override void Awake()
        {
            base.Awake();

            _signClient.OnConnect += SignClientOnOnConnect;
            _signClient.OnSessionApproved += SignClientOnOnSessionApproved;
        }

        private void SignClientOnOnSessionApproved(object sender, SessionStruct e)
        {
            if (WalletConnect.WalletConnectUnity.Instance.UseDeeplink)
                return;

            // TODO Perhaps ensure we are using Unity's Sync context inside WalletConnectSharp
            MTQ.Enqueue(() =>
            {
                // Trigger the Unity events
                onSignClientAuthorized.Invoke();
                onSignClientAuthorizedWithArgs.Invoke(e);
            });
        }

        private void SignClientOnOnConnect(object sender, ConnectedData e)
        {
            if (WalletConnect.WalletConnectUnity.Instance.UseDeeplink)
                return;

            if (loader == null)
            {
                GenerateQrCode(e);
            }
            else
            {
                StartCoroutine(ShowLoader(e));
            }
            TriggerEvents(e);
        }

        private void TriggerEvents(ConnectedData e)
        {
            onSignClientReady.Invoke();
            onSignClientReadyWithArgs.Invoke(e);
        }

        private IEnumerator ShowLoader(ConnectedData data)
        {
            GenerateQrCode(data);
            //hide the QRcode, show the loader, then wait a sec and then do the inverse
            QRCodeImage.enabled = false;
            loader.SetActive(true);
            yield return new WaitForSeconds(1);
            QRCodeImage.enabled = true;
            loader.SetActive(false);
        }

        private void GenerateQrCode(ConnectedData data)
        {
            // Grab the WC URL and generate a QR code for it. Note: The ECCLevel is the "Error Correction Code" level which
            // is basically how much checksum data to add to the code - the more checksum data the more likely the code can
            // be recovered on a slightly dodgy read. We'll go with the UnityWalletConnect default of Q(uality) as it's a
            // good compromise between readability and data storage capacity.
            // See: https://www.qrcode.com/en/about/version.html
            var url = data.Uri;
            Debug.Log("Connecting to: " + url);
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            UnityQRCode qrCode = new UnityQRCode(qrCodeData);

            // Create the QR code as a Texture2D. Note: "pixelsPerModule" means the size of each black-or-white block in the
            // QR code image. For example, a size of 2 will give us a 138x138 image (too small!), while 20 will give us a
            // 1380x1380 image (too big!). Here we'll use a value of 10 which gives us a 690x690 pixel image.
            Texture2D qrCodeAsTexture2D = qrCode.GetGraphic(pixelsPerModule: 10);

            // Change the filtering mode to point (i.e. nearest) rather than the default of linear - we want sharp edges on
            // the blocks, not blurry interpolated edges!
            qrCodeAsTexture2D.filterMode = FilterMode.Point;

            // Convert the texture into a sprite and assign it to our QR code image
            var qrCodeSprite = Sprite.Create(qrCodeAsTexture2D, new Rect(0, 0, qrCodeAsTexture2D.width, qrCodeAsTexture2D.height), new Vector2(0.5f, 0.5f), 100f);
            QRCodeImage.sprite = qrCodeSprite;
            currentConnectData = data;
        }

        public void CopyURI()
        {
            // Copy the URL to the clipboard to allow for manual connection in wallet apps that support it
            GUIUtility.systemCopyBuffer = currentConnectData.Uri;
        }
    }
}
