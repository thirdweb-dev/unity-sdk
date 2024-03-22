using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.Signer;
using System.Security.Cryptography;
using EntryPointContract = Thirdweb.Contracts.EntryPoint.ContractDefinition;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Linq;
using Nethereum.Util;
using System;
using System.Diagnostics;
using NBitcoin;

namespace Thirdweb.AccountAbstraction
{
    public static class UserOpUtils
    {
        public static BigInteger GetRandomInt192()
        {
            byte[] randomBytes = GetRandomBytes(24); // 192 bits = 24 bytes
            BigInteger randomInt = new(randomBytes);
            randomInt = BigInteger.Abs(randomInt) % (BigInteger.One << 192);
            return randomInt;
        }

        private static byte[] GetRandomBytes(int byteCount)
        {
            using (RNGCryptoServiceProvider rng = new())
            {
                byte[] randomBytes = new byte[byteCount];
                rng.GetBytes(randomBytes);
                return randomBytes;
            }
        }

        public static async Task<byte[]> HashAndSignUserOp(this UserOperationV0_6 userOp, string entryPoint)
        {
            var userOpHash = await TransactionManager.ThirdwebRead<GetUserOpHashFunctionV0_6, GetUserOpHashOutputDTO>(entryPoint, new GetUserOpHashFunctionV0_6() { UserOp = userOp });

            var smartWallet = ThirdwebManager.Instance.SDK.session.ActiveWallet;
            if (smartWallet.GetLocalAccount() != null)
            {
                var localWallet = smartWallet.GetLocalAccount();
                return new EthereumMessageSigner().Sign(userOpHash.ReturnValue1, new EthECKey(localWallet.PrivateKey)).HexStringToByteArray();
            }
            else
            {
                var sig = await ThirdwebManager.Instance.SDK.session.Request<string>(
                    "personal_sign",
                    userOpHash.ReturnValue1.ByteArrayToHexString(),
                    await ThirdwebManager.Instance.SDK.wallet.GetSignerAddress()
                );
                return sig.HexStringToByteArray();
            }
        }

        public static UserOperationHexifiedV0_6 EncodeUserOperation(this UserOperationV0_6 userOperation)
        {
            return new UserOperationHexifiedV0_6()
            {
                sender = userOperation.Sender,
                nonce = userOperation.Nonce.ToHexBigInteger().HexValue,
                initCode = userOperation.InitCode.ByteArrayToHexString(),
                callData = userOperation.CallData.ByteArrayToHexString(),
                callGasLimit = userOperation.CallGasLimit.ToHexBigInteger().HexValue,
                verificationGasLimit = userOperation.VerificationGasLimit.ToHexBigInteger().HexValue,
                preVerificationGas = userOperation.PreVerificationGas.ToHexBigInteger().HexValue,
                maxFeePerGas = userOperation.MaxFeePerGas.ToHexBigInteger().HexValue,
                maxPriorityFeePerGas = userOperation.MaxPriorityFeePerGas.ToHexBigInteger().HexValue,
                paymasterAndData = userOperation.PaymasterAndData.ByteArrayToHexString(),
                signature = userOperation.Signature.ByteArrayToHexString()
            };
        }

