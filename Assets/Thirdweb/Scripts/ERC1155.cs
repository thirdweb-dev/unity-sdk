using System;
using System.Threading.Tasks;

namespace Thirdweb
{
    /// <summary>
    /// Interact with any <c>ERC1155</c> compatible contract.
    /// </summary>
    public class ERC1155
    {
        public string chain;
        public string address;
        public ERC1155Signature signature;

        public ERC1155(string chain, string address)
        {
            this.chain = chain;
            this.address = address;
            this.signature = new ERC1155Signature(chain, address);
        }

        /// READ FUNCTIONS

        public async Task<NFT> Get(string tokenId)
        {
            return await Bridge.InvokeRoute<NFT>(getRoute("get"), Utils.ToJsonStringArray(tokenId));
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
            return await Bridge.InvokeRoute<NFT[]>(getRoute("getOwned"), Utils.ToJsonStringArray(address));
        }

        public async Task<string> Balance(string tokenId)
        {
            return await Bridge.InvokeRoute<string>(getRoute("balance"), new string[] { });
        }

        public async Task<string> BalanceOf(string address, string tokenId) 
        {
            return await Bridge.InvokeRoute<string>(getRoute("balanceOf"), Utils.ToJsonStringArray(address, tokenId));
        }

        public async Task<string> IsApprovedForAll(string address, string approvedContract)
        {
            return await Bridge.InvokeRoute<string>(getRoute("isApproved"), Utils.ToJsonStringArray(address, approvedContract));
        }

        public async Task<int> TotalCount()
        {
            return await Bridge.InvokeRoute<int>(getRoute("totalCount"), new string[] { });
        }

        public async Task<int> TotalSupply(string tokenId)
        {
            return await Bridge.InvokeRoute<int>(getRoute("totalUnclaimedSupply"), Utils.ToJsonStringArray(tokenId));
        }

        /// WRITE FUNCTIONS

        public async Task<TransactionResult> SetApprovalForAll(string contractToApprove, bool approved)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("isApproved"), Utils.ToJsonStringArray(contractToApprove, approved));
        }

        public async Task<TransactionResult> Transfer(string to, string tokenId, int amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, tokenId, amount));
        }

        public async Task<TransactionResult> Burn(string tokenId, int amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("burn"), Utils.ToJsonStringArray(tokenId, amount));
        }

        public async Task<TransactionResult[]> Claim(string tokenId, int amount)
        {
            return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claim"), Utils.ToJsonStringArray(tokenId, amount));
        }

        public async Task<TransactionResult[]> ClaimTo(string address, string tokenId, int amount)
        {
            return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claimTo"), Utils.ToJsonStringArray(address, tokenId, amount));
        }

        public async Task<TransactionResult> Mint(NFTMetadataWithSupply nft)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(nft));
        }

        public async Task<TransactionResult> MintTo(string address, NFTMetadataWithSupply nft)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintTo"), Utils.ToJsonStringArray(address, nft));
        }

        public async Task<TransactionResult> MintAdditionalSupply(string tokenId, int additionalSupply)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintAdditionalSupply"), Utils.ToJsonStringArray(tokenId, additionalSupply, additionalSupply));
        }

        public async Task<TransactionResult> MintAdditionalSupplyTo(string address, string tokenId, int additionalSupply)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mintAdditionalSupplyTo"), Utils.ToJsonStringArray(address, tokenId, additionalSupply));
        }

        /// PRIVATE

        private string getRoute(string functionPath) {
            return this.address + ".erc1155." + functionPath;
        }
    }

    [System.Serializable]
    #nullable enable
    public class ERC1155MintPayload
    {
        public string tokenId;
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

        public ERC1155MintPayload() {
            this.tokenId = ""; // TODO max uint256 by default
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
    public struct ERC1155SignedPayload
    {
        public string signature;
        public ERC1155MintPayload payload;
    }

    public class ERC1155Signature
    {
        public string chain;
        public string address;

        public ERC1155Signature(string chain, string address)
        {
            this.chain = chain;
            this.address = address;
        }

        public async Task<ERC1155SignedPayload> Generate(ERC1155MintPayload payloadToSign)
        {
            return await Bridge.InvokeRoute<ERC1155SignedPayload>(getRoute("generate"), Utils.ToJsonStringArray(payloadToSign));
        }

        public async Task<bool> Verify(ERC1155SignedPayload signedPayload)
        {
            return await Bridge.InvokeRoute<bool>(getRoute("verify"), Utils.ToJsonStringArray(signedPayload));
        }

        public async Task<TransactionResult> Mint(ERC1155SignedPayload signedPayload)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("mint"), Utils.ToJsonStringArray(signedPayload));
        }

        private string getRoute(string functionPath) {
            return this.address + ".erc1155.signature." + functionPath;
        }
    }
}