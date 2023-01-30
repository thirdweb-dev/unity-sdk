using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thirdweb
{
    /// <summary>
    /// Interact with a Pack contract.
    /// </summary>
    public class Pack : Routable
    {
        public string chain;
        public string address;

        /// <summary>
        /// Interact with a Marketplace contract.
        /// </summary>
        public Pack(string chain, string address) : base($"{address}{subSeparator}pack")
        {
            this.chain = chain;
            this.address = address;
        }

        /// READ FUNCTIONS

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
        /// Get the balance of the given NFT for the connected wallet
        /// </summary>
        public async Task<string> Balance(string tokenId)
        {
            return await Bridge.InvokeRoute<string>(getRoute("balance"), new string[] { });
        }

        /// <summary>
        /// Get the balance of the given NFT for the given wallet address
        /// </summary>
        public async Task<string> BalanceOf(string address, string tokenId)
        {
            return await Bridge.InvokeRoute<string>(getRoute("balanceOf"), Utils.ToJsonStringArray(address, tokenId));
        }

        /// <summary>
        /// Check whether the given contract address has been approved to transfer NFTs on behalf of the given wallet address
        /// </summary>
        /// <param name="address">The wallet address</param>
        /// <param name="contractAddress">The contract address to check approval for</param>
        public async Task<string> IsApprovedForAll(string address, string approvedContract)
        {
            return await Bridge.InvokeRoute<string>(getRoute("isApproved"), Utils.ToJsonStringArray(address, approvedContract));
        }

        public async Task<int> TotalCount()
        {
            return await Bridge.InvokeRoute<int>(getRoute("totalCount"), new string[] { });
        }

        /// <summary>
        /// Get the total suppply in circulation for thge given NFT
        /// </summary>
        public async Task<int> TotalSupply(string tokenId)
        {
            return await Bridge.InvokeRoute<int>(getRoute("totalSupply"), Utils.ToJsonStringArray(tokenId));
        }

        /// <summary>
        /// Get all the possible contents of a given pack
        /// </summary>
        public async Task<PackContents> GetPackContents(string packId)
        {
            return await Bridge.InvokeRoute<PackContents>(getRoute("getPackContents"), Utils.ToJsonStringArray(packId));
        }

        /// WRITE FUNCTIONS

        /// <summary>
        /// Set approval to the given contract to transfer NFTs on behalf of the connected wallet
        /// </summary>
        public async Task<TransactionResult> SetApprovalForAll(string contractToApprove, bool approved)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("isApproved"), Utils.ToJsonStringArray(contractToApprove, approved));
        }

        /// <summary>
        /// Transfer NFTs to the given address
        /// </summary>
        public async Task<TransactionResult> Transfer(string to, string tokenId, int amount)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), Utils.ToJsonStringArray(to, tokenId, amount));
        }

        /// <summary>
        /// Create a new Pack with all the possible rewards (requires approval to transfer tokens/NFTs defined as rewards)
        /// </summary>
        public async Task<TransactionResult> Create(NewPackInput pack)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("create"), Utils.ToJsonStringArray(pack));
        }

        /// <summary>
        /// Create a new Pack with all the possible rewards and mints it to the given address (requires approval to transfer tokens/NFTs defined as rewards)
        /// </summary>
        public async Task<TransactionResult> CreateTo(string receiverAddress, NewPackInput pack)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("createTo"), Utils.ToJsonStringArray(receiverAddress, pack));
        }

        /// <summary>
        /// Add new contents to an existing pack
        /// </summary>
        public async Task<TransactionResult> AddPackContents(string packId, PackRewards newContents)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("addPackContents"), Utils.ToJsonStringArray(packId, newContents));
        }

        /// <summary>
        /// Open a pack and transfer the rewards to the connected wallet
        /// </summary>
        public async Task<PackRewards> Open(string packId, string amount = "1")
        {
            return await Bridge.InvokeRoute<PackRewards>(getRoute("open"), Utils.ToJsonStringArray(packId, amount));
        }
    }

    [System.Serializable]
    public struct PackRewards
    {
        public List<ERC20Reward> erc20Rewards;
        public List<ERC721Reward> erc721Rewards;
        public List<ERC1155Reward> erc1155Rewards;
    }

    [System.Serializable]
    public class PackContents
    {
        public List<ERC20Contents> erc20Rewards;
        public List<ERC721Contents> erc721Rewards;
        public List<ERC1155Contents> erc1155Rewards;
    }

    [System.Serializable]
    public class NewPackInput : PackContents
    {
        /// The Metadata of the pack NFT itself
        public NFTMetadata packMetadata;
        /// How many rewards can be obtained by opening a single pack
        public string rewardsPerPack;
    }

    [System.Serializable]
    public class ERC20Reward
    {
        /// the Token contract address
        public string contractAddress;
        /// How many tokens can be otained when opening a pack and receiving this reward
        public string quantityPerReward;
    }

    [System.Serializable]
    public class ERC20Contents : ERC20Reward
    {
        public string totalRewards;
    }

    [System.Serializable]
    public class ERC721Reward
    {
        /// the ERC721 contract address
        public string contractAddress;
        /// the tokenId of the NFT to be rewarded
        public string tokenId;
    }

    [System.Serializable]
    public class ERC721Contents : ERC721Reward
    {
    }

    [System.Serializable]
    public class ERC1155Reward
    {
        /// the ERC1155 contract address
        public string contractAddress;
        /// the tokenId of the NFT to be rewarded
        public string tokenId;
        /// How many NFTs can be otained when opening a pack and receiving this reward
        public string quantityPerReward;
    }

    [System.Serializable]
    public class ERC1155Contents : ERC1155Reward
    {
        public string totalRewards;
    }
}