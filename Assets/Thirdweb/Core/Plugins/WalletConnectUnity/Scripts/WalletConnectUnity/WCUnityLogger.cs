using System;
using UnityEngine;
using WalletConnectUnity.Utils;
using ILogger = WalletConnectSharp.Common.Logging.ILogger;

namespace WalletConnect
{
    public class WCUnityLogger : ILogger
    {
        public void Log(string message)
        {
            MTQ.Enqueue(() => Debug.Log(message));
        }

        public void LogError(string message)
        {
            MTQ.Enqueue(() => Debug.LogError(message));
        }

        public void LogError(Exception e)
        {
            MTQ.Enqueue(() => Debug.LogError(e));
        }
    }
}