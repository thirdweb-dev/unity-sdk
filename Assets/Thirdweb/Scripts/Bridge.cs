
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Thirdweb
{
    public class Bridge
    {
        [System.Serializable]
        private struct Result<T>
        {
            public T result;
        }

        [System.Serializable]
        private struct RequestMessageBody
        {
            public RequestMessageBody(string[] arguments)
            {
                this.arguments = arguments;
            }
            public string[] arguments;
        }

        private static Dictionary<string, TaskCompletionSource<string>> taskMap = new Dictionary<string, TaskCompletionSource<string>>();

        [AOT.MonoPInvokeCallback(typeof(Action<string, string, string>))]
        private static void jsCallback(string taskId, string result, string error)
        {
            if (taskMap.ContainsKey(taskId))
            {
                if (error != null) 
                {
                    // TODO wrap error in proper result type
                    taskMap[taskId].TrySetException(new Exception(error));
                } else 
                {
                    taskMap[taskId].TrySetResult(result);
                }
                taskMap.Remove(taskId);
            }
        }

        public static void Initialize(string chainOrRPC, ThirdwebSDK.Options options) {
            if (Application.isEditor) {
                Debug.LogWarning("Initializing the thirdweb SDK is not supported in the editor. Please build and run the app instead.");
                return;
            }
            ThirdwebInitialize(chainOrRPC, Utils.ToJson(options));
        }

        public static async Task<string> Connect() {
            if (Application.isEditor) {
                Debug.LogWarning("Connecting wallets is not supported in the editor. Please build and run the app instead.");
                return "0x0000000000000000000000000000000000000000";
            }
            var task = new TaskCompletionSource<string>();
            string taskId = Guid.NewGuid().ToString();
            taskMap[taskId] = task;
            ThirdwebConnect(taskId, jsCallback);
            string result = await task.Task;
            return result;
        }

        public static async Task SwitchNetwork(int chainId) {
            if (Application.isEditor) {
                Debug.LogWarning("Switching networks is not supported in the editor. Please build and run the app instead.");
                return;
            }
            var task = new TaskCompletionSource<string>();
            string taskId = Guid.NewGuid().ToString();
            taskMap[taskId] = task;
            ThirdwebSwitchNetwork(taskId, chainId, jsCallback);
            await task.Task;
            return;
        }

        public static async Task<T> InvokeRoute<T>(string route, string[] body)
        {
            if (Application.isEditor) {
                Debug.LogWarning("Interacting with the thirdweb SDK is not supported in the editor. Please build and run the app instead.");
                return default(T);
            }
            var msg = Utils.ToJson(new RequestMessageBody(body));
            string taskId = Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
            ThirdwebInvoke(taskId, route, msg, jsCallback);
            try 
            {
                string result = await task.Task;
                // Debug.LogFormat("Result from {0}: {1}", route, result);
                return JsonConvert.DeserializeObject<Result<T>>(result).result;
            } 
            catch (Exception)
            {
                // Debug.LogFormat("Error from {0}: {1}", route, e);
                return default(T);
            }
        }

        [DllImport("__Internal")]
        private static extern string ThirdwebInvoke(string taskId, string route, string payload, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebInitialize(string chainOrRPC, string options);
        [DllImport("__Internal")]
        private static extern string ThirdwebConnect(string taskId, Action<string, string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebSwitchNetwork(string taskId, int chainId, Action<string, string, string> cb);
    }
}
