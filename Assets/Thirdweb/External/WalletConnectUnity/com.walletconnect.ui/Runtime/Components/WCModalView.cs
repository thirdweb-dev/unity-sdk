using System.Collections;
using UnityEngine;

namespace WalletConnectUnity.UI
{
    public class WCModalView : MonoBehaviour
    {
        [Header("Scene References")] [SerializeField]
        private Canvas _canvas;

        [SerializeField] private string _title;
        [SerializeField] protected RectTransform rootTransform;

        [Header("Settings")]
        [SerializeField] private TransformConfig _portraitTransformConfig;

        [SerializeField] private TransformConfig _landscapeTransformConfig;

        protected WCModal parentModal;
        private ScreenOrientation _lastOrientation;

        public bool IsActive => _canvas.enabled;

        protected virtual void Awake()
        {
            OrientationTracker.OrientationChanged += OnOrientationChanged;

            if (Screen.orientation != _lastOrientation)
                OnOrientationChanged(this, Screen.orientation);
        }

        public virtual float GetViewHeight()
        {
            return rootTransform.rect.height;
        }

        public virtual void Show(WCModal modal, IEnumerator effectCoroutine, object options = null)
        {
            parentModal = modal;
            StartCoroutine(ShowAfterEffectRoutine(effectCoroutine));
        }

        public virtual void Hide()
        {
            _canvas.enabled = false;
        }

        public virtual string GetTitle() => _title;

        protected virtual IEnumerator ShowAfterEffectRoutine(IEnumerator effectCoroutine)
        {
            yield return StartCoroutine(effectCoroutine);
            _canvas.enabled = true;
        }

        protected void OnOrientationChanged(object sender, ScreenOrientation orientation)
        {
            StartCoroutine(OnOrientationChangedRoutine(orientation));
        }

        private IEnumerator OnOrientationChangedRoutine(ScreenOrientation orientation)
        {
            // On some Android devices need to wait for a moment for the screen to update
            yield return new WaitForSecondsRealtime(0.1f);

            ApplyScreenOrientation(orientation);

            if (IsActive)
                parentModal.StartCoroutine(parentModal.ResizeModalRoutine(GetViewHeight()));
        }

        protected virtual void ApplyScreenOrientation(ScreenOrientation orientation)
        {
            var config = orientation is ScreenOrientation.Portrait or ScreenOrientation.PortraitUpsideDown
                ? _portraitTransformConfig
                : _landscapeTransformConfig;

            rootTransform.anchorMin = config.anchorMin;
            rootTransform.anchorMax = config.anchorMax;
            rootTransform.sizeDelta = new Vector2(config.sizeDelta.x, rootTransform.sizeDelta.y); // preserve height
            rootTransform.pivot = config.pivot;

            _lastOrientation = orientation;
        }

#if UNITY_EDITOR
        public void Reset()
        {
            _canvas = GetComponent<Canvas>();
            rootTransform = GetComponent<RectTransform>();
        }
#endif
    }

    public class ViewParams
    {
    }
}