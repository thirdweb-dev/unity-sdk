using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        /// <summary>
        /// Interact with any ERC721 compatible contract.
        /// </summary>
        public ERC721(string parentRoute) : base(Routable.append(parentRoute, "erc721"))
        {
            this.signature = new ERC721Signature(baseRoute);
            this.claimConditions = new ERC721ClaimConditions(baseRoute);
        }

        // READ FUNCTIONS

        /// <summary>
        /// Get a NFT in this contract by its ID
        /// </summary>
        public async Task<NFT> Get(string tokenId)
        {
            return await Bridge.InvokeRoute<NFT>(getRoute("get"), Utils.ToJsonStringArray(tokenId));
        }

        /// <summary>
        /// Get a all NFTs in this contract
        /// </summary>
        public async Task<List<NFT>> GetAll(QueryAllParams queryParams = null)
        {
            return await Bridge.InvokeRoute<List<NFT>>(getRoute("getAll"), Utils.ToJsonStringArray(queryParams));
        }

        /// <summary>
        /// Get a all NFTs owned by the connected wallet
        /// </summary>
        /// <param name="address">Optional wallet address to query NFTs of</param>
        public async Task<List<NFT>> GetOwned(string address = null)
        {
            return await Bridge.InvokeRoute<List<NFT>>(getRoute("getOwned"), Utils.ToJsonStringArray(address));
        }

        /// <summary>
        /// Get the owner of a NFT in this contract
        /// </summary>
        public async Task<string> OwnerOf(string tokenId)
        {
            return await Bridge.InvokeRoute<string>(getRoute("ownerOf"), Utils.ToJsonStringArray(tokenId));
        }

        /// <summary>
        /// Get the balance of NFTs in this contract for the connected wallet
        /// </summary>
        public async Task<string> Balance()
        {
            return await Bridge.InvokeRoute<string>(getRoute("balance"), new string[] { });
        }

        /// <summary>
        /// Get the balance of NFTs in this contract for the given wallet address
        /// </summary>
        public async Task<string> BalanceOf(string address)
        {
            return await Bridge.InvokeRoute<string>(getRoute("balanceOf"), Utils.ToJsonStringArray(address));
        }

        /// <summary>
        /// Check whether the given contract address has been approved to transfer NFTs on behalf of the given wallet address
        /// </summary>
        /// <param name="address">The wallet address</param>
        /// <param name="contractAddress">The contract address to check approval for</param>
        public async Task<bool> IsApprovedForAll(string address, string approvedContract)
        {
            return await Bridge.InvokeRoute<bool>(getRoute("isApproved"), Utils.ToJsonStringArray(address, approvedContract));
        }

        /// <summary>
        /// Get the total suppply in circulation
        /// </summary>
        public async Task<int> TotalCount()
        {
            return await Bridge.InvokeRoute<int>(getRoute("totalCount"), new string[] { });
        }

        /// <summary>
        /// Get the total claimed suppply for Drop contracts
        /// </summary>
        public async Task<int> TotalClaimedSupply()
        {
            return await Bridge.InvokeRoute<int>(getRoute("totalClaimedSupply"), new string[] { });
        }

        /// <summary>
        /// Get the total unclaimed suppply for Drop contracts
        /// </summary>
        public async Task<int> TotalUnclaimedSupply()
        {
            return await Bridge.InvokeRoute<int>(getRoute("totalUnclaimedSupply"), new string[] { });
        }

        // WRITE FUNCTIONS

        /// <summary>
        /// Set approval to the given contract to transfer NFTs on behalf of the connected wallet
        /// </summary>
        public async Task<TransactionResult> SetApprovalForAll(string contractToApprove, bool approved)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("setApprovalForAll"), Utils.ToJsonStringArray(contractToApprove, approved));
        }

        /// <summary>
        /// Transfer a given NFT to the given address
        /// </summary>
        public async Task<TransactionResult> Transfer(string to, string tokenId)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, tokenId));
        }

        /// <summary>
        /// Burn a given NFT
        /// </summary>
        public async Task<TransactionResult> Burn(string tokenId)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("burn"), Utils.ToJsonStringArray(tokenId));
        }

        /// <summary>
        /// Claim NFTs from a Drop contract
        /// </summary>
        public async Task<TransactionResult[]> Claim(int quantity)
        {
            return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claim"), Utils.ToJsonStringArray(quantity));
        }

        /// <summary>
        /// Claim NFTs from a Drop contract and send them to the given address
        /// </summary>
        public async Task<TransactionResult[]> ClaimTo(string address, int quantity)
        {
            return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claimTo"), Utils.ToJsonStringArray(address, quantity));
        }

        /// <summary>
        /// Mint an NFT (requires minting permission)
        /// </summary>
        public async Task<TransactionResult> Mint(NFTMetadata nft)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(nft));
        }

        /// <summary>
        /// Mint an NFT and send it to the given wallet (requires minting permission)
        /// </summary>
        public async Task<TransactionResult> MintTo(string address, NFTMetadata nft)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintTo"), Utils.ToJsonStringArray(address, nft));
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