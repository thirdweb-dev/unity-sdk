namespace WalletConnectSharp.Core.Models.Ethereum
{
    public class WalletAddEthChain : EthGenericRequest<EthChainData>
    {
        public WalletAddEthChain(EthChainData chainData)
            : base(ValidJsonRpcRequestMethods.WalletAddEthereumChain, chainData) { }
    }
}
