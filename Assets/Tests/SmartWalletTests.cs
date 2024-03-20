using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class SmartWalletTests : ConfigManager
{
    private GameObject _go;
    private string _managedAccountFactory = "0xbf1C9aA4B1A085f7DA890a44E82B0A1289A40052";

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
    public IEnumerator Connect_WithGaslessManagedAccountFactory_Success()
    {
        Utils.DeleteLocalAccount();

        ThirdwebManager.Instance.clientId = GetClientId();
        ThirdwebManager.Instance.factoryAddress = _managedAccountFactory;
        ThirdwebManager.Instance.gasless = true;
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var connection = new WalletConnection(provider: WalletProvider.SmartWallet, chainId: 421614, personalWallet: WalletProvider.LocalWallet);
        var connectTask = ThirdwebManager.Instance.SDK.wallet.Connect(connection);
        yield return new WaitUntil(() => connectTask.IsCompleted);
        Assert.IsTrue(connectTask.IsCompletedSuccessfully);
    }

    [UnityTest]
    public IEnumerator Deploy_WithSign_Success()
    {
        yield return Connect_WithGaslessManagedAccountFactory_Success();

        var task = ThirdwebManager.Instance.SDK.wallet.Sign("Hello World");
        yield return new WaitUntil(() => task.IsCompleted);
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.Length == 132);
    }

    [UnityTest]
    public IEnumerator CreateSessionKey_WithValidSignerCheck_Success()
    {
        yield return Connect_WithGaslessManagedAccountFactory_Success();

        var task = ThirdwebManager.Instance.SDK.wallet.CreateSessionKey(
            signerAddress: "0x22b79AD6c6009525933ac2FF40bC9F30dF14Ecfb",
            approvedTargets: new List<string>() { "0x450b943729Ddba196Ab58b589Cea545551DF71CC" },
            nativeTokenLimitPerTransactionInWei: "0",
            permissionStartTimestamp: "0",
            permissionEndTimestamp: (Utils.GetUnixTimeStampNow() + 86400).ToString(),
            reqValidityStartTimestamp: "0",
            reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
        );
        yield return new WaitUntil(() => task.IsCompleted);
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);

        var getAllActiveSignersTask = ThirdwebManager.Instance.SDK.wallet.GetAllActiveSigners();
        yield return new WaitUntil(() => getAllActiveSignersTask.IsCompleted);
        Assert.IsTrue(getAllActiveSignersTask.IsCompletedSuccessfully);
        Assert.IsNotNull(getAllActiveSignersTask.Result);
        Assert.IsTrue(getAllActiveSignersTask.Result.Count > 0);

        bool exists = false;
        foreach (var signer in getAllActiveSignersTask.Result)
        {
            if (signer.signer == "0x22b79AD6c6009525933ac2FF40bC9F30dF14Ecfb")
            {
                exists = true;
                break;
            }
        }
        Assert.IsTrue(exists);
    }

    [UnityTest]
    public IEnumerator IsDeployed_Success()
    {
        yield return Deploy_WithSign_Success();

        var task = ThirdwebManager.Instance.SDK.wallet.IsDeployed();
        yield return new WaitUntil(() => task.IsCompleted);
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsTrue(task.Result);
    }
}
