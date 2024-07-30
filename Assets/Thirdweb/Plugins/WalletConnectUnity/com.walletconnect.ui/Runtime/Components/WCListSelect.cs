using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WalletConnectUnity.UI
{
    public class WCListSelect : WCButton
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _iconBorder;
        [SerializeField] private GameObject _installedLabelObject;
        [SerializeField] private Color _defaultBorderColor;
        [SerializeField] private TMP_Text _tagText;

        private WCModalView _targetView;
        private object _targetViewParameters;
        private Action _onClick;
        private RemoteSprite<Image> _remoteSprite;

        private bool _initialized;

        protected override void Awake()
        {
            base.Awake();

            // Unfortunately, can't override `Press()` method because it's private
            onClick.AddListener(OnClick);
        }

        public void Initialize(in Params parameters)
        {
            if (_initialized)
            {
                ResetDefaults();
            }

            _title.text = parameters.title;
            _onClick = parameters.onClick;
            _remoteSprite = parameters.remoteSprite;

            _iconBorder.color = parameters.borderColor == default
                ? _defaultBorderColor
                : parameters.borderColor;

            if (!string.IsNullOrWhiteSpace(parameters.tagText))
            {
                _tagText.text = parameters.tagText;
                _tagText.gameObject.SetActive(true);
            }

            if (parameters.remoteSprite == null)
            {
                _icon.sprite = parameters.sprite;
                _icon.color = Color.white;
            }
            else
            {
                parameters.remoteSprite.SubscribeImage(_icon);
            }

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (parameters.isInstalled)
                EnableInstalledLabel();

            _initialized = true;
        }

        public void OnClick()
        {
            _onClick?.Invoke();
        }

        public void ResetDefaults()
        {
            _remoteSprite?.UnsubscribeImage(_icon);
            _icon.color = new Color(1, 1, 1, 0.1f);
            _title.text = string.Empty;
            _installedLabelObject.SetActive(false);
            _tagText.gameObject.SetActive(false);
        }

        private void EnableInstalledLabel()
        {
            _installedLabelObject.SetActive(true);
        }

        public struct Params
        {
            public RemoteSprite<Image> remoteSprite;
            public Sprite sprite; // used if remoteSprite is null
            public string title;
            public Action onClick;
            public Color borderColor;
            public bool isInstalled;
            public string tagText;
        }
    }
}