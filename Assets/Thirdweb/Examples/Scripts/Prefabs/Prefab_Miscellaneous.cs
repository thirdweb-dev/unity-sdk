using UnityEngine;
using Thirdweb;


public class Prefab_Miscellaneous : MonoBehaviour
{
    public async void GetBalance()
    {
        try
        {
            CurrencyValue balance = await ThirdwebManager.Instance.SDK.wallet.GetBalance();
            Debugger.Instance.Log("[Get Balance] Native Balance", balance.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Get Balance] Error", e.Message);
        }
    }

    public async void CustomCall()
    {
        try
        {
            Contract contract = new Contract("goerli", "0x62Cf5485B6C24b707E47C5E0FB2EAe7EbE18EC4c");

            string uri = await contract.Read<string>("uri", 0);
            Debug.Log($"[Custom Call] Read Custom URI:\n{uri}");

            TransactionResult transactionResult = await contract.Write("claimKitten");
            Debugger.Instance.Log("[Custom Call] Write Successful", transactionResult.ToString());

            // With Transaction Overrides:
            // await contract.Write("claim", new TransactionRequest
            // {
            //     value = "0.05".ToWei() // 0.05 ETH
            // }, "0xE79ee09bD47F4F5381dbbACaCff2040f2FbC5803", 0, 1);
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Custom Call] Error", e.Message);
        }
    }

    public async void Authenticate()
    {
        try
        {
            LoginPayload data = await ThirdwebManager.Instance.SDK.wallet.Authenticate("example.com");
            Debugger.Instance.Log("[Authenticate] Successful", data.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Authenticate] Error", e.Message);
        }
    }

    public async void Deploy()
    {
        try
        {
            string address = await ThirdwebManager.Instance.SDK.deployer.DeployNFTCollection(
                new NFTContractDeployMetadata
                {
                    name = "Unity Collection",
                    primary_sale_recipient = await ThirdwebManager.Instance.SDK.wallet.GetAddress(),
                }
            );
            Debugger.Instance.Log("[Deploy] Successful", $"Address: {address}");
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Deploy] Error", e.Message);
        }
    }


}