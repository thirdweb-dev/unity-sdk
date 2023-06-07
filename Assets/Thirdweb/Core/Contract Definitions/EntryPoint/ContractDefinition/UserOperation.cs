using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.EntryPoint.ContractDefinition
{
    public partial class UserOperation : UserOperationBase { }

    public class UserOperationBase
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
}
