using System;

using MetaMask.Models;

namespace MetaMask.Transports
{

    /// <summary>
    /// A transport is responsible for getting the request to the
    /// user, e.g. by opening request URIs or displaying QR codes.
    /// </summary>
    public interface IMetaMaskTransport
    {

        /// <summary>
        /// Gets the transport's user agent to be used in headers.
        /// </summary>
        string UserAgent { get; }

        /// <summary>
        /// Initialize the transport.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Present a connection request to the user.
        /// </summary>
        /// <param name="url"></param>
        void Connect(string url);

        void OnConnectRequest(string url);

        /// <summary>
        /// Present a request to the user.
        /// </summary>
        void OnRequest(string id, MetaMaskEthereumRequest request);

        /// <summary>
        /// Present a session-based request to the user.
        /// </summary>
        /// <param name="session"></param>
        void OnSessionRequest(MetaMaskSessionData session);

        /// <summary>
        /// Called when the request has failed.
        /// </summary>
        /// <param name="error"></param>
        void OnFailure(Exception error);

        /// <summary>
        /// Called when the request has been successful.
        /// </summary>
        void OnSuccess();

    }
}
