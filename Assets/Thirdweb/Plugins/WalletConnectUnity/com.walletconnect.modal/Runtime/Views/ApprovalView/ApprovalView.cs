using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Views
{
    public class ApprovalView : WCModalView
    {
        [Space, SerializeField]
        private ApprovalViewTabsController _tabsController;

        [SerializeField]
        private ScrollRect _scrollRect;

        [Space, SerializeField]
        private ApprovalViewPageBase _qrCodePage;

        [SerializeField]
        private ApprovalViewPageBase _deepLinkPage;

        [SerializeField]
        private ApprovalViewPageBase _webAppPage;

        private CancellationTokenSource _viewCts;
        private Wallet _walletData;

        protected override void Awake()
        {
            _tabsController.Initialize();

#pragma warning disable 618
            WalletConnectModal.Connected += OnConnected;
#pragma warning restore 618

            base.Awake();
        }

        public override async void Show(WCModal modal, IEnumerator effectCoroutine, object options = null)
        {
            var parameters = (Params)options;

            Assert.IsNotNull(parameters, "QRCodeView options is not Params type or null.");

            _walletData = parameters.walletData;
            _viewCts = new CancellationTokenSource();

            if (parameters.walletData == null)
            {
                await _qrCodePage.InitializeAsync(parameters.walletData, modal, parameters.walletIconRemoteSprite, _viewCts.Token);
            }
            else
            {
                await Task.WhenAll(
                    _qrCodePage.InitializeAsync(parameters.walletData, modal, parameters.walletIconRemoteSprite, _viewCts.Token),
                    _deepLinkPage.InitializeAsync(parameters.walletData, modal, parameters.walletIconRemoteSprite, _viewCts.Token),
                    _webAppPage.InitializeAsync(parameters.walletData, modal, parameters.walletIconRemoteSprite, _viewCts.Token)
                );
            }

            _tabsController.Enable(parameters.walletData);

            base.Show(modal, effectCoroutine, options);

            await WaitForUserConnectionAsync(_viewCts.Token);
        }

        public override void Hide()
        {
            base.Hide();

            _viewCts?.Cancel();

            _qrCodePage.Disable();
            _deepLinkPage.Disable();
            _tabsController.Disable();
        }

        public override string GetTitle()
        {
            return _walletData == null ? base.GetTitle() : _walletData.Name;
        }

        protected override void ApplyScreenOrientation(ScreenOrientation orientation)
        {
            base.ApplyScreenOrientation(orientation);

            // Because QR Code page can influence the modal height, we need to recalculate it after orientation change
            _tabsController.ResizeModalToFitPage();

            _scrollRect.content.anchoredPosition = Vector2.zero;
            _scrollRect.enabled = orientation is ScreenOrientation.LandscapeLeft or ScreenOrientation.LandscapeRight;
        }

        private async Task WaitForUserConnectionAsync(CancellationToken cancellationToken)
        {
            var connectedData = await WalletConnectModal.ConnectionController.GetConnectionDataAsync(cancellationToken);

            try
            {
                // Wait for the approval task or cancellation (if the QRCodeView is closed)
                await Task.WhenAny(connectedData.Approval, Task.Delay(Timeout.Infinite, cancellationToken));

                if (_viewCts.Token.IsCancellationRequested)
                    return;

                if (connectedData.Approval.IsCompletedSuccessfully)
                {
                    _ = await connectedData.Approval;
                }
            }
            catch (OperationCanceledException) { }
        }

        private void OnConnected(object sender, EventArgs e)
        {
            WalletUtils.SetRecentWallet(_walletData);
        }

        public class Params
        {
            public RemoteSprite<Image> walletIconRemoteSprite;
            public Wallet walletData;
        }
    }
}
