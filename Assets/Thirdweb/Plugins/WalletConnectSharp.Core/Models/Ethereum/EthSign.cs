namespace WalletConnectSharp.Core.Models.Ethereum
{
    public sealed class EthSign : EthGenericRequest<string>
    {
        public EthSign(string address, string hexData)
            : base(ValidJsonRpcRequestMethods.EthSign, address, hexData) { }
    }
}
