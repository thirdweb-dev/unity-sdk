using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.EntryPoint.ContractDefinition
{
    public partial class MemoryUserOp : MemoryUserOpBase { }

    public class MemoryUserOpBase
    {
        [Parameter("address", "sender", 1)]
        public virtual string Sender { get; set; }

        [Parameter("uint256", "nonce", 2)]
        public virtual BigInteger Nonce { get; set; }

        [Parameter("uint256", "callGasLimit", 3)]
        public virtual BigInteger CallGasLimit { get; set; }

        [Parameter("uint256", "verificationGasLimit", 4)]
        public virtual BigInteger VerificationGasLimit { get; set; }

        [Parameter("uint256", "preVerificationGas", 5)]
        public virtual BigInteger PreVerificationGas { get; set; }

        [Parameter("address", "paymaster", 6)]
        public virtual string Paymaster { get; set; }

        [Parameter("uint256", "maxFeePerGas", 7)]
        public virtual BigInteger MaxFeePerGas { get; set; }

        [Parameter("uint256", "maxPriorityFeePerGas", 8)]
        public virtual BigInteger MaxPriorityFeePerGas { get; set; }
    }
}
