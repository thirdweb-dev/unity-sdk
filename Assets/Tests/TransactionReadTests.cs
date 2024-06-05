using System.Collections;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class TransactionReadTests : ConfigManager
{
    private GameObject _go;

    [SetUp]
    public void SetUp()
    {
        var existingManager = GameObject.FindObjectOfType<ThirdwebManager>();
        if (existingManager != null)
            GameObject.DestroyImmediate(existingManager.gameObject);

        _go = new GameObject("ThirdwebManager");
        _go.AddComponent<ThirdwebManager>();

        ThirdwebManager.Instance.clientId = GetClientId();
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

    [UnityTest]
    public IEnumerator Static_WaitForTransactionResult_Success()
    {
        string txHash = "0x52b79681f549d7b01b12b8be5fa9dd88f7fee1411f965cbe7ec6e157ccb48af1";
        var task = Transaction.WaitForTransactionResult(txHash, ThirdwebManager.Instance.SDK.Session.ChainId);
        yield return new WaitUntil(() => task.IsCompleted);
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsNotNull(task.Result.receipt.blockNumber);
        Assert.Greater(task.Result.receipt.blockNumber, BigInteger.Zero);
    }
}
