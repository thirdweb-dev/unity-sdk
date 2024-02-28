using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace MetaMask.Scripts.Utilities
{
    public class JSCallback : MonoBehaviour
    {
        private Dictionary<string, Action> callbackCache = new Dictionary<string, Action>();
        private Dictionary<string, Action<string>> callbackJsonCache = new Dictionary<string, Action<string>>();
        
        public static JSCallback Instance
        {
            get
            {
                var instance = GameObject.FindObjectOfType<JSCallback>();
                if (instance == null)
                {
                    var obj = new GameObject("__JSCallback_Manager__");
                    instance = obj.AddComponent<JSCallback>();
                }

                return instance;
            }
        }

        public static string Using(Action callback)
        {
            return Instance.QueueAction(callback);
        }
        
        public static string Using<T>(Action<T> callback)
        {
            return Instance.QueueActionWithJson(json =>
            {
                var obj = JsonConvert.DeserializeObject<T>(json);
                callback(obj);
            });
        }

        public static string UsingJson(Action<string> callback)
        {
            return Instance.QueueActionWithJson(callback);
        }

        public string QueueAction(Action callback)
        {
            // find a new guid for callback
            string guid;
            do
            {
                guid = Guid.NewGuid().ToString();
            } while (callbackCache.ContainsKey(guid));
            
            callbackCache.Add(guid, callback);

            return $"{gameObject.name}:DoCallback:{guid}";
        }

        public string QueueActionWithJson(Action<string> callback)
        {
            // find a new guid for callback
            string guid;
            do
            {
                guid = Guid.NewGuid().ToString();
            } while (callbackJsonCache.ContainsKey(guid));
            
            callbackJsonCache.Add(guid, callback);

            return $"{gameObject.name}:DoJsonCallback:{guid}";
        }

        [Preserve]
        public void DoCallback(string json)
        {
            var data = JsonConvert.DeserializeObject<string[]>(json);
            var guid = data[0];
            
            if (!callbackCache.ContainsKey(guid))
                return;

            callbackCache[guid]();
        }

        [Preserve]
        public void DoJsonCallback(string json)
        {
            var data = JsonConvert.DeserializeObject<string[]>(json);
            var guid = data[0];
            
            if (!callbackJsonCache.ContainsKey(guid))
                return;

            callbackJsonCache[guid](data[1]);
        }
    }
}