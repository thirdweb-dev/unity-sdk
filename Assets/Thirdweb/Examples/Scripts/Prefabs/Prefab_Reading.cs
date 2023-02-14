using System.Collections.Generic;
using UnityEngine;
using Thirdweb;


public class Prefab_Reading : MonoBehaviour
{
    public async void FetchERC20()
    {
        try
        {
            Contract contract = new Contract("goerli", "0xB4870B21f80223696b68798a755478C86ce349bE");

            Currency currencyInfo = await contract.ERC20.Get();
            Debugger.Instance.Log("[Fetch ERC20] Get", currencyInfo.ToString());

            // CurrencyValue currencyValue = await contract.ERC20.TotalSupply();
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Fetch ERC20] Error", e.Message);
        }
    }

    public async void FetchERC721()
    {
        try
        {
            Contract contract = new Contract("goerli", "0x2e01763fA0e15e07294D74B63cE4b526B321E389");

            NFT getResult = await contract.ERC721.Get("1");
            Debugger.Instance.Log("[Fetch ERC721] Get", getResult.ToString());

            // List<NFT> getAllResult = await contract.ERC721.GetAll(new Thirdweb.QueryAllParams() { start = 0, count = 10 });
            // List<NFT> getOwnedResult = await contract.ERC721.GetOwned("someAddress");
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Fetch ERC721] Error", e.Message);
        }
    }

    public async void FetchERC1155()
    {
        try
        {
            Contract contract = new Contract("goerli", "0x86B7df0dc0A790789D8fDE4C604EF8187FF8AD2A");

            NFT getResult = await contract.ERC1155.Get("1");
            Debugger.Instance.Log("[Fetch ERC1155] Get", getResult.ToString());

            // List<NFT> getAllResult = await contract.ERC1155.GetAll(new Thirdweb.QueryAllParams() { start = 0, count = 10 });
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Fetch ERC1155] Error", e.Message);
        }
    }

    public async void FetchListings()
    {
        try
        {
            Contract contract = new Contract("goerli", "0xC7DBaD01B18403c041132C5e8c7e9a6542C4291A");
            Marketplace marketplace = contract.marketplace;

            List<Listing> getAllListingsResult = await marketplace.GetAllListings();
            Debugger.Instance.Log("[Fetch Listings] Listing #1", getAllListingsResult[0].ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Fetch Listings] Error", e.Message);
        }
    }
}
