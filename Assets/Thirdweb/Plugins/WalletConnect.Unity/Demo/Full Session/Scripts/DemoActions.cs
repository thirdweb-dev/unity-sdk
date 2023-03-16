// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using WalletConnectSharp.Core;
// using WalletConnectSharp.Core.Models;
// using WalletConnectSharp.Core.Models.Ethereum;
// using WalletConnectSharp.Unity;
// using WalletConnectUnity.Demo.Scripts;

// public class DemoActions : WalletConnectActions
// {
//     public Text resultText;
//     public Text accountText;

//     private int count;

//     // Update is called once per frame
//     void FixedUpdate()
//     {
//         if (WalletConnect.ActiveSession.Accounts == null)
//             return;

//         accountText.text = "\nConnected to Chain " + WalletConnect.ActiveSession.ChainId + ":\n" + WalletConnect.ActiveSession.Accounts[0];
//     }

//     public async void OnClickSwitchChain()
//     {
//         try
//         {
//             var chainId = new EthChain();
//             chainId.chainId = "0x13881";
//             var results = await WalletSwitchEthChain(chainId);
//         }
//         catch
//         {
//             //If the client rejected or doesn't have that chain, try to add it.
//             addEthChain();
//         }
//     }

//     public async void addEthChain()
//     {
//         List<string> list = new List<string>();
//         list.Add("https://rpc-mumbai.maticvigil.com/");
//         var chainData = new EthChainData()
//         {
//             chainId = "0x13881",
//             chainName = "Mumbai Testnet",
//             rpcUrls = list.ToArray(),
//             nativeCurrency = new NativeCurrency()
//             {
//                 name = "MATIC",
//                 symbol = "MATIC",
//                 decimals = 18
//             }
//         };
//         var results = await WalletAddEthChain(chainData);
//     }

//     public async void OnClickPersonalSign()
//     {
//         var results = await PersonalSign("This is a test!");

//         resultText.text = results;
//         resultText.gameObject.SetActive(true);
//     }

//     public async void OnClickSendTransaction()
//     {
//         var address = WalletConnect.ActiveSession.Accounts[0];
//         var transaction = new TransactionData()
//         {
//             data = "0x",
//             from = address,
//             to = address,
//             gas = "21000",
//             value = "0",
//             chainId = 2,
//         };

//         var results = await SendTransaction(transaction);

//         resultText.text = results;
//         resultText.gameObject.SetActive(true);
//     }

//     public async void OnClickSignTransaction()
//     {
//         var address = WalletConnect.ActiveSession.Accounts[0];
//         var transaction = new TransactionData()
//         {
//             data = "0x",
//             from = address,
//             to = address,
//             gas = "21000",
//             value = "0",
//             chainId = 2,
//             nonce = "0",
//             gasPrice = "50000000000"
//         };

//         var results = await SignTransaction(transaction);

//         resultText.text = results;
//         resultText.gameObject.SetActive(true);
//     }

//     public async void OnClickSignTypedData()
//     {
//         var address = WalletConnect.ActiveSession.Accounts[0];

//         var results = await SignTypedData(DemoSignTypedData.ExampleData, DemoSignTypedData.Eip712Domain);

//         resultText.text = results;
//         resultText.gameObject.SetActive(true);
//     }

//     public async void OnClickDisconnectAndConnect()
//     {
//         bool shouldConnect = !WalletConnect.Instance.createNewSessionOnSessionDisconnect;
//         CloseSession(shouldConnect);
//     }
// }
