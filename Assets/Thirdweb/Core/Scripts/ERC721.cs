using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Thirdweb.Contracts.TokenERC721;
using Thirdweb.Contracts.DropERC721;

namespace Thirdweb
{
    /// <summary>
    /// Interact with any ERC721 compatible contract.
    /// </summary>
    public class ERC721 : Routable
    {
        /// <summary>
        /// Handle signature minting functionality
        /// </summary>
        public ERC721Signature signature;
        /// <summary>
        /// Query claim conditions
        /// </summary>
        public ERC721ClaimConditions claimConditions;

        TokenERC721Service tokenERC721Service;
        DropERC721Service dropERC721Service;

        /// <summary>
        /// Interact with any ERC721 compatible contract.
        /// </summary>
        public ERC721(string parentRoute, string contractAddress) : base(Routable.append(parentRoute, "erc721"))
        {
            if (!Utils.IsWebGLBuild())
            {
                this.tokenERC721Service = new TokenERC721Service(ThirdwebManager.Instance.SDK.web3, contractAddress);
                this.dropERC721Service = new DropERC721Service(ThirdwebManager.Instance.SDK.web3, contractAddress);
            }

            this.signature = new ERC721Signature(baseRoute);
            this.claimConditions = new ERC721ClaimConditions(baseRoute);
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
                nft.owner = await OwnerOf(tokenId);
                nft.type = "ERC721";
                nft.supply = await TotalCount();
                nft.quantityOwned = 1;

                string tokenURI = await tokenERC721Service.TokenURIQueryAsync(BigInteger.Parse(tokenId));
                tokenURI = tokenURI.ReplaceIPFS();

                using (UnityWebRequest req = UnityWebRequest.Get(tokenURI))
                {
                    await req.SendWebRequest();
                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning($"Unable to fetch token {tokenId} uri metadata!");
                        return nft;
                    }

                    string json = req.downloadHandler.text;
                    nft.metadata = JsonConvert.DeserializeObject<NFTMetadata>(json);
                }

                nft.metadata.image = nft.metadata.image.ReplaceIPFS();
                nft.metadata.id = tokenId;
                nft.metadata.uri = tokenURI;

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
                int totalSupply = (int)await tokenERC721Service.TotalSupplyQueryAsync();
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
                    end = totalSupply - 1;
                }

