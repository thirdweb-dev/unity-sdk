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
using WalletConnectSharp.Core.Models.Ethereum.Types;
using WalletConnectSharp.Unity;
using Newtonsoft.Json;

namespace Thirdweb
{
    public static class EIP712
    {
        /// SIGNATURE GENERATION ///

        public static string GenerateSignature_TokenERC20(Account account, string domainName, string version, BigInteger chainId, string verifyingContract, TokenERC20Contract.MintRequest mintRequest)
        {
            var signer = new Eip712TypedDataSigner();
            var key = new EthECKey(account.PrivateKey);
            var typedData = GetTypedDefinition_TokenERC20(domainName, version, chainId, verifyingContract);
            var signature = signer.SignTypedDataV4(mintRequest, typedData, key);

            // Debug.Log("Typed Data JSON: " + typedData.ToJson(mintRequest));
            // Debug.Log("Signing address: " + key.GetPublicAddress());
            // Debug.Log("Signature: " + signature);

            // var addressRecovered = signer.RecoverFromSignatureV4(mintRequest, typedData, signature);
            // Debug.Log("Recovered address from signature:" + addressRecovered);

            return signature;
        }

        public async static Task<string> GenerateSignature_TokenERC721(string domainName, string version, BigInteger chainId, string verifyingContract, TokenERC721Contract.MintRequest mintRequest)
        {
            if (ThirdwebManager.Instance.SDK.nativeSession.account != null)
            {
                var signer = new Eip712TypedDataSigner();
                var key = new EthECKey(ThirdwebManager.Instance.SDK.nativeSession.account.PrivateKey);
                var typedData = GetTypedDefinition_TokenERC721(domainName, version, chainId, verifyingContract);
                var signature = signer.SignTypedDataV4(mintRequest, typedData, key);
                return signature;
            }
            else
            {
                if (Utils.ActiveWalletConnectSession())
                {
                    return await WalletConnect.Instance.SignTypedData(mintRequest, new EIP712Domain(domainName, version, (int)chainId, verifyingContract));
                }
                else
                {
                    throw new UnityException("No account connected!");
                }
            }
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
            return signature;
        }

        /// DOMAIN TYPES ///

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

        /// RELAYER TYPES ///

        public async static Task<string> SignTypedData_WalletConnect(RelayRequest req)
        {
            var address = WalletConnect.Instance.Session.Accounts[0];
            return await WalletConnect.Instance.SignTypedData(req, ForwarderDomain(await ThirdwebManager.Instance.SDK.wallet.GetChainId()));
        }

        public class GasData
        {
            [EvmType("uint256")]
            public virtual string gasLimit { get; set; }

            [EvmType("uint256")]
            public string gasPrice { get; set; }

            [EvmType("uint256")]
            public string baseRelayFee { get; set; }

            [EvmType("uint256")]
            public string pctRelayFee { get; set; }
        }

        public class RelayData
        {
            [EvmType("address")]
            public string senderAddress { get; set; }

            [EvmType("uint256")]
            public string senderNonce { get; set; }

            [EvmType("address")]
            public string relayWorker { get; set; }

            [EvmType("address")]
            public string paymaster { get; set; }
        }

        public class RelayRequest
        {
            public string target;

            public string encodedFunction;

            public GasData gasData;

            public RelayData relayData;

            public RelayRequest() { }

            public RelayRequest(string target, string encodedFunction, GasData gasData, RelayData relayData)
            {
                this.target = target;
                this.encodedFunction = encodedFunction;
                this.gasData = gasData;
                this.relayData = relayData;
            }
        }

        // public static RelayRequest ExampleData = new RelayRequest()
        // {
        //     target = "0x9cf40ef3d1622efe270fe6fe720585b4be4eeeff",
        //     encodedFunction = "0xa9059cbb0000000000000000000000002e0d94754b348d208d64d52d78bcd443afa9fa520000000000000000000000000000000000000000000000000000000000000007",
        //     gasData = new GasData()
        //     {
        //         gasLimit = "39507",
        //         gasPrice = "1700000000",
        //         pctRelayFee = "70",
        //         baseRelayFee = "0"
        //     },
        //     relayData = new RelayData()
        //     {
        //         senderAddress = "0x22d491bde2303f2f43325b2108d26f1eaba1e32b",
        //         senderNonce = "3",
        //         relayWorker = "0x3baee457ad824c94bd3953183d725847d023a2cf",
        //         paymaster = "0x957F270d45e9Ceca5c5af2b49f1b5dC1Abb0421c"
        //     }
        // };

        public static EIP712Domain ForwarderDomain(int chainId)
        {
            return new EIP712Domain("GSNv2 Forwarder", "0.0.1", chainId, FORWARDER_ADDRESS);
        }

        public static string FORWARDER_ADDRESS = "0x6453D37248Ab2C16eBd1A8f782a2CBC65860E60B";
    }
}
