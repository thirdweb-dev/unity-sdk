using System;
using UnityEngine;

namespace WalletConnectUnity.UI
{
    public class OrientationTracker : MonoBehaviour
    {
#pragma warning disable 67
        public static event EventHandler<ScreenOrientation> OrientationChanged;
#pragma warning restore 67

        private ScreenOrientation _lastOrientation;

        private static OrientationTracker _instance;

        public static ScreenOrientation LastOrientation
        {
            get => _instance._lastOrientation;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                Debug.LogError("OrientationTracker already exists. Destroying new instance.", gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _lastOrientation = Screen.orientation;
        }

#if UNITY_IOS || UNITY_ANDROID
        private void FixedUpdate()
        {
            var orientation = Screen.orientation;

            if (orientation != _lastOrientation)
            {
                _lastOrientation = orientation;
                OrientationChanged?.Invoke(null, orientation);
            }
        }
#endif

        public static void Enable()
        {
            _instance.enabled = true;
        }

        public static void Disable()
        {
            _instance.enabled = false;
        }
    }
}
