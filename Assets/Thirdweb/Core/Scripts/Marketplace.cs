using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb
{
    /// <summary>
    /// Interact with a Marketplace contract.
    /// </summary>
    public class Marketplace : Routable
    {
        public string chain;
        public string address;

        /// <summary>
        /// Handle direct listings
        /// </summary>
        public MarketplaceDirect direct;

        /// <summary>
        /// Handle auctions
        /// </summary>
        public MarketplaceAuction auction;

        private string contractAddress;

        /// <summary>
        /// Interact with a Marketplace contract.
        /// </summary>
        public Marketplace(string chain, string contractAddress)
            : base($"{contractAddress}{subSeparator}marketplace")
        {
            this.chain = chain;
            this.contractAddress = contractAddress;
            this.direct = new MarketplaceDirect(baseRoute);
            this.auction = new MarketplaceAuction(baseRoute);
        }

        /// READ FUNCTIONS

        /// <summary>
        /// Get a listing information
        /// </summary>
        public async Task<Listing> GetListing(string listingId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<Listing>(getRoute("getListing"), Utils.ToJsonStringArray(listingId));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
                // Listing listing = new Listing();
                // var result = await marketplaceService.ListingsQueryAsync(BigInteger.Parse(listingId));
                // listing.id = result.ListingId.ToString();
                // listing.sellerAddress = result.TokenOwner;
                // listing.assetContractAddress = result.AssetContract;
                // listing.tokenId = result.TokenId.ToString();
                // listing.quantity = (int)result.Quantity;
                // listing.currencyContractAddress = result.Currency;
                // listing.buyoutPrice = result.BuyoutPricePerToken.ToString();
                // listing.type = result.TokenType;

                // Contract nftContract = ThirdwebManager.Instance.SDK.GetContract(result.AssetContract);
                // NFT tempNft = await nftContract.ERC721.Get(result.TokenId.ToString());
                // listing.asset = tempNft.metadata;

                // Contract tokenContract = ThirdwebManager.Instance.SDK.GetContract(result.Currency);
                // Currency tempCurrency = await tokenContract.ERC20.Get();
                // listing.buyoutCurrencyValuePerToken = new CurrencyValue(
                //     tempCurrency.name,
                //     tempCurrency.symbol,
                //     tempCurrency.decimals,
                //     result.BuyoutPricePerToken.ToString(),
                //     result.BuyoutPricePerToken.ToString().ToEth()
                // );

                // return listing;
            }
        }

        /// <summary>
        /// Get all listings in this marketplace (including non-buyable ones)
        /// </summary>
        public async Task<List<Listing>> GetAllListings(MarketplaceFilter filter = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<Listing>>(getRoute("getAllListings"), Utils.ToJsonStringArray(filter));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");

                // // TODO: Multicall
                // List<Listing> allListings = new List<Listing>();
                // int totalListingsCount = (int)await marketplaceService.TotalListingsQueryAsync();
                // for (int i = 0; i < totalListingsCount; i++)
                //     allListings.Add(await GetListing(i.ToString()));

                // if (filter != null)
                // {
                //     List<Listing> filteredListings = new List<Listing>();
                //     int startId = filter.start;
                //     int count = filter.count == 0 ? allListings.Count : filter.count;
                //     for (int i = startId; i < count; i++)
                //     {
                //         if (!string.IsNullOrEmpty(filter.seller))
                //         {
                //             if (allListings[i].sellerAddress != filter.seller || allListings[i].assetContractAddress != filter.tokenContract || allListings[i].tokenId != filter.tokenId)
                //             {
                //                 continue;
                //             }
                //         }
                //         else
                //         {
                //             filteredListings.Add(allListings[i]);
                //         }
                //     }
                //     return filteredListings;
                // }

                // return allListings;
            }
        }

        /// <summary>
        /// Get active listings in this marketplace (only ones that can be bought)
        /// </summary>
        public async Task<List<Listing>> GetActiveListings(MarketplaceFilter filter = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<Listing>>(getRoute("getActiveListings"), Utils.ToJsonStringArray(filter));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// <summary>
        /// Get all offers on a listing
        /// </summary>
        public async Task<List<Offer>> GetOffers(string listingId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<List<Offer>>(getRoute("getOffers"), Utils.ToJsonStringArray(listingId));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        /// WRITE FUNCTIONS

        /// <summary>
        /// Buy a listing
        /// </summary>
        public async Task<TransactionResult> BuyListing(string listingId, int quantityDesired, string receiverAddress = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("buyoutListing"), Utils.ToJsonStringArray(listingId, quantityDesired, receiverAddress));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");

                // var listing = await GetListing(listingId);
                // string buyFor = receiverAddress == null ? await ThirdwebManager.Instance.SDK.wallet.GetAddress() : receiverAddress;
                // var receipt = await marketplaceService.BuyRequestAndWaitForReceiptAsync(
                //     BigInteger.Parse(listingId),
                //     buyFor,
                //     quantityDesired,
                //     listing.currencyContractAddress,
                //     BigInteger.Parse(listing.buyoutPrice) * quantityDesired
                // );
                // return receipt.ToTransactionResult();
            }
        }

        /// <summary>
        /// Make an offer on a listing
        /// </summary>
        public async Task<TransactionResult> MakeOffer(string listingId, string pricePerToken, int? quantity = null)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("makeOffer"), Utils.ToJsonStringArray(listingId, pricePerToken, quantity));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }
    }

    // DIRECT

    public class MarketplaceDirect : Routable
    {
        public MarketplaceDirect(string parentRoute)
            : base(Routable.append(parentRoute, "direct")) { }

        public async Task<DirectListing> GetListing(string listingId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<DirectListing>(getRoute("getListing"), Utils.ToJsonStringArray(listingId));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<Offer> GetActiveOffer(string listingId, string address)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<Offer>(getRoute("getActiveOffer"), Utils.ToJsonStringArray(listingId));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<TransactionResult> CreateListing(NewDirectListing listing)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("createListing"), Utils.ToJsonStringArray(listing));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<TransactionResult> AcceptOffer(NewDirectListing listing, string addressOfOfferor)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("acceptOffer"), Utils.ToJsonStringArray(listing, addressOfOfferor));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<TransactionResult> CancelListing(string listingId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("cancelListing"), Utils.ToJsonStringArray(listingId));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }
    }

    // AUCTION

    public class MarketplaceAuction : Routable
    {
        public MarketplaceAuction(string parentRoute)
            : base(Routable.append(parentRoute, "auction")) { }

        public async Task<AuctionListing> GetListing(string listingId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<AuctionListing>(getRoute("getListing"), Utils.ToJsonStringArray(listingId));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<Offer> GetWinningBid(string listingId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<Offer>(getRoute("getWinningBid"), Utils.ToJsonStringArray(listingId));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<CurrencyValue> GetMinimumNextBid(string listingId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<CurrencyValue>(getRoute("getMinimumNextBid"), Utils.ToJsonStringArray(listingId));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<string> GetWinner(string listingId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("getWinner"), Utils.ToJsonStringArray(listingId));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<TransactionResult> CreateListing(NewAuctionListing listing)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("createListing"), Utils.ToJsonStringArray(listing));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<TransactionResult> CancelListing(string listingId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("cancelListing"), Utils.ToJsonStringArray(listingId));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }

        public async Task<TransactionResult> ExecuteSale(string listingId)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("executeSale"), Utils.ToJsonStringArray(listingId));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your current platform.");
            }
        }
    }
}
