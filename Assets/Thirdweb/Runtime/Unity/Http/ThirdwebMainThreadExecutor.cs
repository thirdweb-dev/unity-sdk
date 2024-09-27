using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb.Unity.Helpers
{
    public class ThirdwebMainThreadExecutor : MonoBehaviour
    {
        private readonly Queue<Action> actions = new Queue<Action>();
        private static ThirdwebMainThreadExecutor instance;

        public static ThirdwebMainThreadExecutor Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_6000_0_OR_NEWER
                    var existingInstance = FindAnyObjectByType<ThirdwebMainThreadExecutor>();
#else
                    var existingInstance = FindObjectOfType<ThirdwebMainThreadExecutor>();
#endif
                    if (existingInstance != null)
                    {
                        instance = existingInstance;
                    }
                    else
                    {
                        var executorObject = new GameObject("ThirdwebMainThreadExecutor");
                        instance = executorObject.AddComponent<ThirdwebMainThreadExecutor>();
                        DontDestroyOnLoad(executorObject);
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public async Task RunOnMainThread(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            lock (actions)
            {
                actions.Enqueue(() =>
                {
                    action();
                    tcs.SetResult(true);
                });
            }

            await tcs.Task;
        }

        public void ExecuteAllActions()
        {
            lock (actions)
            {
                while (actions.Count > 0)
                {
                    actions.Dequeue().Invoke();
                }
            }
        }

        void Update()
        {
            ExecuteAllActions();
        }
    }
}
