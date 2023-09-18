using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Thirdweb.Redcode.Awaiting
{
    /// <summary>
    /// This class can be awaited.
    /// Run code after awaiting in background thread.
    /// </summary>
    public class WaitForBackgroundThread
    {
        /// <summary>
        /// Gets awaiter object.
        /// </summary>
        /// <returns></returns>
        public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            var task = new Task(() => { });
            task.Start();

            return task.ConfigureAwait(false).GetAwaiter();
        }
    }
}
