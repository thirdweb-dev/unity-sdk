using System.Collections;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class ERC20WriteTests : ConfigManager
{
    private GameObject _go;
    private string _dropErc20Address = "0xEBB8a39D865465F289fa349A67B3391d8f910da9";

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
        var connectTask = ThirdwebManager.Instance.SDK.wallet.Connect(connection);
        yield return new WaitUntil(() => connectTask.IsCompleted);
        if (connectTask.IsFaulted)
            throw connectTask.Exception;
        Assert.IsTrue(connectTask.IsCompletedSuccessfully);
    }

    [UnityTest]
    public IEnumerator ERC20_SetAllowance_Success()
    {
        yield return ConnectSmartWallet();

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc20Address);
        var task = contract.ERC20.SetAllowance(_dropErc20Address, "42");
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    }

    [UnityTest]
    public IEnumerator ERC20_Claim_Success()
    {
        yield return ConnectSmartWallet();

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc20Address);
        var task = contract.ERC20.Claim("42");
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    }

    [UnityTest]
    public IEnumerator ERC20_Transfer_Success()
    {
        yield return ERC20_Claim_Success();

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc20Address);
        var task = contract.ERC20.Transfer(_dropErc20Address, "4.2");
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    }

    [UnityTest]
    public IEnumerator ERC20_Burn_Success()
    {
        yield return ERC20_Claim_Success();

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc20Address);
        var task = contract.ERC20.Burn("4.2");
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    }
}
