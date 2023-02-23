using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Thirdweb;
using UnityEngine;

using Nethereum.Unity.Rpc;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.Model;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Util;

using ERC20Query = Nethereum.Unity.Contracts.Standards.ERC20;
using ERC721Query = Nethereum.Unity.Contracts.Standards.ERC721;
using ERC1155Query = Nethereum.Unity.Contracts.Standards.ERC1155;

using ERC20Contract = Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using ERC721Contract = Nethereum.Contracts.Standards.ERC721.ContractDefinition;
using ERC1155Contract = Nethereum.Contracts.Standards.ERC1155.ContractDefinition;
using Nethereum.Web3;

public class NethereumTest : MonoBehaviour
{
    string rpc = "https://polygon-mumbai.g.alchemy.com/v2/8xhjCEWFVQ1gJZAW_6KgpjMgdnkqrBNl";
    string account = "0xDaaBDaaC8073A7dAbdC96F6909E8476ab4001B34";
    string testERC20ContractAddress = "0x326C977E6efc84E512bB9C30f76E30c160eD06FB";
    string testERC721ContractAddress = "0xc10d3bac7885fb9e474788b4a69da4dd4c0aae2c";
    string testERC1155ContractAddress = "0x2953399124f0cbb46d2cbacd8a89cf0599974963";

    Web3 web3;

    private void Awake()
    {
        web3 = new Web3(rpc);
    }

    async void Start()
    {
        Thirdweb.Contract c = ThirdwebManager.Instance.SDK.GetContract(testERC20ContractAddress);
        CurrencyValue balanceOf = await c.ERC20.BalanceOf(account);
        CurrencyValue allowanceOf = await c.ERC20.AllowanceOf(account, account);
        CurrencyValue totalSupply = await c.ERC20.TotalSupply();

        Debug.Log($"My Balance: {balanceOf.displayValue} | My Allowance: {allowanceOf.displayValue} | Total Supply: {totalSupply.displayValue}");
        Debug.Log(balanceOf.ToString());

        // StartCoroutine(TestInOrder());
    }

    IEnumerator TestInOrder()
    {
        // BlockNumber();
        yield return StartCoroutine(GetBlockNumber());

        // NativeBalance();
        yield return StartCoroutine(GetNativeBalance());

        // ERC20Balance();
        yield return StartCoroutine(GetERC20Balance());

        // ERC721Balance();
        yield return StartCoroutine(GetERC721Balance());

        // ERC1155Balance();
        yield return StartCoroutine(GetERC1155Balance());

        // Event();
        yield return StartCoroutine(GetEvent());
    }

    // Get Block

    IEnumerator GetBlockNumber()
    {
        var req = new EthBlockNumberUnityRequest(rpc);
        yield return req.SendRequest();
        BigInteger blockNumber = req.Result.Value;
        Debug.Log($"Block: {blockNumber}");
    }

    // async void BlockNumber()
    // {
    //     // ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;
    //     var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
    //     Debug.Log($"Block: {blockNumber}");
    // }

    // Get Native Balance

    IEnumerator GetNativeBalance()
    {

        var req = new EthGetBalanceUnityRequest(rpc);
        yield return req.SendRequest(account, BlockParameter.CreateLatest());
        BigInteger balance = req.Result.Value;
        Debug.Log($"Native Balance: {balance}");
    }

    // async void NativeBalance()
    // {
    //     BigInteger balance = await web3.Eth.GetBalance.SendRequestAsync(account);
    //     Debug.Log($"Native Balance: {balance}");
    // }

    // Get ERC20 Balance

    IEnumerator GetERC20Balance()
    {
        var req = new ERC20Query.BalanceOfQueryRequest(rpc, testERC20ContractAddress);
        yield return req.Query(account);
        BigInteger balance = req.Result.Balance;
        Debug.Log($"ERC20 Balance: {balance}");
    }

    // async void ERC20Balance()
    // {
    //     BigInteger balance = await web3.Eth.ERC20.GetContractService(testERC20ContractAddress).BalanceOfQueryAsync(account);
    //     Debug.Log($"ERC20 Balance: {balance}");
    // }

