using System.Threading.Tasks;

namespace WalletConnectSharp.Sign.Models.Engine
{
    /// <summary>
    /// A class that representing a pending session proposal. Includes a URI that can be given to a
    /// wallet app out-of-band and an Approval Task that can be awaited.
    /// </summary>
    public class ConnectedData
    {
        /// <summary>
        /// The URI that can be used to retrieve the submitted session proposal. This should be shared
        /// SECURELY out-of-band to a wallet supporting the SDK
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// A task that will resolve to an approved session. If the session proposal is rejected, then this
        /// task will throw an exception.
        /// </summary>
        public Task<SessionStruct> Approval { get; set; }
    }
}
