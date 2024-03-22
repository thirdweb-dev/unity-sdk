using System.Collections;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class ERC20ReadTests : ConfigManager
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

    [UnityTest]
    public IEnumerator GetContract_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc20Address);
        Assert.IsNotNull(contract);
        Assert.AreEqual(_dropErc20Address, contract.address);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC20_Get_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc20Address);
        var currencyInfoTask = contract.ERC20.Get();
        yield return new WaitUntil(() => currencyInfoTask.IsCompleted);
        if (currencyInfoTask.IsFaulted)
            throw currencyInfoTask.Exception;
        Assert.IsTrue(currencyInfoTask.IsCompletedSuccessfully);
        Assert.IsNotNull(currencyInfoTask.Result);
        Assert.AreEqual("18", currencyInfoTask.Result.decimals);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC20_BalanceOf_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc20Address);
        var balanceTask = contract.ERC20.BalanceOf(_dropErc20Address);
        yield return new WaitUntil(() => balanceTask.IsCompleted);
        if (balanceTask.IsFaulted)
            throw balanceTask.Exception;
        Assert.IsTrue(balanceTask.IsCompletedSuccessfully);
        Assert.IsNotNull(balanceTask.Result);
        Assert.IsNotNull(balanceTask.Result.value);
        Assert.GreaterOrEqual(BigInteger.Parse(balanceTask.Result.value), BigInteger.Zero);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC20_AllowanceOf_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc20Address);
        var allowanceTask = contract.ERC20.AllowanceOf(_dropErc20Address, _dropErc20Address);
        yield return new WaitUntil(() => allowanceTask.IsCompleted);
        if (allowanceTask.IsFaulted)
            throw allowanceTask.Exception;
        Assert.IsTrue(allowanceTask.IsCompletedSuccessfully);
        Assert.IsNotNull(allowanceTask.Result);
        Assert.IsNotNull(allowanceTask.Result.value);
        Assert.GreaterOrEqual(BigInteger.Parse(allowanceTask.Result.value), BigInteger.Zero);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC20_TotalSupply_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc20Address);
        var totalSupplyTask = contract.ERC20.TotalSupply();
        yield return new WaitUntil(() => totalSupplyTask.IsCompleted);
        if (totalSupplyTask.IsFaulted)
            throw totalSupplyTask.Exception;
        Assert.IsTrue(totalSupplyTask.IsCompletedSuccessfully);
        Assert.IsNotNull(totalSupplyTask.Result);
        Assert.IsNotNull(totalSupplyTask.Result.value);
        Assert.GreaterOrEqual(BigInteger.Parse(totalSupplyTask.Result.value), BigInteger.Zero);
        yield return null;
    }
}
