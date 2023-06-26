using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// An attribute that defines the RPC options for this class (or struct). These options
    /// are used when this class type is used as a parameter of a Json RPC
    /// request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RpcRequestOptionsAttribute : RpcOptionsAttribute
    {
        /// <summary>
        /// Returns the first RpcOptionsAttribute found on the given type T.
        /// If no attribute is found, then null is returned
        /// </summary>
        /// <typeparam name="T">The type to inspect for RpcOptionsAttribute</typeparam>
        /// <returns>The first RpcOptionsAttribute found on the given type T</returns>
        public static RpcRequestOptionsAttribute GetOptionsForType<T>()
        {
            return GetOptionsForType(typeof(T));
        }

        /// <summary>
        /// Returns the first RpcOptionsAttribute found on the given type t.
        /// If no attribute is found, then null is returned
        /// </summary>
        /// <param name="t">The type to inspect for RpcOptionsAttribute</param>
        /// <returns>The first RpcOptionsAttribute found on the given type t</returns>
        public static RpcRequestOptionsAttribute GetOptionsForType(Type t)
        {
            var attribute = t.GetCustomAttributes(typeof(RpcRequestOptionsAttribute), true).Cast<RpcRequestOptionsAttribute>().FirstOrDefault();

            return attribute;
        }
        
        public RpcRequestOptionsAttribute(long ttl, int tag) : base(ttl, tag)
        {
        }
    }
}
