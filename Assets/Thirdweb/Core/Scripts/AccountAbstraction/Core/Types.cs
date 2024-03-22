using System.Numerics;
using System.Text.Json.Serialization;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;

namespace Thirdweb.AccountAbstraction
{
    public class EthEstimateUserOperationGasResponse
    {
        public string preVerificationGas { get; set; }
        public string verificationGasLimit { get; set; }
        public string callGasLimit { get; set; }
        public string paymasterVerificationGasLimit { get; set; }
        public string paymasterPostOpGasLimit { get; set; }
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
        public string paymaster { get; set; }
        public string paymasterData { get; set; }
        public string preVerificationGas { get; set; }
        public string verificationGasLimit { get; set; }
        public string callGasLimit { get; set; }
        public string paymasterVerificationGasLimit { get; set; }
        public string paymasterPostOpGasLimit { get; set; }
    }

    public class PackedUserOperation
    {
        [Parameter("address", "sender", 1)]
        public virtual string Sender { get; set; }

        [Parameter("uint256", "nonce", 2)]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("bytes", "initCode", 3)]
        public virtual byte[] InitCode { get; set; }

        [Parameter("bytes", "callData", 4)]
        public virtual byte[] CallData { get; set; }

        [Parameter("bytes32", "accountGasLimits", 5)]
        public virtual byte[] AccountGasLimits { get; set; }

        [Parameter("uint256", "preVerificationGas", 6)]
        public virtual BigInteger PreVerificationGas { get; set; }

        [Parameter("bytes32", "gasFees", 7)]
        public virtual byte[] GasFees { get; set; }

        [Parameter("bytes", "paymasterAndData", 8)]
        public virtual byte[] PaymasterAndData { get; set; }

        [Parameter("bytes", "signature", 9)]
        public virtual byte[] Signature { get; set; }
    }

    [Function("getUserOpHash", "bytes32")]
    public class GetUserOpHashFunctionV0_6 : FunctionMessage
    {
        [Parameter("tuple", "userOp", 1)]
        public virtual UserOperationV0_6 UserOp { get; set; }
    }

    [Function("getUserOpHash", "bytes32")]
    public class GetUserOpHashFunctionV0_7 : FunctionMessage
    {
        [Parameter("tuple", "userOp", 1)]
        public virtual PackedUserOperation UserOp { get; set; }
    }

    [FunctionOutput]
    public class GetUserOpHashOutputDTO : IFunctionOutputDTO
    {
        [Parameter("bytes32", "", 1)]
        public virtual byte[] ReturnValue1 { get; set; }
    }

    public class UserOperationV0_6
    {
        [Parameter("address", "sender", 1)]
        public virtual string Sender { get; set; }

        [Parameter("uint256", "nonce", 2)]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("bytes", "initCode", 3)]
        public virtual byte[] InitCode { get; set; }

        [Parameter("bytes", "callData", 4)]
        public virtual byte[] CallData { get; set; }

        [Parameter("uint256", "callGasLimit", 5)]
        public virtual BigInteger CallGasLimit { get; set; }

        [Parameter("uint256", "verificationGasLimit", 6)]
        public virtual BigInteger VerificationGasLimit { get; set; }

        [Parameter("uint256", "preVerificationGas", 7)]
        public virtual BigInteger PreVerificationGas { get; set; }

        [Parameter("uint256", "maxFeePerGas", 8)]
        public virtual BigInteger MaxFeePerGas { get; set; }

        [Parameter("uint256", "maxPriorityFeePerGas", 9)]
        public virtual BigInteger MaxPriorityFeePerGas { get; set; }

        [Parameter("bytes", "paymasterAndData", 10)]
        public virtual byte[] PaymasterAndData { get; set; }

        [Parameter("bytes", "signature", 11)]
        public virtual byte[] Signature { get; set; }
    }

    public class UserOperationV0_7
    {
        [Parameter("address", "sender", 1)]
        public virtual string Sender { get; set; }

        [Parameter("uint256", "nonce", 2)]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("address", "factory", 3)]
        public virtual string Factory { get; set; }

        [Parameter("bytes", "factoryData", 4)]
        public virtual byte[] FactoryData { get; set; }

        [Parameter("bytes", "callData", 5)]
        public virtual byte[] CallData { get; set; }

        [Parameter("uint256", "callGasLimit", 6)]
        public virtual BigInteger CallGasLimit { get; set; }

        [Parameter("uint256", "verificationGasLimit", 7)]
        public virtual BigInteger VerificationGasLimit { get; set; }

        [Parameter("uint256", "preVerificationGas", 8)]
        public virtual BigInteger PreVerificationGas { get; set; }

        [Parameter("uint256", "maxFeePerGas", 9)]
        public virtual BigInteger MaxFeePerGas { get; set; }

        [Parameter("uint256", "maxPriorityFeePerGas", 10)]
        public virtual BigInteger MaxPriorityFeePerGas { get; set; }

        [Parameter("address", "paymaster", 11)]
        public virtual string Paymaster { get; set; }

        [Parameter("uint256", "paymasterVerificationGasLimit", 12)]
        public virtual BigInteger PaymasterVerificationGasLimit { get; set; }

        [Parameter("uint256", "paymasterPostOpGasLimit", 13)]
        public virtual BigInteger PaymasterPostOpGasLimit { get; set; }

        [Parameter("bytes", "paymasterData", 14)]
        public virtual byte[] PaymasterData { get; set; }

        [Parameter("bytes", "signature", 15)]
        public virtual byte[] Signature { get; set; }
    }

    public class UserOperationHexifiedV0_6
    {
        public string sender { get; set; }
        public string nonce { get; set; }
        public string initCode { get; set; }
        public string callData { get; set; }
        public string callGasLimit { get; set; }
        public string verificationGasLimit { get; set; }
        public string preVerificationGas { get; set; }
        public string maxFeePerGas { get; set; }
        public string maxPriorityFeePerGas { get; set; }
        public string paymasterAndData { get; set; }
        public string signature { get; set; }
    }

    public class UserOperationHexifiedV0_7
    {
        public string sender { get; set; }
        public string nonce { get; set; }
        public string factory { get; set; }
        public string factoryData { get; set; }
        public string callData { get; set; }
        public string callGasLimit { get; set; }
        public string verificationGasLimit { get; set; }
        public string preVerificationGas { get; set; }
        public string maxFeePerGas { get; set; }
        public string maxPriorityFeePerGas { get; set; }
        public string paymaster { get; set; }
        public string paymasterVerificationGasLimit { get; set; }
        public string paymasterPostOpGasLimit { get; set; }
        public string paymasterData { get; set; }
        public string signature { get; set; }
    }
}
