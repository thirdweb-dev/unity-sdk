using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
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

    public class ZkPaymasterDataResponse
    {
        public string paymaster { get; set; }
        public string paymasterInput { get; set; }
    }

    public class ZkBroadcastTransactionResponse
    {
        public string transactionHash { get; set; }
    }

    [Struct("Transaction")]
    public class ZkSyncAATransaction
    {
        [Parameter("uint256", "txType", 1)]
        public virtual BigInteger TxType { get; set; }

        [Parameter("uint256", "from", 2)]
        public virtual BigInteger From { get; set; }

        [Parameter("uint256", "to", 3)]
        public virtual BigInteger To { get; set; }

        [Parameter("uint256", "gasLimit", 4)]
        public virtual BigInteger GasLimit { get; set; }

        [Parameter("uint256", "gasPerPubdataByteLimit", 5)]
        public virtual BigInteger GasPerPubdataByteLimit { get; set; }

        [Parameter("uint256", "maxFeePerGas", 6)]
        public virtual BigInteger MaxFeePerGas { get; set; }

        [Parameter("uint256", "maxPriorityFeePerGas", 7)]
        public virtual BigInteger MaxPriorityFeePerGas { get; set; }

        [Parameter("uint256", "paymaster", 8)]
        public virtual BigInteger Paymaster { get; set; }

        [Parameter("uint256", "nonce", 9)]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("uint256", "value", 10)]
        public virtual BigInteger Value { get; set; }

        [Parameter("bytes", "data", 11)]
        public virtual byte[] Data { get; set; }

        [Parameter("bytes32[]", "factoryDeps", 12)]
        public virtual List<byte[]> FactoryDeps { get; set; }

        [Parameter("bytes", "paymasterInput", 13)]
        public virtual byte[] PaymasterInput { get; set; }
    }
}
