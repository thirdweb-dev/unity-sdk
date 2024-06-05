using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.EIP712;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using TokenERC20Contract = Thirdweb.Contracts.TokenERC20.ContractDefinition;
using TokenERC721Contract = Thirdweb.Contracts.TokenERC721.ContractDefinition;
using TokenERC1155Contract = Thirdweb.Contracts.TokenERC1155.ContractDefinition;
using MinimalForwarder = Thirdweb.Contracts.Forwarder.ContractDefinition;
using AccountContract = Thirdweb.Contracts.Account.ContractDefinition;
using System;
using System.Collections.Generic;
using Nethereum.Model;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using System.Linq;

namespace Thirdweb
{
    public static class EIP712
    {
        #region Signature Generation

        public async static Task<string> GenerateSignature_MinimalForwarder(
            ThirdwebSDK sdk,
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            MinimalForwarder.ForwardRequest forwardRequest,
            string privateKeyOverride = null
        )
        {
            var typedData = GetTypedDefinition_MinimalForwarder(domainName, version, chainId, verifyingContract);

            if (privateKeyOverride != null)
            {
                var signer = new Eip712TypedDataSigner();
                var key = new EthECKey(privateKeyOverride);
                var signature = signer.SignTypedDataV4(forwardRequest, typedData, key);
                return signature;
            }
            else
            {
                return await sdk.Wallet.SignTypedDataV4(forwardRequest, typedData);
            }
        }

        public async static Task<string> GenerateSignature_TokenERC20(
            ThirdwebSDK sdk,
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            TokenERC20Contract.MintRequest mintRequest,
            string privateKeyOverride = null
        )
        {
            var typedData = GetTypedDefinition_TokenERC20(domainName, version, chainId, verifyingContract);

            if (privateKeyOverride != null)
            {
                var signer = new Eip712TypedDataSigner();
                var key = new EthECKey(privateKeyOverride);
                var signature = signer.SignTypedDataV4(mintRequest, typedData, key);
                return signature;
            }
            else
            {
                return await sdk.Wallet.SignTypedDataV4(mintRequest, typedData);
            }
        }

        public async static Task<string> GenerateSignature_TokenERC721(
            ThirdwebSDK sdk,
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            TokenERC721Contract.MintRequest mintRequest,
            string privateKeyOverride = null
        )
        {
            var typedData = GetTypedDefinition_TokenERC721(domainName, version, chainId, verifyingContract);

            if (privateKeyOverride != null)
            {
                var signer = new Eip712TypedDataSigner();
                var key = new EthECKey(privateKeyOverride);
                var signature = signer.SignTypedDataV4(mintRequest, typedData, key);
                return signature;
            }
            else
            {
                return await sdk.Wallet.SignTypedDataV4(mintRequest, typedData);
            }
        }

        public async static Task<string> GenerateSignature_TokenERC1155(
            ThirdwebSDK sdk,
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            TokenERC1155Contract.MintRequest mintRequest,
            string privateKeyOverride = null
        )
        {
            var typedData = GetTypedDefinition_TokenERC1155(domainName, version, chainId, verifyingContract);

            if (privateKeyOverride != null)
            {
                var signer = new Eip712TypedDataSigner();
                var key = new EthECKey(privateKeyOverride);
                var signature = signer.SignTypedDataV4(mintRequest, typedData, key);
                return signature;
            }
            else
            {
                return await sdk.Wallet.SignTypedDataV4(mintRequest, typedData);
            }
        }

        public async static Task<string> GenerateSignature_SmartAccount(
            ThirdwebSDK sdk,
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            AccountContract.SignerPermissionRequest signerPermissionRequest,
            string privateKeyOverride = null
        )
        {
            var typedData = GetTypedDefinition_SmartAccount(domainName, version, chainId, verifyingContract);

            if (privateKeyOverride != null)
            {
                var signer = new Eip712TypedDataSigner();
                var key = new EthECKey(privateKeyOverride);
                var signature = signer.SignTypedDataV4(signerPermissionRequest, typedData, key);
                return signature;
            }
            else
            {
                return await sdk.Wallet.SignTypedDataV4(signerPermissionRequest, typedData);
            }
        }

        public async static Task<string> GenerateSignature_SmartAccount_AccountMessage(ThirdwebSDK sdk, string domainName, string version, BigInteger chainId, string verifyingContract, byte[] message)
        {
            var typedData = GetTypedDefinition_SmartAccount_AccountMessage(domainName, version, chainId, verifyingContract);
            var accountMessage = new AccountMessage { Message = message };
            return await sdk.Wallet.SignTypedDataV4(accountMessage, typedData);
        }

        public static async Task<string> GenerateSignature_ZkSyncTransaction(ThirdwebSDK sdk, string domainName, string version, BigInteger chainId, AccountAbstraction.ZkSyncAATransaction transaction)
        {
            var typedData = GetTypedDefinition_ZkSyncTransaction(domainName, version, chainId);
            var signatureHex = await sdk.Wallet.SignTypedDataV4(transaction, typedData);
            var signatureRaw = EthECDSASignatureFactory.ExtractECDSASignature(signatureHex);
            return SerializeEip712(transaction, signatureRaw, chainId);
        }

