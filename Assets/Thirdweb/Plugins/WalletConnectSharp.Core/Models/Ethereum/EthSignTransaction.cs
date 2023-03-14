namespace WalletConnectSharp.Core.Models.Ethereum
{
    public sealed class EthSignTransaction : EthGenericRequest<TransactionData>
    {
        public EthSignTransaction(params TransactionData[] transactionDatas)
            : base(ValidJsonRpcRequestMethods.EthSignTransaction, transactionDatas) { }
    }
}
