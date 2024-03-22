using System.Collections.Generic;
using UnityEngine;

namespace Thirdweb.Examples
{
    public class Prefab_Reading : MonoBehaviour
    {
        private const string TOKEN_ERC20_CONTRACT = "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e";
        private const string DROP_ERC20_CONTRACT = "0xEBB8a39D865465F289fa349A67B3391d8f910da9";
        private const string TOKEN_ERC721_CONTRACT = "0x345E7B4CCA26725197f1Bed802A05691D8EF7770";
        private const string DROP_ERC721_CONTRACT = "0xD811CB13169C175b64bf8897e2Fd6a69C6343f5C";
        private const string TOKEN_ERC1155_CONTRACT = "0x83b5851134DAA0E28d855E7fBbdB6B412b46d26B";
        private const string DROP_ERC1155_CONTRACT = "0x6A7a26c9a595E6893C255C9dF0b593e77518e0c3";
        private const string MARKETPLACE_CONTRACT = "0xc9671F631E8313D53ec0b5358e1a499c574fCe6A";
        private const string PACK_CONTRACT = "0xE33653ce510Ee767d8824b5EcDeD27125D49889D";

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

        public async void FetchPackContents()
        {
            try
            {
                Contract contract = ThirdwebManager.Instance.SDK.GetContract(PACK_CONTRACT);

                PackContents packContents = await contract.Pack.GetPackContents("0");
                Debugger.Instance.Log("[Fetch Pack Contents] Pack #0", packContents.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Fetch Pack Contents] Error", e.Message);
            }
        }
    }
}