    // Get ERC721 Balance

    IEnumerator GetERC721Balance()
    {
        var req = new ERC721Query.BalanceOfQueryRequest(rpc, testERC721ContractAddress);
        yield return req.Query(account);
        BigInteger balance = req.Result.ReturnValue1;
        Debug.Log($"ER721 Balance: {balance}");
    }

    // async void ERC721Balance()
    // {
    //     BigInteger balance = await web3.Eth.ERC721.GetContractService(testERC721ContractAddress).BalanceOfQueryAsync(account);
    //     Debug.Log($"ER721 Balance: {balance}");
    // }

    // Get ERC1155 Balance

    IEnumerator GetERC1155Balance()
    {
        var req = new ERC1155Query.BalanceOfQueryRequest(rpc, testERC1155ContractAddress);
        yield return req.Query(account, BigInteger.Parse("98907841070571154799872550564665626750057551199604130120461408711760764993537"));
        BigInteger balance = req.Result.ReturnValue1;
        Debug.Log($"ERC1155 Balance: {balance}");
    }

    // async void ERC1155Balance()
    // {
    //     BigInteger balance = await web3.Eth.ERC1155.GetContractService(testERC1155ContractAddress).BalanceOfQueryAsync(account, BigInteger.Parse("98907841070571154799872550564665626750057551199604130120461408711760764993537"));
    //     Debug.Log($"ERC1155 Balance: {balance}");
    // }

    // Get Events

    IEnumerator GetEvent()
    {
        var req = new EthGetLogsUnityRequest(rpc);
        var eventTransfer = EventExtensions.GetEventABI<ERC20Contract.TransferEventDTO>();
        yield return req.SendRequest(eventTransfer.CreateFilterInput(testERC20ContractAddress, account));
        List<EventLog<ERC20Contract.TransferEventDTO>> events = req.Result.DecodeAllEvents<ERC20Contract.TransferEventDTO>();
        Debug.Log($"Event Decoded #1 | Event: {events[0].Event.ToString()} | Block Number: {events[0].Log.BlockNumber}");
    }

    // async void Event()
    // {
    //     var eventHandler = web3.Eth.GetEvent<ERC20Contract.TransferEventDTO>(testERC20ContractAddress);
    //     var eventFilter = eventHandler.CreateFilterInput(account);
    //     var events = await eventHandler.GetAllChangesAsync(eventFilter);

    //     Debug.Log($"Event Decoded #1 | Event: {events[0].Event.ToString()} | Block Number: {events[0].Log.BlockNumber}");
    // }

    // Transfer Ether

    // IEnumerator TransferEther()
    // {
    //     var privateKey = "";
    //     var receivingAddress = account;

    //     // Send TX
    //     var transferReq = new EthTransferUnityRequest(rpc, privateKey, 80001);
    //     yield return transferReq.TransferEther(receivingAddress, 1.1m, 2);
    //     string transactionHash = transferReq.Result;
    //     Debug.Log("[Transfer Ether] Transaction Hash: " + transactionHash);

    //     // Wait for TX
    //     var txReceiptPollingReq = new TransactionReceiptPollingRequest(rpc);
    //     yield return txReceiptPollingReq.PollForReceipt(transactionHash, 2);
    //     var transferReceipt = txReceiptPollingReq.Result;
    //     Debug.Log("[Transfer Ether] Transaction Mined | Transfer Receipt: " + transferReceipt);

    //     // Check Balance
    //     var balanceReq = new EthGetBalanceUnityRequest(rpc);
    //     yield return balanceReq.SendRequest(receivingAddress, BlockParameter.CreateLatest());
    //     Debug.Log("[Transfer Ether] Balance Of Receiving Address: " + UnitConversion.Convert.FromWei(balanceReq.Result.Value));
    // }

    // // Transfer ERC20

    // IEnumerator TransferERC20()
    // {
    //     var privateKey = "";
    //     var receivingAddress = account;
    //     var amount = 1000;

