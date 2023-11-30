using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace WalletConnectUnity.Utils
{
    /// <summary>
    /// Main thread actions dispatcher.
    /// </summary>
    /// <example>
    /// <code>
    /// MTQ.Enqueue(() =>
    /// {
    ///        // some actions that require the Unity main thread
    /// });
    /// </code>
    /// </example>
    public class MTQ : MonoBehaviour
    {
        #region Instance

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            var go = new GameObject("Main Thread Dispatcher");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<MTQ>();
        }

        public static MTQ Instance { get; private set; }

        #endregion

        private readonly ConcurrentQueue<Action> pending = new ConcurrentQueue<Action>();

        public void Invoke(Action fn) => this.pending.Enqueue(fn);

        public static void Enqueue(Action a)
        {
            if (Instance == null)
                return;
            
            Instance.Invoke(a);
        } 

        private void Update()
        {
            while(this.pending.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch(Exception e)
                {
                    Debug.LogError(
                        $"An error has occurred during processing one of the queued actions in the main thread dispatcher:\n{e}",
                        this
                    );
                }
            }
        }
    }
}