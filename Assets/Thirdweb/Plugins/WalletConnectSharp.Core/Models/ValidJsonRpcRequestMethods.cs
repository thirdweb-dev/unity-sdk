namespace WalletConnectSharp.Core.Models
{
    public static class ValidJsonRpcRequestMethods
    {
        public static readonly string EthFeeHistory = "eth_feeHistory";
        public static readonly string EthSendTransaction = "eth_sendTransaction";
        public static readonly string EthSendRawTransaction = "eth_sendRawTransaction";
        public static readonly string EthSign = "eth_sign";
        public static readonly string EthSignTransaction = "eth_signTransaction";
        public static readonly string EthSignTypedData = "eth_signTypedData";
        public static readonly string EthSignTypedDataV1 = "eth_signTypedData_v1";
        public static readonly string EthSignTypedDataV2 = "eth_signTypedData_v2";
        public static readonly string EthSignTypedDataV3 = "eth_signTypedData_v3";
        public static readonly string EthSignTypedDataV4 = "eth_signTypedData_v4";
        public static readonly string PersonalSign = "personal_sign";
        public static readonly string WalletAddEthereumChain = "wallet_addEthereumChain";
        public static readonly string WalletSwitchEthereumChain = "wallet_switchEthereumChain";
    }
}
