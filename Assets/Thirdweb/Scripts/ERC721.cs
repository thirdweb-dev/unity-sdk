using System;
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

        public ERC721(string chain, string address)
        {
            this.chain = chain;
            this.address = address;
            this.signature = new ERC721Signature(chain, address);
        }

        /// READ FUNCTIONS

        public async Task<NFT> Get(string tokenId)
        {
            return await Bridge.InvokeRoute<NFT>(getRoute("get"), new string[] { tokenId });
        }

        public async Task<NFT[]> GetAll()
        {
            return await Bridge.InvokeRoute<NFT[]>(getRoute("getAll"), new string[] { });
        }

        public async Task<NFT[]> GetOwned()
        {
            return await Bridge.InvokeRoute<NFT[]>(getRoute("getOwned"), new string[] { });
        }

        public async Task<NFT[]> GetOwned(string address)
        {
            return await Bridge.InvokeRoute<NFT[]>(getRoute("getOwned"), new string[] { address });
        }

        public async Task<string> OwnerOf(string tokenId)
        {
            return await Bridge.InvokeRoute<string>(getRoute("ownerOf"), new string[] { tokenId });
        }

        public async Task<string> Balance()
        {
            return await Bridge.InvokeRoute<string>(getRoute("balance"), new string[] { });
        }

        public async Task<string> BalancOf(string address)
        {
            return await Bridge.InvokeRoute<string>(getRoute("balanceOf"), new string[] { address });
        }

        public async Task<string> IsApprovedForAll(string address, string approvedContract)
        {
            return await Bridge.InvokeRoute<string>(getRoute("isApproved"), new string[] { address, approvedContract });
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
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("isApproved"), new string[] { contractToApprove, approved.ToString() });
        }

        public async Task<TransactionResult> Transfer(string to, string tokenId)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), new string[] { to, tokenId });
        }

        public async Task<TransactionResult> Burn(string tokenId)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("burn"), new string[] { tokenId });
        }

        public async Task<TransactionResult[]> Claim(int quantity)
        {
            return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claim"), new string[] { quantity.ToString() });
        }

        public async Task<TransactionResult[]> ClaimTo(string address, int quantity)
        {
            return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claimTo"), new string[] { address, quantity.ToString() });
        }

        public async Task<TransactionResult> Mint(NFTMetadata nft)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(nft));
        }

        public async Task<TransactionResult> MintTo(string address, NFTMetadata nft)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintTo"), Utils.ToJsonStringArray(address, nft));
        }

        public async Task<TransactionResult> GenerateSignature(NFTMetadata nft)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("signature.generate"), Utils.ToJsonStringArray(nft));
        }

        /// PRIVATE

        private string getRoute(string functionPath) {
            return this.address + ".erc721." + functionPath;
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
        public NFTMetadata? metadata;
        public string uid;
        // TODO implement these, needs JS bridging support
        public long mintStartTime;
        public long mintEndTime;

        public ERC721MintPayload() {
            this.to = Utils.AddressZero;
            this.price = "0";
            this.currencyAddress = Utils.AddressZero;
            this.primarySaleRecipient = Utils.AddressZero;
            this.royaltyRecipient = Utils.AddressZero;
            this.royaltyBps = 0;
            this.quantity = 1;
            this.metadata = null;
            this.uid = Utils.ToBytes32HexString(Guid.NewGuid().ToByteArray());
            // TODO temporary solution
            this.mintStartTime = Utils.UnixTimeNowMs() * 1000L;
            this.mintEndTime = this.mintStartTime + 1000L * 60L * 60L * 24L * 365L;
        }
    }

    [System.Serializable]
    public struct ERC721SignedPayload
    {
        public string signature;
        public ERC721MintPayload payload;
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