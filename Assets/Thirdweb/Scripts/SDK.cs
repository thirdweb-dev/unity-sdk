using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Thirdweb
{
    public class SDK
    {
        public static async Task<string> Initialize()
        {
            var result = await Bridge.InvokeRouteRaw("initialize", new string[] { });
            return result;
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