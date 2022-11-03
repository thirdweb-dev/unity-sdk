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

    public class ERC721
    {
        public string chain;
        public string address;
        public ERC721(string chain, string address)
        {
            this.chain = chain;
            this.address = address;
        }
        public async Task<NFT> GetNFT(string id)
        {
            return await Bridge.InvokeRoute<NFT>(getRoute("get"), new string[] { id });
        }

        private string getRoute(string functionPath) {
            return this.address + ".erc721." + functionPath;
        }

        // public Currency GetCurrency(string address)
        // {
        //     if (!currencyModules.ContainsKey(address))
        //     {
        //         currencyModules[address] = new Currency(this, this.bridge, address);
        //     }

        //     return currencyModules[address];
        // }

        // public NFT GetNFT(string address)
        // {
        //     if (!nftModules.ContainsKey(address))
        //     {
        //         nftModules[address] = new NFT(this, this.bridge, address);
        //     }

        //     return nftModules[address];
        // }

        // public Market GetMarket(string address)
        // {
        //     if (!marketModules.ContainsKey(address))
        //     {
        //         marketModules[address] = new Market(this, this.bridge, address);
        //     }

        //     return marketModules[address];
        // }
    }
}