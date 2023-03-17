using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using UnityEngine;
using TokenERC20Contract = Thirdweb.Contracts.TokenERC20.ContractDefinition;
using TokenERC721Contract = Thirdweb.Contracts.TokenERC721.ContractDefinition;
using TokenERC1155Contract = Thirdweb.Contracts.TokenERC1155.ContractDefinition;
using WalletConnectSharp.Unity;
using WalletConnectSharp.NEthereum;
using MinimalForwarder = Thirdweb.Contracts.Forwarder.ContractDefinition;

namespace Thirdweb
{
    public static class EIP712
    {
        /// SIGNATURE GENERATION ///

        public async static Task<string> GenerateSignature_TokenERC20(string domainName, string version, BigInteger chainId, string verifyingContract, TokenERC20Contract.MintRequest mintRequest)
        {
            if (ThirdwebManager.Instance.SDK.nativeSession.account != null)
            {
                var signer = new Eip712TypedDataSigner();
                var key = new EthECKey(ThirdwebManager.Instance.SDK.nativeSession.account.PrivateKey);
                var typedData = GetTypedDefinition_TokenERC20(domainName, version, chainId, verifyingContract);
                var signature = signer.SignTypedDataV4(mintRequest, typedData, key);
                return signature;
            }
            else
            {
                if (Utils.ActiveWalletConnectSession())
                {
                    var walletConnectMintRequest = new ERC20MintRequestWalletConnect()
                    {
                        To = mintRequest.To,
                        PrimarySaleRecipient = mintRequest.PrimarySaleRecipient,
                        Quantity = mintRequest.Quantity,
                        Price = mintRequest.Price,
                        Currency = mintRequest.Currency,
                        ValidityStartTimestamp = mintRequest.ValidityStartTimestamp,
                        ValidityEndTimestamp = mintRequest.ValidityEndTimestamp,
                        Uid = mintRequest.Uid.ByteArrayToHexString()
                    };
                    var typedData = GetTypedDefinition_TokenERC20(domainName, version, chainId, verifyingContract);
                    return await WalletConnect.Instance.Session.EthSignTypedData(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), walletConnectMintRequest, typedData);
                }
                else
                {
                    throw new UnityException("No account connected!");
                }
            }

            // Debug.Log("Typed Data JSON: " + typedData.ToJson(mintRequest));
            // Debug.Log("Signing address: " + key.GetPublicAddress());
            // Debug.Log("Signature: " + signature);

            // var addressRecovered = signer.RecoverFromSignatureV4(mintRequest, typedData, signature);
            // Debug.Log("Recovered address from signature:" + addressRecovered);
        }

        public partial class ERC20MintRequestWalletConnect : ERC20MintRequestBaseWalletConnect { }

        public class ERC20MintRequestBaseWalletConnect
        {
            [Parameter("address", "to", 1)]
            public virtual string To { get; set; }

            [Parameter("address", "primarySaleRecipient", 2)]
            public virtual string PrimarySaleRecipient { get; set; }

            [Parameter("uint256", "quantity", 3)]
            public virtual BigInteger Quantity { get; set; }

            [Parameter("uint256", "price", 4)]
            public virtual BigInteger Price { get; set; }

            [Parameter("address", "currency", 5)]
            public virtual string Currency { get; set; }

            [Parameter("uint128", "validityStartTimestamp", 6)]
            public virtual BigInteger ValidityStartTimestamp { get; set; }

            [Parameter("uint128", "validityEndTimestamp", 7)]
            public virtual BigInteger ValidityEndTimestamp { get; set; }

