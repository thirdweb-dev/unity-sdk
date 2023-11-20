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
using Nethereum.Model;

namespace Thirdweb
{
    public static class EIP712
    {
        /// SIGNATURE GENERATION ///

        public async static Task<string> GenerateSignature_MinimalForwarder(
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
                return await ThirdwebManager.Instance.SDK.wallet.SignTypedDataV4(forwardRequest, typedData);
            }
        }

        public async static Task<string> GenerateSignature_TokenERC20(
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
                return await ThirdwebManager.Instance.SDK.wallet.SignTypedDataV4(mintRequest, typedData);
            }
        }

        public async static Task<string> GenerateSignature_TokenERC721(
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
                return await ThirdwebManager.Instance.SDK.wallet.SignTypedDataV4(mintRequest, typedData);
            }
        }

        public async static Task<string> GenerateSignature_TokenERC1155(
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
                return await ThirdwebManager.Instance.SDK.wallet.SignTypedDataV4(mintRequest, typedData);
            }
        }

        public async static Task<string> GenerateSignature_SmartAccount(
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
                return await ThirdwebManager.Instance.SDK.wallet.SignTypedDataV4(signerPermissionRequest, typedData);
            }
        }

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

        #endregion
    }
}
