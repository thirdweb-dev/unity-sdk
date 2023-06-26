using WalletConnectSharp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Core.Controllers
{
    /// <summary>
    /// A factory that creates/tracks instances of <see cref="JsonRpcHistory{T,TR}"/> for the given
    /// types T and TR. This is used to track the history of requests and responses for a given session. This factory
    /// is NOT a singleton, rather each ICore instance should have its own instance of this factory. The
    /// <see cref="JsonRpcHistory{T,TR}"/> instance the factory returns must act as a singleton for the given
    /// ICore context. The factory implementation must be context-aware, meaning that it must be able to separate different
    /// singleton instances of <see cref="JsonRpcHistory{T,TR}"/> for different ICore instances.
    /// </summary>
    public class JsonRpcHistoryFactory : IJsonRpcHistoryFactory
    {
        /// <summary>
        /// A holder class that holds different singletons by a context string for the specific
        /// types T, TR
        /// This is needed so that JsonRpcHistoryFactory does NOT need to be a generic class and can
        /// instead use a generic method. Each singleton instance of this holder class
        /// holds a single instance of <see cref="JsonRpcHistory{T,TR}"/>
        /// </summary>
        /// <typeparam name="T">The request type to store history for</typeparam>
        /// <typeparam name="TR">The response type to store history for</typeparam>
        public class JsonRpcHistoryHolder<T, TR>
        {
            private static Dictionary<string, JsonRpcHistoryHolder<T, TR>> _instance = new Dictionary<string, JsonRpcHistoryHolder<T, TR>>();

            /// <summary>
            /// Get the singleton instance for a specific ICore context. If no singleton already
            /// exists, then a new instance will be created and stored.
            /// </summary>
            /// <param name="core">The ICoe module to use the context string from</param>
            /// <returns>The singleton instance for the given ICore context</returns>
            public static async Task<JsonRpcHistoryHolder<T, TR>> InstanceForContext(ICore core)
            {
                if (_instance.ContainsKey(core.Context))
                    return _instance[core.Context];

                var historyHolder = new JsonRpcHistoryHolder<T, TR>(core);
                _instance.Add(core.Context, historyHolder);
                await historyHolder.History.Init();
                return historyHolder;
            }

            /// <summary>
            /// The <see cref="IJsonRpcHistory{T,TR}"/> instance
            /// this singleton is holding
            /// </summary>
            public IJsonRpcHistory<T, TR> History { get; }

            private JsonRpcHistoryHolder(ICore core)
            {
                History = new JsonRpcHistory<T, TR>(core);
            }
        }

        /// <summary>
        /// The <see cref="ICore"/> instance this factory is for
        /// </summary>
        public ICore Core { get; }

        /// <summary>
        /// Create a new factory using the given <see cref="ICore"/> module
        /// </summary>
        /// <param name="core">The <see cref="ICore"/> module to use for the factory</param>
        public JsonRpcHistoryFactory(ICore core)
        {
            this.Core = core;
        }

        /// <summary>
        /// Get the <see cref="IJsonRpcHistory{T,TR}"/> singleton instance with the given types
        /// T, TR using the <see cref="ICore"/> instance context string
        /// </summary>
        /// <typeparam name="T">The request type to store history for</typeparam>
        /// <typeparam name="TR">The response type to store history for</typeparam>
        /// <returns>The <see cref="IJsonRpcHistory{T,TR}"/> singleton instance with the given types T, TR</returns>
        public async Task<IJsonRpcHistory<T, TR>> JsonRpcHistoryOfType<T, TR>()
        {
            return (await JsonRpcHistoryHolder<T, TR>.InstanceForContext(Core)).History;
        }
    }
}
