using UnityEngine;

namespace Thirdweb.Examples
{
    public class Prefab_Miscellaneous : MonoBehaviour
    {
        public async void GetBalance()
        {
            try
            {
                CurrencyValue balance = await ThirdwebManager.Instance.SDK.Wallet.GetBalance();
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
                string abi = await Contract.FetchAbi("0xEBB8a39D865465F289fa349A67B3391d8f910da9", await ThirdwebManager.Instance.SDK.Wallet.GetChainId());
                Contract contract = ThirdwebManager.Instance.SDK.GetContract("0xEBB8a39D865465F289fa349A67B3391d8f910da9", abi);

                // Contract.Read
                string uri = await contract.Read<string>("name");
                Debugger.Instance.Log("[Custom Call] Read Successful", uri);

                // Contract.Write
                // TransactionResult transactionResult = await contract.Write("approve", "0xEBB8a39D865465F289fa349A67B3391d8f910da9", 1);
                // Debugger.Instance.Log("[Custom Call] Write Successful", transactionResult.ToString());
                // // With Transaction Overrides:
                // await contract.Write(
                //     "approve",
                //     new TransactionRequest
                //     {
                //         value = "0.05".ToWei() // 0.05 ETH
                //     },
                //     "0xEBB8a39D865465F289fa349A67B3391d8f910da9",
                //     1
                // );

                // Contract.Prepare
                // string connectedAddress = await ThirdwebManager.Instance.SDK.wallet.GetAddress();
                // Transaction transaction = await contract.Prepare(functionName: "approve", from: connectedAddress, "0xEBB8a39D865465F289fa349A67B3391d8f910da9", 1);
                // // transaction.SetValue("0.00000000001");
                // // transaction.SetGasLimit("100000");

                // try
                // {
                //     var data = await transaction.Simulate();
                //     Debugger.Instance.Log("[Custom Call] Simulate Successful", "Data: " + data.ToString() + " \nSending Transaction...");
                // }
                // catch (System.Exception e)
                // {
                //     Debugger.Instance.Log("[Custom Call] Simulate Error", e.Message);
                //     return;
                // }

                // await transaction.EstimateAndSetGasLimitAsync();

                // var gasPrice = await transaction.GetGasPrice();
                // Debug.Log($"Gas Price: {gasPrice}");

                // var gasCosts = await transaction.EstimateGasCosts();
                // Debug.Log($"Gas Cost: {gasCosts.wei} WEI");

                // Debugger.Instance.Log("[Custom Call] Transaction Preview", transaction.ToString());

                // try
                // {
                //     string transactionResult = await transaction.Send(gasless: false);
                //     Debugger.Instance.Log("[Custom Call] Send Successful", "Tx Hash: " + transactionResult);
                // }
                // catch (System.Exception e)
                // {
                //     Debugger.Instance.Log("[Custom Call] Send Error", e.ToString());
                // }
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
                // Generate and sign
                LoginPayload data = await ThirdwebManager.Instance.SDK.Wallet.Authenticate("example.com");
                // Verify
                string resultAddressOrError = await ThirdwebManager.Instance.SDK.Wallet.Verify(data);
                if (await ThirdwebManager.Instance.SDK.Wallet.GetAddress() == resultAddressOrError)
                {
                    Debugger.Instance.Log("[Authenticate] Successful", resultAddressOrError);
                }
                else
                {
                    Debugger.Instance.Log("[Authenticate] Invalid", resultAddressOrError);
                }
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Authenticate] Error", e.Message);
            }
        }

        public async void Sign()
        {
            try
            {
                string message = "Hello World";
                string sig = await ThirdwebManager.Instance.SDK.Wallet.Sign(message);
                Debugger.Instance.Log("[Sign] Successful", $"Signature: {sig}");
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Sign] Error", e.ToString());
            }
        }
    }
}
