using System.Collections.Generic;
using UnityEngine;
using Thirdweb;

public class Prefab_Reading : MonoBehaviour
{
    public async void FetchERC20()
    {
        try
        {
            Contract contract = ThirdwebManager.Instance.SDK.GetContract("0xc70053b15F13625D40d5ae21d3624eABa702d15a");

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
            Contract contract = ThirdwebManager.Instance.SDK.GetContract("0x0Ae3359B31697f352118cf7CE1C7bea0E4e285F0");

            NFT getResult = await contract.ERC721.Get("1");
            Debugger.Instance.Log("[Fetch ERC721] Get", getResult.ToString());

            // List<NFT> getAllResult = await contract.ERC721.GetAll(new Thirdweb.QueryAllParams() { start = 0, count = 10 });
            // Debugger.Instance.Log("[Fetch ERC721] Get", getAllResult[0].ToString());

            // List<NFT> getOwnedResult = await contract.ERC721.GetOwned("someAddress");
            // Debugger.Instance.Log("[Fetch ERC721] Get", getOwnedResult[0].ToString());
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
            Contract contract = ThirdwebManager.Instance.SDK.GetContract("0xBA6d340e55F7cA896B6644a54f7D381f96bE98C0");

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
            Contract contract = ThirdwebManager.Instance.SDK.GetContract("0xC7DBaD01B18403c041132C5e8c7e9a6542C4291A");
            Marketplace marketplace = contract.marketplace;

            List<Listing> getAllListingsResult = await marketplace.GetAllListings();
            Debugger.Instance.Log("[Fetch Listings] Listing #1", getAllListingsResult[0].ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Fetch Listings] Error", e.Message);
        }
    }

    public async void FetchPackContents()
    {
        try
        {
            Contract contract = ThirdwebManager.Instance.SDK.GetContract("0xC7DBaD01B18403c041132C5e8c7e9a6542C4291A");
            Pack pack = contract.pack;

            PackContents packContents = await pack.GetPackContents("0");
            Debugger.Instance.Log("[Fetch Pack Contents] Pack #1", packContents.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Fetch Pack Contents] Error", e.Message);
        }
    }
}
