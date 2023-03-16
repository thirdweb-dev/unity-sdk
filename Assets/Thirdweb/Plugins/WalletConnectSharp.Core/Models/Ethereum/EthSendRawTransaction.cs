namespace WalletConnectSharp.Core.Models.Ethereum
{
    public class EthSendRawTransaction : EthGenericRequest<string>
    {
        public EthSendRawTransaction(string rawTransaction)
            : base(ValidJsonRpcRequestMethods.EthSendRawTransaction, rawTransaction) { }
    }
}
