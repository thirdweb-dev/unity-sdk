using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using Nethereum.Signer;
using Nethereum.Web3;
using EntryPointContract = Thirdweb.Contracts.EntryPoint.ContractDefinition;

namespace Thirdweb.AccountAbstraction
{
    public static class UserOpUtils
    {
        public static async Task<byte[]> HashAndSignUserOp(this EntryPointContract.UserOperation userOp, string entryPoint)
        {
            var userOpHash = await TransactionManager.ThirdwebRead<EntryPointContract.GetUserOpHashFunction, EntryPointContract.GetUserOpHashOutputDTO>(
                entryPoint,
                new EntryPointContract.GetUserOpHashFunction() { UserOp = userOp }
            );
            if (ThirdwebManager.Instance.SDK.session.SmartWallet.PersonalWalletProvider == WalletProvider.LocalWallet)
            {
                return new EthereumMessageSigner().Sign(userOpHash.ReturnValue1, new EthECKey(ThirdwebManager.Instance.SDK.session.LocalAccount.PrivateKey)).HexStringToByteArray();
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
