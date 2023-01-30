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
        public Dictionary<string, object> properties;

        public override string ToString()
        {
            string propertiesStr = "";
            foreach (var property in properties)
                propertiesStr = $"\n>>key: {property.Key.ToString()} // value: {property.Value.ToString()}";
            return
            $"NFTMetadata:"
            + $"\n>id: {id}"
            + $"\n>uri: {uri}"
            + $"\n>description: {description}"
            + $"\n>image: {image}"
            + $"\n>name: {name}"
            + $"\n>external_url: {external_url}"
            + $"\n>properties: {propertiesStr}";
        }
    }

    [System.Serializable]
    public struct NFTMetadataWithSupply
    {
        public NFTMetadata metadata;
        public int supply;

        public override string ToString()
        {
            return
            $"NFTMetadataWithSupply:"
            + $"\n>>>>>\n{metadata.ToString()}\n<<<<<"
            + $"\n>supply: {supply}";
        }
    }

    [System.Serializable]
    public struct NFT
    {
        public NFTMetadata metadata;
        public string owner;
        public string type;
        public int supply;
        public int quantityOwned; // only for ERC1155.GetOwned()

        public override string ToString()
        {
            return
            $"NFT:"
            + $"\n>>>>>\n{metadata.ToString()}\n<<<<<"
            + $"\n>owner: {owner}"
            + $"\n>type: {type}"
            + $"\n>supply: {supply}"
            + $"\n>quantityOwned: {quantityOwned}";
        }
    }

    // Tokens

    [System.Serializable]
    public struct Currency
    {
        public string name;
        public string symbol;
        public string decimals;

        public override string ToString()
        {
            return
            $"Currency:"
            + $"\n>name: {name}"
            + $"\n>symbol: {symbol}"
            + $"\n>decimals: {decimals}";
        }
    }

    [System.Serializable]
    public struct CurrencyValue
    {
        public string name;
        public string symbol;
        public string decimals;
        public string value;
        public string displayValue;

        public override string ToString()
        {
            return
            $"CurrencyValue:"
            + $"\n>name: {name}"
            + $"\n>symbol: {symbol}"
            + $"\n>decimals: {decimals}"
            + $"\n>value: {value}"
            + $"\n>displayValue: {displayValue}";
        }
    }

    // Marketplace

    [System.Serializable]
    public class Listing
    {
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

        public override string ToString()
        {
            return
            $"Listing:"
            + $"\n>id: {id}"
            + $"\n>sellerAddress: {sellerAddress}"
            + $"\n>assetContractAddress: {assetContractAddress}"
            + $"\n>tokenId: {tokenId}"
            + $"\n>>>>>\n{asset.ToString()}\n<<<<<"
            + $"\n>quantity: {quantity}"
            + $"\n>currencyContractAddress: {currencyContractAddress}"
            + $"\n>buyoutPrice: {buyoutPrice}"
            + $"\n>>>>>\n{buyoutCurrencyValuePerToken.ToString()}\n<<<<<"
            + $"\n>type: {type}";
        }
    }

    [System.Serializable]
    public class DirectListing : Listing
    {
        public string startTimeInSeconds;
        public string secondsUntilEnd;

        public override string ToString()
        {
            return
            $"DirectListing:"
            + $"\n>>>>>\n{base.ToString()}\n<<<<<"
            + $"\n>startTimeInSeconds: {startTimeInSeconds}"
            + $"\n>secondsUntilEnd: {secondsUntilEnd}";
        }
    }

    [System.Serializable]
    public class AuctionListing : Listing
    {
        public string startTimeInEpochSeconds;
        public string endTimeInEpochSeconds;
        public string reservePrice;
        public CurrencyValue reservePriceCurrencyValuePerToken;

        public override string ToString()
        {
            return
            $"AuctionListing:"
            + $"\n>>>>>\n{base.ToString()}\n<<<<<"
            + $"\n>startTimeInEpochSeconds: {startTimeInEpochSeconds}"
            + $"\n>endTimeInEpochSeconds: {endTimeInEpochSeconds}"
            + $"\n>reservePrice: {reservePrice}"
            + $"\n>>>>>\n{reservePriceCurrencyValuePerToken.ToString()}\n<<<<<";
        }
    }

    [System.Serializable]
    public abstract class NewListing
    {
        public string type;
        public string assetContractAddress;
        public string tokenId;
        public long startTimestamp;
        public int listingDurationInSeconds;
        public int quantity;
        public string currencyContractAddress;
        public string reservePricePerToken;
        public string buyoutPricePerToken;

        public override string ToString()
        {
            return
            $"NewListing:"
            + $"\n>type: {type}"
            + $"\n>assetContractAddress: {assetContractAddress}"
            + $"\n>tokenId: {tokenId}"
            + $"\n>startTimestamp: {startTimestamp}"
            + $"\n>listingDurationInSeconds: {listingDurationInSeconds}"
            + $"\n>quantity: {quantity}"
            + $"\n>currencyContractAddress: {currencyContractAddress}"
            + $"\n>reservePricePerToken: {reservePricePerToken}"
            + $"\n>buyoutPricePerToken: {buyoutPricePerToken}";
        }
    }

    [System.Serializable]
    public class NewAuctionListing : NewListing
    {
        public new string reservePricePerToken;

        public NewAuctionListing()
        {
            this.type = "NewAuctionListing";
        }

        public override string ToString()
        {
            return
            $"NewAuctionListing:"
            + $"\n>>>>>\n{base.ToString()}\n<<<<<"
            + $"\n>reservePricePerToken: {reservePricePerToken}";
        }
    }

    [System.Serializable]
    public class NewDirectListing : NewListing
    {
        public NewDirectListing()
        {
            this.type = "NewDirectListing";
        }

        public override string ToString()
        {
            return
            $"NewDirectListing:"
            + $"\n>>>>>\n{base.ToString()}\n<<<<<";
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

        public override string ToString()
        {
            return
            $"Offer:"
            + $"\n>listingId: {listingId}"
            + $"\n>buyerAddress: {buyerAddress}"
            + $"\n>quantityDesired: {quantityDesired}"
            + $"\n>pricePerToken: {pricePerToken}"
            + $"\n>>>>>\n{currencyValue.ToString()}\n<<<<<"
            + $"\n>currencyContractAddress: {currencyContractAddress}";
        }
    }

    [System.Serializable]
    public class QueryAllParams
    {
        public int start;
        public int count;

        public override string ToString()
        {
            return
            $"QueryAllParams:"
            + $"\n>start: {start}"
            + $"\n>count: {count}";
        }
    }

    [System.Serializable]
    public class MarketplaceFilter : QueryAllParams
    {
        public string seller;
        public string tokenContract;
        public string tokenId;

        public override string ToString()
        {
            return
            $"MarketplaceFilter:"
            + $"\n>>>>>\n{base.ToString()}\n<<<<<"
            + $"\n>seller: {seller}"
            + $"\n>tokenContract: {tokenContract}"
            + $"\n>tokenId: {tokenId}";
        }
    }

    // Claim conditions

    [System.Serializable]
    public class ClaimConditions
    {
        public string availableSupply;
        public string currentMintSupply;
        public CurrencyValue currencyMetadata;
        public string currencyAddress;
        public string maxClaimableSupply;
        public string maxClaimablePerWallet;
        public string waitInSeconds;

        public override string ToString()
        {
            return
            $"ClaimConditions:"
            + $"\n>availableSupply: {availableSupply}"
            + $"\n>currentMintSupply: {currentMintSupply}"
            + $"\n>>>>>\n{currencyMetadata.ToString()}\n<<<<<"
            + $"\n>currencyAddress: {currencyAddress}"
            + $"\n>maxClaimableSupply: {maxClaimableSupply}"
            + $"\n>maxClaimablePerWallet: {maxClaimablePerWallet}"
            + $"\n>waitInSeconds: {waitInSeconds}";
        }
    }

    [System.Serializable]
    public class SnapshotEntry
    {
        public string address;
        public string maxClaimable;
        public string price;
        public string currencyAddress;

        public override string ToString()
        {
            return
            $"SnapshotEntry:"
            + $"\n>address: {address}"
            + $"\n>maxClaimable: {maxClaimable}"
            + $"\n>price: {price}"
            + $"\n>currencyAddress: {currencyAddress}";
        }
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

        public override string ToString()
        {
            return
            $"Receipt:"
            + $"\n>from: {from}"
            + $"\n>to: {to}"
            + $"\n>transactionIndex: {transactionIndex}"
            + $"\n>gasUsed: {gasUsed}"
            + $"\n>blockHash: {blockHash}"
            + $"\n>transactionHash: {transactionHash}";
        }
    }

    [System.Serializable]
    public class TransactionResult
    {
        public Receipt receipt;
        public string id;

        public bool isSuccessful()
        {
            return receipt.transactionHash != null;
        }

        public override string ToString()
        {
            return
            $"TransactionResult:"
            + $"\n{receipt.ToString()}"
            + $"\n>id: {id}";
        }
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

        public override string ToString()
        {
            return
            $"TransactionRequest:"
            + $"\n>from: {from}"
            + $"\n>to: {to}"
            + $"\n>data: {data}"
            + $"\n>value: {value}"
            + $"\n>gasLimit: {gasLimit}"
            + $"\n>gasPrice: {gasPrice}";
        }
    }

    [System.Serializable]
    public struct LoginPayload
    {
        public LoginPayloadData payload;
        public string signature;

        public override string ToString()
        {
            return
            $"LoginPayloadData:"
            + $"\n>>>>>\n{payload.ToString()}\n<<<<<"
            + $"\n>signature: {signature}";
        }
    }

    [System.Serializable]
    public struct LoginPayloadData
    {
        public string domain;
        public string address;
        public string nonce;
        public string expiration_time;
        public string chain_id;

        public override string ToString()
        {
            return
            $"LoginPayloadData:"
            + $"\n>domain: {domain}"
            + $"\n>address: {address}"
            + $"\n>nonce: {nonce}"
            + $"\n>expiration_time: {expiration_time}"
            + $"\n>chain_id: {chain_id}";
        }
    }

    [System.Serializable]
    public struct FundWalletOptions
    {
        public string appId;
        public string address;
        public int chainId;
        public List<string> assets;
    }
}