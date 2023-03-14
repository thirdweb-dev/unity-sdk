namespace WalletConnectSharp.Core.Models
{
    public class JsonRpcRequestMethod
    {
        public string Name { get; set; }
        public bool IsSigning { get; set; }
        public bool IsRedirect { get; set; }

        public override string ToString() => Name;
    }
}
