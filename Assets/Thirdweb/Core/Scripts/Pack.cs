using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Contracts;
using Newtonsoft.Json;
using Thirdweb.Contracts.Pack;
using UnityEngine;
using UnityEngine.Networking;
using PackContract = Thirdweb.Contracts.Pack.ContractDefinition;

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
                this.packService = new PackService(ThirdwebManager.Instance.SDK.nativeSession.web3, address);
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
                var erc20R = new List<ERC20Contents>();
                var erc721R = new List<ERC721Contents>();
                var erc1155R = new List<ERC1155Contents>();
                foreach (var tokenReward in result.Contents)
                {
                    switch (tokenReward.TokenType)
                    {
                        case 0:
                            var tempERC20 = new ERC20Contents();
                            tempERC20.contractAddress = tokenReward.AssetContract;
                            tempERC20.quantityPerReward = tokenReward.TotalAmount.ToString();
                            erc20R.Add(tempERC20);
                            Debug.Log("Found ERC20");
                            break;
                        case 1:
                            var tempERC721 = new ERC721Contents();
                            tempERC721.contractAddress = tokenReward.AssetContract;
                            tempERC721.tokenId = tokenReward.TokenId.ToString();
                            erc721R.Add(tempERC721);
                            Debug.Log("Found ERC721");
                            break;
                        case 2:
                            var tempERC1155 = new ERC1155Contents();
                            tempERC1155.contractAddress = tokenReward.AssetContract;
                            tempERC1155.tokenId = tokenReward.TokenId.ToString();
                            tempERC1155.quantityPerReward = tokenReward.TotalAmount.ToString();
                            erc1155R.Add(tempERC1155);
                            Debug.Log("Found ERC1155");
                            break;
                        default:
                            break;
                    }
                }
                PackContents contents = new PackContents();
                contents.erc20Contents = erc20R;
                contents.erc721Contents = erc721R;
                contents.erc1155Contents = erc1155R;
                return contents;
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
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("isApproved"), Utils.ToJsonStringArray(contractToApprove, approved));
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
                var uri = await ThirdwebManager.Instance.SDK.storage.UploadText(JsonConvert.SerializeObject(pack.packMetadata));
                var receipt = await packService.CreatePackRequestAndWaitForReceiptAsync(
                    pack.ToPackTokenList(),
                    pack.ToPackRewardUnitsList(),
                    uri.IpfsHash.cidToIpfsUrl(),
                    await Utils.GetCurrentBlockTimeStamp(),
                    BigInteger.Parse(pack.rewardsPerPack),
                    receiverAddress
                );
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
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("addPackContents"), Utils.ToJsonStringArray(packId, newContents));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Open a pack and transfer the rewards to the connected wallet
        /// </summary>
        public async Task<PackRewards> Open(string packId, string amount = "1", int gasLimit = 500000)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<PackRewards>(getRoute("open"), Utils.ToJsonStringArray(packId, amount));
            }
            else
            {
                var openPackFunction = new PackContract.OpenPackFunction() { PackId = BigInteger.Parse(packId), AmountToOpen = BigInteger.Parse(amount) };
                // var gasEstimate = await packService.ContractHandler.EstimateGasAsync(openPackFunction);
                openPackFunction.Gas = gasLimit;
                var receipt = await packService.OpenPackRequestAndWaitForReceiptAsync(openPackFunction);
                var packOpenedEvents = receipt.DecodeAllEvents<PackContract.PackOpenedEventDTO>();
                List<PackContract.Token> tokensAwarded = new List<PackContract.Token>();
                foreach (var packOpenedEvent in packOpenedEvents)
                {
                    tokensAwarded.AddRange(packOpenedEvent.Event.RewardUnitsDistributed);
                }
                PackRewards packRewards = new PackRewards()
                {
                    erc20Rewards = new List<ERC20Reward>(),
                    erc721Rewards = new List<ERC721Reward>(),
                    erc1155Rewards = new List<ERC1155Reward>()
                };
                foreach (var tokenAwarded in tokensAwarded)
                {
                    if (tokenAwarded.TokenType == 0)
                    {
                        packRewards.erc20Rewards.Add(new ERC20Reward() { contractAddress = tokenAwarded.AssetContract, quantityPerReward = tokenAwarded.TotalAmount.ToString() });
                    }
                    else if (tokenAwarded.TokenType == 1)
                    {
                        packRewards.erc721Rewards.Add(new ERC721Reward() { contractAddress = tokenAwarded.AssetContract, tokenId = tokenAwarded.TokenId.ToString() });
                    }
                    else if (tokenAwarded.TokenType == 2)
                    {
                        packRewards.erc1155Rewards.Add(
                            new ERC1155Reward()
                            {
                                contractAddress = tokenAwarded.AssetContract,
                                tokenId = tokenAwarded.TokenId.ToString(),
                                quantityPerReward = tokenAwarded.TotalAmount.ToString()
                            }
                        );
                    }
                }
                return packRewards;
            }
        }
    }

    [System.Serializable]
    public class PackRewards
    {
        public List<ERC20Reward> erc20Rewards;
        public List<ERC721Reward> erc721Rewards;
        public List<ERC1155Reward> erc1155Rewards;

        public override string ToString()
        {
            string erc20str = "ERC20 Rewards:\n";
            foreach (var reward in erc20Rewards)
                erc20str += reward.ToString();
            string erc721str = "ERC721 Rewards:\n";
            foreach (var reward in erc721Rewards)
                erc721str += reward.ToString();
            string erc1155str = "ERC1155 Rewards:\n";
            foreach (var reward in erc1155Rewards)
                erc1155str += reward.ToString();
            return "PackRewards:\n" + erc20str + erc721str + erc1155str;
        }
    }

    [System.Serializable]
    public class PackContents
    {
        public List<ERC20Contents> erc20Contents;
        public List<ERC721Contents> erc721Contents;
        public List<ERC1155Contents> erc1155Contents;

        public override string ToString()
        {
            string erc20str = "ERC20 Contents:\n";
            foreach (var content in erc20Contents)
                erc20str += content.ToString();
            string erc721str = "ERC721 Contents:\n";
            foreach (var content in erc721Contents)
                erc721str += content.ToString();
            string erc1155str = "ERC1155 Contents:\n";
            foreach (var content in erc1155Contents)
                erc1155str += content.ToString();
            return "PackContents:\n" + erc20str + erc721str + erc1155str;
        }
    }

    [System.Serializable]
    public class NewPackInput : PackContents
    {
        /// The Metadata of the pack NFT itself
        public NFTMetadata packMetadata;

        /// How many rewards can be obtained by opening a single pack
        public string rewardsPerPack;

        public override string ToString()
        {
            return "NewPackInput:\n" + $"packMetadata: {packMetadata.ToString()}\n" + $"rewardsPerPack: {rewardsPerPack.ToString()}\n";
        }
    }

    [System.Serializable]
    public class ERC20Reward
    {
        /// the Token contract address
        public string contractAddress;

        /// How many tokens can be otained when opening a pack and receiving this reward
        public string quantityPerReward;

        public override string ToString()
        {
            return "ERC20Reward:\n" + $"contractAddress: {contractAddress.ToString()}\n" + $"quantityPerReward: {quantityPerReward.ToString()}\n";
        }
    }

    [System.Serializable]
    public class ERC20Contents : ERC20Reward
    {
        public string totalRewards;

        public override string ToString()
        {
            return "ERC20Contents:\n" + $"totalRewards: {totalRewards.ToString()}\n" + base.ToString();
        }
    }

    [System.Serializable]
    public class ERC721Reward
    {
        /// the ERC721 contract address
        public string contractAddress;

        /// the tokenId of the NFT to be rewarded
        public string tokenId;

        public override string ToString()
        {
            return "ERC721Reward:\n" + $"contractAddress: {contractAddress.ToString()}\n" + $"tokenId: {tokenId.ToString()}\n";
        }
    }

    [System.Serializable]
    public class ERC721Contents : ERC721Reward
    {
        public override string ToString()
        {
            return base.ToString();
        }
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

        public override string ToString()
        {
            return "ERC1155Reward:\n" + $"contractAddress: {contractAddress.ToString()}\n" + $"tokenId: {tokenId.ToString()}\n" + $"contractAddress: {tokenId.ToString()}\n";
        }
    }

    [System.Serializable]
    public class ERC1155Contents : ERC1155Reward
    {
        public string totalRewards;

        public override string ToString()
        {
            return "ERC1155Contents:\n" + $"totalRewards: {totalRewards.ToString()}\n" + base.ToString();
        }
    }
}
