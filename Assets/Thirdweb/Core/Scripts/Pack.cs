using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thirdweb.Contracts.Pack;
using UnityEngine;
using UnityEngine.Networking;

namespace Thirdweb
{
    /// <summary>
    /// Interact with a Pack contract.
    /// </summary>
    public class Pack : Routable
    {
        public string chain;
        public string address;

        PackService packService;

        /// <summary>
        /// Interact with a Marketplace contract.
        /// </summary>
        public Pack(string chain, string address)
            : base($"{address}{subSeparator}pack")
        {
            this.chain = chain;
            this.address = address;

            if (!Utils.IsWebGLBuild())
            {
                this.packService = new PackService(ThirdwebManager.Instance.SDK.web3, address);
            }
        }

        /// READ FUNCTIONS

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

                string tokenURI = await packService.UriQueryAsync(BigInteger.Parse(tokenId));
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
                throw new UnityException("This functionality is not yet available on your current platform.");
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
                throw new UnityException("This functionality is not yet available on your current platform.");
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
                return (await packService.BalanceOfQueryAsync(address, BigInteger.Parse(tokenId))).ToString();
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
                return (await packService.IsApprovedForAllQueryAsync(address, approvedContract)).ToString();
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
                throw new UnityException("This functionality is not yet available on your current platform.");
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
                return (int)await packService.TotalSupplyQueryAsync(BigInteger.Parse(tokenId));
            }
        }

        /// <summary>
        /// Get all the possible contents of a given pack
        /// </summary>
        public async Task<PackContents> GetPackContents(string packId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<PackContents>(getRoute("getPackContents"), Utils.ToJsonStringArray(packId));
            }
            else
            {
                var result = await packService.GetPackContentsQueryAsync(BigInteger.Parse(packId));
                PackContents packContents = new PackContents();
                List<ERC20Reward> erc20Rewards = new List<ERC20Reward>();
                List<ERC721Reward> erc721Rewards = new List<ERC721Reward>();
                List<ERC1155Reward> erc1155Rewards = new List<ERC1155Reward>();
                foreach (var tokenReward in result.Contents)
                {
                    switch (tokenReward.TokenType.ToString())
                    {
                        case "ERC20":
                            ERC20Reward tempERC20 = new ERC20Reward();
                            tempERC20.contractAddress = tokenReward.AssetContract;
                            tempERC20.quantityPerReward = tokenReward.TotalAmount.ToString();
                            erc20Rewards.Add(tempERC20);
                            break;
                        case "ERC721":
                            ERC721Reward tempERC721 = new ERC721Reward();
                            tempERC721.contractAddress = tokenReward.AssetContract;
                            tempERC721.tokenId = tokenReward.TokenId.ToString();
                            erc721Rewards.Add(tempERC721);
                            break;
                        case "ERC1155":
                            ERC1155Reward tempERC1155 = new ERC1155Reward();
                            tempERC1155.contractAddress = tokenReward.AssetContract;
                            tempERC1155.tokenId = tokenReward.TokenId.ToString();
                            tempERC1155.quantityPerReward = tokenReward.TotalAmount.ToString();
                            erc1155Rewards.Add(tempERC1155);
                            break;
                        default:
                            break;
                    }
                }
                return packContents;
            }
        }

        /// WRITE FUNCTIONS

        /// <summary>
        /// Set approval to the given contract to transfer NFTs on behalf of the connected wallet
        /// </summary>
        public async Task<TransactionResult> SetApprovalForAll(string contractToApprove, bool approved)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(
                    getRoute("isApproved"),
                    Utils.ToJsonStringArray(contractToApprove, approved)
                );
            }
            else
            {
                var receipt = await packService.SetApprovalForAllRequestAndWaitForReceiptAsync(contractToApprove, approved);
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
                var receipt = await packService.SafeTransferFromRequestAndWaitForReceiptAsync(
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
        /// Create a new Pack with all the possible rewards (requires approval to transfer tokens/NFTs defined as rewards)
        /// </summary>
        public async Task<TransactionResult> Create(NewPackInput pack)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("create"), Utils.ToJsonStringArray(pack));
            }
            else
            {
                return await CreateTo(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), pack);
            }
        }

        /// <summary>
        /// Create a new Pack with all the possible rewards and mints it to the given address (requires approval to transfer tokens/NFTs defined as rewards)
        /// </summary>
        public async Task<TransactionResult> CreateTo(string receiverAddress, NewPackInput pack)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("createTo"), Utils.ToJsonStringArray(receiverAddress, pack));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
                var receipt = await packService.CreatePackRequestAndWaitForReceiptAsync(
                    null,
                    null,
                    pack.packMetadata.uri,
                    404,
                    BigInteger.Parse(pack.rewardsPerPack),
                    receiverAddress
                ); // TODO: fix
                return receipt.ToTransactionResult();
            }
        }

        /// <summary>
        /// Add new contents to an existing pack
        /// </summary>
        public async Task<TransactionResult> AddPackContents(string packId, PackRewards newContents)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(
                    getRoute("addPackContents"),
                    Utils.ToJsonStringArray(packId, newContents)
                );
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
                var receipt = await packService.AddPackContentsRequestAndWaitForReceiptAsync(BigInteger.Parse(packId), null, null, null); // TODO: fix
                return receipt.ToTransactionResult();
            }
        }

        /// <summary>
        /// Open a pack and transfer the rewards to the connected wallet
        /// </summary>
        public async Task<PackRewards> Open(string packId, string amount = "1")
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<PackRewards>(getRoute("open"), Utils.ToJsonStringArray(packId, amount));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
                var receipt = await packService.OpenPackRequestAndWaitForReceiptAsync(BigInteger.Parse(packId), BigInteger.Parse(amount)); // TODO: fix
                return new PackRewards(); // TODO: Decode event and create/return PackRewards
            }
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
    public class ERC721Contents : ERC721Reward { }

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
