using System.Threading.Tasks;

namespace Thirdweb
{

    [System.Serializable]
    public struct NFTMetadata
    {
        public string id;
        public string uri;
        public string description;
        public string image;
        public string name;
        // TODO: support properties;
    }

    [System.Serializable]
    public struct NFT
    {
        public NFTMetadata metadata;
        public string owner;
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

    public class ERC721
    {
        public string chain;
        public string address;
        public ERC721(string chain, string address)
        {
            this.chain = chain;
            this.address = address;
        }
        public async Task<NFT> GetNFT(string tokenId)
        {
            return await Bridge.InvokeRoute<NFT>(getRoute("get"), new string[] { tokenId });
        }
        public async Task<TransactionResult> Transfer(string to, string tokenId)
        {
            return await Bridge.InvokeRoute<TransactionResult>(getRoute("transfer"), new string[] { to, tokenId });
        }

        public async Task<TransactionResult[]> Claim(int quantity)
        {
            return await Bridge.InvokeRoute<TransactionResult[]>(getRoute("claim"), new string[] { quantity.ToString() });
        }

        private string getRoute(string functionPath) {
            return this.address + ".erc721." + functionPath;
        }
    }
}