            [Parameter("bytes32", "uid", 8)]
            public virtual string Uid { get; set; }
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
                    var walletConnectMintRequest = new ERC721MintRequestWalletConnect()
                    {
                        To = mintRequest.To,
                        RoyaltyRecipient = mintRequest.RoyaltyRecipient,
                        RoyaltyBps = mintRequest.RoyaltyBps,
                        PrimarySaleRecipient = mintRequest.PrimarySaleRecipient,
                        Uri = mintRequest.Uri,
                        Price = mintRequest.Price,
                        Currency = mintRequest.Currency,
                        ValidityStartTimestamp = mintRequest.ValidityStartTimestamp,
                        ValidityEndTimestamp = mintRequest.ValidityEndTimestamp,
                        Uid = mintRequest.Uid.ByteArrayToHexString()
                    };
                    var typedData = GetTypedDefinition_TokenERC721(domainName, version, chainId, verifyingContract);
                    return await WalletConnect.Instance.Session.EthSignTypedData(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), walletConnectMintRequest, typedData);
                }
                else
                {
                    throw new UnityException("No account connected!");
                }
            }
        }

        public partial class ERC721MintRequestWalletConnect : ERC721MintRequestBaseWalletConnect { }

        public class ERC721MintRequestBaseWalletConnect
        {
            [Parameter("address", "to", 1)]
            public virtual string To { get; set; }

            [Parameter("address", "royaltyRecipient", 2)]
            public virtual string RoyaltyRecipient { get; set; }

            [Parameter("uint256", "royaltyBps", 3)]
            public virtual BigInteger RoyaltyBps { get; set; }

            [Parameter("address", "primarySaleRecipient", 4)]
            public virtual string PrimarySaleRecipient { get; set; }

            [Parameter("string", "uri", 5)]
            public virtual string Uri { get; set; }

            [Parameter("uint256", "price", 6)]
            public virtual BigInteger Price { get; set; }

            [Parameter("address", "currency", 7)]
            public virtual string Currency { get; set; }

            [Parameter("uint128", "validityStartTimestamp", 8)]
            public virtual BigInteger ValidityStartTimestamp { get; set; }

            [Parameter("uint128", "validityEndTimestamp", 9)]
            public virtual BigInteger ValidityEndTimestamp { get; set; }

            [Parameter("bytes32", "uid", 10)]
            public virtual string Uid { get; set; }
        }

        public async static Task<string> GenerateSignature_TokenERC1155(string domainName, string version, BigInteger chainId, string verifyingContract, TokenERC1155Contract.MintRequest mintRequest)
        {
            if (ThirdwebManager.Instance.SDK.nativeSession.account != null)
            {
                var signer = new Eip712TypedDataSigner();
                var key = new EthECKey(ThirdwebManager.Instance.SDK.nativeSession.account.PrivateKey);
                var typedData = GetTypedDefinition_TokenERC1155(domainName, version, chainId, verifyingContract);
                var signature = signer.SignTypedDataV4(mintRequest, typedData, key);
                return signature;
            }
            else
            {
                if (Utils.ActiveWalletConnectSession())
                {
                    var walletConnectMintRequest = new ERC1155MintRequestWalletConnect()
                    {
                        To = mintRequest.To,
                        RoyaltyRecipient = mintRequest.RoyaltyRecipient,
                        RoyaltyBps = mintRequest.RoyaltyBps,
                        PrimarySaleRecipient = mintRequest.PrimarySaleRecipient,
                        TokenId = mintRequest.TokenId,
                        Uri = mintRequest.Uri,
                        Quantity = mintRequest.Quantity,
                        PricePerToken = mintRequest.PricePerToken,
                        Currency = mintRequest.Currency,
                        ValidityStartTimestamp = mintRequest.ValidityStartTimestamp,
                        ValidityEndTimestamp = mintRequest.ValidityEndTimestamp,
                        Uid = mintRequest.Uid.ByteArrayToHexString()
                    };
                    var typedData = GetTypedDefinition_TokenERC1155(domainName, version, chainId, verifyingContract);
                    return await WalletConnect.Instance.Session.EthSignTypedData(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), walletConnectMintRequest, typedData);
                }
                else
                {
                    throw new UnityException("No account connected!");
                }
            }
        }

        public partial class ERC1155MintRequestWalletConnect : ERC1155MintRequestBaseWalletConnect { }

        public class ERC1155MintRequestBaseWalletConnect
        {
            [Parameter("address", "to", 1)]
            public virtual string To { get; set; }

            [Parameter("address", "royaltyRecipient", 2)]
            public virtual string RoyaltyRecipient { get; set; }

            [Parameter("uint256", "royaltyBps", 3)]
            public virtual BigInteger RoyaltyBps { get; set; }

            [Parameter("address", "primarySaleRecipient", 4)]
            public virtual string PrimarySaleRecipient { get; set; }

            [Parameter("uint256", "tokenId", 5)]
            public virtual BigInteger TokenId { get; set; }

            [Parameter("string", "uri", 6)]
            public virtual string Uri { get; set; }

            [Parameter("uint256", "quantity", 7)]
            public virtual BigInteger Quantity { get; set; }

            [Parameter("uint256", "pricePerToken", 8)]
            public virtual BigInteger PricePerToken { get; set; }

            [Parameter("address", "currency", 9)]
            public virtual string Currency { get; set; }

            [Parameter("uint128", "validityStartTimestamp", 10)]
            public virtual BigInteger ValidityStartTimestamp { get; set; }

            [Parameter("uint128", "validityEndTimestamp", 11)]
            public virtual BigInteger ValidityEndTimestamp { get; set; }

            [Parameter("bytes32", "uid", 12)]
            public virtual string Uid { get; set; }
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

        /// MINIMAL FORWARDER ///

        public async static Task<string> GenerateSignature_MinimalForwarder(
            string domainName,
            string version,
            BigInteger chainId,
            string verifyingContract,
            MinimalForwarder.ForwardRequest forwardRequest
        )
        {
            if (ThirdwebManager.Instance.SDK.nativeSession.account != null)
            {
                var signer = new Eip712TypedDataSigner();
                var key = new EthECKey(ThirdwebManager.Instance.SDK.nativeSession.account.PrivateKey);
                var typedData = GetTypedDefinition_MinimalForwarder(domainName, version, chainId, verifyingContract);
                var signature = signer.SignTypedDataV4(forwardRequest, typedData, key);
                return signature;
            }
            else
            {
                if (Utils.ActiveWalletConnectSession())
                {
                    var typedData = GetTypedDefinition_MinimalForwarder(domainName, version, chainId, verifyingContract);
                    return await WalletConnect.Instance.Session.EthSignTypedData(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), forwardRequest, typedData);
                }
                else
                {
                    throw new UnityException("No account connected!");
                }
            }
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
    }
}