                List<NFT> allNfts = new List<NFT>();
                var erc721 = ThirdwebManager.Instance.SDK.web3.Eth.ERC721.GetContractService(tokenERC721Service.ContractHandler.ContractAddress);
                var rawNfts = await erc721.GetAllMetadataUrlsUsingIdRangeAndMultiCallAsync(start, end);
                foreach (var rawNft in rawNfts)
                {
                    NFT nft = new NFT();
                    nft.owner = rawNft.Owner;
                    nft.type = "ERC721";
                    nft.supply = totalSupply;
                    nft.quantityOwned = 1;

                    string tokenURI = rawNft.MetadataUrl;
                    tokenURI = tokenURI.ReplaceIPFS();

                    using (UnityWebRequest req = UnityWebRequest.Get(tokenURI))
                    {
                        await req.SendWebRequest();
                        if (req.result != UnityWebRequest.Result.Success)
                        {
                            Debug.LogWarning($"Unable to fetch token {rawNft.TokenId} uri metadata!");
                            allNfts.Add(nft);
                            continue;
                        }

                        string json = req.downloadHandler.text;
                        nft.metadata = JsonConvert.DeserializeObject<NFTMetadata>(json);

                        nft.metadata.image = nft.metadata.image.ReplaceIPFS();
                        nft.metadata.id = rawNft.TokenId.ToString();
                        nft.metadata.uri = tokenURI;
                        allNfts.Add(nft);
                    }
                }

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
                var erc721 = ThirdwebManager.Instance.SDK.web3.Eth.ERC721.GetContractService(tokenERC721Service.ContractHandler.ContractAddress);
                string owner = address == null ? await ThirdwebManager.Instance.SDK.wallet.GetAddress() : address;
                var tokenIdsOfOwner = await erc721.GetAllTokenIdsOfOwnerUsingTokenOfOwnerByIndexAndMultiCallAsync(owner);
                List<NFT> ownedNfts = new List<NFT>();
                foreach (var tokenId in tokenIdsOfOwner)
                {
                    ownedNfts.Add(await Get(tokenId.ToString()));
                }
                return ownedNfts;
            }
        }

        /// <summary>
        /// Get the owner of a NFT in this contract
        /// </summary>
        public async Task<string> OwnerOf(string tokenId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("ownerOf"), Utils.ToJsonStringArray(tokenId));
            }
            else
            {
                return (await tokenERC721Service.OwnerOfQueryAsync(BigInteger.Parse(tokenId))).ToString();
            }
        }

        /// <summary>
        /// Get the balance of NFTs in this contract for the connected wallet
        /// </summary>
        public async Task<string> Balance()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("balance"), new string[] { });
            }
            else
            {
                return await BalanceOf(await ThirdwebManager.Instance.SDK.wallet.GetAddress());
            }
        }

        /// <summary>
        /// Get the balance of NFTs in this contract for the given wallet address
        /// </summary>
        public async Task<string> BalanceOf(string address)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("balanceOf"), Utils.ToJsonStringArray(address));
            }
            else
            {
                return (await tokenERC721Service.BalanceOfQueryAsync(address)).ToString();
            }
        }

        /// <summary>
        /// Check whether the given contract address has been approved to transfer NFTs on behalf of the given wallet address
        /// </summary>
        /// <param name="address">The wallet address</param>
        /// <param name="contractAddress">The contract address to check approval for</param>
        public async Task<bool> IsApprovedForAll(string address, string approvedContract)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<bool>(getRoute("isApproved"), Utils.ToJsonStringArray(address, approvedContract));
            }
            else
            {
                return await tokenERC721Service.IsApprovedForAllQueryAsync(address, approvedContract);
            }
        }

        /// <summary>
        /// Get the total suppply in circulation
        /// </summary>
        public async Task<int> TotalCount()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<int>(getRoute("totalCount"), new string[] { });
            }
            else
            {
                return (int)(await tokenERC721Service.TotalSupplyQueryAsync());
            }
        }

        /// <summary>
        /// Get the total claimed suppply for Drop contracts
        /// </summary>
        public async Task<int> TotalClaimedSupply()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<int>(getRoute("totalClaimedSupply"), new string[] { });
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Get the total unclaimed suppply for Drop contracts
        /// </summary>
        public async Task<int> TotalUnclaimedSupply()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<int>(getRoute("totalUnclaimedSupply"), new string[] { });
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
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
                var result = await tokenERC721Service.SetApprovalForAllRequestAndWaitForReceiptAsync(contractToApprove, approved);
                return result.ToTransactionResult();
            }
        }

        /// <summary>
        /// Transfer a given NFT to the given address
        /// </summary>
        public async Task<TransactionResult> Transfer(string to, string tokenId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, tokenId));
            }
            else
            {
                var result = await tokenERC721Service.TransferFromRequestAndWaitForReceiptAsync(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), to, BigInteger.Parse(tokenId));
                return result.ToTransactionResult();
            }
        }

        /// <summary>
        /// Burn a given NFT
        /// </summary>
        public async Task<TransactionResult> Burn(string tokenId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("burn"), Utils.ToJsonStringArray(tokenId));
            }
            else
            {
                var result = await tokenERC721Service.BurnRequestAndWaitForReceiptAsync(BigInteger.Parse(tokenId));
                return result.ToTransactionResult();
            }
        }

        /// <summary>
        /// Claim NFTs from a Drop contract
        /// </summary>
        public async Task<TransactionResult[]> Claim(int quantity)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claim"), Utils.ToJsonStringArray(quantity));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Claim NFTs from a Drop contract and send them to the given address
        /// </summary>
        public async Task<TransactionResult[]> ClaimTo(string address, int quantity)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claimTo"), Utils.ToJsonStringArray(address, quantity));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Mint an NFT (requires minting permission)
        /// </summary>
        public async Task<TransactionResult> Mint(NFTMetadata nft)
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
        public async Task<TransactionResult> MintTo(string address, NFTMetadata nft)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintTo"), Utils.ToJsonStringArray(address, nft));
            }
            else
            {
                var result = await tokenERC721Service.MintToRequestAndWaitForReceiptAsync(address, nft.uri);
                return result.ToTransactionResult();
            }
        }
    }

    /// <summary>
    /// Fetch claim conditions for a given ERC721 drop contract
    /// </summary>
    public class ERC721ClaimConditions : Routable
    {
        public ERC721ClaimConditions(string parentRoute) : base(Routable.append(parentRoute, "claimConditions"))
        {
        }

        /// <summary>
        /// Get the active claim condition
        /// </summary>
        public async Task<ClaimConditions> GetActive()
        {
            return await Bridge.InvokeRoute<ClaimConditions>(getRoute("getActive"), new string[] { });
        }

        /// <summary>
        /// Check whether the connected wallet is eligible to claim
        /// </summary>
        public async Task<bool> CanClaim(int quantity, string addressToCheck = null)
        {
            return await Bridge.InvokeRoute<bool>(getRoute("canClaim"), Utils.ToJsonStringArray(quantity, addressToCheck));
        }

        /// <summary>
        /// Get the reasons why the connected wallet is not eligible to claim
        /// </summary>
        public async Task<string[]> GetIneligibilityReasons(int quantity, string addressToCheck = null)
        {
            return await Bridge.InvokeRoute<string[]>(getRoute("getClaimIneligibilityReasons"), Utils.ToJsonStringArray(quantity, addressToCheck));
        }

        /// <summary>
        /// Get the special values set in the allowlist for the given wallet
        /// </summary>
        public async Task<bool> GetClaimerProofs(string claimerAddress)
        {
            return await Bridge.InvokeRoute<bool>(getRoute("getClaimerProofs"), Utils.ToJsonStringArray(claimerAddress));
        }
    }

    [System.Serializable]