    //     // Send TX
    //     var transferReq = new TransactionSignedUnityRequest(rpc, privateKey, 80001);
    //     var transactionMessage = new ERC20Contract.TransferFunction
    //     {
    //         To = receivingAddress,
    //         Value = amount,
    //     };
    //     yield return transferReq.SignAndSendTransaction(transactionMessage, testERC20ContractAddress);
    //     var transactionHash = transferReq.Result;
    //     Debug.Log("[Transfer ERC20] Transaction Hash: " + transactionHash);

    //     // Wait for TX
    //     var txReceiptPollingReq = new TransactionReceiptPollingRequest(rpc);
    //     yield return txReceiptPollingReq.PollForReceipt(transactionHash, 2);
    //     var transferReceipt = txReceiptPollingReq.Result;
    //     Debug.Log("[Transfer ERC20] Transaction Mined | Transfer Receipt: " + transferReceipt);

    //     // Check Event
    //     var transferEvent = transferReceipt.DecodeAllEvents<ERC20Contract.TransferEventDTO>();
    //     Debug.Log("[Transfer ERC20] Transfer Event: " + transferEvent[0].Event.ToString());
    // }

    // // Transfer ERC721

    // IEnumerator TransferERC721()
    // {
    //     var privateKey = "";
    //     var receivingAddress = account;
    //     var tokenId = 1;

    //     // Send TX
    //     var transferReq = new TransactionSignedUnityRequest(rpc, privateKey, 80001);
    //     var transactionMessage = new ERC721Contract.TransferFromFunction
    //     {
    //         From = account,
    //         To = receivingAddress,
    //         TokenId = tokenId,
    //     };
    //     yield return transferReq.SignAndSendTransaction(transactionMessage, testERC721ContractAddress);
    //     var transactionHash = transferReq.Result;
    //     Debug.Log("[Transfer ERC721] Transaction Hash: " + transactionHash);

    //     // Wait for TX
    //     var txReceiptPollingReq = new TransactionReceiptPollingRequest(rpc);
    //     yield return txReceiptPollingReq.PollForReceipt(transactionHash, 2);
    //     var transferReceipt = txReceiptPollingReq.Result;
    //     Debug.Log("[Transfer ERC721] Transaction Mined | Transfer Receipt: " + transferReceipt);

    //     // Check Event
    //     var transferEvent = transferReceipt.DecodeAllEvents<ERC721Contract.TransferEventDTO>();
    //     Debug.Log("[Transfer ERC721] Transfer Event: " + transferEvent[0].Event.ToString());
    // }

    // // Transfer ERC1155

    // IEnumerator TransferERC1155()
    // {
    //     var privateKey = "";
    //     var receivingAddress = account;
    //     var tokenId = 1;
    //     var amount = 1;
    //     var data = new byte[0];

    //     // Send TX
    //     var transferReq = new TransactionSignedUnityRequest(rpc, privateKey, 80001);
    //     var transactionMessage = new ERC1155Contract.SafeTransferFromFunction
    //     {
    //         From = account,
    //         To = receivingAddress,
    //         Id = tokenId,
    //         Amount = amount,
    //         Data = data,
    //     };
    //     yield return transferReq.SignAndSendTransaction(transactionMessage, testERC1155ContractAddress);
    //     var transactionHash = transferReq.Result;
    //     Debug.Log("[Transfer ERC1155] Transaction Hash: " + transactionHash);

    //     // Wait for TX
    //     var txReceiptPollingReq = new TransactionReceiptPollingRequest(rpc);
    //     yield return txReceiptPollingReq.PollForReceipt(transactionHash, 2);
    //     var transferReceipt = txReceiptPollingReq.Result;
    //     Debug.Log("[Transfer ERC1155] Transaction Mined | Transfer Receipt: " + transferReceipt);

    //     // Check Event
    //     var transferEvent = transferReceipt.DecodeAllEvents<ERC1155Contract.SafeTransferFromFunction>();
    //     Debug.Log("[Transfer ERC1155] Transfer Event: " + transferEvent[0].Event.ToString());
    // }
}
