using Nethereum.ABI.EIP712;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        // public async void Authenticate()
        // {
        //     try
        //     {
        //         // Authenticate with backend
        //         string result = await ThirdwebManager.Instance.SDK.Wallet.Authenticate(domain: "http://localhost:8000", chainId: 421614);
        //         Debug.Log($"Result: {result}");
        //         string authToken = JsonConvert.DeserializeObject<JToken>(result)["token"].ToString();
        //         Debugger.Instance.Log("[Authenticate] Successful", $"Token: {authToken}");
        //     }
        //     catch (System.Exception e)
        //     {
        //         Debugger.Instance.Log("[Authenticate] Error", "This functionality is only available if you have a backend server set up with thirdweb auth! " + e.Message);
        //     }
        // }

        public async void SignTypedData()
        {
            try
            {
                var myAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
                var myTokenERC721 = "0x345E7B4CCA26725197f1Bed802A05691D8EF7770";

                // Values
                var mintRequest = new Thirdweb.Contracts.TokenERC721.ContractDefinition.MintRequest
                {
                    To = myAddress,
                    RoyaltyRecipient = myAddress,
                    RoyaltyBps = 0,
                    PrimarySaleRecipient = myAddress,
                    Uri = "https://example.com",
                    Price = 0,
                    Currency = myTokenERC721,
                    ValidityStartTimestamp = 0,
                    ValidityEndTimestamp = Utils.GetUnixTimeStampIn10Years(),
                    Uid = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                };

                // Types
                var typedData = new TypedData<Domain>
                {
                    Domain = new Domain
                    {
                        Name = "TokenERC721",
                        Version = "1",
                        ChainId = 421614,
                        VerifyingContract = myTokenERC721,
                    },
                    Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(Thirdweb.Contracts.TokenERC721.ContractDefinition.MintRequest)),
                    PrimaryType = nameof(Thirdweb.Contracts.TokenERC721.ContractDefinition.MintRequest),
                };

                // Sign
                var sig = await ThirdwebManager.Instance.SDK.Wallet.SignTypedDataV4(mintRequest, typedData);

                Debugger.Instance.Log("[Sign] Successful", $"Signature: {sig}");
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Sign] Error", e.ToString());
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
