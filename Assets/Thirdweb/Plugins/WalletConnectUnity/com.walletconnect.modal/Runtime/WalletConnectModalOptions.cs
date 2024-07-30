using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnectUnity.Modal
{
    public sealed class WalletConnectModalOptions
    {
        public ConnectOptions ConnectOptions { get; set; }

        public string[] IncludedWalletIds { get; set; }

        public string[] ExcludedWalletIds { get; set; }
    }
}