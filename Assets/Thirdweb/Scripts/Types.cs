using System.Collections.Generic;

namespace Thirdweb
{

    // NFTs

    [System.Serializable]
    public struct NFTMetadata
    {
        public string id;
        public string uri;
        public string description;
        public string image;
        public string name;
        public string external_url;
        public Dictionary<string, string> properties;
    }

    [System.Serializable]
    public struct NFTMetadataWithSupply
    {
        public NFTMetadata metadata;
        public int supply;
    }

    [System.Serializable]
    public struct NFT
    {
        public NFTMetadata metadata;
        public string owner;
        public string type;
        public int supply;
        public int quantityOwned; // only for ERC1155.GetOwned()
    }

    // Tokens

    [System.Serializable]
    public struct Currency
    {
        public string name;
        public string symbol;
        public string decimals;
    }

    [System.Serializable]
    public struct CurrencyValue
    {
        public string name;
        public string symbol;
        public string decimals;
        public string value;
        public string displayValue;
    }

    // Marketplace

    [System.Serializable]
    public class Listing {
        public string id;
        public string sellerAddress;
        public string assetContractAddress;
        public string tokenId;
        public NFTMetadata asset;
        public int quantity;
        public string currencyContractAddress;
        public string buyoutPrice;
        public CurrencyValue buyoutCurrencyValuePerToken;
        public int type;
    }

    [System.Serializable]
    public class DirectListing : Listing
    {
        public string startTimeInSeconds;
        public string secondsUntilEnd;
    }

    [System.Serializable]
    public class AuctionListing : Listing
    {
        public string startTimeInEpochSeconds;
        public string endTimeInEpochSeconds;
        public string reservePrice;
        public CurrencyValue reservePriceCurrencyValuePerToken;
    }

    [System.Serializable]
    public abstract class NewListing {
        public string type;
        public string assetContractAddress;
        public string tokenId;
        public long startTimestamp;
        public int listingDurationInSeconds;
        public int quantity;
        public string currencyContractAddress;
        public string reservePricePerToken;
        public string buyoutPricePerToken;
    }

    [System.Serializable]
    public class NewAuctionListing : NewListing {
        public new string reservePricePerToken;

        public NewAuctionListing() {
            this.type = "NewAuctionListing";
        }
    }

    [System.Serializable]
    public class NewDirectListing : NewListing {
        public NewDirectListing() {
            this.type = "NewDirectListing";
        }
    }

    [System.Serializable]
    public struct Offer 
    {
        public string listingId;
        public string buyerAddress;
        public int quantityDesired;
        public string pricePerToken;
        public CurrencyValue currencyValue;
        public string currencyContractAddress;
    }

    [System.Serializable]
    public class QueryAllParams
    {
        public int start;
        public int count;
    }

    [System.Serializable]
    public class MarketplaceFilter: QueryAllParams
    {
        public string seller;
        public string tokenContract;
        public string tokenId;
    }

    // Transactions

    [System.Serializable]
    public struct Receipt
    {
        public string from;
        public string to;
        public int transactionIndex;
        public string gasUsed;
        public string blockHash;
        public string transactionHash;
    }

    [System.Serializable]
    public struct TransactionResult
    {
        public Receipt receipt;
        public string id;
    }

    [System.Serializable]
    public struct TransactionRequest
    {
        public string from;
        public string to;
        public string data;
        public string value;
        public string gasLimit;
        public string gasPrice;
    }
}