using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;
using Newtonsoft.Json;
using Thirdweb.Contracts.TokenERC1155;
using Thirdweb.Contracts.DropERC1155;
using TokenERC1155Contract = Thirdweb.Contracts.TokenERC1155.ContractDefinition;

namespace Thirdweb
{
    /// <summary>
    /// Interact with any ERC1155 compatible contract.
    /// </summary>
    public class ERC1155 : Routable
    {
        /// <summary>
        /// Handle signature minting functionality
        /// /// </summary>
        public ERC1155Signature signature;

        /// <summary>
        /// Query claim conditions
        /// </summary>
        public ERC1155ClaimConditions claimConditions;

        TokenERC1155Service tokenERC1155Service;
        DropERC1155Service dropERC1155Service;

        /// <summary>
        /// Interact with any ERC1155 compatible contract.
        /// </summary>
        public ERC1155(string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "erc1155"))
        {
            if (!Utils.IsWebGLBuild())
            {
                this.tokenERC1155Service = new TokenERC1155Service(ThirdwebManager.Instance.SDK.nativeSession.web3, contractAddress);
                this.dropERC1155Service = new DropERC1155Service(ThirdwebManager.Instance.SDK.nativeSession.web3, contractAddress);
            }

            this.signature = new ERC1155Signature(baseRoute, contractAddress);
            this.claimConditions = new ERC1155ClaimConditions(baseRoute, contractAddress);
        }

        // READ FUNCTIONS

