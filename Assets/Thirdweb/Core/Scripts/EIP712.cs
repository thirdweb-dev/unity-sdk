using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.Web3.Accounts;
using Thirdweb;
using UnityEngine;
using TokenERC20Contract = Thirdweb.Contracts.TokenERC20.ContractDefinition;
using TokenERC721Contract = Thirdweb.Contracts.TokenERC721.ContractDefinition;
using TokenERC1155Contract = Thirdweb.Contracts.TokenERC1155.ContractDefinition;

namespace Thirdweb
{
    public static class EIP712
    {
        public static string GenerateSignature_TokenERC20(Account account, string domainName, string version, BigInteger chainId, string verifyingContract, TokenERC20Contract.MintRequest mintRequest)
        {
            var signer = new Eip712TypedDataSigner();
            var key = new EthECKey(account.PrivateKey);
            var typedData = GetTypedDefinition_TokenERC20(domainName, version, chainId, verifyingContract);
            var signature = signer.SignTypedDataV4(mintRequest, typedData, key);

            Debug.Log("Typed Data JSON: " + typedData.ToJson(mintRequest));
            Debug.Log("Signing address: " + key.GetPublicAddress());
            Debug.Log("Signature: " + signature);

            var addressRecovered = signer.RecoverFromSignatureV4(mintRequest, typedData, signature);
            Debug.Log("Recovered address from signature:" + addressRecovered);

            return signature;
        }

        public static string GenerateSignature_TokenERC721(
            Account account,
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            TokenERC721Contract.MintRequest mintRequest
        )
        {
            var signer = new Eip712TypedDataSigner();
            var key = new EthECKey(account.PrivateKey);
            var typedData = GetTypedDefinition_TokenERC721(domainName, version, chainId, verifyingContract);
            var signature = signer.SignTypedDataV4(mintRequest, typedData, key);

            // Debug.Log("Typed Data JSON: " + typedData.ToJson(mintRequest));
            // Debug.Log("Signing address: " + key.GetPublicAddress());
            // Debug.Log("Signature: " + signature);

            // var addressRecovered = signer.RecoverFromSignatureV4(mintRequest, typedData, signature);
            // Debug.Log("Recovered address from signature:" + addressRecovered);

            return signature;
        }

        public static string GenerateSignature_TokenERC1155(
            Account account,
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            TokenERC1155Contract.MintRequest mintRequest
        )
        {
            var signer = new Eip712TypedDataSigner();
            var key = new EthECKey(account.PrivateKey);
            var typedData = GetTypedDefinition_TokenERC1155(domainName, version, chainId, verifyingContract);
            var signature = signer.SignTypedDataV4(mintRequest, typedData, key);

            // Debug.Log("Typed Data JSON: " + typedData.ToJson(mintRequest));
            // Debug.Log("Signing address: " + key.GetPublicAddress());
            // Debug.Log("Signature: " + signature);

            // var addressRecovered = signer.RecoverFromSignatureV4(mintRequest, typedData, signature);
            // Debug.Log("Recovered address from signature:" + addressRecovered);

            return signature;
        }

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
    }
}
