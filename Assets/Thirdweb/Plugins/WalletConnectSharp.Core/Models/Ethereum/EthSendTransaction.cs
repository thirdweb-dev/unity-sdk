namespace WalletConnectSharp.Core.Models.Ethereum
{
    public sealed class EthSendTransaction : EthGenericRequest<TransactionData>
    {
        public EthSendTransaction(params TransactionData[] transactionDatas)
            : base(ValidJsonRpcRequestMethods.EthSendTransaction, transactionDatas) { }
    }
}
