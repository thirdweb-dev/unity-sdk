using System;
using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;
using System.Collections.Generic;

namespace Thirdweb
{
    /// <summary>
    /// Interact with any MarketplaceV3 contract.
    /// </summary>
    public class Marketplace : Routable
    {
        public DirectListings directListings;
        public EnglishAuctions englishAuctions;
        public Offers offers;

        private string contractAddress;

        public Marketplace(string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "marketplace-v3"))
        {
            this.contractAddress = contractAddress;
            this.directListings = new DirectListings(baseRoute, contractAddress);
            this.englishAuctions = new EnglishAuctions(baseRoute, contractAddress);
            this.offers = new Offers(baseRoute, contractAddress);
        }
    }

    public class DirectListings : Routable
    {
        private string contractAddress;

        public DirectListings(string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "directListings"))
        {
            this.contractAddress = contractAddress;
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        // WRITE FUNCTIONS

        public async Task<TransactionResult> ApproveBuyerForReservedListing(string listingID, string walletAddress)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("approveBuyerForReservedListing"), Utils.ToJsonStringArray(listingID, walletAddress));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }
    }

    public class EnglishAuctions : Routable
    {
        private string contractAddress;

        public EnglishAuctions(string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "auctions"))
        {
            this.contractAddress = contractAddress;
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        public async Task<Auction> GetAuction(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<Auction>(getRoute("getAuction"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        public async Task<int> GetBidBufferBps(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<int>(getRoute("getBidBufferBps"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        public async Task<CurrencyValue> GetMinimumNextBid(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<CurrencyValue>(getRoute("getMinimumNextBid"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        public async Task<string> GetWinner(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<string>(getRoute("getWinner"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        public async Task<Bid> GetWinningBid(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<Bid>(getRoute("getWinningBid"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        public async Task<bool> IsWinningBid(string listingID, string bidAmount)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<bool>(getRoute("isWinningBid"), Utils.ToJsonStringArray(listingID, bidAmount));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        // WRITE FUNCTIONS

        public async Task<TransactionResult> BuyoutAuction(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("buyoutAuction"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        public async Task<TransactionResult> CancelAuction(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("cancelAuction"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        public async Task<TransactionResult> CloseAuctionForBidder(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("closeAuctionForBidder"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        public async Task<TransactionResult> CloseAuctionForSeller(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("closeAuctionForSeller"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        public async Task<TransactionResult> ExecuteSale(string listingID)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("executeSale"), Utils.ToJsonStringArray(listingID));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }

        public async Task<TransactionResult> MakeBid(string listingID, string bidAmount)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("makeBid"), Utils.ToJsonStringArray(listingID, bidAmount));
            }
            else
            {
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }
    }

    public class Offers : Routable
    {
        private string contractAddress;

        public Offers(string parentRoute, string contractAddress)
            : base(Routable.append(parentRoute, "offers"))
        {
            this.contractAddress = contractAddress;
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
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
                throw new UnityException("This functionality is not yet available on your platform.");
            }
        }
    }
}
