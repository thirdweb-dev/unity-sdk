using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace WalletConnectUnity.UI
{
    public class WCLoadingAnimator : MonoBehaviour
    {
        [SerializeField] private Color _colorA;
        [SerializeField] private Color _colorB;
        [SerializeField] private float _speed = 1f;
        [SerializeField] private AnimationCurve _lerpCurve;

        private readonly HashSet<Graphic> _subscribedGraphics = new();
        private readonly HashSet<VisualElement> _subscribedVisualElements = new();

        private Color _currentColor;
        private bool _isAnimating;
        private bool _isPaused;

        public static WCLoadingAnimator Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void Subscribe<T>(T element) where T : class
        {
            switch (element)
            {
                case Graphic graphic:
                    _subscribedGraphics.Add(graphic);
                    break;
                case VisualElement visualElement:
                    _subscribedVisualElements.Add(visualElement);
                    break;
            }

            if (!_isAnimating && !_isPaused)
                StartAnimation();
        }

        public void Unsubscribe<T>(T element) where T : class
        {
            switch (element)
            {
                case Graphic graphic:
                    _subscribedGraphics.Remove(graphic);
                    break;
                case VisualElement visualElement:
                    _subscribedVisualElements.Remove(visualElement);
                    break;
            }

            if (_subscribedGraphics.Count == 0 && _subscribedVisualElements.Count == 0)
                StopAnimation();
        }

        private IEnumerator AnimateColorRoutine()
        {
            var t = 0f;
            _isAnimating = true;

            while (_isAnimating)
            {
                if (_isPaused)
                    yield return new WaitUntil(() => !_isPaused);

                _currentColor = Color.Lerp(_colorA, _colorB, _lerpCurve.Evaluate(t));
                t += Time.deltaTime * _speed;
                if (t > 1f)
                {
                    t = 0f;
                    (_colorA, _colorB) = (_colorB, _colorA);
                }

                foreach (var graphic in _subscribedGraphics)
                    graphic.color = _currentColor;

                foreach (var visualElement in _subscribedVisualElements)
                    visualElement.style.backgroundColor = _currentColor;

                yield return null;
            }
        }

        public void PauseAnimation()
        {
            _isPaused = true;
        }

        public void ResumeAnimation()
        {
            if (!_isPaused)
                return;

            _isPaused = false;

            if (!_isAnimating && (_subscribedGraphics.Count > 0 || _subscribedVisualElements.Count > 0))
                StartAnimation();
        }

        private void StartAnimation()
        {
            StartCoroutine(AnimateColorRoutine());
        }

        private void StopAnimation()
        {
            _isAnimating = false;
            StopAllCoroutines();
        }
    }
}