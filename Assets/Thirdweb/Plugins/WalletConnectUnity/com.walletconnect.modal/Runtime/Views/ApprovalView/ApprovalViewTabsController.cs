using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Views
{
    public class ApprovalViewTabsController : MonoBehaviour, ITabsController
    {
        [SerializeField] private WCModal _modal;
        [SerializeField] private WCTabsBar _tabsBar;
        [SerializeField] private RectTransform _rectTransform;

        [SerializeField] private ConnectionTypeToTabPageDictionary _connectionTypeToPageDictionary = new();

        private readonly List<WCTabPage> _pagesBuffer = new();
        private bool _isPageTransitionInProgress;
        private WCTabPage _currentPage;

        public event EventHandler<WCTabPage> PageSelected;

        public void Initialize()
        {
            _tabsBar.Initialize(this);
        }

        public void Enable(Wallet wallet)
        {
            _pagesBuffer.Clear();

            if (wallet == null)
            {
                if (_connectionTypeToPageDictionary.TryGetValue(ConnectionType.QRCode, out var qrCodePage))
                    _pagesBuffer.Add(qrCodePage);
            }
            else
            {
#if UNITY_IOS || UNITY_ANDROID
                if (wallet is { MobileLink: not null })
                {
                    if (_connectionTypeToPageDictionary.TryGetValue(ConnectionType.DeepLink, out var deepLinkPage))
                        _pagesBuffer.Add(deepLinkPage);
                }
#else
                if (_connectionTypeToPageDictionary.TryGetValue(ConnectionType.QRCode, out var qrCodePage))
                    _pagesBuffer.Add(qrCodePage);

                if (wallet.DesktopLink != null)
                    if (_connectionTypeToPageDictionary.TryGetValue(ConnectionType.DeepLink, out var deepLinkPage))
                        _pagesBuffer.Add(deepLinkPage);
#endif
                if (wallet is { WebappLink: not null })
                    if (_connectionTypeToPageDictionary.TryGetValue(ConnectionType.Webapp, out var webappPage))
                        _pagesBuffer.Add(webappPage);
            }

            if (_pagesBuffer.Count == 0)
            {
                throw new Exception($"Wallet {wallet?.Name ?? string.Empty} has no available connection types.");
            }

            if (_pagesBuffer.Count > 1)
            {
                _tabsBar.Enable(_pagesBuffer);
            }

            SelectPage(_pagesBuffer.First());
        }

        public void Disable()
        {
            _tabsBar.Disable();
        }

        public void SelectPage(WCTabPage page)
        {
            if (_isPageTransitionInProgress) return;

            _isPageTransitionInProgress = true;

            var pages = _connectionTypeToPageDictionary.Values;

            // Disable all pages
            foreach (var p in pages)
                p.PageTransform.gameObject.SetActive(false);

            _currentPage = page;

            // Update tabs bar
            PageSelected?.Invoke(this, page);

            // Resize modal and enable page
            ResizeModalToFitPage(page);
        }

        public void ResizeModalToFitPage()
        {
            if (_currentPage == null) return;
            ResizeModalToFitPage(_currentPage);
        }

        public void ResizeModalToFitPage(WCTabPage page)
        {
            StartCoroutine(ResizeModalAndEnablePageRoutine(page));
        }

        private IEnumerator ResizeModalAndEnablePageRoutine(WCTabPage page)
        {
            var newHeight = page.GetPageHeight();

            if (_tabsBar.RootTransform.gameObject.activeSelf)
                newHeight += _tabsBar.RootTransform.sizeDelta.y;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);

            yield return _modal.ResizeModalRoutine(newHeight);

            page.PageTransform.gameObject.SetActive(true);

            _isPageTransitionInProgress = false;
        }

        private enum ConnectionType
        {
            QRCode = 1,
            DeepLink,
            Webapp
        }

        [Serializable]
        private class ConnectionTypeToTabPageDictionary : SerializableDictionary<ConnectionType, WCTabPage>
        {
        }
    }
}