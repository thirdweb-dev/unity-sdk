using WalletConnectSharp.Core.Models.Verify;
using WalletConnectSharp.Network.Models;

namespace WalletConnectSharp.Sign.Models
{
    /// <summary>
    /// Event args that are used in <see cref="TypedEventHandler{T,TR}"/> and <see cref="SessionRequestEventHandler{T,TR}"/>
    /// when a new response of the given type TR is received.
    /// Stores information about the current request, such as the topic, and the request.
    ///
    /// These event args can also set the corresponding Response or Error, which will be sent automatically after
    /// the event has finished propagating
    /// </summary>
    /// <typeparam name="T">The request type</typeparam>
    /// <typeparam name="TR">The response type</typeparam>
    public class RequestEventArgs<T, TR>
    {
        /// <summary>
        /// The topic this request came from
        /// </summary>
        public string Topic { get; }
        
        /// <summary>
        /// The request data for this event
        /// </summary>
        public JsonRpcRequest<T> Request { get; }
        
        /// <summary>
        /// The current response to send when this event finishes propagating. You can set this value
        /// to send a response when this event completes.
        /// If the <see cref="Error"/> field is non-null, then this field will not be sent and the
        /// <see cref="Error"/> will be sent instead
        /// </summary>
        public TR Response { get; set; }
        
        /// <summary>
        /// The current error to send when this event finishes propagating. You can set this value
        /// to send an Error response when this event completes.
        /// This value will always override <see cref="Response"/> if the value is non-null
        /// </summary>
        public Error Error { get; set; }
        
        public VerifiedContext VerifiedContext { get; set; }

        internal RequestEventArgs(string topic, JsonRpcRequest<T> request, VerifiedContext context)
        {
            Topic = topic;
            Request = request;
            VerifiedContext = context;
        }
    }
}
