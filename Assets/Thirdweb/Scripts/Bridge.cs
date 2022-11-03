
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
            public string ack_id;
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

        private static TaskCompletionSource<string> utcs;

        [AOT.MonoPInvokeCallback(typeof(Action<string>))]
        private static void testFuncCB(string result)
        {
            utcs.TrySetResult(result);  
        }

        public static async Task<T> InvokeRoute<T>(string route, string[] body)
        {
            var msg = JsonUtility.ToJson(new RequestMessageBody(body));
            var ack_id = ThirdwebInvoke(route, msg, testFuncCB);
            var tr = new TaskCompletionSource<string>();
            string result = await tr.Task;
            // Debug.LogFormat("Result from {0}: {1}", route, result);
            return JsonUtility.FromJson<Result<T>>(result).result;
        }

        public static async Task<string> InvokeRouteRaw(string route, string[] body)
        {
            var msg = JsonUtility.ToJson(new RequestMessageBody(body));
            Debug.LogFormat("Calling JS");
            utcs = new TaskCompletionSource<string>();
            ThirdwebInvoke(route, msg, testFuncCB);
            var result = await utcs.Task;
            // Debug.LogFormat("Result from {0}: {1}", route, result);
            return result;
        }

        [DllImport("__Internal")]
        private static extern string ThirdwebInvoke(string route, string payload, Action<string> cb);
    }
}
