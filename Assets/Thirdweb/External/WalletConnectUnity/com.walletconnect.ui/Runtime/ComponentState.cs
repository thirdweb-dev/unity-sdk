using System;
using UnityEngine;

namespace WalletConnectUnity.UI
{
    [Serializable]
    public struct ComponentState
    {
        public bool backgroundEnabled;
        public Color backgroundColor;

        public bool borderEnabled;
        public Color borderColor;

        public bool ringEnabled;
        public Color ringColor;
    }
}