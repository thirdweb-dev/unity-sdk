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
using TokenERC721Contract = Thirdweb.Contracts.TokenERC721.ContractDefinition;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Text;

namespace Thirdweb
{
    public class EIP712
    {
        public static string GenerateSignatureForSignatureMint(Account account, BigInteger chainId, string verifyingContract, TokenERC721Contract.MintRequest mintRequest)
        {
            var signer = new Eip712TypedDataSigner();
            var key = new EthECKey(account.PrivateKey);
            var typedData = GetSignatureMintTypedDefinition(chainId, verifyingContract);
            typedData.Domain.ChainId = chainId;
            var signature = signer.SignTypedDataV4(mintRequest, typedData, key);
            var addressRecovered = signer.RecoverFromSignatureV4(mintRequest, typedData, signature);

            // Debug.Log("Typed Data JSON: " + typedData.ToJson(mintRequest));
            Debug.Log("Signing address: " + key.GetPublicAddress());
            Debug.Log("Signature: " + signature);
            Debug.Log("Recovered address from signature:" + addressRecovered);

            return signature;
        }

        public static TypedData<Domain> GetSignatureMintTypedDefinition(BigInteger chainId, string verifyingContract)
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = "TokenERC721",
                    Version = "1",
                    ChainId = chainId,
                    VerifyingContract = verifyingContract,
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(TokenERC721Contract.MintRequest)),
                PrimaryType = nameof(TokenERC721Contract.MintRequest),
            };
        }
    }
}
