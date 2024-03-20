using System.Collections;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class ERC721ReadTests : ConfigManager
{
    private GameObject _go;
    private string _dropErc721Address = "0xD811CB13169C175b64bf8897e2Fd6a69C6343f5C";

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

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc721Address);
        Assert.IsNotNull(contract);
        Assert.AreEqual(_dropErc721Address, contract.address);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC721_Get_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc721Address);
        var currencyInfoTask = contract.ERC721.Get("1");
        yield return new WaitUntil(() => currencyInfoTask.IsCompleted);
        Assert.IsTrue(currencyInfoTask.IsCompletedSuccessfully);
        Assert.IsNotNull(currencyInfoTask.Result);
        Assert.AreEqual("1", currencyInfoTask.Result.metadata.id);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC721_GetAll_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc721Address);
        var currencyInfoTask = contract.ERC721.GetAll();
        yield return new WaitUntil(() => currencyInfoTask.IsCompleted);
        Assert.IsTrue(currencyInfoTask.IsCompletedSuccessfully);
        Assert.IsNotNull(currencyInfoTask.Result);
        Assert.GreaterOrEqual(currencyInfoTask.Result.Count, 0);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC721_GetOwned_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc721Address);
        var currencyInfoTask = contract.ERC721.GetOwned(_dropErc721Address);
        yield return new WaitUntil(() => currencyInfoTask.IsCompleted);
        Assert.IsTrue(currencyInfoTask.IsCompletedSuccessfully);
        Assert.IsNotNull(currencyInfoTask.Result);
        Assert.GreaterOrEqual(currencyInfoTask.Result.Count, 0);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC721_OwnerOf_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc721Address);
        var ownerTask = contract.ERC721.OwnerOf("1");
        yield return new WaitUntil(() => ownerTask.IsCompleted);
        Assert.IsTrue(ownerTask.IsCompletedSuccessfully);
        Assert.IsNotNull(ownerTask.Result);
        Assert.IsTrue(ownerTask.Result.Length == 42);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC721_BalanceOf_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc721Address);
        var balanceTask = contract.ERC721.BalanceOf(_dropErc721Address);
        yield return new WaitUntil(() => balanceTask.IsCompleted);
        Assert.IsTrue(balanceTask.IsCompletedSuccessfully);
        Assert.IsNotNull(balanceTask.Result);
        Assert.GreaterOrEqual(balanceTask.Result, BigInteger.Zero);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC721_IsApprovedForAll_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc721Address);
        var allowanceTask = contract.ERC721.IsApprovedForAll(_dropErc721Address, _dropErc721Address);
        yield return new WaitUntil(() => allowanceTask.IsCompleted);
        Assert.IsTrue(allowanceTask.IsCompletedSuccessfully);
        Assert.IsNotNull(allowanceTask.Result);
        Assert.IsTrue(allowanceTask.Result == true || allowanceTask.Result == false);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ERC721_TotalCount_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_dropErc721Address);
        var totalSupplyTask = contract.ERC721.TotalCount();
        yield return new WaitUntil(() => totalSupplyTask.IsCompleted);
        Assert.IsTrue(totalSupplyTask.IsCompletedSuccessfully);
        Assert.IsNotNull(totalSupplyTask.Result);
        Assert.GreaterOrEqual(totalSupplyTask.Result, 0);
        yield return null;
    }
}
