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
        var connectTask = ThirdwebManager.Instance.SDK.Wallet.Connect(connection);
        yield return new WaitUntil(() => connectTask.IsCompleted);
        Assert.IsTrue(connectTask.IsCompletedSuccessfully);
    }

    [UnityTest]
    public IEnumerator Deploy_WithSign_Success()
    {
        yield return Connect_WithGaslessManagedAccountFactory_Success();

        var task = ThirdwebManager.Instance.SDK.Wallet.Sign("Hello World");
        yield return new WaitUntil(() => task.IsCompleted);
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.Length == 132);
    }

    [UnityTest]
    public IEnumerator CreateSessionKey_WithValidSignerCheck_Success()
    {
        yield return Connect_WithGaslessManagedAccountFactory_Success();

        var task = ThirdwebManager.Instance.SDK.Wallet.CreateSessionKey(
            signerAddress: "0xA86F78b995a3899785FA1508eB1E62aEa501fc3c",
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

        var getAllActiveSignersTask = ThirdwebManager.Instance.SDK.Wallet.GetAllActiveSigners();
        yield return new WaitUntil(() => getAllActiveSignersTask.IsCompleted);
        Assert.IsTrue(getAllActiveSignersTask.IsCompletedSuccessfully);
        Assert.IsNotNull(getAllActiveSignersTask.Result);
        Assert.IsTrue(getAllActiveSignersTask.Result.Count > 0);

        bool exists = false;
        foreach (var signer in getAllActiveSignersTask.Result)
        {
            if (signer.signer == "0xA86F78b995a3899785FA1508eB1E62aEa501fc3c")
            {
                exists = true;
                break;
            }
        }
        Assert.IsTrue(exists);

        // Also check if admin is in here
        exists = false;
        var adminTask = ThirdwebManager.Instance.SDK.Wallet.GetSignerAddress();
        yield return new WaitUntil(() => adminTask.IsCompleted);
        Assert.IsTrue(adminTask.IsCompletedSuccessfully);
        Assert.IsNotNull(adminTask.Result);
        var admin = adminTask.Result;
        foreach (var signer in getAllActiveSignersTask.Result)
        {
            if (signer.signer == admin)
            {
                exists = true;
                Assert.IsTrue(signer.isAdmin);
                break;
            }
        }
        Assert.IsTrue(exists);
    }

    [UnityTest]
    public IEnumerator RevokeSessionKey_WithValidSignerCheck_Success()
    {
        yield return CreateSessionKey_WithValidSignerCheck_Success();

        var task = ThirdwebManager.Instance.SDK.Wallet.RevokeSessionKey(signerAddress: "0xA86F78b995a3899785FA1508eB1E62aEa501fc3c");
        yield return new WaitUntil(() => task.IsCompleted);
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);

        var getAllActiveSignersTask = ThirdwebManager.Instance.SDK.Wallet.GetAllActiveSigners();
        yield return new WaitUntil(() => getAllActiveSignersTask.IsCompleted);
        Assert.IsTrue(getAllActiveSignersTask.IsCompletedSuccessfully);
        Assert.IsNotNull(getAllActiveSignersTask.Result);

        bool exists = false;
        foreach (var signer in getAllActiveSignersTask.Result)
        {
            if (signer.signer == "0xA86F78b995a3899785FA1508eB1E62aEa501fc3c")
            {
                exists = true;
                break;
            }
        }
        Assert.IsFalse(exists);
    }

    [UnityTest]
    public IEnumerator IsDeployed_Success()
    {
        yield return Deploy_WithSign_Success();

        var task = ThirdwebManager.Instance.SDK.Wallet.IsDeployed();
        yield return new WaitUntil(() => task.IsCompleted);
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsTrue(task.Result);
    }
}