        /// <summary>
        /// Get a NFT in this contract by its ID
        /// </summary>
        public async Task<NFT> Get(string tokenId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<NFT>(getRoute("get"), Utils.ToJsonStringArray(tokenId));
            }
            else
            {
                NFT nft = new NFT();
                nft.owner = "";
                nft.type = "ERC1155";
                nft.supply = await TotalSupply(tokenId);
                nft.quantityOwned = 404;
                string tokenURI = await tokenERC1155Service.UriQueryAsync(BigInteger.Parse(tokenId));
                nft.metadata = await ThirdwebManager.Instance.SDK.storage.DownloadText<NFTMetadata>(tokenURI);
                nft.metadata.image = nft.metadata.image.ReplaceIPFS();
                nft.metadata.id = tokenId;
                nft.metadata.uri = tokenURI.ReplaceIPFS();
                return nft;
            }
        }

        /// <summary>
        /// Get a all NFTs in this contract
        /// </summary>
        public async Task<List<NFT>> GetAll(QueryAllParams queryParams = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<NFT>>(getRoute("getAll"), Utils.ToJsonStringArray(queryParams));
            }
            else
            {
                int totalCount = await TotalCount();
                int start;
                int end;
                if (queryParams != null)
                {
                    start = queryParams.start;
                    end = queryParams.start + queryParams.count;
                }
                else
                {
                    start = 0;
                    end = totalCount - 1;
                }
                // TODO: Add Multicall
                List<NFT> allNfts = new List<NFT>();
                for (int i = start; i < end; i++)
                    allNfts.Add(await Get(i.ToString()));
                return allNfts;
            }
        }

        /// <summary>
        /// Get a all NFTs owned by the connected wallet
        /// </summary>
        /// <param name="address">Optional wallet address to query NFTs of</param>
        public async Task<List<NFT>> GetOwned(string address = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<NFT>>(getRoute("getOwned"), Utils.ToJsonStringArray(address));
            }
            else
            {
                string owner = address == null ? await ThirdwebManager.Instance.SDK.wallet.GetAddress() : address;
                // TODO: Add Multicall
                int totalCount = await TotalCount();
                List<NFT> ownedNfts = new List<NFT>();
                for (int i = 0; i < totalCount; i++)
                {
                    BigInteger ownedBalance = BigInteger.Parse(await Balance(i.ToString()));
                    if (ownedBalance == 0)
                    {
                        continue;
                    }
                    else
                    {
                        NFT tempNft = await Get(i.ToString());
                        tempNft.owner = owner;
                        tempNft.quantityOwned = (int)ownedBalance;
                        ownedNfts.Add(tempNft);
                    }
                }
                return ownedNfts;
            }
        }

        /// <summary>
        /// Get the balance of the given NFT for the connected wallet
        /// </summary>
        public async Task<string> Balance(string tokenId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("balance"), new string[] { });
            }
            else
            {
                return await BalanceOf(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), tokenId);
            }
        }

        /// <summary>
        /// Get the balance of the given NFT for the given wallet address
        /// </summary>
        public async Task<string> BalanceOf(string address, string tokenId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("balanceOf"), Utils.ToJsonStringArray(address, tokenId));
            }
            else
            {
                return (await tokenERC1155Service.BalanceOfQueryAsync(address, BigInteger.Parse(tokenId))).ToString();
            }
        }

        /// <summary>
        /// Check whether the given contract address has been approved to transfer NFTs on behalf of the given wallet address
        /// </summary>
        /// <param name="address">The wallet address</param>
        /// <param name="contractAddress">The contract address to check approval for</param>
        public async Task<string> IsApprovedForAll(string address, string approvedContract)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("isApproved"), Utils.ToJsonStringArray(address, approvedContract));
            }
            else
            {
                return (await tokenERC1155Service.IsApprovedForAllQueryAsync(address, approvedContract)).ToString();
            }
        }

        public async Task<int> TotalCount()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<int>(getRoute("totalCount"), new string[] { });
            }
            else
            {
                return (int)await tokenERC1155Service.NextTokenIdToMintQueryAsync();
            }
        }

        /// <summary>
        /// Get the total suppply in circulation for thge given NFT
        /// </summary>
        public async Task<int> TotalSupply(string tokenId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<int>(getRoute("totalSupply"), Utils.ToJsonStringArray(tokenId));
            }
            else
            {
                return (int)(await tokenERC1155Service.TotalSupplyQueryAsync(BigInteger.Parse(tokenId)));
            }
        }

        // WRITE FUNCTIONS

        /// <summary>
        /// Set approval to the given contract to transfer NFTs on behalf of the connected wallet
        /// </summary>
        public async Task<TransactionResult> SetApprovalForAll(string contractToApprove, bool approved)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("setApprovalForAll"), Utils.ToJsonStringArray(contractToApprove, approved));
            }
            else
            {
                var receipt = await tokenERC1155Service.SetApprovalForAllRequestAndWaitForReceiptAsync(contractToApprove, approved);
                return receipt.ToTransactionResult();
            }
        }

        /// <summary>
        /// Transfer NFTs to the given address
        /// </summary>
        public async Task<TransactionResult> Transfer(string to, string tokenId, int amount)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, tokenId, amount));
            }
            else
            {
                var receipt = await tokenERC1155Service.SafeTransferFromRequestAndWaitForReceiptAsync(
                    await ThirdwebManager.Instance.SDK.wallet.GetAddress(),
                    to,
                    BigInteger.Parse(tokenId),
                    amount,
                    new byte[0]
                );
                return receipt.ToTransactionResult();
            }
        }

        /// <summary>
        /// Burn NFTs
        /// </summary>
        public async Task<TransactionResult> Burn(string tokenId, int amount)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("burn"), Utils.ToJsonStringArray(tokenId, amount));
            }
            else
            {
                var receipt = await tokenERC1155Service.BurnRequestAndWaitForReceiptAsync(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), BigInteger.Parse(tokenId), amount);
                return receipt.ToTransactionResult();
            }
        }

        /// <summary>
        /// Claim NFTs from a Drop contract
        /// </summary>
        public async Task<TransactionResult> Claim(string tokenId, int quantity)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("claim"), Utils.ToJsonStringArray(tokenId, quantity));
            }
            else
            {
                return await ClaimTo(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), tokenId, quantity);
            }
        }

        /// <summary>
        /// Claim NFTs from a Drop contract and send them to the given address
        /// </summary>
        public async Task<TransactionResult> ClaimTo(string address, string tokenId, int quantity)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("claimTo"), Utils.ToJsonStringArray(address, tokenId, quantity));
            }
            else
            {
                var claimCondition = await claimConditions.GetActive(tokenId);
                var result = await dropERC1155Service.ClaimRequestAndWaitForReceiptAsync(
                    address,
                    BigInteger.Parse(tokenId),
                    quantity,
                    claimCondition.currencyAddress,
                    BigInteger.Parse(claimCondition.currencyMetadata.value),
                    new Contracts.DropERC1155.ContractDefinition.AllowlistProof
                    {
                        Proof = new List<byte[]>(),
                        Currency = claimCondition.currencyAddress,
                        PricePerToken = BigInteger.Parse(claimCondition.currencyMetadata.value),
                        QuantityLimitPerWallet = BigInteger.Parse(claimCondition.maxClaimablePerWallet),
                    }, // TODO add support for allowlists
                    new byte[] { }
                );
                return result.ToTransactionResult();
            }
        }

        /// <summary>
        /// Mint an NFT (requires minting permission)
        /// </summary>
        public async Task<TransactionResult> Mint(NFTMetadataWithSupply nft)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(nft));
            }
            else
            {
                return await MintTo(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), nft);
            }
        }

        /// <summary>
        /// Mint an NFT and send it to the given wallet (requires minting permission)
        /// </summary>
        public async Task<TransactionResult> MintTo(string address, NFTMetadataWithSupply nft)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintTo"), Utils.ToJsonStringArray(address, nft));
            }
            else
            {
                var uri = await ThirdwebManager.Instance.SDK.storage.UploadText(JsonConvert.SerializeObject(nft.metadata));
                var receipt = await tokenERC1155Service.MintToRequestAndWaitForReceiptAsync(
                    await ThirdwebManager.Instance.SDK.wallet.GetAddress(),
                    Utils.GetMaxUint256(),
                    uri.IpfsHash.cidToIpfsUrl(),
                    nft.supply
                );
                return receipt.ToTransactionResult();
            }
        }

        /// <summary>
        /// Mint additional supply of a given NFT (requires minting permission)
        /// </summary>
        public async Task<TransactionResult> MintAdditionalSupply(string tokenId, int additionalSupply)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintAdditionalSupply"), Utils.ToJsonStringArray(tokenId, additionalSupply, additionalSupply));
            }
            else
            {
                return await MintAdditionalSupplyTo(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), tokenId, additionalSupply);
            }
        }

        /// <summary>
        /// Mint additional supply of a given NFT and send it to the given wallet (requires minting permission)
        /// </summary>
        public async Task<TransactionResult> MintAdditionalSupplyTo(string address, string tokenId, int additionalSupply)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintAdditionalSupplyTo"), Utils.ToJsonStringArray(address, tokenId, additionalSupply));
            }
            else
            {
                var uri = await tokenERC1155Service.UriQueryAsync(BigInteger.Parse(tokenId));
                var receipt = await tokenERC1155Service.MintToRequestAndWaitForReceiptAsync(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), BigInteger.Parse(tokenId), uri, additionalSupply);
                return receipt.ToTransactionResult();
            }
        }
    }

    /// <summary>
    /// Fetch claim conditions for a given ERC1155 drop contract
    /// </summary>
    public class ERC1155ClaimConditions : Routable
    {
        private DropERC1155Service dropERC1155Service;

        public ERC1155ClaimConditions(string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "claimConditions"))
        {
            if (!Utils.IsWebGLBuild())
            {
                this.dropERC1155Service = new DropERC1155Service(ThirdwebManager.Instance.SDK.nativeSession.web3, contractAddress);
            }
        }

        /// <summary>
        /// Get the active claim condition
        /// </summary>
        public async Task<ClaimConditions> GetActive(string tokenId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<ClaimConditions>(getRoute("getActive"), Utils.ToJsonStringArray(tokenId));
            }
            else
            {
                var tokenIdParsed = BigInteger.Parse(tokenId);
                var id = await dropERC1155Service.GetActiveClaimConditionIdQueryAsync(tokenIdParsed);
                var data = await dropERC1155Service.GetClaimConditionByIdQueryAsync(tokenIdParsed, id);
                return new ClaimConditions()
                {
                    availableSupply = (data.Condition.MaxClaimableSupply - data.Condition.SupplyClaimed).ToString(),
                    currencyAddress = data.Condition.Currency,
                    currencyMetadata = new CurrencyValue() { value = data.Condition.PricePerToken.ToString(), },
                    currentMintSupply = data.Condition.SupplyClaimed.ToString(),
                    maxClaimablePerWallet = data.Condition.QuantityLimitPerWallet.ToString(),
                    maxClaimableSupply = data.Condition.MaxClaimableSupply.ToString(),
                };
            }
        }

        /// <summary>
        /// Check whether the connected wallet is eligible to claim
        /// </summary>
        public async Task<bool> CanClaim(string tokenId, int quantity, string addressToCheck = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<bool>(getRoute("canClaim"), Utils.ToJsonStringArray(tokenId, quantity, addressToCheck));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Get the reasons why the connected wallet is not eligible to claim
        /// </summary>
        public async Task<string[]> GetIneligibilityReasons(string tokenId, int quantity, string addressToCheck = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string[]>(getRoute("getClaimIneligibilityReasons"), Utils.ToJsonStringArray(tokenId, quantity, addressToCheck));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Get the special values set in the allowlist for the given wallet
        /// </summary>
        public async Task<bool> GetClaimerProofs(string tokenId, string claimerAddress)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<bool>(getRoute("getClaimerProofs"), Utils.ToJsonStringArray(claimerAddress));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }
    }

    // TODO switch to another JSON serializer that supports polymorphism
    [System.Serializable]
