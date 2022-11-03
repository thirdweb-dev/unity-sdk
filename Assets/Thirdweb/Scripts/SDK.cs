using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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

    public class SDK
    {
        public static async Task<NFT> GetNFT(string id)
        {
            var result = await Bridge.InvokeRoute<NFT>("erc721.get", new string[] { id });
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