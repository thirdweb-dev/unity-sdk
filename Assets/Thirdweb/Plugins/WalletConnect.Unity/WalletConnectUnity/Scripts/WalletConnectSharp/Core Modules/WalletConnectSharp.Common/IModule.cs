using System;

namespace WalletConnectSharp.Common
{
    /// <summary>
    /// An interface that represents a module
    /// </summary>
    public interface IModule /* : IDisposable */
    {
        /// <summary>
        /// The name of this module
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// The context string of this module, used to define
        /// separate instances of the same module
        /// </summary>
        string Context { get; }
    }
}