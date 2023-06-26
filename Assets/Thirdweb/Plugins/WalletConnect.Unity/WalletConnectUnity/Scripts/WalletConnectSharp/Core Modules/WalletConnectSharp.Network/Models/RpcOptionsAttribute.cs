using System;
using System.Reflection;

namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// An attribute that defines the RPC options for this class. These options
    /// are used when this class type is used as a parameter or result of a Json RPC
    /// request/response. This cannot be used directly, use either
    /// * RpcRequestOptions
    /// * RpcResponseOptions
    /// </summary>
    public abstract class RpcOptionsAttribute : Attribute
    {
        /// <summary>
        /// The TTL (time to live)
        /// </summary>
        public long TTL { get; }

        /// <summary>
        /// The Tag
        /// </summary>
        public int Tag { get; }

        /// <summary>
        /// Create a new RpcOptions attribute with the given ttl and tag parameters
        /// </summary>
        /// <param name="ttl">Time to live</param>
        /// <param name="tag">The tag</param>
        protected RpcOptionsAttribute(long ttl, int tag)
        {
            TTL = ttl;
            Tag = tag;
        }
    }
}
