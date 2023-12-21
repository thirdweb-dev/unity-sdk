using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.Signer;
using System.Security.Cryptography;
using EntryPointContract = Thirdweb.Contracts.EntryPoint.ContractDefinition;

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

        public static async Task<byte[]> HashAndSignUserOp(this EntryPointContract.UserOperation userOp, string entryPoint)
        {
            var userOpHash = await TransactionManager.ThirdwebRead<EntryPointContract.GetUserOpHashFunction, EntryPointContract.GetUserOpHashOutputDTO>(
                entryPoint,
                new EntryPointContract.GetUserOpHashFunction() { UserOp = userOp }
            );

            var smartWallet = ThirdwebManager.Instance.SDK.session.ActiveWallet;
            if (smartWallet.GetLocalAccount() != null)
            {
                var localWallet = smartWallet.GetLocalAccount();
                return new EthereumMessageSigner().Sign(userOpHash.ReturnValue1, new EthECKey(localWallet.PrivateKey)).HexStringToByteArray();
            }
            else
            {
                var sig = await ThirdwebManager.Instance.SDK.wallet.Sign(userOpHash.ReturnValue1.ByteArrayToHexString());
                return sig.HexStringToByteArray();
            }
        }

        public static UserOperationHexified EncodeUserOperation(this EntryPointContract.UserOperation userOperation)
        {
            return new UserOperationHexified()
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
    }
}
