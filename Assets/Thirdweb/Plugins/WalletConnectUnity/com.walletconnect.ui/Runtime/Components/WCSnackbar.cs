using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectUnity.Core.Utils;

namespace WalletConnectUnity.UI
{
    public class WCSnackbar : MonoBehaviour
    {
        [Header("Scene References"), SerializeField]
        private Image _iconBackground;

        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Settings"), SerializeField] private SerializableDictionary<Type, Config> _configs = new();

        private readonly WaitForSeconds _wait = new(3f);
        private Coroutine _coroutine;

        public void Show(Type type, string text)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
                _canvasGroup.alpha = 0;
            }

            _coroutine = StartCoroutine(ShowCoroutine(type, text));
        }

        private IEnumerator ShowCoroutine(Type type, string text)
        {
            var config = _configs[type];

            _iconBackground.color = config.iconBackgroundColor;
            _iconImage.color = config.iconImageColor;
            _iconImage.sprite = config.icon;
            _text.text = text;

            _canvas.enabled = true;

            // Increase alpha
            var t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 5f;
                _canvasGroup.alpha = t;
                yield return null;
            }

            yield return _wait;

            _canvas.enabled = false;
            _canvasGroup.alpha = 0;
            _coroutine = null;
        }


        public enum Type
        {
            Success,
            Info,
            Error
        }

        [Serializable]
        private struct Config
        {
            public Sprite icon;
            public Color iconImageColor;
            public Color iconBackgroundColor;
        }
    }
}