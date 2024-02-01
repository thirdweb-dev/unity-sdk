using Thirdweb.Redcode.Awaiting.Engine;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Thirdweb.Redcode.Awaiting
{
    /// <summary>
    /// Contains 'GetAwaiter' extensions methods for many classes.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets <see cref="ManualAwaiter"/> object for <see cref="YieldInstruction"/>.
        /// </summary>
        /// <param name="instructuion">Target instruction.</param>
        /// <returns><see cref="ManualAwaiter"/> object, which knows how to await <see cref="YieldInstruction"/>.</returns>
        public static ManualAwaiter GetAwaiter(this YieldInstruction instructuion) => ExtensionsHelper.GetAwaiterForInstuction(instructuion);

        /// <summary>
        /// Gets <see cref="ManualAwaiter"/> object for <see cref="CustomYieldInstruction"/>.
        /// </summary>
        /// <param name="instructuion">Target instruction.</param>
        /// <returns><see cref="ManualAwaiter"/> object, which knows how to await <see cref="CustomYieldInstruction"/>.</returns>
        public static ManualAwaiter GetAwaiter(this CustomYieldInstruction instructuion) => ExtensionsHelper.GetAwaiterForInstuction(instructuion);

        /// <summary>
        /// Gets <see cref="ManualAwaiter"/> object for <see cref="IEnumerator"/>.
        /// </summary>
        /// <param name="enumerator">Target enumerator.</param>
        /// <returns><see cref="ManualAwaiter"/> object, which knows how to await <see cref="IEnumerator"/>.</returns>
        public static ManualAwaiter GetAwaiter(this IEnumerator enumerator) => ExtensionsHelper.GetAwaiterForEnumerator(enumerator);

        public static ManualAwaiter<AssetBundleRequest> GetAwaiter(this AssetBundleRequest request) => ExtensionsHelper.GetAwaiterWithResultForInstuction(request);

        /// <summary>
        /// Await current <paramref name="task"/> and rethrow exception if it happens.
        /// </summary>
        /// <param name="task">Current task.</param>
        public static async void CatchErrors(this Task task) => await task;

        /// <summary>
        /// Convert <paramref name="task"/> to <see cref="IEnumerator"/> object.
        /// </summary>
        /// <param name="task">Current task.</param>
        public static IEnumerator AsEnumerator(this Task task)
        {
            while (!task.IsCompleted)
                yield return null;
        }

        /// <summary>
        /// Convert <paramref name="task"/> object to <see cref="Coroutine"/>.
        /// </summary>
        /// <param name="task">Current task.</param>
        public static Coroutine AsCoroutine(this Task task) => RoutineHelper.Instance.StartCoroutine(task.AsEnumerator());
    }
}