        #endregion

        #region Typed Data Definitions

        public static TypedData<Domain> GetTypedDefinition_TokenERC20(string domainName, string version, BigInteger chainId, string verifyingContract)
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId,
                    VerifyingContract = verifyingContract,
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(TokenERC20Contract.MintRequest)),
                PrimaryType = nameof(TokenERC20Contract.MintRequest),
            };
        }

        public static TypedData<Domain> GetTypedDefinition_TokenERC721(string domainName, string version, BigInteger chainId, string verifyingContract)
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId,
                    VerifyingContract = verifyingContract,
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(TokenERC721Contract.MintRequest)),
                PrimaryType = nameof(TokenERC721Contract.MintRequest),
            };
        }

        public static TypedData<Domain> GetTypedDefinition_TokenERC1155(string domainName, string version, BigInteger chainId, string verifyingContract)
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId,
                    VerifyingContract = verifyingContract,
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(TokenERC1155Contract.MintRequest)),
                PrimaryType = nameof(TokenERC1155Contract.MintRequest),
            };
        }

        public static TypedData<Domain> GetTypedDefinition_MinimalForwarder(string domainName, string version, BigInteger chainId, string verifyingContract)
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId,
                    VerifyingContract = verifyingContract,
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(MinimalForwarder.ForwardRequest)),
                PrimaryType = nameof(MinimalForwarder.ForwardRequest),
            };
        }

        public static TypedData<Domain> GetTypedDefinition_SmartAccount(string domainName, string version, BigInteger chainId, string verifyingContract)
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId,
                    VerifyingContract = verifyingContract,
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(AccountContract.SignerPermissionRequest)),
                PrimaryType = nameof(AccountContract.SignerPermissionRequest),
            };
        }

        public static TypedData<Domain> GetTypedDefinition_SmartAccount_AccountMessage(string domainName, string version, BigInteger chainId, string verifyingContract)
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId,
                    VerifyingContract = verifyingContract,
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(AccountMessage)),
                PrimaryType = nameof(AccountMessage),
            };
        }

        public static TypedData<DomainWithNameVersionAndChainId> GetTypedDefinition_ZkSyncTransaction(string domainName, string version, BigInteger chainId)
        {
            return new TypedData<DomainWithNameVersionAndChainId>
            {
                Domain = new DomainWithNameVersionAndChainId
                {
                    Name = domainName,
                    Version = version,
                    ChainId = chainId,
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(DomainWithNameVersionAndChainId), typeof(AccountAbstraction.ZkSyncAATransaction)),
                PrimaryType = "Transaction",
            };
        }

        #endregion

        #region Helpers

        private static string SerializeEip712(AccountAbstraction.ZkSyncAATransaction transaction, EthECDSASignature signature, BigInteger chainId)
        {
            if (chainId == 0)
            {
                throw new ArgumentException("Chain ID must be provided for EIP712 transactions!");
            }

            var fields = new List<byte[]>
            {
                transaction.Nonce == 0 ? new byte[0] : transaction.Nonce.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.MaxPriorityFeePerGas == 0 ? new byte[0] : transaction.MaxPriorityFeePerGas.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.MaxFeePerGas.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.GasLimit.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.To.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.Value == 0 ? new byte[0] : transaction.Value.ToByteArray(isUnsigned: true, isBigEndian: true),
                transaction.Data == null ? new byte[0] : transaction.Data,
            };

            fields.Add(signature.IsVSignedForYParity() ? new byte[] { 0x1b } : new byte[] { 0x1c });
            fields.Add(signature.R);
            fields.Add(signature.S);

            fields.Add(chainId.ToByteArray(isUnsigned: true, isBigEndian: true));
            fields.Add(transaction.From.ToByteArray(isUnsigned: true, isBigEndian: true));

            // Add meta
            fields.Add(transaction.GasPerPubdataByteLimit.ToByteArray(isUnsigned: true, isBigEndian: true));
            fields.Add(new byte[] { }); // TODO: FactoryDeps
            fields.Add(signature.CreateStringSignature().HexToByteArray());
            // add array of rlp encoded paymaster/paymasterinput
            fields.Add(RLP.EncodeElement(transaction.Paymaster.ToByteArray(isUnsigned: true, isBigEndian: true)).Concat(RLP.EncodeElement(transaction.PaymasterInput)).ToArray());

            return "0x71" + RLP.EncodeDataItemsAsElementOrListAndCombineAsList(fields.ToArray(), new int[] { 13, 15 }).ToHex();
        }

        #endregion
    }

    public partial class AccountMessage : AccountMessageBase { }

    public class AccountMessageBase
    {
        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("bytes", "message", 1)]
        public virtual byte[] Message { get; set; }
    }
}
