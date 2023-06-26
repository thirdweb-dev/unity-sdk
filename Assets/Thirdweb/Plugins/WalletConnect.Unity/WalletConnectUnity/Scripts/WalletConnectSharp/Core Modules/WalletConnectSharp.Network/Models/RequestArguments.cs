namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// A class that holds request arguments that can be
    /// placed inside a Json RPC request. Does not represent
    /// a full Json RPC request
    /// </summary>
    /// <typeparam name="T">The type of the parameter in this request</typeparam>
    public class RequestArguments<T> : IRequestArguments<T>
    {
        /// <summary>
        /// The method to use
        /// </summary>
        public string Method { get; set; }
        
        /// <summary>
        /// The method for this request
        /// </summary>
        public T Params { get; set; }
    }
}
