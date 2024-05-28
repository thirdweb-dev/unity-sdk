using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectUnity.Core.Utils;
using DeviceType = WalletConnectUnity.Core.Utils.DeviceType;

namespace WalletConnectUnity.UI
{
    [AddComponentMenu("WalletConnect/UI/WC Modal")]
    public class WCModal : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Canvas _canvas;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _rootRectTransform;
        [SerializeField] private CanvasScaler _rootCanvasScaler;
        [SerializeField] private Canvas _globalBackgroundCanvas;
        [SerializeField] private Image _modalMaskImage;
        [SerializeField] private Image _modalBorderImage;

        [field: SerializeField] public WCModalHeader Header { get; private set; }

        [Header("Settings")]
        [SerializeField, Range(0, 1)] private float _mobileMaxHeightPercent = 0.8f;

        [SerializeField] private TransformConfig _mobileTransformConfig;

        [SerializeField] private TransformConfig _desktopTransformConfig;

        [Header("Asset References")]
        [SerializeField] private Sprite _mobileModalMaskSprite;

        [SerializeField] private Sprite _mobileModalBorderSprite;

        public bool IsOpen => _canvas.enabled;

        public Canvas Canvas => _canvas;

        public RectTransform RootRectTransform => _rootRectTransform;

        public float MobileMaxHeightPercent => _mobileMaxHeightPercent;

        public event EventHandler Opened;
        public event EventHandler Closed;

        private readonly Stack<WCModalView> _viewsStack = new();
        private bool _hasGlobalBackground;
        private bool _resizingModal;

        private Coroutine _backInputCoroutine;

        private void Awake()
        {
            _hasGlobalBackground = _globalBackgroundCanvas != null;

            HandleConstantPhysicalSize();
        }

        public void OpenView(WCModalView view, WCModal modal = null, object parameters = null)
        {
            if (_viewsStack.Count == 0)
                EnableModal();

            if (_viewsStack.Count > 0)
                _viewsStack.Peek().Hide();

            modal ??= this;


            var resizeCoroutine = ResizeModalRoutine(view.GetViewHeight());
            _viewsStack.Push(view);
            view.Show(modal, resizeCoroutine, parameters);

            Header.Title = view.GetTitle();
        }

        public void CloseView()
        {
            if (_viewsStack.Count <= 0) return;

            var currentView = _viewsStack.Pop();
            currentView.Hide();

            if (_viewsStack.Count > 0)
            {
                var nextView = _viewsStack.Peek();
                Header.Title = nextView.GetTitle();
                var resizeCoroutine = ResizeModalRoutine(nextView.GetViewHeight());
                nextView.Show(this, resizeCoroutine);
            }
            else
            {
                DisableModal();
            }
        }

        public void CloseModal()
        {
            if (_viewsStack.Count > 0)
            {
                var lastView = _viewsStack.Pop();
                lastView.Hide();
            }

            _viewsStack.Clear();
            DisableModal();

            if (_backInputCoroutine != null)
                StopCoroutine(_backInputCoroutine);

            Closed?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerator ResizeModalRoutine(float targetHeight)
        {
            if (_resizingModal) yield break;
            _resizingModal = true;

            targetHeight = targetHeight + Header.Height + 12;

#if UNITY_ANDROID || UNITY_IOS
            if (DeviceUtils.GetDeviceType() == DeviceType.Phone)
                targetHeight += 8;
#endif

            var rootTransformSizeDelta = _rectTransform.sizeDelta;
            var originalHeight = rootTransformSizeDelta.y;
            var elapsedTime = 0f;
            var duration = .25f; // TODO: serialize this

            targetHeight = Mathf.Min(targetHeight, _rootRectTransform.sizeDelta.y * _mobileMaxHeightPercent);

            while (elapsedTime < duration)
            {
                var lerp = Mathf.Lerp(originalHeight, targetHeight, elapsedTime / duration);
                _rectTransform.sizeDelta = new Vector2(rootTransformSizeDelta.x, lerp);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _rectTransform.sizeDelta = new Vector2(rootTransformSizeDelta.x, targetHeight);
            _resizingModal = false;
        }

        private void HandleConstantPhysicalSize()
        {
            const float targetDPI = 160;

            // When using Game view instead of Device Simulator, you may want to change the target DPI for better scaling, e.g.:
// #if (UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS))
//             targetDPI = 96;
// #endif

            if (Screen.dpi != 0)
                _rootCanvasScaler.scaleFactor = Screen.dpi / targetDPI;
            else
                _rootCanvasScaler.scaleFactor = 1f;
        }

        private void EnableModal()
        {
            var deviceType = DeviceUtils.GetDeviceType();

            if (deviceType == DeviceType.Phone)
            {
                _mobileTransformConfig.Apply(_rectTransform);

                _modalMaskImage.sprite = _mobileModalMaskSprite;
                _modalBorderImage.sprite = _mobileModalBorderSprite;

                OrientationTracker.Enable();
            }
            else
            {
                _desktopTransformConfig.Apply(_rectTransform);
            }

            _canvas.enabled = true;

            if (_hasGlobalBackground)
                _globalBackgroundCanvas.enabled = true;

            _backInputCoroutine = StartCoroutine(BackInputRoutine());

            Opened?.Invoke(this, EventArgs.Empty);
        }

        private void DisableModal()
        {
            _canvas.enabled = false;

            if (_hasGlobalBackground)
                _globalBackgroundCanvas.enabled = false;

#if UNITY_ANDROID || UNITY_IOS
            OrientationTracker.Disable();
#endif
        }

        private IEnumerator BackInputRoutine()
        {
            while (_canvas.enabled)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (_viewsStack.Count > 0)
                    {
                        CloseView();
                    }
                    else
                    {
                        CloseModal();
                    }
                }

                yield return null;
            }
        }
    }
}