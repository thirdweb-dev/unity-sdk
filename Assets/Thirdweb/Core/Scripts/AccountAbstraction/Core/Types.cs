using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb.AccountAbstraction
{
    public class EthEstimateUserOperationGasResponse
    {
        public string PreVerificationGas { get; set; }
        public string VerificationGas { get; set; }
        public string CallGasLimit { get; set; }
    }

    public class EthGetUserOperationByHashResponse
    {
        public string entryPoint { get; set; }
        public string transactionHash { get; set; }
        public string blockHash { get; set; }
        public string blockNumber { get; set; }
    }

    public class EthGetUserOperationReceiptResponse
    {
        public TransactionReceipt receipt { get; set; }
    }

    public class EntryPointWrapper
    {
        public string entryPoint { get; set; }
    }

    public class PMSponsorOperationResponse
    {
        public string paymasterAndData { get; set; }
    }

    public class ThirdwebGetUserOperationGasPriceResponse
    {
        public string maxFeePerGas { get; set; }
        public string maxPriorityFeePerGas { get; set; }
    }
}
