using System;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;
using System.Collections.Generic;
using DirectListingsContract = Thirdweb.Contracts.DirectListingsLogic.ContractDefinition;
using EnglishAuctionsContract = Thirdweb.Contracts.EnglishAuctionsLogic.ContractDefinition;
using OffersContract = Thirdweb.Contracts.OffersLogic.ContractDefinition;

namespace Thirdweb
{
    /// <summary>
    /// Interact with any MarketplaceV3 contract.
    /// </summary>
    public class Marketplace : Routable
    {
        public DirectListings DirectListings;
        public EnglishAuctions EnglishAuctions;
        public Offers Offers;

        private readonly ThirdwebSDK _sdk;

        public Marketplace(ThirdwebSDK sdk, string parentRoute, string contractAddress)
            : base(parentRoute)
        {
            _sdk = sdk;
            this.DirectListings = new DirectListings(sdk, baseRoute, contractAddress);
            this.EnglishAuctions = new EnglishAuctions(sdk, baseRoute, contractAddress);
            this.Offers = new Offers(sdk, baseRoute, contractAddress);
        }
    }

    public class DirectListings : Routable
    {
        private readonly string _contractAddress;
        private readonly ThirdwebSDK _sdk;

        public DirectListings(ThirdwebSDK sdk, string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "directListings"))
        {
            this._contractAddress = contractAddress;
            this._sdk = sdk;
        }

        // READ FUNCTIONS

