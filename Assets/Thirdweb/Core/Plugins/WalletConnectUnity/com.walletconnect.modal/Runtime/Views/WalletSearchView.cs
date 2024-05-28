using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Views
{
    public class WalletSearchView : WCModalView
    {
        [Header("Scene References")] [SerializeField]
        private ApprovalView _approvalView;

        [SerializeField] private RectTransform _parent;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private WCButton _qrCodeButton;
        [SerializeField] private GameObject _noWalletFound;
        [SerializeField] private WCInputField _searchInputField;
        [SerializeField] private List<WCListSelect> _cardsPool = new();

        [Header("Asset References")] [SerializeField]
        private WCListSelect _cardPrefab;

        [SerializeField] private Sprite _walletConnectLogo;

        [Header("Configuration")] [SerializeField, Range(0.01f, 0.9f)]
        private float _loadThreshold = 0.5f;

        [SerializeField] private int _countPerPage = 12;

        private readonly Dictionary<string, RemoteSprite<Image>> _sprites = new();

        private int _countPerPageRealtime = 0;
        private int _usedCardsCount = 0;
        private bool _isPageLoading = false;
        private int _nextPageToLoad = 1;
        private bool _reachedMaxWalletsCount = false;

        private string _searchQuery = null;

        private int _maxWalletsCount = -1;

        protected override void Awake()
        {
            base.Awake();

#if UNITY_IOS || UNITY_ANDROID
            _qrCodeButton.gameObject.SetActive(true);
            _qrCodeButton.onClick.AddListener(() => parentModal.OpenView(_approvalView,
                modal: parentModal,
                new ApprovalView.Params())
            );
#endif

            _searchInputField.onValueChanged.AddListener(OnSearch);
        }

        public override void Show(WCModal modal, IEnumerator effectCoroutine, object options = null)
        {
            base.Show(modal, effectCoroutine, options);

            _countPerPageRealtime = _countPerPage;

            StartCoroutine(LoadNextPage());
        }

        public override void Hide()
        {
            base.Hide();

            StopAllCoroutines();

            for (int i = _cardsPool.Count - 1; i >= 0; i--)
            {
                WCListSelect card = _cardsPool[i];
                if (card)
                {
                    card.ResetDefaults();
                    card.gameObject.SetActive(false);
                }
                else
                {
                    _cardsPool.RemoveAt(i);
                }
            }

            _usedCardsCount = 0;
            _nextPageToLoad = 1;
            _isPageLoading = false;
            _reachedMaxWalletsCount = false;

            _searchInputField.text = string.Empty;
            _searchQuery = null;
        }

        public void OnSearch(string search)
        {
            if (string.IsNullOrWhiteSpace(search) || search.Length == 1)
            {
                _searchQuery = null;
            }
            else
            {
                _searchQuery = search.Trim();
            }

            StopAllCoroutines();

            _maxWalletsCount = -1;
            _usedCardsCount = 0;
            _nextPageToLoad = 1;
            _isPageLoading = false;
            _reachedMaxWalletsCount = false;
            _countPerPageRealtime = _countPerPage;

            for (int i = _cardsPool.Count - 1; i >= 0; i--)
            {
                WCListSelect card = _cardsPool[i];
                if (card)
                {
                    card.ResetDefaults();
                    card.gameObject.SetActive(false);
                }
                else
                {
                    _cardsPool.RemoveAt(i);
                }
            }

            StartCoroutine(LoadNextPage());
        }

        private void FixedUpdate()
        {
            if (IsActive &&
                !_isPageLoading &&
                !_reachedMaxWalletsCount &&
                _scrollRect.verticalNormalizedPosition < _loadThreshold)
                StartCoroutine(LoadNextPage());
        }

        private IEnumerator LoadNextPage()
        {
            _isPageLoading = true;

            if (_maxWalletsCount != -1)
            {
                if (_nextPageToLoad * _countPerPageRealtime > _maxWalletsCount)
                {
                    _countPerPageRealtime = _maxWalletsCount - _usedCardsCount;
                    _reachedMaxWalletsCount = true;
                }
            }

            using var uwr =
                WalletConnectModal.WalletsRequestsFactory.GetWallets(
                    _nextPageToLoad,
                    _countPerPageRealtime,
                    _searchQuery);

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[WalletConnectUnity] Failed to get wallets: {uwr.error}", this);
                yield break;
            }

            var response = JsonConvert.DeserializeObject<GetWalletsResponse>(uwr.downloadHandler.text);

            _noWalletFound.SetActive(response.Count == 0);

            if (_maxWalletsCount == -1)
            {
                _maxWalletsCount = response.Count;

                if (_nextPageToLoad * _countPerPageRealtime > _maxWalletsCount)
                {
                    _countPerPageRealtime = _maxWalletsCount - _usedCardsCount;
                    _reachedMaxWalletsCount = true;
                }
            }

            var walletsCount = response.Data.Length;

            if (walletsCount > _cardsPool.Count - _usedCardsCount)
                yield return IncreaseCardsPoolSize(walletsCount + _usedCardsCount);

            for (var i = 0; i < walletsCount; i++)
            {
                var wallet = response.Data[i];
                var card = _cardsPool[i + _usedCardsCount];
                var sprite = GetSprite(wallet.ImageId);

                card.Initialize(new WCListSelect.Params
                {
                    title = wallet.Name,
                    remoteSprite = sprite,
                    onClick = () =>
                    {
                        parentModal.OpenView(_approvalView, parameters: new ApprovalView.Params
                        {
                            walletIconRemoteSprite = sprite,
                            walletData = wallet,
                        });
                    },
                    isInstalled = WalletUtils.IsWalletInstalled(wallet)
                });
            }

            _usedCardsCount += walletsCount;
            _nextPageToLoad++;

            _isPageLoading = false;
        }

        private RemoteSprite<Image> GetSprite(string walletImageId)
        {
            if (_sprites.TryGetValue(walletImageId, out var sprite))
                return sprite;

            sprite = RemoteSpriteFactory.GetRemoteSprite<Image>($"https://api.web3modal.com/getWalletImage/{walletImageId}");
            _sprites.Add(walletImageId, sprite);
            return sprite;
        }

        private IEnumerator IncreaseCardsPoolSize(int newSize)
        {
            if (newSize <= _cardsPool.Count)
                throw new ArgumentException("New size must be greater than current size");

            var oldSize = _cardsPool.Count;
            _cardsPool.AddRange(new WCListSelect[newSize - oldSize]);

            for (var i = oldSize; i < newSize; i++)
            {
                var card = Instantiate(_cardPrefab, _parent);
                _cardsPool[i] = card;

                // After every 3 new cards, wait for a frame to reduce lag
                if ((i - oldSize + 1) % 3 == 0)
                    yield return null;
            }
        }
    }
}