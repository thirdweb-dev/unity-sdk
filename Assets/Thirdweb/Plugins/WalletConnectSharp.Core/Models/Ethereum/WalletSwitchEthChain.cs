namespace WalletConnectSharp.Core.Models.Ethereum
{
    public class WalletSwitchEthChain : EthGenericRequest<EthChain>
    {
        public WalletSwitchEthChain(params EthChain[] chainId)
            : base(ValidJsonRpcRequestMethods.WalletSwitchEthereumChain, chainId) { }
    }
}
