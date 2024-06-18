using System.Collections;

namespace Thirdweb.Redcode.Awaiting.Engine
{
    /// <summary>
    /// Helps create awaiters for awaitable objects.
    /// </summary>
    internal static class ExtensionsHelper
    {
        /// <summary>
        /// Create awaiter for any instruction.
        /// </summary>
        /// <param name="instruction">Instruction for yielding.</param>
        /// <returns>Awaiter which awaiting passed instruction.</returns>
        internal static ManualAwaiter GetAwaiterForInstuction(object instruction)
        {
            var awaiter = new ManualAwaiter();

            if (ContextHelper.IsMainThread)
                RoutineHelper.Instance.StartCoroutine(WaitForInstructionAndRunContinuation(instruction, awaiter));
            else
                ContextHelper.UnitySynchronizationContext.Post(
                    (state) =>
                    {
                        RoutineHelper.Instance.StartCoroutine(WaitForInstructionAndRunContinuation(instruction, awaiter));
                    },
                    null
                );

            return awaiter;
        }

        private static IEnumerator WaitForInstructionAndRunContinuation(object instruction, ManualAwaiter awaiter)
        {
            yield return instruction;
            awaiter.RunContinuation();
        }

        /// <summary>
        /// Create awaiter for <paramref name="enumerator"/> object.
        /// </summary>
        /// <param name="enumerator">Object which can enumerate.</param>
        /// <returns>Awaiter which awaiting passed enumerator.</returns>
        internal static ManualAwaiter GetAwaiterForEnumerator(IEnumerator enumerator)
        {
            var awaiter = new ManualAwaiter();

            if (ContextHelper.IsMainThread)
                RoutineHelper.Instance.StartCoroutine(WaitForEnumeratorAndContinueRoutine(enumerator, awaiter));
            else
                ContextHelper.UnitySynchronizationContext.Post(
                    (state) =>
                    {
                        RoutineHelper.Instance.StartCoroutine(WaitForEnumeratorAndContinueRoutine(enumerator, awaiter));
                    },
                    null
                );

            return awaiter;
        }

        private static IEnumerator WaitForEnumeratorAndContinueRoutine(IEnumerator enumerator, ManualAwaiter awaiter)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;

            awaiter.RunContinuation();
        }

        /// <summary>
        /// Create awaiter with result value for any instruction.
        /// </summary>
        /// <typeparam name="T">Instruction type.</typeparam>
        /// <param name="instruction">Instruction object.</param>
        /// <returns><see cref="ManualAwaiter{T}"/> object.</returns>
        internal static ManualAwaiter<T> GetAwaiterWithResultForInstuction<T>(T instruction)
        {
            var awaiter = new ManualAwaiter<T>();
            awaiter.SetResultGetter(() => instruction);
            RoutineHelper.Instance.StartCoroutine(WaitForInstructionWithResultAndRunContinuation(instruction, awaiter));
            return awaiter;
        }

        private static IEnumerator WaitForInstructionWithResultAndRunContinuation(object instruction, ManualAwaiter awaiter)
        {
            yield return instruction;
            awaiter.RunContinuation();
        }
    }
}
