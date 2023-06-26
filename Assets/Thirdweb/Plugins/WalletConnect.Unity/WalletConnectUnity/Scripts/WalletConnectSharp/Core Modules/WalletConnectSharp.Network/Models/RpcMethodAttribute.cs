using System;
using System.Linq;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// A class (or struct) attribute that defines the Rpc method
    /// that should be used when the class is used as request parameters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RpcMethodAttribute : Attribute
    {
        /// <summary>
        /// The Json RPC method to use when this class is used
        /// as a request parameter
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Define the Json RPC method to use when this class is
        /// used as a request parameter
        /// </summary>
        /// <param name="method">The method name to use</param>
        public RpcMethodAttribute(string method)
        {
            MethodName = method;
        }
        
        /// <summary>
        /// Get the method name that should be used for a given class type T. This is
        /// defined by the RpcMethodAttribute attached to the type T. If the type T has no
        /// RpcMethodAttribute, then an Exception is thrown
        /// </summary>
        /// <typeparam name="T">The type T to get the method name for</typeparam>
        /// <returns>The method name to use as a string</returns>
        /// <exception cref="Exception">If the type T has no
        /// RpcMethodAttribute, then an Exception is thrown</exception>
        public static string MethodForType<T>()
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(RpcMethodAttribute), true);
            if (attributes.Length != 1)
                throw new Exception($"Type {typeof(T).FullName} has no WcMethod attribute!");

            var method = attributes.Cast<RpcMethodAttribute>().First().MethodName;

            return method;
        }
    }
}
