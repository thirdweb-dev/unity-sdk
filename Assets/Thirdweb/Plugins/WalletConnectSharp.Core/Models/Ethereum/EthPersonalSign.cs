namespace WalletConnectSharp.Core.Models.Ethereum
{
    public sealed class EthPersonalSign : EthGenericRequest<string>
    {
        public EthPersonalSign(string address, string hexData, string password = "")
            : base(ValidJsonRpcRequestMethods.PersonalSign, hexData, address, password) { }
    }
}
