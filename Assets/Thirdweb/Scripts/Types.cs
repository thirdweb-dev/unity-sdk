using System.Collections.Generic;

namespace Thirdweb {
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
}