using MetaMask.Unity.Utils;
using TMPro;

namespace MetaMask.Logging
{
    public class MetaMaskLogToText : BindableMonoBehavior
    {
        [BindComponent]
        private TMP_Text logText;

        void Start()
        {
            MetaMaskUnityLogger.Instance.onLog += OnLog;
        }

        private void OnDestroy()
        {
            MetaMaskUnityLogger.Instance.onLog -= OnLog;
        }

        void OnLog(object obj)
        {
            logText.text = $"{obj}\n{logText.text}";
        }
    }
}