using System;
using System.Runtime.CompilerServices;

namespace Thirdweb.Redcode.Awaiting.Engine
{
    /// <summary>
    /// This class can be awaited.
    /// Also can control calling of continuation when you want.
    /// Not return result after awaiting.
    /// </summary>
    public class ManualAwaiter : INotifyCompletion
    {
        /// <summary>
        /// Represent continuation which you can call later.
        /// </summary>
        protected Action _continuation;

        /// <summary>
        /// Represent completion state. Always return false value, this means that
        /// await will not execute continuation immediatly (in the same thread).
        /// </summary>
        public bool IsCompleted => false;

        /// <summary>
        /// This method invoked when you await ManualAwaiter object.
        /// Continuation will be stored and will be used later (when you give command).
        /// </summary>
        /// <param name="continuation">Continuation method which will be stored.</param>
        public void OnCompleted(Action continuation) => _continuation = continuation;

        /// <summary>
        /// Indicates whether await can expect the result (ManualAwaiter not support result after awaiting).
        /// </summary>
        public void GetResult() { }

        /// <summary>
        /// Run your continuation in the calling thread.
        /// </summary>
        public void RunContinuation() => _continuation();
    }

    /// <summary>
    /// This class can be awaited.
    /// Also can control calling of continuation when you want.
    /// Return result after awaiting.
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    public sealed class ManualAwaiter<T> : ManualAwaiter
    {
        private Func<T> _resultGetter;

        /// <summary>
        /// Get calculated result.
        /// Used by await instruction.
        /// </summary>
        /// <returns>Calculated result.</returns>
        public new T GetResult() => _resultGetter();

        /// <summary>
        /// Save result getter method.
        /// The getter method will be used later when GetResult method will be called.
        /// </summary>
        /// <param name="resultGetter">Method which return result.</param>
        public void SetResultGetter(Func<T> resultGetter) => _resultGetter = resultGetter;
    }
}
