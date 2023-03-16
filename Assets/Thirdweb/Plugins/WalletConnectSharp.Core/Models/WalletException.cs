using System.IO;

namespace WalletConnectSharp.Core.Models
{
    public class WalletException : IOException
    {
        public JsonRpcResponse.JsonRpcError RpcError { get; private set; }

        public WalletException(JsonRpcResponse.JsonRpcError error)
            : base(error.Message)
        {
            this.RpcError = error;
        }
    }
}
