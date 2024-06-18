using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WalletConnectUnity.UI
{
    [AddComponentMenu("WalletConnect/UI/WC Button")]
    public class WCButton : Button
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _border;
        [SerializeField] private Image _ring;

        [Space, SerializeField] private Config _normalConfig;
        [SerializeField] private Config _highlightedConfig;
        [SerializeField] private Config _selectedConfig;
        [SerializeField] private Config _pressedConfig;

        private bool _isPointerInside;

        protected override void OnEnable()
        {
            base.OnEnable();
            ApplyConfig(in _normalConfig);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            // Unity automatically selects the button after pressed which we don't want when using a cursor
            var currentState = currentSelectionState;
            if (currentState is SelectionState.Highlighted) return;

            ApplyConfig(in _selectedConfig);

            base.OnSelect(eventData);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerInside = true;
            ApplyConfig(in _highlightedConfig);
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            _isPointerInside = false;
            ApplyConfig(in _normalConfig);
            base.OnPointerExit(eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            ApplyConfig(in _pressedConfig);
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            ApplyConfig(_isPointerInside ? _highlightedConfig : _normalConfig);
            base.OnPointerUp(eventData);
        }

        protected virtual void ApplyConfig(in Config config)
        {
            _background.color = config.backgroundColor;
            _border.color = config.borderColor;
            _ring.color = config.ringColor;
        }

        [Serializable]
        protected struct Config
        {
            public Color backgroundColor;
            public Color borderColor;
            public Color ringColor;
        }
    }
}