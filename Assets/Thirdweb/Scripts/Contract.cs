namespace Thirdweb
{

    public class Contract
    {
        public string chain;
        public string address;
        public ERC721 ERC721;
        public Contract(string chain, string address) {
            this.chain = chain;
            this.address = address;
            this.ERC721 = new ERC721(chain, address);
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