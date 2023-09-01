using System;
using UnityEngine;

namespace MetaMask.Samples.Main.Scripts.Utils
{
    public class UnityDebugger : MonoBehaviour
    {
        private int logStepCursor;
        
        public void LogStep()
        {
            logStepCursor++;
            Debug.Log($"[{gameObject.name}] " + logStepCursor);
        }

        public void Log(string message)
        {
            Debug.Log(message);
        }

        public void LogError(object e)
        {
            Debug.LogError(e);
        }

        public void LogException(Exception e)
        {
            Debug.LogException(e);
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }
    }

    public static class UnityDebuggerExtensions
    {
        public static UnityDebugger Debugger(this GameObject obj)
        {
            var debug = obj.GetComponent<UnityDebugger>();
            if (debug == null)
                debug = obj.AddComponent<UnityDebugger>();

            return debug;
        }

        public static void LogStep(this GameObject obj)
        {
            obj.Debugger().LogStep();
        }
        
        public static void Log(this GameObject obj, string message)
        {
            obj.Debugger().Log(message);
        }

        public static void LogError(this GameObject obj, object e)
        {
            obj.Debugger().LogError(e);
        }

        public static void LogException(this GameObject obj, Exception e)
        {
            obj.Debugger().LogException(e);
        }

        public static void LogWarning(this GameObject obj, string message)
        {
            obj.Debugger().LogWarning(message);
        }
    }
}