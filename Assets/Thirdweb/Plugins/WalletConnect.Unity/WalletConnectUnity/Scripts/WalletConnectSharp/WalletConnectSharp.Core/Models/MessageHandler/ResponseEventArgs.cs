using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// Event args that are used in <see cref="TypedEventHandler{T,TR}"/> and <see cref="SessionRequestEventHandler{T,TR}"/>
    /// when a new response of the given type TR is received.
    /// Stores information about the current response, such as the topic, and the response itself.
    /// </summary>
    /// <typeparam name="TR">The response type</typeparam>
    public class ResponseEventArgs<TR>
    {
        /// <summary>
        /// The topic this response came from
        /// </summary>
        public string Topic { get; }
        
        /// <summary>
        /// The response data
        /// </summary>
        public JsonRpcResponse<TR> Response { get; }

        internal ResponseEventArgs(JsonRpcResponse<TR> response, string topic)
        {
            Response = response;
            Topic = topic;
        }
    }
}