#nullable enable
    public class ERC721MintPayload
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

        public ERC721MintPayload(string receiverAddress, NFTMetadata metadata)
        {
            this.metadata = metadata;
            this.to = receiverAddress;
            this.price = "0";
            this.currencyAddress = Utils.AddressZero;
            this.primarySaleRecipient = Utils.AddressZero;
            this.royaltyRecipient = Utils.AddressZero;
            this.royaltyBps = 0;
            this.quantity = 1;
            this.uid = Utils.ToBytes32HexString(Guid.NewGuid().ToByteArray());
            // TODO temporary solution
            // this.mintStartTime = Utils.UnixTimeNowMs() * 1000L;
            // this.mintEndTime = this.mintStartTime + 1000L * 60L * 60L * 24L * 365L;
        }
    }

    [System.Serializable]
    public struct ERC721SignedPayloadOutput
    {
        public string to;
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
    public struct ERC721SignedPayload
    {
        public string signature;
        public ERC721SignedPayloadOutput payload;
    }

    /// <summary>
    /// Generate, verify and mint signed mintable payloads
    /// </summary>
    public class ERC721Signature : Routable
    {
        /// <summary>
        /// Generate, verify and mint signed mintable payloads
        /// </summary>
        public ERC721Signature(string parentRoute) : base(Routable.append(parentRoute, "signature"))
        {
        }

        /// <summary>
        /// Generate a signed mintable payload. Requires minting permission.
        /// </summary>
        public async Task<ERC721SignedPayload> Generate(ERC721MintPayload payloadToSign)
        {
            return await Bridge.InvokeRoute<ERC721SignedPayload>(getRoute("generate"), Utils.ToJsonStringArray(payloadToSign));
        }

        /// <summary>
        /// Verify that a signed mintable payload is valid
        /// </summary>
        public async Task<bool> Verify(ERC721SignedPayload signedPayload)
        {
            return await Bridge.InvokeRoute<bool>(getRoute("verify"), Utils.ToJsonStringArray(signedPayload));
        }

        /// <summary>
        /// Mint a signed mintable payload
        /// </summary>
        public async Task<TransactionResult> Mint(ERC721SignedPayload signedPayload)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(signedPayload));
        }
    }
}