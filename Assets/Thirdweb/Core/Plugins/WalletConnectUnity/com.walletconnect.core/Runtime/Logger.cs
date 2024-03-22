using System;
using UnityEngine;
using ILogger = WalletConnectSharp.Common.Logging.ILogger;

namespace WalletConnectUnity.Core
{
    public class Logger : ILogger
    {
        public void Log(string message) => Debug.Log(message);

        public void LogError(string message) => Debug.LogError(message);

        public void LogError(Exception e) => Debug.LogException(e);
    }
}