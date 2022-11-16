using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thirdweb
{
    /// <summary>
    /// Interact with any <c>ERC721</c> compatible contract.
    /// </summary>
    public class ERC721
    {
        public string chain;
        public string address;
        public ERC721Signature signature;
        public ERC721ClaimConditions claimConditions;

        public ERC721(string chain, string address)
        {
            this.chain = chain;
            this.address = address;
            this.signature = new ERC721Signature(chain, address);
            this.claimConditions = new ERC721ClaimConditions(chain, address);
        }

        /// READ FUNCTIONS

        public async Task<NFT> Get(string tokenId)
        {
            return await Bridge.InvokeRoute<NFT>(getRoute("get"), Utils.ToJsonStringArray(tokenId));
        }

        public async Task<List<NFT>> GetAll()
        {
            return await Bridge.InvokeRoute<List<NFT>>(getRoute("getAll"), new string[] { });
        }

        public async Task<List<NFT>> GetOwned()
        {
            return await Bridge.InvokeRoute<List<NFT>>(getRoute("getOwned"), new string[] { });
        }

        public async Task<List<NFT>> GetOwned(string address)
        {
            return await Bridge.InvokeRoute<List<NFT>>(getRoute("getOwned"), Utils.ToJsonStringArray(address));
        }

        public async Task<string> OwnerOf(string tokenId)
        {
            return await Bridge.InvokeRoute<string>(getRoute("ownerOf"), Utils.ToJsonStringArray(tokenId));
        }

        public async Task<string> Balance()
        {
            return await Bridge.InvokeRoute<string>(getRoute("balance"), new string[] { });
        }

        public async Task<string> BalanceOf(string address)
        {
            return await Bridge.InvokeRoute<string>(getRoute("balanceOf"), Utils.ToJsonStringArray(address));
        }

        public async Task<string> IsApprovedForAll(string address, string approvedContract)
        {
            return await Bridge.InvokeRoute<string>(getRoute("isApproved"), Utils.ToJsonStringArray(address, approvedContract));
        }

        public async Task<int> TotalClaimedSupply()
        {
            return await Bridge.InvokeRoute<int>(getRoute("totalClaimedSupply"), new string[] { });
        }

        public async Task<int> TotalUnclaimedSupply()
        {
            return await Bridge.InvokeRoute<int>(getRoute("totalUnclaimedSupply"), new string[] { });
        }

        /// WRITE FUNCTIONS

        public async Task<TransactionResult> SetApprovalForAll(string contractToApprove, bool approved)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("isApproved"), Utils.ToJsonStringArray(contractToApprove, approved));
        }

        public async Task<TransactionResult> Transfer(string to, string tokenId)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, tokenId));
        }

        public async Task<TransactionResult> Burn(string tokenId)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("burn"), Utils.ToJsonStringArray(tokenId));
        }

        public async Task<TransactionResult[]> Claim(int quantity)
        {
            return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claim"), Utils.ToJsonStringArray(quantity));
        }

        public async Task<TransactionResult[]> ClaimTo(string address, int quantity)
        {
            return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claimTo"), Utils.ToJsonStringArray(address, quantity));
        }

        public async Task<TransactionResult> Mint(NFTMetadata nft)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(nft));
        }

        public async Task<TransactionResult> MintTo(string address, NFTMetadata nft)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintTo"), Utils.ToJsonStringArray(address, nft));
        }

        /// PRIVATE

        private string getRoute(string functionPath) {
            return this.address + ".erc721." + functionPath;
        }
    }

    /// <summary>
    /// Fetch claim conditions for a given ERC721 drop contract
    /// </summary>
    public class ERC721ClaimConditions
    {
        public string chain;
        public string address;

        public ERC721ClaimConditions(string chain, string address)
        {
            this.chain = chain;
            this.address = address;
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

        private string getRoute(string functionPath) {
            return this.address + ".erc721.claimConditions." + functionPath;
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

        public ERC721MintPayload(string receiverAddress, NFTMetadata metadata) {
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

    public class ERC721Signature
    {
        public string chain;
        public string address;

        public ERC721Signature(string chain, string address)
        {
            this.chain = chain;
            this.address = address;
        }

        public async Task<ERC721SignedPayload> Generate(ERC721MintPayload payloadToSign)
        {
            return await Bridge.InvokeRoute<ERC721SignedPayload>(getRoute("generate"), Utils.ToJsonStringArray(payloadToSign));
        }

        public async Task<bool> Verify(ERC721SignedPayload signedPayload)
        {
            return await Bridge.InvokeRoute<bool>(getRoute("verify"), Utils.ToJsonStringArray(signedPayload));
        }

        public async Task<TransactionResult> Mint(ERC721SignedPayload signedPayload)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(signedPayload));
        }

        private string getRoute(string functionPath) {
            return this.address + ".erc721.signature." + functionPath;
        }
    }
}