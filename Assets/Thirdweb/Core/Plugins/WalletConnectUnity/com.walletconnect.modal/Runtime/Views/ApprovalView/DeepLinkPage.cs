using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Views
{
    public class DeepLinkPage : ApprovalViewPageBase
    {
        [Header("DeepLink Page")]
        [SerializeField] private TMP_Text _titleText;

        [SerializeField] private string _titleTextFormat = "Continue in {0}";
        [SerializeField] private TMP_Text _dontHaveWalletText;
        [SerializeField] private string _dontHaveWalletTextFormat = "Don't have {0}?";
        [SerializeField] private GameObject _tryAgainButton;

        [Header("Loader")]
        [SerializeField] private float _radiansPerSecond = 1f;

        [SerializeField] private RectTransform _loadingSector;

        public override async Task InitializeAsync(
            Wallet wallet,
            WCModal modal,
            RemoteSprite<Image> remoteWalletIcon,
            CancellationToken cancellationToken)
        {
            _titleText.text = string.Format(_titleTextFormat, wallet.Name);
            _dontHaveWalletText.text = string.Format(_dontHaveWalletTextFormat, wallet.Name);
            await base.InitializeAsync(wallet, modal, remoteWalletIcon, cancellationToken);

            _tryAgainButton.SetActive(true);
        }

        // Called by Try Again Button onClick Unity event
        public void OnTryAgainButtonClicked()
        {
            StartCoroutine(OpenSessionProposalDeepLinkRoutine());
        }

        // Called by Get Button onClick Unity event
        public void OnGetWallet()
        {
#if UNITY_IOS
            Application.OpenURL(Wallet.AppStore);
#elif UNITY_ANDROID
            Application.OpenURL(Wallet.PlayStore);
#else
            Application.OpenURL(Wallet.Homepage);
#endif
        }

        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(Uri)) return;

            StartCoroutine(OpenSessionProposalDeepLinkRoutine());
            StartCoroutine(SectorRotationRoutine());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _loadingSector.gameObject.SetActive(false);
        }

        public override void Disable()
        {
            base.Disable();
            _tryAgainButton.SetActive(false);
        }

        private IEnumerator OpenSessionProposalDeepLinkRoutine()
        {
            // Skip one frame to not block the UI rendering
            yield return null;
            Linker.OpenSessionProposalDeepLink(Uri, Wallet);
        }

        private IEnumerator SectorRotationRoutine()
        {
            _loadingSector.gameObject.SetActive(true);
            while (true)
            {
                _loadingSector.Rotate(0f, 0f, _radiansPerSecond * Time.deltaTime);
                yield return null;
            }
        }
    }
}