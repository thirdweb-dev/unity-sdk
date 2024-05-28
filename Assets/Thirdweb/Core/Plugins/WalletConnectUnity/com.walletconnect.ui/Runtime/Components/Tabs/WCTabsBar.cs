using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace WalletConnectUnity.UI
{
    public class WCTabsBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _activeTabSelectionBackground;
        [SerializeField] private float _selectionBackgroundTransitionSeconds = .15f;
        [SerializeField] private RectTransform _mainBackground;

        [SerializeField] private WCTabButton[] _buttons;

        [field: SerializeField] public RectTransform RootTransform { get; private set; }

        private readonly Dictionary<WCTabButton, WCTabPage> _buttonToPageDictionary = new();

        private bool _isInitialized;
        private ITabsController _controller;
        private WCTabButton _selectedTabButton;

        private bool IsEnabled => RootTransform.gameObject.activeSelf;

        public void Initialize(ITabsController controller)
        {
            if (_isInitialized) return;

            _controller = controller;
            _controller.PageSelected += OnPageSelected;

            OrientationTracker.OrientationChanged += OnOrientationChanged;

            foreach (var button in _buttons)
                button.Clicked += OnButtonSelected;

            _isInitialized = true;
        }

        public void Enable(List<WCTabPage> pages)
        {
            Assert.IsTrue(pages.Count <= _buttons.Length, "pages.Length <= _buttons.Length");

            for (var i = 0; i < pages.Count; i++)
            {
                var page = pages[i];
                var button = _buttons[i];

#if UNITY_IOS || UNITY_ANDROID
                button.Label.text = page.MobileTabButtonLabel;
#else
                button.Label.text = page.DesktopTabButtonLabel;
#endif
                button.RootTransform.gameObject.SetActive(true);

                _buttonToPageDictionary.Add(button, page);
            }

            RootTransform.gameObject.SetActive(true);

            StartCoroutine(ResizeMainBackground());
        }

        public void Disable()
        {
            if (!IsEnabled) return;

            StopAllCoroutines();

            foreach (var button in _buttons)
            {
                button.RootTransform.gameObject.SetActive(false);
            }

            _buttonToPageDictionary.Clear();

            RootTransform.gameObject.SetActive(false);
        }

        public void OnButtonSelected(object tabButtonObj, EventArgs _)
        {
            var tabButton = (WCTabButton)tabButtonObj;
            if (_selectedTabButton == tabButton) return;

            _controller.SelectPage(_buttonToPageDictionary[tabButton]);
        }

        public void OnPageSelected(object _, WCTabPage page)
        {
            if (!IsEnabled) return;

            var tabButton = _buttonToPageDictionary
                .FirstOrDefault(kvp => kvp.Value == page)
                .Key;

            if (_selectedTabButton == tabButton) return;

            if (_selectedTabButton != null)
                _selectedTabButton.Deselect();

            _selectedTabButton = tabButton;
            _selectedTabButton.Select();

            StartCoroutine(AdjustSelectionBackgroundTransformRoutine(tabButton));
        }

        private IEnumerator ResizeMainBackground()
        {
            // Skip one frame to allow layout groups to update
            yield return null;

            var totalTabsWidth = _buttons
                .Where(button => button.IsVisible)
                .Sum(button => button.RootTransform.sizeDelta.x);

            const float padding = 10;

            _mainBackground.sizeDelta = new Vector2(totalTabsWidth + padding, _mainBackground.sizeDelta.y);
        }

        private IEnumerator AdjustSelectionBackgroundTransformRoutine(WCTabButton tabButton)
        {
            // Skip a few frames to allow layout groups to update
            yield return null;
            yield return null;

            var initialPosition = _activeTabSelectionBackground.anchoredPosition;
            var targetPosition = tabButton.RootTransform.anchoredPosition;

            var initialSize = _activeTabSelectionBackground.sizeDelta;
            var targetSize = tabButton.RootTransform.sizeDelta;

            var timeElapsed = 0f;

            while (timeElapsed < _selectionBackgroundTransitionSeconds)
            {
                var t = timeElapsed / _selectionBackgroundTransitionSeconds;

                _activeTabSelectionBackground.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, t);
                _activeTabSelectionBackground.sizeDelta = Vector2.Lerp(initialSize, targetSize, t);

                timeElapsed += Time.deltaTime;

                yield return null;
            }

            _activeTabSelectionBackground.anchoredPosition = targetPosition;
            _activeTabSelectionBackground.sizeDelta = targetSize;
        }

        private void OnOrientationChanged(object sender, ScreenOrientation _)
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(AdjustSelectionBackgroundTransformRoutine(_selectedTabButton));
        }
    }
}