using System.Collections.Generic;
using System.Threading.Tasks;

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

        /// <summary>
        /// Interact with a Marketplace contract.
        /// </summary>
        public Marketplace(string chain, string address) : base($"{address}{subSeparator}marketplace")
        {
            this.chain = chain;
            this.address = address;
            this.direct = new MarketplaceDirect(baseRoute);
            this.auction = new MarketplaceAuction(baseRoute);
        }

        /// READ FUNCTIONS

        /// <summary>
        /// Get a listing information
        /// </summary>
        public async Task<Listing> GetListing(string listingId)
        {
            return await Bridge.InvokeRoute<Listing>(getRoute("getListing"), Utils.ToJsonStringArray(listingId));
        }

        /// <summary>
        /// Get all listings in this marketplace (including non-buyable ones)
        /// </summary>
        public async Task<List<Listing>> GetAllListings(MarketplaceFilter filter = null)
        {
            return await Bridge.InvokeRoute<List<Listing>>(getRoute("getAllListings"), Utils.ToJsonStringArray(filter));
        }

        /// <summary>
        /// Get active listings in this marketplace (only ones that can be bought)
        /// </summary>
        public async Task<List<Listing>> GetActiveListings(MarketplaceFilter filter = null)
        {
            return await Bridge.InvokeRoute<List<Listing>>(getRoute("getActiveListings"), Utils.ToJsonStringArray(filter));
        }

        /// <summary>
        /// Get all offers on a listing
        /// </summary>
        public async Task<List<Offer>> GetOffers(string listingId)
        {
            return await Bridge.InvokeRoute<List<Offer>>(getRoute("getOffers"), Utils.ToJsonStringArray(listingId));
        }

        /// WRITE FUNCTIONS

        /// <summary>
        /// Buy a listing
        /// </summary>
        public async Task<TransactionResult> BuyListing(string listingId, int quantityDesired, string receiverAddress = null)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("buyoutListing"), Utils.ToJsonStringArray(listingId, quantityDesired, receiverAddress));
        }

        /// <summary>
        /// Make an offer on a listing
        /// </summary>
        public async Task<TransactionResult> MakeOffer(string listingId, string pricePerToken, int? quantity = null)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("makeOffer"), Utils.ToJsonStringArray(listingId, pricePerToken, quantity));
        }
    }

    // DIRECT

    public class MarketplaceDirect : Routable
    {
        public MarketplaceDirect(string parentRoute) : base(Routable.append(parentRoute, "direct"))
        {
        }

        public async Task<DirectListing> GetListing(string listingId)
        {
            return await Bridge.InvokeRoute<DirectListing>(getRoute("getListing"), Utils.ToJsonStringArray(listingId));
        }

        public async Task<Offer> GetActiveOffer(string listingId, string address)
        {
            return await Bridge.InvokeRoute<Offer>(getRoute("getActiveOffer"), Utils.ToJsonStringArray(listingId));
        }

        public async Task<TransactionResult> CreateListing(NewDirectListing listing)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("createListing"), Utils.ToJsonStringArray(listing));
        }

        public async Task<TransactionResult> AcceptOffer(NewDirectListing listing, string addressOfOfferor)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("acceptOffer"), Utils.ToJsonStringArray(listing, addressOfOfferor));
        }

        public async Task<TransactionResult> CancelListing(string listingId)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("cancelListing"), Utils.ToJsonStringArray(listingId));
        }
    }

    // AUCTION

    public class MarketplaceAuction : Routable
    {
        public MarketplaceAuction(string parentRoute) : base(Routable.append(parentRoute, "auction"))
        {
        }

        public async Task<AuctionListing> GetListing(string listingId)
        {
            return await Bridge.InvokeRoute<AuctionListing>(getRoute("getListing"), Utils.ToJsonStringArray(listingId));
        }

        public async Task<Offer> GetWinningBid(string listingId)
        {
            return await Bridge.InvokeRoute<Offer>(getRoute("getWinningBid"), Utils.ToJsonStringArray(listingId));
        }

        public async Task<CurrencyValue> GetMinimumNextBid(string listingId)
        {
            return await Bridge.InvokeRoute<CurrencyValue>(getRoute("getMinimumNextBid"), Utils.ToJsonStringArray(listingId));
        }

        public async Task<string> GetWinner(string listingId)
        {
            return await Bridge.InvokeRoute<string>(getRoute("getWinner"), Utils.ToJsonStringArray(listingId));
        }

        public async Task<TransactionResult> CreateListing(NewAuctionListing listing)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("createListing"), Utils.ToJsonStringArray(listing));
        }

        public async Task<TransactionResult> CancelListing(string listingId)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("cancelListing"), Utils.ToJsonStringArray(listingId));
        }

        public async Task<TransactionResult> ExecuteSale(string listingId)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("executeSale"), Utils.ToJsonStringArray(listingId));
        }
    }
}