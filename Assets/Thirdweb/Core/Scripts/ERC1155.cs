using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;
using Newtonsoft.Json;
using TokenERC1155Contract = Thirdweb.Contracts.TokenERC1155.ContractDefinition;
using DropERC1155Contract = Thirdweb.Contracts.DropERC1155.ContractDefinition;
using System.Linq;

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

        private readonly string contractAddress;

        /// <summary>
        /// Interact with any ERC1155 compatible contract.
        /// </summary>
        public ERC1155(string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "erc1155"))
        {
            this.contractAddress = contractAddress;
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
                var tokenURI = await TransactionManager.ThirdwebRead<TokenERC1155Contract.UriFunction, TokenERC1155Contract.UriOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.UriFunction() { TokenId = BigInteger.Parse(tokenId) }
                );

                tokenURI.ReturnValue1 = tokenURI.ReturnValue1.Contains("0x{id}") ? tokenURI.ReturnValue1.Replace("0x{id}", tokenId) : tokenURI.ReturnValue1;

                var nft = new NFT
                {
                    owner = "",
                    type = "ERC1155",
                    supply = await TotalSupply(tokenId),
                    quantityOwned = 404,
                    metadata = await ThirdwebManager.Instance.SDK.storage.DownloadText<NFTMetadata>(tokenURI.ReturnValue1)
                };
                nft.metadata.image = nft.metadata.image.ReplaceIPFS();
                nft.metadata.id = tokenId;
                nft.metadata.uri = tokenURI.ReturnValue1.ReplaceIPFS();
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
                int start = queryParams?.start ?? 0;
                int count = queryParams?.count ?? totalCount;
                int end = Math.Min(start + count, totalCount);
                List<NFT> allNfts = new();
                try
                {
                    var uriFunctions = Enumerable.Range(start, end - start).Select(i => new TokenERC1155Contract.UriFunction() { TokenId = new BigInteger(i) }).ToArray();
                    var uriResults = await TransactionManager.ThirdwebMulticallRead<TokenERC1155Contract.UriFunction, TokenERC1155Contract.UriOutputDTO>(contractAddress, uriFunctions);
                    var metadataFetchTasks = new List<Task<NFTMetadata>>();
                    for (int i = 0; i < uriResults.Length; i++)
                    {
                        var tokenUri = uriResults[i].ReturnValue1.Replace("0x{id}", uriFunctions[i].TokenId.ToString()).ReplaceIPFS();
                        metadataFetchTasks.Add(ThirdwebManager.Instance.SDK.storage.DownloadText<NFTMetadata>(tokenUri));
                    }
                    var metadataResults = await Task.WhenAll(metadataFetchTasks);
                    allNfts = new List<NFT>();
                    for (int i = 0; i < uriResults.Length; i++)
                    {
                        var tokenId = uriFunctions[i].TokenId.ToString();
                        var metadata = metadataResults[i];
                        metadata.image = metadata.image.ReplaceIPFS();
                        metadata.id = tokenId;
                        metadata.uri = uriResults[i].ReturnValue1.ReplaceIPFS();

                        var nft = new NFT
                        {
                            owner = "",
                            type = "ERC1155",
                            supply = await TotalSupply(tokenId),
                            quantityOwned = 404,
                            metadata = metadata
                        };

                        allNfts.Add(nft);
                    }
                }
                catch
                {
                    ThirdwebDebug.LogWarning("Unable to fetch using Multicall3, likely not deployed on this chain, falling back to single queries.");
                    allNfts = new List<NFT>();
                    for (int i = start; i < end; i++)
                        allNfts.Add(await Get(i.ToString()));
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
                string owner = address ?? await ThirdwebManager.Instance.SDK.wallet.GetAddress();
                int totalCount = await TotalCount();
                List<NFT> ownedNfts = new();

                try
                {
                    var balanceFunctions = Enumerable.Range(0, totalCount).Select(i => new TokenERC1155Contract.BalanceOfFunction() { Account = owner, Id = new BigInteger(i) }).ToArray();
                    var balanceResults = await TransactionManager.ThirdwebMulticallRead<TokenERC1155Contract.BalanceOfFunction, TokenERC1155Contract.BalanceOfOutputDTO>(
                        contractAddress,
                        balanceFunctions
                    );
                    var nonZeroBalanceTokenIds = balanceResults.Select((result, index) => (Balance: result.ReturnValue1, TokenId: index)).Where(x => x.Balance > 0).ToList();
                    var uriFunctions = nonZeroBalanceTokenIds.Select(x => new TokenERC1155Contract.UriFunction() { TokenId = new BigInteger(x.TokenId) }).ToArray();
                    var uriResults = await TransactionManager.ThirdwebMulticallRead<TokenERC1155Contract.UriFunction, TokenERC1155Contract.UriOutputDTO>(contractAddress, uriFunctions);
                    var metadataFetchTasks = uriResults.Select(uriResult => ThirdwebManager.Instance.SDK.storage.DownloadText<NFTMetadata>(uriResult.ReturnValue1.ReplaceIPFS())).ToList();
                    var metadataResults = await Task.WhenAll(metadataFetchTasks);
                    ownedNfts = new List<NFT>();
                    for (int i = 0; i < nonZeroBalanceTokenIds.Count; i++)
                    {
                        var tokenId = nonZeroBalanceTokenIds[i].TokenId.ToString();
                        var balance = nonZeroBalanceTokenIds[i].Balance;
                        var metadata = metadataResults[i];
                        metadata.image = metadata.image.ReplaceIPFS();
                        metadata.id = tokenId;
                        metadata.uri = uriResults[i].ReturnValue1.ReplaceIPFS();

                        ownedNfts.Add(
                            new NFT
                            {
                                owner = owner,
                                type = "ERC1155",
                                supply = await TotalSupply(tokenId),
                                quantityOwned = (int)balance,
                                metadata = metadata
                            }
                        );
                    }
                }
                catch
                {
                    ThirdwebDebug.LogWarning("Unable to fetch using Multicall3, likely not deployed on this chain, falling back to single queries.");
                    for (int i = 0; i < totalCount; i++)
                    {
                        BigInteger ownedBalance = BigInteger.Parse(await BalanceOf(owner, i.ToString()));
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
                return await Bridge.InvokeRoute<string>(getRoute("balance"), Utils.ToJsonStringArray(tokenId));
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
                var balance = await TransactionManager.ThirdwebRead<TokenERC1155Contract.BalanceOfFunction, TokenERC1155Contract.BalanceOfOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.BalanceOfFunction() { Account = address, Id = BigInteger.Parse(tokenId) }
                );
                return balance.ReturnValue1.ToString();
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
                var IsApprovedForAll = await TransactionManager.ThirdwebRead<TokenERC1155Contract.IsApprovedForAllFunction, TokenERC1155Contract.IsApprovedForAllOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.IsApprovedForAllFunction() { Account = address, Operator = approvedContract }
                );
                return IsApprovedForAll.ReturnValue1.ToString();
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
                var nextTokenIdToMint = await TransactionManager.ThirdwebRead<TokenERC1155Contract.NextTokenIdToMintFunction, TokenERC1155Contract.NextTokenIdToMintOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.NextTokenIdToMintFunction() { }
                );
                return (int)nextTokenIdToMint.ReturnValue1;
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
                var totalSupply = await TransactionManager.ThirdwebRead<TokenERC1155Contract.TotalSupplyFunction, TokenERC1155Contract.TotalSupplyOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.TotalSupplyFunction() { ReturnValue1 = BigInteger.Parse(tokenId) }
                );
                return (int)totalSupply.ReturnValue1;
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
                return await TransactionManager.ThirdwebWrite(contractAddress, new TokenERC1155Contract.SetApprovalForAllFunction() { Operator = contractToApprove, Approved = approved });
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
                return await TransactionManager.ThirdwebWrite(
                    contractAddress,
                    new TokenERC1155Contract.SafeTransferFromFunction()
                    {
                        From = await ThirdwebManager.Instance.SDK.wallet.GetAddress(),
                        To = to,
                        Id = BigInteger.Parse(tokenId),
                        Amount = amount,
                        Data = new byte[0]
                    }
                );
            }
        }

        /// <summary>
        /// Batch transfer NFTs to the given address
        /// </summary>
        /// <param name="to">Address to transfer to</param>
        /// <param name="tokenIds">ERC1155 token ids to transfer</param>
        /// <param name="amounts">ERC1155 token id amounts to transfer</param>
        /// <returns>The transaction result as a <see cref="TransactionResult"/> object</returns>
        public async Task<TransactionResult> TransferBatch(string to, BigInteger[] tokenIds, BigInteger[] amounts)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("transferBatch"), Utils.ToJsonStringArray(to, tokenIds, amounts));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(
                    contractAddress,
                    new TokenERC1155Contract.SafeBatchTransferFromFunction()
                    {
                        From = await ThirdwebManager.Instance.SDK.wallet.GetAddress(),
                        To = to,
                        Ids = tokenIds.ToList(),
                        Amounts = amounts.ToList(),
                        Data = new byte[0]
                    }
                );
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
                return await TransactionManager.ThirdwebWrite(
                    contractAddress,
                    new TokenERC1155Contract.BurnFunction()
                    {
                        Account = await ThirdwebManager.Instance.SDK.wallet.GetAddress(),
                        Id = BigInteger.Parse(tokenId),
                        Value = amount
                    }
                );
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
                BigInteger rawPrice = BigInteger.Parse(claimCondition.currencyMetadata.value);
                return await TransactionManager.ThirdwebWrite(
                    contractAddress,
                    new DropERC1155Contract.ClaimFunction()
                    {
                        Receiver = address,
                        TokenId = BigInteger.Parse(tokenId),
                        Quantity = quantity,
                        Currency = claimCondition.currencyAddress,
                        PricePerToken = rawPrice,
                        AllowlistProof = new DropERC1155Contract.AllowlistProof
                        {
                            Proof = new List<byte[]>(),
                            Currency = claimCondition.currencyAddress,
                            PricePerToken = rawPrice,
                            QuantityLimitPerWallet = BigInteger.Parse(claimCondition.maxClaimablePerWallet),
                        }, // TODO add support for allowlists
                        Data = new byte[] { }
                    },
                    claimCondition.currencyAddress == Utils.NativeTokenAddress ? quantity * rawPrice : 0
                );
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
                return await TransactionManager.ThirdwebWrite(
                    contractAddress,
                    new TokenERC1155Contract.MintToFunction()
                    {
                        To = address,
                        TokenId = Utils.GetMaxUint256(),
                        Uri = uri.IpfsHash.CidToIpfsUrl(),
                        Amount = nft.supply
                    }
                );
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
                var uri = await TransactionManager.ThirdwebRead<TokenERC1155Contract.UriFunction, TokenERC1155Contract.UriOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.UriFunction() { TokenId = BigInteger.Parse(tokenId) }
                );

                return await TransactionManager.ThirdwebWrite(
                    contractAddress,
                    new TokenERC1155Contract.MintToFunction()
                    {
                        To = await ThirdwebManager.Instance.SDK.wallet.GetAddress(),
                        TokenId = BigInteger.Parse(tokenId),
                        Uri = uri.ReturnValue1,
                        Amount = additionalSupply
                    }
                );
            }
        }
    }

    /// <summary>
    /// Fetch claim conditions for a given ERC1155 drop contract
    /// </summary>
    public class ERC1155ClaimConditions : Routable
    {
        private readonly string contractAddress;

        public ERC1155ClaimConditions(string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "claimConditions"))
        {
            this.contractAddress = contractAddress;
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
                var conditionId = await TransactionManager.ThirdwebRead<DropERC1155Contract.GetActiveClaimConditionIdFunction, DropERC1155Contract.GetActiveClaimConditionIdOutputDTO>(
                    contractAddress,
                    new DropERC1155Contract.GetActiveClaimConditionIdFunction() { TokenId = BigInteger.Parse(tokenId) }
                );

                var data = await TransactionManager.ThirdwebRead<DropERC1155Contract.GetClaimConditionByIdFunction, DropERC1155Contract.GetClaimConditionByIdOutputDTO>(
                    contractAddress,
                    new DropERC1155Contract.GetClaimConditionByIdFunction() { TokenId = BigInteger.Parse(tokenId), ConditionId = conditionId.ReturnValue1 }
                );

                var currency = new Currency();
                try
                {
                    currency = await ThirdwebManager.Instance.SDK.GetContract(data.Condition.Currency).ERC20.Get();
                }
                catch
                {
                    ThirdwebDebug.Log("Could not fetch currency metadata, proceeding without it.");
                }

                return new ClaimConditions()
                {
                    availableSupply = (data.Condition.MaxClaimableSupply - data.Condition.SupplyClaimed).ToString(),
                    currencyAddress = data.Condition.Currency,
                    currencyMetadata = new CurrencyValue(
                        currency.name,
                        currency.symbol,
                        currency.decimals,
                        data.Condition.PricePerToken.ToString(),
                        data.Condition.PricePerToken.ToString().FormatERC20(4, int.Parse(currency.decimals), true)
                    ),
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
                return await Bridge.InvokeRoute<bool>(getRoute("getClaimerProofs"), Utils.ToJsonStringArray(tokenId, claimerAddress));
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
        public long mintStartTime;
        public long mintEndTime;

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
            this.mintStartTime = Utils.GetUnixTimeStampNow() - 60;
            this.mintEndTime = Utils.GetUnixTimeStampIn10Years();
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
        public long mintStartTime;
        public long mintEndTime;

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
            this.mintStartTime = Utils.GetUnixTimeStampNow() - 60;
            this.mintEndTime = Utils.GetUnixTimeStampIn10Years();
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
        private readonly string contractAddress;

        /// <summary>
        /// Generate, verify and mint signed mintable payloads
        /// </summary>
        public ERC1155Signature(string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "signature"))
        {
            this.contractAddress = contractAddress;
        }

        /// <summary>
        /// Generate a signed mintable payload. Requires minting permission.
        /// </summary>
        public async Task<ERC1155SignedPayload> Generate(ERC1155MintPayload payloadToSign, string privateKeyOverride = "")
        {
            if (Utils.IsWebGLBuild())
            {
                var signedPayload = await Bridge.InvokeRoute<ERC1155SignedPayload>(getRoute("generate"), Utils.ToJsonStringArray(payloadToSign));

                if (privateKeyOverride == "")
                    return signedPayload;

                var req = new TokenERC1155Contract.MintRequest()
                {
                    To = payloadToSign.to,
                    RoyaltyRecipient = signedPayload.payload.royaltyRecipient,
                    RoyaltyBps = signedPayload.payload.royaltyBps,
                    PrimarySaleRecipient = signedPayload.payload.primarySaleRecipient,
                    TokenId = Utils.GetMaxUint256(),
                    Uri = signedPayload.payload.uri,
                    Quantity = payloadToSign.quantity,
                    PricePerToken = BigInteger.Parse(payloadToSign.price.ToWei()),
                    Currency = payloadToSign.currencyAddress,
                    ValidityStartTimestamp = payloadToSign.mintStartTime,
                    ValidityEndTimestamp = payloadToSign.mintEndTime,
                    Uid = payloadToSign.uid.HexStringToByteArray()
                };

                string signature = await Thirdweb.EIP712.GenerateSignature_TokenERC1155(
                    "TokenERC1155",
                    "1",
                    await ThirdwebManager.Instance.SDK.wallet.GetChainId(),
                    contractAddress,
                    req,
                    string.IsNullOrEmpty(privateKeyOverride) ? null : privateKeyOverride
                );

                signedPayload = new ERC1155SignedPayload()
                {
                    signature = signature,
                    payload = new ERC1155SignedPayloadOutput()
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
                    }
                };

                return signedPayload;
            }
            else
            {
                var uri = await ThirdwebManager.Instance.SDK.storage.UploadText(JsonConvert.SerializeObject(payloadToSign.metadata));
                var royalty = await TransactionManager.ThirdwebRead<TokenERC1155Contract.GetDefaultRoyaltyInfoFunction, TokenERC1155Contract.GetDefaultRoyaltyInfoOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.GetDefaultRoyaltyInfoFunction() { }
                );
                var primarySaleRecipient = await TransactionManager.ThirdwebRead<TokenERC1155Contract.PrimarySaleRecipientFunction, TokenERC1155Contract.PrimarySaleRecipientOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.PrimarySaleRecipientFunction() { }
                );

                var req = new TokenERC1155Contract.MintRequest()
                {
                    To = payloadToSign.to,
                    RoyaltyRecipient = royalty.ReturnValue1,
                    RoyaltyBps = royalty.ReturnValue2,
                    PrimarySaleRecipient = primarySaleRecipient.ReturnValue1,
                    TokenId = Utils.GetMaxUint256(),
                    Uri = uri.IpfsHash.CidToIpfsUrl(),
                    Quantity = payloadToSign.quantity,
                    PricePerToken = BigInteger.Parse(payloadToSign.price.ToWei()),
                    Currency = payloadToSign.currencyAddress,
                    ValidityStartTimestamp = payloadToSign.mintStartTime,
                    ValidityEndTimestamp = payloadToSign.mintEndTime,
                    Uid = payloadToSign.uid.HexStringToByteArray()
                };

                string signature = await Thirdweb.EIP712.GenerateSignature_TokenERC1155(
                    "TokenERC1155",
                    "1",
                    await ThirdwebManager.Instance.SDK.wallet.GetChainId(),
                    contractAddress,
                    req,
                    string.IsNullOrEmpty(privateKeyOverride) ? null : privateKeyOverride
                );

                var signedPayload = new ERC1155SignedPayload()
                {
                    signature = signature,
                    payload = new ERC1155SignedPayloadOutput()
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
                    }
                };

                return signedPayload;
            }
        }

        public async Task<ERC1155SignedPayload> GenerateFromTokenId(ERC1155MintAdditionalPayload payloadToSign, string privateKeyOverride = "")
        {
            if (Utils.IsWebGLBuild())
            {
                var signedPayload = await Bridge.InvokeRoute<ERC1155SignedPayload>(getRoute("generateFromTokenId"), Utils.ToJsonStringArray(payloadToSign));

                if (privateKeyOverride == "")
                    return signedPayload;

                var req = new TokenERC1155Contract.MintRequest()
                {
                    To = payloadToSign.to,
                    RoyaltyRecipient = signedPayload.payload.royaltyRecipient,
                    RoyaltyBps = (BigInteger)signedPayload.payload.royaltyBps,
                    PrimarySaleRecipient = signedPayload.payload.primarySaleRecipient,
                    TokenId = BigInteger.Parse(payloadToSign.tokenId),
                    Uri = signedPayload.payload.uri,
                    Quantity = payloadToSign.quantity,
                    PricePerToken = BigInteger.Parse(payloadToSign.price),
                    Currency = payloadToSign.currencyAddress,
                    ValidityStartTimestamp = payloadToSign.mintStartTime,
                    ValidityEndTimestamp = payloadToSign.mintEndTime,
                    Uid = payloadToSign.uid.HexStringToByteArray()
                };
                string signature = await Thirdweb.EIP712.GenerateSignature_TokenERC1155(
                    "TokenERC1155",
                    "1",
                    await ThirdwebManager.Instance.SDK.wallet.GetChainId(),
                    contractAddress,
                    req,
                    string.IsNullOrEmpty(privateKeyOverride) ? null : privateKeyOverride
                );

                signedPayload = new ERC1155SignedPayload()
                {
                    signature = signature,
                    payload = new ERC1155SignedPayloadOutput()
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
                    }
                };
                return signedPayload;
            }
            else
            {
                // var uri = await ThirdwebManager.Instance.SDK.storage.UploadText(JsonConvert.SerializeObject(payloadToSign.metadata));
                var uri = await TransactionManager.ThirdwebRead<TokenERC1155Contract.UriFunction, TokenERC1155Contract.UriOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.UriFunction() { TokenId = BigInteger.Parse(payloadToSign.tokenId) }
                );
                var royalty = await TransactionManager.ThirdwebRead<TokenERC1155Contract.GetDefaultRoyaltyInfoFunction, TokenERC1155Contract.GetDefaultRoyaltyInfoOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.GetDefaultRoyaltyInfoFunction() { }
                );
                var primarySaleRecipient = await TransactionManager.ThirdwebRead<TokenERC1155Contract.PrimarySaleRecipientFunction, TokenERC1155Contract.PrimarySaleRecipientOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.PrimarySaleRecipientFunction() { }
                );

                var req = new TokenERC1155Contract.MintRequest()
                {
                    To = payloadToSign.to,
                    RoyaltyRecipient = royalty.ReturnValue1,
                    RoyaltyBps = royalty.ReturnValue2,
                    PrimarySaleRecipient = primarySaleRecipient.ReturnValue1,
                    TokenId = BigInteger.Parse(payloadToSign.tokenId),
                    Uri = uri.ReturnValue1,
                    Quantity = payloadToSign.quantity,
                    PricePerToken = BigInteger.Parse(payloadToSign.price),
                    Currency = payloadToSign.currencyAddress,
                    ValidityStartTimestamp = payloadToSign.mintStartTime,
                    ValidityEndTimestamp = payloadToSign.mintEndTime,
                    Uid = payloadToSign.uid.HexStringToByteArray()
                };

                string signature = await Thirdweb.EIP712.GenerateSignature_TokenERC1155(
                    "TokenERC1155",
                    "1",
                    await ThirdwebManager.Instance.SDK.wallet.GetChainId(),
                    contractAddress,
                    req,
                    string.IsNullOrEmpty(privateKeyOverride) ? null : privateKeyOverride
                );

                var signedPayload = new ERC1155SignedPayload()
                {
                    signature = signature,
                    payload = new ERC1155SignedPayloadOutput()
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
                    }
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
                var verify = await TransactionManager.ThirdwebRead<TokenERC1155Contract.VerifyFunction, TokenERC1155Contract.VerifyOutputDTO>(
                    contractAddress,
                    new TokenERC1155Contract.VerifyFunction()
                    {
                        Req = new TokenERC1155Contract.MintRequest()
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
                        },
                        Signature = signedPayload.signature.HexStringToByteArray()
                    }
                );

                return verify.ReturnValue1;
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
                return await TransactionManager.ThirdwebWrite(
                    contractAddress,
                    new TokenERC1155Contract.MintWithSignatureFunction()
                    {
                        Req = new TokenERC1155Contract.MintRequest()
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
                        },
                        Signature = signedPayload.signature.HexStringToByteArray()
                    }
                );
            }
        }
    }
}
