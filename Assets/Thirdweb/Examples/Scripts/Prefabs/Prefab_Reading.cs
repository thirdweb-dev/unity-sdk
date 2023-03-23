using System.Collections.Generic;
using UnityEngine;
using Thirdweb;

public class Prefab_Reading : MonoBehaviour
{
    private const string TOKEN_ERC20_CONTRACT = "0x76Ec89310842DBD9d0AcA3B2E27858E85cdE595A";
    private const string DROP_ERC20_CONTRACT = "0x450b943729Ddba196Ab58b589Cea545551DF71CC";
    private const string TOKEN_ERC721_CONTRACT = "0x45c498Dfc0b4126605DD91eB1850fd6b5BCe9efC";
    private const string DROP_ERC721_CONTRACT = "0x8ED1C3618d70785d23E5fdE767058FA6cA6D9E43";
    private const string TOKEN_ERC1155_CONTRACT = "0x82c488a1BC64ab3b91B927380cca9Db7bF347879";
    private const string DROP_ERC1155_CONTRACT = "0x408308c85D7073192deEAcC1703E234A783fFfF1";
    private const string MARKETPLACE_CONTRACT = "0xC7DBaD01B18403c041132C5e8c7e9a6542C4291A";
    private const string PACK_CONTRACT = "0xC04104DE55dEC5d63542f7ADCf8171278942048E";

    public async void FetchERC20()
    {
        try
        {
            Contract contract = ThirdwebManager.Instance.SDK.GetContract(TOKEN_ERC20_CONTRACT);

            Currency currencyInfo = await contract.ERC20.Get();
            Debugger.Instance.Log("[Fetch ERC20] Get", currencyInfo.ToString());

            // CurrencyValue myBalance = await contract.ERC20.Balance();

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
            Contract contract = ThirdwebManager.Instance.SDK.GetContract(TOKEN_ERC721_CONTRACT);

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
            Contract contract = ThirdwebManager.Instance.SDK.GetContract(TOKEN_ERC1155_CONTRACT);

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
            Contract contract = ThirdwebManager.Instance.SDK.GetContract(MARKETPLACE_CONTRACT);
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
            Contract contract = ThirdwebManager.Instance.SDK.GetContract(PACK_CONTRACT);

            PackContents packContents = await contract.pack.GetPackContents("0");
            Debugger.Instance.Log("[Fetch Pack Contents] Pack #0", packContents.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Fetch Pack Contents] Error", e.Message);
        }
    }
}
