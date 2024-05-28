using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace WalletConnectUnity.UI
{
    public sealed class WCModalHeader : MonoBehaviour
    {
        [field: SerializeField] public WCSnackbar Snackbar { get; private set; }

        [field: SerializeField] public RectTransform RectTransform { get; private set; }

        [field: SerializeField] private TMP_Text TitleText { get; set; }

        [field: SerializeField] private Button LeftButton { get; set; }

        [field: SerializeField] private Image LeftButtonImage { get; set; }

        [field: SerializeField] private Button RightButton { get; set; }

        [field: SerializeField, Space] private WCModal Modal { get; set; }

        public float Height => RectTransform.rect.height;

        private bool _leftButtonCustom;
        private Action _leftButtonAction;
        private Sprite _leftButtonDefaultSprite;

        public string Title
        {
            get => TitleText.text;
            set => TitleText.text = value;
        }

        private void Awake()
        {
            Assert.IsNotNull(RectTransform, $"Missing {nameof(RectTransform)} reference in {name}");
            Assert.IsNotNull(TitleText, $"Missing {nameof(TitleText)} reference in {name}");
            Assert.IsNotNull(LeftButton, $"Missing {nameof(LeftButton)} reference in {name}");
            Assert.IsNotNull(RightButton, $"Missing {nameof(RightButton)} reference in {name}");

            Assert.IsNotNull(Modal, $"Missing {nameof(Modal)} reference in {name}");

            LeftButton.onClick.AddListener(OnLeftButtonClicked);
            RightButton.onClick.AddListener(OnRightButtonClicked);

            _leftButtonDefaultSprite = LeftButtonImage.sprite;
        }

        public void SetCustomLeftButton(Sprite sprite, Action onClick)
        {
            _leftButtonCustom = true;
            _leftButtonAction = onClick;

            LeftButtonImage.sprite = sprite;

            if (LeftButton.gameObject.activeSelf == false)
                LeftButton.gameObject.SetActive(true);
        }

        public void RemoveCustomLeftButton()
        {
            _leftButtonCustom = false;
            _leftButtonAction = null;

            LeftButtonImage.sprite = _leftButtonDefaultSprite;
        }

        private void OnLeftButtonClicked()
        {
            if (_leftButtonCustom)
            {
                _leftButtonAction?.Invoke();
            }
            else
            {
                Modal.CloseView();
            }
        }

        private void OnRightButtonClicked()
        {
            Modal.CloseModal();
        }
    }
}