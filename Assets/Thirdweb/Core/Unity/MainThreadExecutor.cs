using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thirdweb.Unity
{
    public static class MainThreadExecutor
    {
        private static readonly Queue<Action> actions = new Queue<Action>();

        public static async Task RunOnMainThread(Action action)
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

        public static void ExecuteAllActions()
        {
            lock (actions)
            {
                while (actions.Count > 0)
                {
                    actions.Dequeue().Invoke();
                }
            }
        }
    }
}
