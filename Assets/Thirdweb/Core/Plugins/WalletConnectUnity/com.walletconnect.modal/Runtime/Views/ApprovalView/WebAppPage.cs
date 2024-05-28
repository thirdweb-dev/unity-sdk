using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Views
{
    public class WebAppPage : ApprovalViewPageBase
    {
        [Header("WebApp Page")]
        [SerializeField] private TMP_Text _titleText;

        [SerializeField] private string _titleTextFormat = "Continue in {0}";
        [SerializeField] private GameObject _openButton;

        public override async Task InitializeAsync(Wallet wallet, WCModal modal, RemoteSprite<Image> remoteWalletIcon,
            CancellationToken cancellationToken)
        {
            _titleText.text = string.Format(_titleTextFormat, wallet.Name);
            await base.InitializeAsync(wallet, modal, remoteWalletIcon, cancellationToken);
            _openButton.SetActive(true);
        }

        public override void Disable()
        {
            base.Disable();
            _openButton.SetActive(false);
        }

        public void OnOpen()
        {
            Application.OpenURL(Path.Combine(Wallet.WebappLink, $"wc?uri={Uri}"));
        }
    }
}