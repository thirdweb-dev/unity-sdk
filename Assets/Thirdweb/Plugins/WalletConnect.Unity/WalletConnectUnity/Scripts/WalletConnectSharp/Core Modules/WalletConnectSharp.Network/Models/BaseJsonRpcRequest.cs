namespace WalletConnectSharp.Network.Models
{
    /// <summary>
    /// A base class for creating custom JSON RPC request objects with a given parameter type T
    /// </summary>
    /// <typeparam name="T">The parameter type for this JSON RPC request</typeparam>
    public abstract class BaseJsonRpcRequest<T> : IRequestArguments<T>
    {
        /// <summary>
        /// The method for this JSON RPC Request
        /// </summary>
        public abstract string Method { get; }
        
        /// <summary>
        /// The parameters for this JSON RPC request
        /// </summary>
        public T Params { get; protected set; }

        protected BaseJsonRpcRequest()
        {
        }

        protected BaseJsonRpcRequest(T @params)
        {
            Params = @params;
        }
    }
}
