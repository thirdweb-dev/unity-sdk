using System.Collections;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class ERC721WriteTests : ConfigManager
{
    private GameObject _go;
    private string _dropErc712Address = "0xD811CB13169C175b64bf8897e2Fd6a69C6343f5C";

    [SetUp]
    public void SetUp()
    {
        var existingManager = GameObject.FindObjectOfType<ThirdwebManager>();
        if (existingManager != null)
            GameObject.DestroyImmediate(existingManager.gameObject);

        _go = new GameObject("ThirdwebManager");
        _go.AddComponent<ThirdwebManager>();

        ThirdwebManager.Instance.clientId = GetClientId();
        ThirdwebManager.Instance.factoryAddress = "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052"; // ManagedAccountFactory
        ThirdwebManager.Instance.gasless = true;
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null)
        {
            GameObject.DestroyImmediate(_go);
            _go = null;
        }
    }

    private IEnumerator ConnectSmartWallet()
    {
        Utils.DeleteLocalAccount();
        var connection = new WalletConnection(provider: WalletProvider.SmartWallet, chainId: 421614, personalWallet: WalletProvider.LocalWallet);
        var connectTask = ThirdwebManager.Instance.SDK.Wallet.Connect(connection);
        yield return new WaitUntil(() => connectTask.IsCompleted);
        if (connectTask.IsFaulted)
            throw connectTask.Exception;
        Assert.IsTrue(connectTask.IsCompletedSuccessfully);
    }

    [UnityTest]
    public IEnumerator ERC721_SetApprovalForAll_Success()
    {
        yield return ConnectSmartWallet();

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc712Address);
        var task = contract.ERC721.SetApprovalForAll(_dropErc712Address, false);
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    }

    // [UnityTest]
    // public IEnumerator ERC721_Claim_Success()
    // {
    //     yield return ConnectSmartWallet();

    //     var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc712Address);
    //     var task = contract.ERC721.Claim(1);
    //     yield return new WaitUntil(() => task.IsCompleted);
    //     if (task.IsFaulted)
    //         throw task.Exception;
    //     Assert.IsTrue(task.IsCompletedSuccessfully);
    //     Assert.IsNotNull(task.Result);
    //     Assert.IsTrue(task.Result[0].receipt.transactionHash.Length == 66);
    // }

    // [UnityTest]
    // public IEnumerator ERC721_Transfer_Success()
    // {
    //     yield return ERC721_Claim_Success();

    //     var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc712Address);

    //     var addressTask = ThirdwebManager.Instance.SDK.wallet.GetAddress();
    //     yield return new WaitUntil(() => addressTask.IsCompleted);
    //     if (addressTask.IsFaulted)
    //         throw addressTask.Exception;
    //     Assert.IsTrue(addressTask.IsCompletedSuccessfully);

    //     var latestTokenIdTask = contract.ERC721.TotalCount();
    //     yield return new WaitUntil(() => latestTokenIdTask.IsCompleted);
    //     if (latestTokenIdTask.IsFaulted)
    //         throw latestTokenIdTask.Exception;
    //     Assert.IsTrue(latestTokenIdTask.IsCompletedSuccessfully);
    //     Assert.IsNotNull(latestTokenIdTask.Result);
    //     var latestTokenId = latestTokenIdTask.Result - 1;

    //     var task = contract.ERC721.Transfer(addressTask.Result, latestTokenId.ToString());
    //     yield return new WaitUntil(() => task.IsCompleted);
    //     if (task.IsFaulted)
    //         throw task.Exception;
    //     Assert.IsTrue(task.IsCompletedSuccessfully);
    //     Assert.IsNotNull(task.Result);
    //     Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    // }

    // [UnityTest]
    // public IEnumerator ERC721_Burn_Success()
    // {
    //     yield return ERC721_Claim_Success();

    //     var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc712Address);

    //     var latestTokenIdTask = contract.ERC721.TotalCount();
    //     yield return new WaitUntil(() => latestTokenIdTask.IsCompleted);
    //     if (latestTokenIdTask.IsFaulted)
    //         throw latestTokenIdTask.Exception;
    //     Assert.IsTrue(latestTokenIdTask.IsCompletedSuccessfully);
    //     Assert.IsNotNull(latestTokenIdTask.Result);
    //     var latestTokenId = latestTokenIdTask.Result - 1;

    //     var task = contract.ERC721.Burn(latestTokenId.ToString());
    //     yield return new WaitUntil(() => task.IsCompleted);
    //     if (task.IsFaulted)
    //         throw task.Exception;
    //     Assert.IsTrue(task.IsCompletedSuccessfully);
    //     Assert.IsNotNull(task.Result);
    //     Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    // }
}
