using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirdweb.Examples
{
    public class Prefab_Marketplace : MonoBehaviour
    {
        private const string TOKEN_ERC20_CONTRACT = "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e";
        private const string DROP_ERC20_CONTRACT = "0xEBB8a39D865465F289fa349A67B3391d8f910da9";
        private const string TOKEN_ERC721_CONTRACT = "0x345E7B4CCA26725197f1Bed802A05691D8EF7770";
        private const string DROP_ERC721_CONTRACT = "0xD811CB13169C175b64bf8897e2Fd6a69C6343f5C";
        private const string TOKEN_ERC1155_CONTRACT = "0x83b5851134DAA0E28d855E7fBbdB6B412b46d26B";
        private const string DROP_ERC1155_CONTRACT = "0x6A7a26c9a595E6893C255C9dF0b593e77518e0c3";
        private const string MARKETPLACE_CONTRACT = "0xc9671F631E8313D53ec0b5358e1a499c574fCe6A";
        private const string PACK_CONTRACT = "0xE33653ce510Ee767d8824b5EcDeD27125D49889D";

        // Fetching

        public async void Fetch_DirectListing()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                var contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.DirectListings.GetListing("1");
                Debugger.Instance.Log("[Fetch_DirectListing] Sucess", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Fetch_DirectListing] Error", e.Message);
            }
        }

        public async void Fetch_Auction()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                var contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.EnglishAuctions.GetAuction("0");
                Debugger.Instance.Log("[Fetch_Auction] Sucess", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Fetch_Auction] Error", e.Message);
            }
        }

        public async void Fetch_Offer()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                var contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.Offers.GetOffer("0");
                Debugger.Instance.Log("[Fetch_Offer] Sucess", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Fetch_Offer] Error", e.Message);
            }
        }

        // Creating

        public async void Create_Listing()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                Contract contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.DirectListings.CreateListing(
                    new CreateListingInput()
                    {
                        assetContractAddress = TOKEN_ERC1155_CONTRACT,
                        tokenId = "4",
                        pricePerToken = "0.000000000000000001", // 1 wei
                        quantity = "100"
                    }
                );
                Debugger.Instance.Log("[Create_Listing] Success", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Create_Listing] Error", e.Message);
            }
        }

        public async void Create_Auction()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                Contract contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.EnglishAuctions.CreateAuction(
                    new CreateAuctionInput()
                    {
                        assetContractAddress = TOKEN_ERC1155_CONTRACT,
                        tokenId = "4",
                        buyoutBidAmount = "0.0000000000000001",
                        minimumBidAmount = "0.000000000000000001"
                    }
                );
                Debugger.Instance.Log("[Create_Auction] Success", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Create_Auction] Error", e.Message);
            }
        }

        public async void Make_Offer()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                Contract contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.Offers.MakeOffer(
                    new MakeOfferInput()
                    {
                        assetContractAddress = TOKEN_ERC1155_CONTRACT,
                        tokenId = "4",
                        totalPrice = "0.000000000000000001",
                    }
                );
                Debugger.Instance.Log("[Make_Offer] Success", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Make_Offer] Error", e.Message);
            }
        }

        // Closing

        public async void Buy_Listing()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                Contract contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.DirectListings.BuyFromListing("2", "1", await ThirdwebManager.Instance.SDK.Wallet.GetAddress());
                Debugger.Instance.Log("[Buy_Listing] Success", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Buy_Listing] Error", e.Message);
            }
        }

        public async void Buyout_Auction()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                Contract contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.EnglishAuctions.BuyoutAuction("0");
                Debugger.Instance.Log("[Buyout_Auction] Success", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Buyout_Auction] Error", e.Message);
            }
        }

        public async void Accept_Offer()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                Contract contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.Offers.AcceptOffer("0");
                Debugger.Instance.Log("[Accept_Offer] Success", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Accept_Offer] Error", e.Message);
            }
        }

        // Cancelling

        public async void Cancel_Listing()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                Contract contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.DirectListings.CancelListing("2");
                Debugger.Instance.Log("[Cancel_Listing] Success", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Cancel_Listing] Error", e.Message);
            }
        }

        public async void Cancel_Auction()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                Contract contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.EnglishAuctions.CancelAuction("0");
                Debugger.Instance.Log("[Cancel_Auction] Success", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Cancel_Auction] Error", e.Message);
            }
        }

        public async void Cancel_Offer()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                Contract contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.Marketplace.Offers.CancelOffer("0");
                Debugger.Instance.Log("[Cancel_Offer] Success", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Cancel_Offer] Error", e.Message);
            }
        }
    }
}