#nullable enable
    public class ERC1155MintPayload
    {
        public string to;
        public string price;
        public string currencyAddress;
        public string primarySaleRecipient;
        public string royaltyRecipient;
        public int royaltyBps;
        public int quantity;
        public NFTMetadata metadata;
        public string uid;

        // TODO implement these, needs JS bridging support
        // public long mintStartTime;
        // public long mintEndTime;

        public ERC1155MintPayload(string receiverAddress, NFTMetadata metadata, int quantity = 1)
        {
            this.metadata = metadata;
            this.to = receiverAddress;
            this.price = "0";
            this.currencyAddress = Utils.AddressZero;
            this.primarySaleRecipient = Utils.AddressZero;
            this.royaltyRecipient = Utils.AddressZero;
            this.royaltyBps = 0;
            this.quantity = quantity;
            this.uid = Utils.ToBytes32HexString(Guid.NewGuid().ToByteArray());
            // TODO temporary solution
            // this.mintStartTime = Utils.UnixTimeNowMs() * 1000L;
            // this.mintEndTime = this.mintStartTime + 1000L * 60L * 60L * 24L * 365L;
        }
    }

    [System.Serializable]
    public class ERC1155MintAdditionalPayload
    {
        public string tokenId;
        public string to;
        public string price;
        public string currencyAddress;
        public string primarySaleRecipient;
        public string royaltyRecipient;
        public int royaltyBps;
        public int quantity;
        public string uid;

        // TODO implement these, needs JS bridging support
        // public long mintStartTime;
        // public long mintEndTime;

        public ERC1155MintAdditionalPayload(string receiverAddress, string tokenId, int quantity = 1)
        {
            this.tokenId = tokenId;
            this.to = receiverAddress;
            this.price = "0";
            this.currencyAddress = Utils.AddressZero;
            this.primarySaleRecipient = Utils.AddressZero;
            this.royaltyRecipient = Utils.AddressZero;
            this.royaltyBps = 0;
            this.quantity = quantity;
            this.uid = Utils.ToBytes32HexString(Guid.NewGuid().ToByteArray());
            // TODO temporary solution
            // this.mintStartTime = Utils.UnixTimeNowMs() * 1000L;
            // this.mintEndTime = this.mintStartTime + 1000L * 60L * 60L * 24L * 365L;
        }
    }

    [System.Serializable]
    public struct ERC1155SignedPayloadOutput
    {
        public string to;
        public string tokenId;
        public string price;
        public string currencyAddress;
        public string primarySaleRecipient;
        public string royaltyRecipient;
        public int royaltyBps;
        public int quantity;
        public string uri;
        public string uid;
        public long mintStartTime;
        public long mintEndTime;
    }

    [System.Serializable]
    public struct ERC1155SignedPayload
    {
        public string signature;
        public ERC1155SignedPayloadOutput payload;
    }

    /// <summary>
    /// Generate, verify and mint signed mintable payloads
    /// </summary>
    public class ERC1155Signature : Routable
    {
#nullable enable
        TokenERC1155Service tokenERC1155Service;

#nullable disable

        /// <summary>
        /// Generate, verify and mint signed mintable payloads
        /// </summary>
        public ERC1155Signature(string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "signature"))
        {
            if (!Utils.IsWebGLBuild())
                this.tokenERC1155Service = new TokenERC1155Service(ThirdwebManager.Instance.SDK.nativeSession.web3, contractAddress);
        }

        /// <summary>
        /// Generate a signed mintable payload. Requires minting permission.
        /// </summary>
        public async Task<ERC1155SignedPayload> Generate(ERC1155MintPayload payloadToSign)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<ERC1155SignedPayload>(getRoute("generate"), Utils.ToJsonStringArray(payloadToSign));
            }
            else
            {
                var uri = await ThirdwebManager.Instance.SDK.storage.UploadText(JsonConvert.SerializeObject(payloadToSign.metadata));
                var startTime = await Utils.GetCurrentBlockTimeStamp();
                var endTime = Utils.GetUnixTimeStampIn10Years();
                TokenERC1155Contract.MintRequest req = new TokenERC1155Contract.MintRequest()
                {
                    To = payloadToSign.to,
                    RoyaltyRecipient = (await tokenERC1155Service.GetDefaultRoyaltyInfoQueryAsync()).ReturnValue1,
                    RoyaltyBps = (await tokenERC1155Service.GetDefaultRoyaltyInfoQueryAsync()).ReturnValue2,
                    PrimarySaleRecipient = await tokenERC1155Service.PrimarySaleRecipientQueryAsync(),
                    TokenId = Utils.GetMaxUint256(),
                    Uri = uri.IpfsHash.cidToIpfsUrl(),
                    Quantity = payloadToSign.quantity,
                    PricePerToken = BigInteger.Parse(payloadToSign.price),
                    Currency = payloadToSign.currencyAddress,
                    ValidityStartTimestamp = startTime,
                    ValidityEndTimestamp = endTime,
                    Uid = payloadToSign.uid.HexStringToByteArray()
                };

                string signature = await Thirdweb.EIP712.GenerateSignature_TokenERC1155(
                    "TokenERC1155",
                    "1",
                    await ThirdwebManager.Instance.SDK.wallet.GetChainId(),
                    tokenERC1155Service.ContractHandler.ContractAddress,
                    req
                );

                ERC1155SignedPayload signedPayload = new ERC1155SignedPayload();
                signedPayload.signature = signature;
                signedPayload.payload = new ERC1155SignedPayloadOutput()
                {
                    to = req.To,
                    tokenId = req.TokenId.ToString(),
                    price = req.PricePerToken.ToString(),
                    currencyAddress = req.Currency,
                    primarySaleRecipient = req.PrimarySaleRecipient,
                    royaltyRecipient = req.RoyaltyRecipient,
                    royaltyBps = (int)req.RoyaltyBps,
                    quantity = (int)req.Quantity,
                    uri = req.Uri,
                    uid = req.Uid.ByteArrayToHexString(),
                    mintStartTime = (long)req.ValidityStartTimestamp,
                    mintEndTime = (long)req.ValidityEndTimestamp
                };
                return signedPayload;
            }
        }

        public async Task<ERC1155SignedPayload> GenerateFromTokenId(ERC1155MintAdditionalPayload payloadToSign)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<ERC1155SignedPayload>(getRoute("generateFromTokenId"), Utils.ToJsonStringArray(payloadToSign));
            }
            else
            {
                // var uri = await ThirdwebManager.Instance.SDK.storage.UploadText(JsonConvert.SerializeObject(payloadToSign.metadata));
                var uri = await tokenERC1155Service.UriQueryAsync(BigInteger.Parse(payloadToSign.tokenId));
                var startTime = await Utils.GetCurrentBlockTimeStamp();
                var endTime = Utils.GetUnixTimeStampIn10Years();
                TokenERC1155Contract.MintRequest req = new TokenERC1155Contract.MintRequest()
                {
                    To = payloadToSign.to,
                    RoyaltyRecipient = (await tokenERC1155Service.GetDefaultRoyaltyInfoQueryAsync()).ReturnValue1,
                    RoyaltyBps = (await tokenERC1155Service.GetDefaultRoyaltyInfoQueryAsync()).ReturnValue2,
                    PrimarySaleRecipient = await tokenERC1155Service.PrimarySaleRecipientQueryAsync(),
                    TokenId = BigInteger.Parse(payloadToSign.tokenId),
                    Uri = uri,
                    Quantity = payloadToSign.quantity,
                    PricePerToken = BigInteger.Parse(payloadToSign.price),
                    Currency = payloadToSign.currencyAddress,
                    ValidityStartTimestamp = startTime,
                    ValidityEndTimestamp = endTime,
                    Uid = payloadToSign.uid.HexStringToByteArray()
                };

                string signature = await Thirdweb.EIP712.GenerateSignature_TokenERC1155(
                    "TokenERC1155",
                    "1",
                    await ThirdwebManager.Instance.SDK.wallet.GetChainId(),
                    tokenERC1155Service.ContractHandler.ContractAddress,
                    req
                );

                ERC1155SignedPayload signedPayload = new ERC1155SignedPayload();
                signedPayload.signature = signature;
                signedPayload.payload = new ERC1155SignedPayloadOutput()
                {
                    to = req.To,
                    tokenId = req.TokenId.ToString(),
                    price = req.PricePerToken.ToString(),
                    currencyAddress = req.Currency,
                    primarySaleRecipient = req.PrimarySaleRecipient,
                    royaltyRecipient = req.RoyaltyRecipient,
                    royaltyBps = (int)req.RoyaltyBps,
                    quantity = (int)req.Quantity,
                    uri = req.Uri,
                    uid = req.Uid.ByteArrayToHexString(),
                    mintStartTime = (long)req.ValidityStartTimestamp,
                    mintEndTime = (long)req.ValidityEndTimestamp
                };
                return signedPayload;
            }
        }

        /// <summary>
        /// Verify that a signed mintable payload is valid
        /// </summary>
        public async Task<bool> Verify(ERC1155SignedPayload signedPayload)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<bool>(getRoute("verify"), Utils.ToJsonStringArray(signedPayload));
            }
            else
            {
                TokenERC1155Contract.MintRequest req = new TokenERC1155Contract.MintRequest()
                {
                    To = signedPayload.payload.to,
                    RoyaltyRecipient = signedPayload.payload.royaltyRecipient,
                    RoyaltyBps = (BigInteger)signedPayload.payload.royaltyBps,
                    PrimarySaleRecipient = signedPayload.payload.primarySaleRecipient,
                    TokenId = BigInteger.Parse(signedPayload.payload.tokenId),
                    Uri = signedPayload.payload.uri,
                    Quantity = signedPayload.payload.quantity,
                    PricePerToken = BigInteger.Parse(signedPayload.payload.price),
                    Currency = signedPayload.payload.currencyAddress,
                    ValidityStartTimestamp = signedPayload.payload.mintStartTime,
                    ValidityEndTimestamp = signedPayload.payload.mintEndTime,
                    Uid = signedPayload.payload.uid.HexStringToByteArray()
                };
                var receipt = await tokenERC1155Service.VerifyQueryAsync(req, signedPayload.signature.HexStringToByteArray());
                return receipt.ReturnValue1;
            }
        }

        /// <summary>
        /// Mint a signed mintable payload
        /// </summary>
        public async Task<TransactionResult> Mint(ERC1155SignedPayload signedPayload)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(signedPayload));
            }
            else
            {
                TokenERC1155Contract.MintRequest req = new TokenERC1155Contract.MintRequest()
                {
                    To = signedPayload.payload.to,
                    RoyaltyRecipient = signedPayload.payload.royaltyRecipient,
                    RoyaltyBps = (BigInteger)signedPayload.payload.royaltyBps,
                    PrimarySaleRecipient = signedPayload.payload.primarySaleRecipient,
                    TokenId = BigInteger.Parse(signedPayload.payload.tokenId),
                    Uri = signedPayload.payload.uri,
                    Quantity = signedPayload.payload.quantity,
                    PricePerToken = BigInteger.Parse(signedPayload.payload.price),
                    Currency = signedPayload.payload.currencyAddress,
                    ValidityStartTimestamp = signedPayload.payload.mintStartTime,
                    ValidityEndTimestamp = signedPayload.payload.mintEndTime,
                    Uid = signedPayload.payload.uid.HexStringToByteArray()
                };
                var receipt = await tokenERC1155Service.MintWithSignatureRequestAndWaitForReceiptAsync(req, signedPayload.signature.HexStringToByteArray());
                return receipt.ToTransactionResult();
            }
        }
    }
}
