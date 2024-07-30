using System;
using UnityEngine;

namespace WalletConnectUnity.UI
{
    [Serializable]
    public struct TransformConfig
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 sizeDelta;
        public Vector2 pivot;

        public void Apply(RectTransform rectTransform)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.pivot = pivot;
        }
    }
}