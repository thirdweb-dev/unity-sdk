using System;
using UnityEngine;

namespace Thirdweb.Unity
{
    public static class ThirdwebDebug
    {
        public static bool IsEnabled { get; set; } = false;

        private const string PREFIX = "[Thirdweb] ";

        public static void Log(object message)
        {
            if (IsEnabled)
                Debug.Log(PREFIX + message);
        }

        public static void LogWarning(object message)
        {
            if (IsEnabled)
                Debug.LogWarning(PREFIX + message);
        }

        public static void LogError(object message)
        {
            if (IsEnabled)
                Debug.LogError(PREFIX + message);
        }

        public static void LogException(Exception exception)
        {
            if (IsEnabled)
                Debug.LogException(exception);
        }
    }
}
