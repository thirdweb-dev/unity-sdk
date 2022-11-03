
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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

        [AOT.MonoPInvokeCallback(typeof(Action<string, string>))]
        private static void jsCallback(string taskId, string result)
        {
            Debug.Log("jsCallback: " + taskId + " " + result);
            if (taskMap.ContainsKey(taskId))
            {
                taskMap[taskId].TrySetResult(result);  
                taskMap.Remove(taskId);
            }
        }

        public static void Initialize(string chainOrRPC) {
            ThirdwebInitialize(chainOrRPC);
        }

        public static async Task<string> Connect() {
            var task = new TaskCompletionSource<string>();
            string taskId = System.Guid.NewGuid().ToString();
            taskMap[taskId] = task;
            ThirdwebConnect(taskId, jsCallback);
            string result = await task.Task;
            return result;
        }

        public static async Task<T> InvokeRoute<T>(string route, string[] body)
        {
            var msg = JsonUtility.ToJson(new RequestMessageBody(body));
            string taskId = System.Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
            ThirdwebInvoke(taskId, route, msg, jsCallback);
            string result = await task.Task;
            // Debug.LogFormat("Result from {0}: {1}", route, result);
            return JsonUtility.FromJson<Result<T>>(result).result;
        }

        public static async Task<string> InvokeRouteRaw(string route, string[] body)
        {
            var msg = JsonUtility.ToJson(new RequestMessageBody(body));
            string taskId = System.Guid.NewGuid().ToString();
            var task = new TaskCompletionSource<string>();
            taskMap[taskId] = task;
            ThirdwebInvoke(taskId, route, msg, jsCallback);
            string result = await task.Task;
            // Debug.LogFormat("Result from {0}: {1}", route, result);
            return result;
        }

        [DllImport("__Internal")]
        private static extern string ThirdwebInvoke(string taskId, string route, string payload, Action<string, string> cb);
        [DllImport("__Internal")]
        private static extern string ThirdwebInitialize(string chainOrRPC);
        [DllImport("__Internal")]
        private static extern string ThirdwebConnect(string taskId, Action<string, string> cb);
    }
}