        public static async Task<byte[]> HashAndSignUserOp(this UserOperationV0_7 userOp, string entryPoint)
        {
            byte[] factoryBytes = userOp.Factory.HexStringToByteArray();
            byte[] factoryDataBytes = userOp.FactoryData;
            byte[] initCodeBuffer = new byte[factoryBytes.Length + factoryDataBytes.Length];
            Buffer.BlockCopy(factoryBytes, 0, initCodeBuffer, 0, factoryBytes.Length);
            Buffer.BlockCopy(factoryDataBytes, 0, initCodeBuffer, factoryBytes.Length, factoryDataBytes.Length);

            byte[] verificationGasLimitBytes = userOp.VerificationGasLimit.ToHexBigInteger().HexValue.HexStringToByteArray().PadBytes(16);
            byte[] callGasLimitBytes = userOp.CallGasLimit.ToHexBigInteger().HexValue.HexStringToByteArray().PadBytes(16);
            byte[] accountGasLimitsBuffer = new byte[32];
            Buffer.BlockCopy(verificationGasLimitBytes, 0, accountGasLimitsBuffer, 0, 16);
            Buffer.BlockCopy(callGasLimitBytes, 0, accountGasLimitsBuffer, 16, 16);

            byte[] maxPriorityFeePerGasBytes = userOp.MaxPriorityFeePerGas.ToHexBigInteger().HexValue.HexStringToByteArray().PadBytes(16);
            byte[] maxFeePerGasBytes = userOp.MaxFeePerGas.ToHexBigInteger().HexValue.HexStringToByteArray().PadBytes(16);
            byte[] gasFeesBuffer = new byte[32];
            Buffer.BlockCopy(maxPriorityFeePerGasBytes, 0, gasFeesBuffer, 0, 16);
            Buffer.BlockCopy(maxFeePerGasBytes, 0, gasFeesBuffer, 16, 16);

            byte[] paymasterBytes = userOp.Paymaster.HexStringToByteArray();
            byte[] paymasterVerificationGasLimitBytes = userOp.PaymasterVerificationGasLimit.ToHexBigInteger().HexValue.HexStringToByteArray().PadBytes(16);
            byte[] paymasterPostOpGasLimitBytes = userOp.PaymasterPostOpGasLimit.ToHexBigInteger().HexValue.HexStringToByteArray().PadBytes(16);
            byte[] paymasterDataBytes = userOp.PaymasterData;
            byte[] paymasterAndDataBuffer = new byte[20 + 16 + 16 + paymasterDataBytes.Length];
            Buffer.BlockCopy(paymasterBytes, 0, paymasterAndDataBuffer, 0, 20);
            Buffer.BlockCopy(paymasterVerificationGasLimitBytes, 0, paymasterAndDataBuffer, 20, 16);
            Buffer.BlockCopy(paymasterPostOpGasLimitBytes, 0, paymasterAndDataBuffer, 20 + 16, 16);
            Buffer.BlockCopy(paymasterDataBytes, 0, paymasterAndDataBuffer, 20 + 16 + 16, paymasterDataBytes.Length);

            var packedOp = new PackedUserOperation()
            {
                Sender = userOp.Sender,
                Nonce = userOp.Nonce,
                InitCode = initCodeBuffer,
                CallData = userOp.CallData,
                AccountGasLimits = accountGasLimitsBuffer,
                PreVerificationGas = userOp.PreVerificationGas,
                GasFees = gasFeesBuffer,
                PaymasterAndData = paymasterAndDataBuffer,
                Signature = userOp.Signature
            };

            var userOpHash = await TransactionManager.ThirdwebRead<GetUserOpHashFunctionV0_7, GetUserOpHashOutputDTO>(entryPoint, new GetUserOpHashFunctionV0_7() { UserOp = packedOp });

            var smartWallet = ThirdwebManager.Instance.SDK.session.ActiveWallet;
            if (smartWallet.GetLocalAccount() != null)
            {
                var localWallet = smartWallet.GetLocalAccount();
                return new EthereumMessageSigner().Sign(userOpHash.ReturnValue1, new EthECKey(localWallet.PrivateKey)).HexStringToByteArray();
            }
            else
            {
                var sig = await ThirdwebManager.Instance.SDK.session.Request<string>(
                    "personal_sign",
                    userOpHash.ReturnValue1.ByteArrayToHexString(),
                    await ThirdwebManager.Instance.SDK.wallet.GetSignerAddress()
                );
                return sig.HexStringToByteArray();
            }
        }

        public static UserOperationHexifiedV0_7 EncodeUserOperation(this UserOperationV0_7 userOperation)
        {
            return new UserOperationHexifiedV0_7()
            {
                sender = userOperation.Sender,
                nonce = userOperation.Nonce.ToHexBigInteger().HexValue,
                factory = userOperation.Factory,
                factoryData = userOperation.FactoryData.ByteArrayToHexString(),
                callData = userOperation.CallData.ByteArrayToHexString(),
                callGasLimit = userOperation.CallGasLimit.ToHexBigInteger().HexValue,
                verificationGasLimit = userOperation.VerificationGasLimit.ToHexBigInteger().HexValue,
                preVerificationGas = userOperation.PreVerificationGas.ToHexBigInteger().HexValue,
                maxFeePerGas = userOperation.MaxFeePerGas.ToHexBigInteger().HexValue,
                maxPriorityFeePerGas = userOperation.MaxPriorityFeePerGas.ToHexBigInteger().HexValue,
                paymaster = userOperation.Paymaster,
                paymasterVerificationGasLimit = userOperation.PaymasterVerificationGasLimit.ToHexBigInteger().HexValue,
                paymasterPostOpGasLimit = userOperation.PaymasterPostOpGasLimit.ToHexBigInteger().HexValue,
                paymasterData = userOperation.PaymasterData.ByteArrayToHexString(),
                signature = userOperation.Signature.ByteArrayToHexString()
            };
        }
    }
}
