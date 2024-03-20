using System.Collections;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class ContractReadTests : ConfigManager
{
    private GameObject _go;
    private string _tokenErc20Address = "0x81ebd23aA79bCcF5AaFb9c9c5B0Db4223c39102e";

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

        var contract = ThirdwebManager.Instance.SDK.GetContract("0x");
        Assert.IsNotNull(contract);
        Assert.AreEqual("0x", contract.address);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Custom_WithoutAbi_Fail()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract("0x");
        var readTask = contract.Read<BigInteger>("balanceOf", "0x");
        yield return new WaitUntil(() => readTask.IsCompleted);
        Assert.IsTrue(readTask.IsFaulted);
        Assert.AreEqual("You must pass an ABI for native platform custom calls", readTask.Exception.InnerException.Message);
    }

    [UnityTest]
    public IEnumerator Custom_WithAbi_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_tokenErc20Address, abi: "function balanceOf(address) external view returns (uint256)");
        var readTask = contract.Read<BigInteger>("balanceOf", _tokenErc20Address);
        yield return new WaitUntil(() => readTask.IsCompleted);
        Assert.IsTrue(readTask.IsCompletedSuccessfully);
        Assert.NotNull(readTask.Result);
    }

    [UnityTest]
    public IEnumerator Custom_WithString_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_tokenErc20Address, abi: "function symbol() external view returns (string)");
        var readTask = contract.Read<string>("symbol");
        yield return new WaitUntil(() => readTask.IsCompleted);
        Assert.IsTrue(readTask.IsCompletedSuccessfully);
        Assert.NotNull(readTask.Result);
    }

    [UnityTest]
    public IEnumerator ERC20_Get_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_tokenErc20Address);
        var currencyInfoTask = contract.ERC20.Get();
        yield return new WaitUntil(() => currencyInfoTask.IsCompleted);
        Assert.IsTrue(currencyInfoTask.IsCompletedSuccessfully);
        Assert.IsNotNull(currencyInfoTask.Result);
        Assert.AreEqual("18", currencyInfoTask.Result.decimals);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC20_BalanceOf_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_tokenErc20Address);
        var balanceTask = contract.ERC20.BalanceOf(_tokenErc20Address);
        yield return new WaitUntil(() => balanceTask.IsCompleted);
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

        var contract = ThirdwebManager.Instance.SDK.GetContract(_tokenErc20Address);
        var allowanceTask = contract.ERC20.AllowanceOf(_tokenErc20Address, _tokenErc20Address);
        yield return new WaitUntil(() => allowanceTask.IsCompleted);
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

        var contract = ThirdwebManager.Instance.SDK.GetContract(_tokenErc20Address);
        var totalSupplyTask = contract.ERC20.TotalSupply();
        yield return new WaitUntil(() => totalSupplyTask.IsCompleted);
        Assert.IsTrue(totalSupplyTask.IsCompletedSuccessfully);
        Assert.IsNotNull(totalSupplyTask.Result);
        Assert.IsNotNull(totalSupplyTask.Result.value);
        Assert.GreaterOrEqual(BigInteger.Parse(totalSupplyTask.Result.value), BigInteger.Zero);
        yield return null;
    }
}
