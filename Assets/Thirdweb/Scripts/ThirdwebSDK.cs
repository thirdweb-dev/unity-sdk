namespace Thirdweb
{

    public class ThirdwebSDK
    {
        private string chainOrRPC;
        public ThirdwebSDK(string chainOrRPC) {
            this.chainOrRPC = chainOrRPC;
            Bridge.Initialize(chainOrRPC);
        }

        public void Connect() {
            Bridge.Connect();
        }

        public Contract GetContract(string address)
        {
            return new Contract(this.chainOrRPC, address);
        }
        // public static async Task<NFT> GetNFT(string id)
        // {
        //     return await Bridge.InvokeRoute<NFT>("erc721.get", new string[] { id });
        // }

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