        public async Task<List<DirectListing>> GetAll(MarketplaceFilters filters = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<DirectListing>>(getRoute("getAll"), filters == null ? new string[] { } : Utils.ToJsonStringArray(filters));
            }
            else
            {
                int totalSupply = int.Parse(await GetTotalCount());
                int start = filters?.start ?? 0;
                int count = filters?.count ?? 0;
                int end = count == 0 ? totalSupply - 1 : start + count;
                var result = await TransactionManager.ThirdwebRead<DirectListingsContract.GetAllListingsFunction, DirectListingsContract.GetAllListingsOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.GetAllListingsFunction() { StartId = start, EndId = end }
                );
                var allListings = result.AllListings;
                var filteredListings = new List<DirectListing>();
                foreach (var listing in allListings)
                {
                    var tempListing = await GetListing(listing.ListingId.ToString());

                    if (filters?.tokenContract != null && filters?.tokenContract != tempListing.assetContractAddress)
                        continue;
                    if (filters?.tokenId != null && filters?.tokenId != tempListing.tokenId)
                        continue;
                    if (filters?.seller != null && filters?.seller != tempListing.creatorAddress)
                        continue;

                    filteredListings.Add(tempListing);
                }
                return filteredListings;
            }
        }

        public async Task<List<DirectListing>> GetAllValid(MarketplaceFilters filters = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<DirectListing>>(getRoute("getAllValid"), filters == null ? new string[] { } : Utils.ToJsonStringArray(filters));
            }
            else
            {
                int totalSupply = int.Parse(await GetTotalCount());
                int start = filters?.start ?? 0;
                int count = filters?.count ?? 0;
                int end = count == 0 ? totalSupply - 1 : start + count;
                var result = await TransactionManager.ThirdwebRead<DirectListingsContract.GetAllValidListingsFunction, DirectListingsContract.GetAllValidListingsOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.GetAllValidListingsFunction() { StartId = start, EndId = end }
                );
                var allValidListings = result.ValidListings;
                var filteredListings = new List<DirectListing>();
                foreach (var listing in allValidListings)
                {
                    var tempListing = await GetListing(listing.ListingId.ToString());

                    if (filters?.tokenContract != null && filters?.tokenContract != tempListing.assetContractAddress)
                        continue;
                    if (filters?.tokenId != null && filters?.tokenId != tempListing.tokenId)
                        continue;
                    if (filters?.seller != null && filters?.seller != tempListing.creatorAddress)
                        continue;

                    filteredListings.Add(tempListing);
                }
                return filteredListings;
            }
        }

        public async Task<DirectListing> GetListing(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<DirectListing>(getRoute("getListing"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                var result = await TransactionManager.ThirdwebRead<DirectListingsContract.GetListingFunction, DirectListingsContract.GetListingOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.GetListingFunction() { ListingId = BigInteger.Parse(listingID) }
                );

                Currency currency = await _sdk.GetContract(result.Listing.Currency).ERC20.Get();
                var metadata = new NFTMetadata();
                try
                {
                    metadata = (await _sdk.GetContract(result.Listing.AssetContract).ERC721.Get(result.Listing.TokenId.ToString())).metadata;
                }
                catch (System.Exception)
                {
                    metadata = (await _sdk.GetContract(result.Listing.AssetContract).ERC1155.Get(result.Listing.TokenId.ToString())).metadata;
                }

                return new DirectListing()
                {
                    id = result.Listing.ListingId.ToString(),
                    creatorAddress = result.Listing.ListingCreator,
                    assetContractAddress = result.Listing.AssetContract,
                    tokenId = result.Listing.TokenId.ToString(),
                    quantity = result.Listing.Quantity.ToString(),
                    currencyContractAddress = result.Listing.Currency,
                    currencyValuePerToken = new CurrencyValue(
                        currency.name,
                        currency.symbol,
                        currency.decimals,
                        result.Listing.PricePerToken.ToString(),
                        result.Listing.PricePerToken.ToString().FormatERC20(4, int.Parse(currency.decimals), true)
                    ),
                    pricePerToken = result.Listing.PricePerToken.ToString(),
                    asset = metadata,
                    startTimeInSeconds = (long)result.Listing.StartTimestamp,
                    endTimeInSeconds = (long)result.Listing.EndTimestamp,
                    isReservedListing = result.Listing.Reserved,
                    status = (MarkteplaceStatus)result.Listing.Status
                };
            }
        }

        public async Task<string> GetTotalCount()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("getTotalCount"), new string[] { });
            }
            else
            {
                var result = await TransactionManager.ThirdwebRead<DirectListingsContract.TotalListingsFunction, DirectListingsContract.TotalListingsOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.TotalListingsFunction() { }
                );
                return result.ReturnValue1.ToString();
            }
        }

        public async Task<bool> IsBuyerApprovedForListing(string listingID, string buyerAddress)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<bool>(getRoute("isBuyerApprovedForListing"), Utils.ToJsonStringArray(listingID, buyerAddress));
            }
            else
            {
                var result = await TransactionManager.ThirdwebRead<DirectListingsContract.IsBuyerApprovedForListingFunction, DirectListingsContract.IsBuyerApprovedForListingOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.IsBuyerApprovedForListingFunction() { ListingId = BigInteger.Parse(listingID), Buyer = buyerAddress }
                );
                return result.ReturnValue1;
            }
        }

        public async Task<bool> IsCurrencyApprovedForListing(string listingID, string currencyContractAddress)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<bool>(getRoute("isCurrencyApprovedForListing"), Utils.ToJsonStringArray(listingID, currencyContractAddress));
            }
            else
            {
                var result = await TransactionManager.ThirdwebRead<DirectListingsContract.IsCurrencyApprovedForListingFunction, DirectListingsContract.IsCurrencyApprovedForListingOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.IsCurrencyApprovedForListingFunction() { ListingId = BigInteger.Parse(listingID), Currency = currencyContractAddress }
                );
                return result.ReturnValue1;
            }
        }

        // WRITE FUNCTIONS

        public async Task<TransactionResult> ApproveBuyerForReservedListing(string listingID, string buyerAddress)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("approveBuyerForReservedListing"), Utils.ToJsonStringArray(listingID, buyerAddress));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.ApproveBuyerForListingFunction() { ListingId = BigInteger.Parse(listingID), Buyer = buyerAddress }
                );
            }
        }

        public async Task<TransactionResult> BuyFromListing(string listingID, string quantity, string walletAddress)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("buyFromListing"), Utils.ToJsonStringArray(listingID, quantity, walletAddress));
            }
            else
            {
                var listing = await GetListing(listingID);

                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.BuyFromListingFunction()
                    {
                        ListingId = BigInteger.Parse(listingID),
                        Quantity = BigInteger.Parse(quantity),
                        BuyFor = walletAddress,
                        Currency = listing.currencyContractAddress,
                        ExpectedTotalPrice = BigInteger.Parse(listing.pricePerToken) * BigInteger.Parse(quantity),
                    },
                    listing.currencyContractAddress.ToLower() == Utils.NativeTokenAddress.ToLower() ? BigInteger.Parse(listing.pricePerToken) * BigInteger.Parse(quantity) : 0
                );
            }
        }

        public async Task<TransactionResult> CancelListing(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("cancelListing"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(_sdk, _contractAddress, new DirectListingsContract.CancelListingFunction() { ListingId = BigInteger.Parse(listingID), });
            }
        }

        public async Task<TransactionResult> CreateListing(CreateListingInput input)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("createListing"), Utils.ToJsonStringArray(input));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.CreateListingFunction()
                    {
                        Params = new DirectListingsContract.ListingParameters()
                        {
                            AssetContract = input.assetContractAddress,
                            TokenId = BigInteger.Parse(input.tokenId),
                            Quantity = BigInteger.Parse(input.quantity ?? "1"),
                            Currency = input.currencyContractAddress ?? Utils.NativeTokenAddress,
                            PricePerToken = BigInteger.Parse(input.pricePerToken.ToWei()),
                            StartTimestamp = input.startTimestamp ?? await _sdk.Blocks.GetLatestBlockTimestamp() + 60,
                            EndTimestamp = (BigInteger)(input.endTimestamp ?? Utils.GetUnixTimeStampNow() + 60 * 60 * 24 * 7),
                            Reserved = input.isReservedListing ?? false,
                        }
                    }
                );
            }
        }

        public async Task<TransactionResult> RevokeBuyerApprovalForReservedListing(string listingId, string buyerAddress)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("revokeBuyerApprovalForReservedListing"), Utils.ToJsonStringArray(listingId, buyerAddress));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.ApproveBuyerForListingFunction()
                    {
                        ListingId = BigInteger.Parse(listingId),
                        Buyer = buyerAddress,
                        ToApprove = false
                    }
                );
            }
        }

        public async Task<TransactionResult> RevokeCurrencyApprovalForListing(string listingId, string currencyContractAddress)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("revokeCurrencyApprovalForListing"), Utils.ToJsonStringArray(listingId, currencyContractAddress));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.ApproveCurrencyForListingFunction()
                    {
                        ListingId = BigInteger.Parse(listingId),
                        Currency = currencyContractAddress,
                        PricePerTokenInCurrency = 0
                    }
                );
            }
        }

        public async Task<TransactionResult> UpdateListing(string listingId, DirectListing listing)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("updateListing"), Utils.ToJsonStringArray(listingId, listing));
            }
            else
            {
                var oldListing = await GetListing(listingId);

                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new DirectListingsContract.UpdateListingFunction()
                    {
                        ListingId = BigInteger.Parse(listingId),
                        Params = new DirectListingsContract.ListingParameters()
                        {
                            AssetContract = listing.assetContractAddress ?? oldListing.assetContractAddress,
                            TokenId = BigInteger.Parse(listing.tokenId ?? oldListing.tokenId),
                            Quantity = BigInteger.Parse(listing.quantity ?? oldListing.quantity),
                            Currency = listing.currencyContractAddress ?? oldListing.currencyContractAddress,
                            PricePerToken = BigInteger.Parse(listing.pricePerToken.ToWei() ?? oldListing.pricePerToken),
                            StartTimestamp = (BigInteger)(listing.startTimeInSeconds ?? oldListing.startTimeInSeconds),
                            EndTimestamp = (BigInteger)(listing.endTimeInSeconds ?? oldListing.endTimeInSeconds),
                            Reserved = listing.isReservedListing ?? oldListing.isReservedListing ?? false,
                        }
                    }
                );
            }
        }
    }

    public class EnglishAuctions : Routable
    {
        private readonly string _contractAddress;
        private readonly ThirdwebSDK _sdk;

        public EnglishAuctions(ThirdwebSDK sdk, string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "englishAuctions"))
        {
            this._contractAddress = contractAddress;
            this._sdk = sdk;
        }

        // READ FUNCTIONS

        public async Task<List<Auction>> GetAll(MarketplaceFilters filters = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<Auction>>(getRoute("getAll"), filters == null ? new string[] { } : Utils.ToJsonStringArray(filters));
            }
            else
            {
                int totalSupply = int.Parse(await GetTotalCount());
                int start = filters?.start ?? 0;
                int count = filters?.count ?? 0;
                int end = count == 0 ? totalSupply - 1 : start + count;
                var result = await TransactionManager.ThirdwebRead<EnglishAuctionsContract.GetAllAuctionsFunction, EnglishAuctionsContract.GetAllAuctionsOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new EnglishAuctionsContract.GetAllAuctionsFunction() { StartId = start, EndId = end }
                );
                var allAuctions = result.AllAuctions;
                var filteredAuctions = new List<Auction>();
                foreach (var auction in allAuctions)
                {
                    var tempAuction = await GetAuction(auction.AuctionId.ToString());

                    if (filters?.tokenContract != null && filters?.tokenContract != tempAuction.assetContractAddress)
                        continue;
                    if (filters?.tokenId != null && filters?.tokenId != tempAuction.tokenId)
                        continue;
                    if (filters?.seller != null && filters?.seller != tempAuction.creatorAddress)
                        continue;

                    filteredAuctions.Add(tempAuction);
                }
                return filteredAuctions;
            }
        }

        public async Task<List<Auction>> GetAllValid(MarketplaceFilters filters = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<Auction>>(getRoute("getAllValid"), filters == null ? new string[] { } : Utils.ToJsonStringArray(filters));
            }
            else
            {
                int totalSupply = int.Parse(await GetTotalCount());
                int start = filters?.start ?? 0;
                int count = filters?.count ?? 0;
                int end = count == 0 ? totalSupply - 1 : start + count;
                var result = await TransactionManager.ThirdwebRead<EnglishAuctionsContract.GetAllValidAuctionsFunction, EnglishAuctionsContract.GetAllValidAuctionsOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new EnglishAuctionsContract.GetAllValidAuctionsFunction() { StartId = start, EndId = end }
                );
                var allValidAuctions = result.ValidAuctions;
                var filteredAuctions = new List<Auction>();
                foreach (var auction in allValidAuctions)
                {
                    var tempAuction = await GetAuction(auction.AuctionId.ToString());

                    if (filters?.tokenContract != null && filters?.tokenContract != tempAuction.assetContractAddress)
                        continue;
                    if (filters?.tokenId != null && filters?.tokenId != tempAuction.tokenId)
                        continue;
                    if (filters?.seller != null && filters?.seller != tempAuction.creatorAddress)
                        continue;

                    filteredAuctions.Add(tempAuction);
                }
                return filteredAuctions;
            }
        }

        public async Task<Auction> GetAuction(string auctionId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<Auction>(getRoute("getAuction"), Utils.ToJsonStringArray(auctionId));
            }
            else
            {
                var result = await TransactionManager.ThirdwebRead<EnglishAuctionsContract.GetAuctionFunction, EnglishAuctionsContract.GetAuctionOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new EnglishAuctionsContract.GetAuctionFunction() { AuctionId = BigInteger.Parse(auctionId) }
                );

                Currency currency = await _sdk.GetContract(result.Auction.Currency).ERC20.Get();
                var metadata = new NFTMetadata();
                try
                {
                    metadata = (await _sdk.GetContract(result.Auction.AssetContract).ERC721.Get(result.Auction.TokenId.ToString())).metadata;
                }
                catch (System.Exception)
                {
                    metadata = (await _sdk.GetContract(result.Auction.AssetContract).ERC1155.Get(result.Auction.TokenId.ToString())).metadata;
                }

                return new Auction()
                {
                    id = result.Auction.AuctionId.ToString(),
                    creatorAddress = result.Auction.AuctionCreator,
                    assetContractAddress = result.Auction.AssetContract,
                    tokenId = result.Auction.TokenId.ToString(),
                    quantity = result.Auction.Quantity.ToString(),
                    currencyContractAddress = result.Auction.Currency,
                    minimumBidAmount = result.Auction.MinimumBidAmount.ToString(),
                    minimumBidCurrencyValue = new CurrencyValue(
                        currency.name,
                        currency.symbol,
                        currency.decimals,
                        result.Auction.MinimumBidAmount.ToString(),
                        result.Auction.MinimumBidAmount.ToString().FormatERC20(4, int.Parse(currency.decimals), true)
                    ),
                    buyoutBidAmount = result.Auction.BuyoutBidAmount.ToString(),
                    buyoutCurrencyValue = new CurrencyValue(
                        currency.name,
                        currency.symbol,
                        currency.decimals,
                        result.Auction.BuyoutBidAmount.ToString(),
                        result.Auction.BuyoutBidAmount.ToString().FormatERC20(4, int.Parse(currency.decimals), true)
                    ),
                    timeBufferInSeconds = (int)result.Auction.TimeBufferInSeconds,
                    bidBufferBps = (int)result.Auction.BidBufferBps,
                    startTimeInSeconds = (long)result.Auction.StartTimestamp,
                    endTimeInSeconds = (long)result.Auction.EndTimestamp,
                    asset = metadata,
                    status = (MarkteplaceStatus)result.Auction.Status
                };
            }
        }

        public async Task<BigInteger> GetBidBufferBps(string auctionId)
        {
            if (Utils.IsWebGLBuild())
            {
                var val = await Bridge.InvokeRoute<string>(getRoute("getBidBufferBps"), Utils.ToJsonStringArray(auctionId));
                return BigInteger.Parse(val);
            }
            else
            {
                var auction = await GetAuction(auctionId);
                return auction.bidBufferBps.Value;
            }
        }

        public async Task<CurrencyValue> GetMinimumNextBid(string auctionId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<CurrencyValue>(getRoute("getMinimumNextBid"), Utils.ToJsonStringArray(auctionId));
            }
            else
            {
                var auction = await GetAuction(auctionId);
                var winningBid = await GetWinningBid(auctionId);
                var cv = auction.minimumBidCurrencyValue.Value;
                cv.value = (BigInteger.Parse(cv.value) + BigInteger.Parse(winningBid.bidAmount)).ToString();
                cv.displayValue = cv.value.FormatERC20(4, int.Parse(cv.decimals), true);
                return cv;
            }
        }

        public async Task<string> GetTotalCount()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("getTotalCount"), new string[] { });
            }
            else
            {
                var result = await TransactionManager.ThirdwebRead<EnglishAuctionsContract.TotalAuctionsFunction, EnglishAuctionsContract.TotalAuctionsOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new EnglishAuctionsContract.TotalAuctionsFunction() { }
                );
                return result.ReturnValue1.ToString();
            }
        }

        public async Task<string> GetWinner(string auctionId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("getWinner"), Utils.ToJsonStringArray(auctionId));
            }
            else
            {
                var result = await TransactionManager.ThirdwebRead<EnglishAuctionsContract.GetWinningBidFunction, EnglishAuctionsContract.GetWinningBidOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new EnglishAuctionsContract.GetWinningBidFunction() { AuctionId = BigInteger.Parse(auctionId) }
                );
                return result.Bidder;
            }
        }

        public async Task<Bid> GetWinningBid(string auctionId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<Bid>(getRoute("getWinningBid"), Utils.ToJsonStringArray(auctionId));
            }
            else
            {
                var winningBid = await TransactionManager.ThirdwebRead<EnglishAuctionsContract.GetWinningBidFunction, EnglishAuctionsContract.GetWinningBidOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new EnglishAuctionsContract.GetWinningBidFunction() { AuctionId = BigInteger.Parse(auctionId) }
                );

                var c = await _sdk.GetContract(winningBid.Currency).ERC20.Get();

                return new Bid()
                {
                    auctionId = auctionId,
                    bidderAddress = winningBid.Bidder,
                    currencyContractAddress = winningBid.Currency,
                    bidAmount = winningBid.BidAmount.ToString(),
                    bidAmountCurrencyValue = new CurrencyValue(
                        c.name,
                        c.symbol,
                        c.decimals,
                        winningBid.BidAmount.ToString(),
                        winningBid.BidAmount.ToString().FormatERC20(4, int.Parse(c.decimals), true)
                    )
                };
            }
        }

        public async Task<bool> IsWinningBid(string auctionId, string bidAmount)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<bool>(getRoute("isWinningBid"), Utils.ToJsonStringArray(auctionId, bidAmount));
            }
            else
            {
                var result = await TransactionManager.ThirdwebRead<EnglishAuctionsContract.IsNewWinningBidFunction, EnglishAuctionsContract.IsNewWinningBidOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new EnglishAuctionsContract.IsNewWinningBidFunction() { AuctionId = BigInteger.Parse(auctionId), BidAmount = BigInteger.Parse(bidAmount) }
                );
                return result.ReturnValue1;
            }
        }

        // WRITE FUNCTIONS

        public async Task<TransactionResult> BuyoutAuction(string auctionId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("buyoutAuction"), Utils.ToJsonStringArray(auctionId));
            }
            else
            {
                var auction = await GetAuction(auctionId);
                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new EnglishAuctionsContract.BidInAuctionFunction() { AuctionId = BigInteger.Parse(auctionId), BidAmount = BigInteger.Parse(auction.buyoutCurrencyValue?.value) },
                    BigInteger.Parse(auction.buyoutCurrencyValue?.value)
                );
            }
        }

        public async Task<TransactionResult> CancelAuction(string auctionId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("cancelAuction"), Utils.ToJsonStringArray(auctionId));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(_sdk, _contractAddress, new EnglishAuctionsContract.CancelAuctionFunction() { AuctionId = BigInteger.Parse(auctionId) });
            }
        }

        public async Task<TransactionResult> CloseAuctionForBidder(string auctionId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("closeAuctionForBidder"), Utils.ToJsonStringArray(auctionId));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(_sdk, _contractAddress, new EnglishAuctionsContract.CollectAuctionTokensFunction() { AuctionId = BigInteger.Parse(auctionId) });
            }
        }

        public async Task<TransactionResult> CloseAuctionForSeller(string auctionId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("closeAuctionForSeller"), Utils.ToJsonStringArray(auctionId));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(_sdk, _contractAddress, new EnglishAuctionsContract.CollectAuctionPayoutFunction() { AuctionId = BigInteger.Parse(auctionId) });
            }
        }

        public async Task<TransactionResult> CreateAuction(CreateAuctionInput input)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("createAuction"), Utils.ToJsonStringArray(input));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new EnglishAuctionsContract.CreateAuctionFunction()
                    {
                        Params = new EnglishAuctionsContract.AuctionParameters()
                        {
                            AssetContract = input.assetContractAddress,
                            TokenId = BigInteger.Parse(input.tokenId),
                            Quantity = BigInteger.Parse(input.quantity ?? "1"),
                            Currency = input.currencyContractAddress ?? Utils.NativeTokenAddress,
                            MinimumBidAmount = BigInteger.Parse(input.minimumBidAmount.ToWei()),
                            BuyoutBidAmount = BigInteger.Parse(input.buyoutBidAmount.ToWei()),
                            TimeBufferInSeconds = ulong.Parse(input.timeBufferInSeconds ?? "900"),
                            BidBufferBps = ulong.Parse(input.bidBufferBps ?? "500"),
                            StartTimestamp = (ulong)(input.startTimestamp ?? await _sdk.Blocks.GetLatestBlockTimestamp() + 60),
                            EndTimestamp = (ulong)(input.endTimestamp ?? Utils.GetUnixTimeStampNow() + 60 * 60 * 24 * 7),
                        }
                    }
                );
            }
        }

        public async Task<TransactionResult> ExecuteSale(string auctionId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("executeSale"), Utils.ToJsonStringArray(auctionId));
            }
            else
            {
                // TODO: Make it a multicall
                await CloseAuctionForSeller(auctionId);
                return await CloseAuctionForBidder(auctionId);
            }
        }

        public async Task<TransactionResult> MakeBid(string auctionId, string bidAmount)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("makeBid"), Utils.ToJsonStringArray(auctionId, bidAmount));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new EnglishAuctionsContract.BidInAuctionFunction() { AuctionId = BigInteger.Parse(auctionId), BidAmount = BigInteger.Parse(bidAmount.ToWei()) },
                    BigInteger.Parse(bidAmount.ToWei())
                );
            }
        }
    }

    public class Offers : Routable
    {
        private readonly string _contractAddress;
        private readonly ThirdwebSDK _sdk;

        public Offers(ThirdwebSDK sdk, string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "offers"))
        {
            this._contractAddress = contractAddress;
            this._sdk = sdk;
        }

        // READ FUNCTIONS

        public async Task<List<Offer>> GetAll(MarketplaceFilters filters = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<Offer>>(getRoute("getAll"), filters == null ? new string[] { } : Utils.ToJsonStringArray(filters));
            }
            else
            {
                int totalSupply = int.Parse(await GetTotalCount());
                int start = filters?.start ?? 0;
                int count = filters?.count ?? 0;
                int end = count == 0 ? totalSupply - 1 : start + count;
                var result = await TransactionManager.ThirdwebRead<OffersContract.GetAllOffersFunction, OffersContract.GetAllOffersOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new OffersContract.GetAllOffersFunction() { StartId = start, EndId = end }
                );
                var allOffers = result.AllOffers;
                var filteredOffers = new List<Offer>();
                foreach (var listing in allOffers)
                {
                    var tempOffer = await GetOffer(listing.OfferId.ToString());

                    if (filters?.tokenContract != null && filters?.tokenContract != tempOffer.assetContractAddress)
                        continue;
                    if (filters?.tokenId != null && filters?.tokenId != tempOffer.tokenId)
                        continue;
                    if (filters?.offeror != null && filters?.seller != tempOffer.offerorAddress)
                        continue;

                    filteredOffers.Add(tempOffer);
                }
                return filteredOffers;
            }
        }

        public async Task<List<Offer>> GetAllValid(MarketplaceFilters filters = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<Offer>>(getRoute("getAllValid"), filters == null ? new string[] { } : Utils.ToJsonStringArray(filters));
            }
            else
            {
                int totalSupply = int.Parse(await GetTotalCount());
                int start = filters?.start ?? 0;
                int count = filters?.count ?? 0;
                int end = count == 0 ? totalSupply - 1 : start + count;
                var result = await TransactionManager.ThirdwebRead<OffersContract.GetAllValidOffersFunction, OffersContract.GetAllValidOffersOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new OffersContract.GetAllValidOffersFunction() { StartId = start, EndId = end }
                );
                var allValidOffers = result.ValidOffers;
                var filteredOffers = new List<Offer>();
                foreach (var listing in allValidOffers)
                {
                    var tempOffer = await GetOffer(listing.OfferId.ToString());

                    if (filters?.tokenContract != null && filters?.tokenContract != tempOffer.assetContractAddress)
                        continue;
                    if (filters?.tokenId != null && filters?.tokenId != tempOffer.tokenId)
                        continue;
                    if (filters?.offeror != null && filters?.seller != tempOffer.offerorAddress)
                        continue;

                    filteredOffers.Add(tempOffer);
                }
                return filteredOffers;
            }
        }

        public async Task<Offer> GetOffer(string offerID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<Offer>(getRoute("getOffer"), Utils.ToJsonStringArray(offerID));
            }
            else
            {
                var result = await TransactionManager.ThirdwebRead<OffersContract.GetOfferFunction, OffersContract.GetOfferOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new OffersContract.GetOfferFunction() { OfferId = BigInteger.Parse(offerID) }
                );

                Currency currency = await _sdk.GetContract(result.Offer.Currency).ERC20.Get();
                var metadata = new NFTMetadata();
                try
                {
                    metadata = (await _sdk.GetContract(result.Offer.AssetContract).ERC721.Get(result.Offer.TokenId.ToString())).metadata;
                }
                catch (System.Exception)
                {
                    metadata = (await _sdk.GetContract(result.Offer.AssetContract).ERC1155.Get(result.Offer.TokenId.ToString())).metadata;
                }

                return new Offer()
                {
                    id = result.Offer.OfferId.ToString(),
                    offerorAddress = result.Offer.Offeror,
                    assetContractAddress = result.Offer.AssetContract,
                    tokenId = result.Offer.TokenId.ToString(),
                    quantity = result.Offer.Quantity.ToString(),
                    currencyContractAddress = result.Offer.Currency,
                    currencyValue = new CurrencyValue(
                        currency.name,
                        currency.symbol,
                        currency.decimals,
                        result.Offer.TotalPrice.ToString(),
                        result.Offer.TotalPrice.ToString().FormatERC20(4, int.Parse(currency.decimals), true)
                    ),
                    totalPrice = result.Offer.TotalPrice.ToString(),
                    asset = metadata,
                    endTimeInSeconds = (long)result.Offer.ExpirationTimestamp,
                    status = (MarkteplaceStatus)result.Offer.Status
                };
            }
        }

        public async Task<string> GetTotalCount()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("getTotalCount"), new string[] { });
            }
            else
            {
                var result = await TransactionManager.ThirdwebRead<OffersContract.TotalOffersFunction, OffersContract.TotalOffersOutputDTO>(
                    _sdk,
                    _contractAddress,
                    new OffersContract.TotalOffersFunction() { }
                );
                return result.ReturnValue1.ToString();
            }
        }

        // WRITE FUNCTIONS

        public async Task<TransactionResult> AcceptOffer(string offerID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("acceptOffer"), Utils.ToJsonStringArray(offerID));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(_sdk, _contractAddress, new OffersContract.AcceptOfferFunction() { OfferId = BigInteger.Parse(offerID) });
            }
        }

        public async Task<TransactionResult> CancelOffer(string offerID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("cancelOffer"), Utils.ToJsonStringArray(offerID));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(_sdk, _contractAddress, new OffersContract.CancelOfferFunction() { OfferId = BigInteger.Parse(offerID) });
            }
        }

        public async Task<TransactionResult> MakeOffer(MakeOfferInput input)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("makeOffer"), Utils.ToJsonStringArray(input));
            }
            else
            {
                return await TransactionManager.ThirdwebWrite(
                    _sdk,
                    _contractAddress,
                    new OffersContract.MakeOfferFunction()
                    {
                        Params = new OffersContract.OfferParams()
                        {
                            AssetContract = input.assetContractAddress,
                            TokenId = BigInteger.Parse(input.tokenId),
                            Quantity = BigInteger.Parse(input.quantity ?? "1"),
                            Currency = input.currencyContractAddress ?? Utils.GetNativeTokenWrapper(_sdk.Session.ChainId),
                            TotalPrice = BigInteger.Parse(input.totalPrice.ToWei()),
                            ExpirationTimestamp = (BigInteger)(input.endTimestamp ?? Utils.GetUnixTimeStampIn10Years())
                        }
                    }
                );
            }
        }
    }
}
