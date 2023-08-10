using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thirdweb.Examples
{
    public class Prefab_Marketplace : MonoBehaviour
    {
        private const string TOKEN_ERC20_CONTRACT = "0x76Ec89310842DBD9d0AcA3B2E27858E85cdE595A";
        private const string DROP_ERC20_CONTRACT = "0x450b943729Ddba196Ab58b589Cea545551DF71CC";
        private const string TOKEN_ERC721_CONTRACT = "0x45c498Dfc0b4126605DD91eB1850fd6b5BCe9efC";
        private const string DROP_ERC721_CONTRACT = "0x8ED1C3618d70785d23E5fdE767058FA6cA6D9E43";
        private const string TOKEN_ERC1155_CONTRACT = "0x82c488a1BC64ab3b91B927380cca9Db7bF347879";
        private const string DROP_ERC1155_CONTRACT = "0x408308c85D7073192deEAcC1703E234A783fFfF1";
        private const string MARKETPLACE_CONTRACT = "0x3Dd51b530e9DBdD93087C321cbD9350f435f742C";
        private const string PACK_CONTRACT = "0xC04104DE55dEC5d63542f7ADCf8171278942048E";

        // Fetching

        public async void Fetch_DirectListing()
        {
            try
            {
                Debugger.Instance.Log("Request Sent", "Pending confirmation...");
                var contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
                var result = await contract.marketplace.directListings.GetListing("0");
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
                var result = await contract.marketplace.englishAuctions.GetAuction("0");
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
                var result = await contract.marketplace.offers.GetOffer("0");
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
                var result = await contract.marketplace.directListings.CreateListing(
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
                var result = await contract.marketplace.englishAuctions.CreateAuction(
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
                var result = await contract.marketplace.offers.MakeOffer(
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
                var result = await contract.marketplace.directListings.BuyFromListing("2", "1", await ThirdwebManager.Instance.SDK.wallet.GetAddress());
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
                var result = await contract.marketplace.englishAuctions.BuyoutAuction("0");
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
                var result = await contract.marketplace.offers.AcceptOffer("0");
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
                var result = await contract.marketplace.directListings.CancelListing("2");
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
                var result = await contract.marketplace.englishAuctions.CancelAuction("0");
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
                var result = await contract.marketplace.offers.CancelOffer("0");
                Debugger.Instance.Log("[Cancel_Offer] Success", result.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Cancel_Offer] Error", e.Message);
            }
        }
    }
}
