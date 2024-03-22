using System.Collections;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class ERC1155WriteTests : ConfigManager
{
    private GameObject _go;
    private string _dropErc1155Address = "0x6A7a26c9a595E6893C255C9dF0b593e77518e0c3";

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
    public IEnumerator ERC1155_SetApprovalForAll_Success()
    {
        yield return ConnectSmartWallet();

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc1155Address);
        var task = contract.ERC1155.SetApprovalForAll(_dropErc1155Address, true);
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    }

    [UnityTest]
    public IEnumerator ERC1155_Claim_Success()
    {
        yield return ConnectSmartWallet();

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc1155Address);
        var task = contract.ERC1155.Claim("1", 1);
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    }

    [UnityTest]
    public IEnumerator ERC1155_Transfer_Success()
    {
        yield return ERC1155_Claim_Success();

        var addressTask = ThirdwebManager.Instance.SDK.Wallet.GetAddress();
        yield return new WaitUntil(() => addressTask.IsCompleted);
        if (addressTask.IsFaulted)
            throw addressTask.Exception;
        Assert.IsTrue(addressTask.IsCompletedSuccessfully);

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc1155Address);
        var task = contract.ERC1155.Transfer(addressTask.Result, "1", 1);
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    }

    [UnityTest]
    public IEnumerator ERC1155_Burn_Success()
    {
        yield return ERC1155_Claim_Success();

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc1155Address);
        var task = contract.ERC1155.Burn("1", 1);
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    }
}
