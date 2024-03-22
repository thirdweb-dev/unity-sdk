using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class WalletTests : ConfigManager
{
    private GameObject _go;
    private string _chain = "arbitrum-sepolia";
    private BigInteger _chainId = 421614;

    [SetUp]
    public void SetUp()
    {
        var existingManager = GameObject.FindObjectOfType<ThirdwebManager>();
        if (existingManager != null)
            GameObject.DestroyImmediate(existingManager.gameObject);

        _go = new GameObject("ThirdwebManager");
        _go.AddComponent<ThirdwebManager>();

        ThirdwebManager.Instance.clientId = GetClientId();
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
    public IEnumerator Connect_WithLocalWallet_Success()
    {
        Utils.DeleteLocalAccount(); // cleanup existing account
        ThirdwebManager.Instance.Initialize(_chain);
        var connection = new WalletConnection(provider: WalletProvider.LocalWallet, chainId: _chainId, password: null); // device uid
        var connectTask = ThirdwebManager.Instance.SDK.wallet.Connect(connection);
        yield return new WaitUntil(() => connectTask.IsCompleted);
        Assert.IsTrue(connectTask.IsCompletedSuccessfully);
        Assert.IsNotNull(connectTask.Result);
        Assert.IsTrue(connectTask.Result.Length == 42);
    }

    [UnityTest]
    public IEnumerator Connect_WithLocalWallet_Fail()
    {
        yield return Connect_WithLocalWallet_Success();

        ThirdwebManager.Instance.Initialize(_chain);
        var connection = new WalletConnection(provider: WalletProvider.LocalWallet, chainId: _chainId, password: "wrongpassword");
        var connectTask = ThirdwebManager.Instance.SDK.wallet.Connect(connection);
        yield return new WaitUntil(() => connectTask.IsCompleted);
        Assert.IsTrue(connectTask.IsFaulted);
    }

    [UnityTest]
    public IEnumerator Disconnect_WithLocalWallet_Success()
    {
        yield return Connect_WithLocalWallet_Success();

        var disconnectTask = ThirdwebManager.Instance.SDK.wallet.Disconnect();
        yield return new WaitUntil(() => disconnectTask.IsCompleted);
        Assert.IsTrue(disconnectTask.IsCompletedSuccessfully);

        var getAddressTask = ThirdwebManager.Instance.SDK.wallet.GetAddress();
        yield return new WaitUntil(() => getAddressTask.IsCompleted);
        Assert.IsTrue(getAddressTask.IsFaulted);
    }

    [UnityTest]
    public IEnumerator Export_WithLocalWallet_Success()
    {
        yield return Connect_WithLocalWallet_Success();

        var exportTask = ThirdwebManager.Instance.SDK.wallet.Export(null);
        yield return new WaitUntil(() => exportTask.IsCompleted);
        Assert.IsTrue(exportTask.IsCompletedSuccessfully);
        Assert.IsNotNull(exportTask.Result);
        Assert.IsTrue(exportTask.Result.Length > 0);
    }

    [UnityTest]
    public IEnumerator Authenticate_WithLocalWallet_Success()
    {
        yield return Connect_WithLocalWallet_Success();

        var authenticateTask = ThirdwebManager.Instance.SDK.wallet.Authenticate("example.com");
        yield return new WaitUntil(() => authenticateTask.IsCompleted);
        Assert.IsTrue(authenticateTask.IsCompletedSuccessfully);
        Assert.IsNotNull(authenticateTask.Result);
        Assert.IsTrue(authenticateTask.Result.signature.Length == 132);
    }

    [UnityTest]
    public IEnumerator Verify_WithLocalWallet_Success()
    {
        yield return Connect_WithLocalWallet_Success();

        var authenticateTask = ThirdwebManager.Instance.SDK.wallet.Authenticate("example.com");
        yield return new WaitUntil(() => authenticateTask.IsCompleted);
        Assert.IsTrue(authenticateTask.IsCompletedSuccessfully);
        Assert.IsNotNull(authenticateTask.Result);
        Assert.IsTrue(authenticateTask.Result.signature.Length == 132);

        var verifyTask = ThirdwebManager.Instance.SDK.wallet.Verify(authenticateTask.Result);
        yield return new WaitUntil(() => verifyTask.IsCompleted);
        Assert.IsTrue(verifyTask.IsCompletedSuccessfully);

        var getAddressTask = ThirdwebManager.Instance.SDK.wallet.GetAddress();
        yield return new WaitUntil(() => getAddressTask.IsCompleted);
        Assert.IsTrue(getAddressTask.IsCompletedSuccessfully);
        Assert.AreEqual(verifyTask.Result, getAddressTask.Result);
    }

    [UnityTest]
    public IEnumerator GetBalance_WithLocalWallet_Success()
    {
        yield return Connect_WithLocalWallet_Success();

        var getBalanceTask = ThirdwebManager.Instance.SDK.wallet.GetBalance();
        yield return new WaitUntil(() => getBalanceTask.IsCompleted);
        Assert.IsTrue(getBalanceTask.IsCompletedSuccessfully);
        Assert.IsNotNull(getBalanceTask.Result);
        Assert.IsTrue(getBalanceTask.Result.value == "0");
    }

    [UnityTest]
    public IEnumerator GetAddress_WithLocalWallet_Success()
    {
        yield return Connect_WithLocalWallet_Success();

        var getAddressTask = ThirdwebManager.Instance.SDK.wallet.GetAddress();
        yield return new WaitUntil(() => getAddressTask.IsCompleted);
        Assert.IsTrue(getAddressTask.IsCompletedSuccessfully);
        Assert.IsNotNull(getAddressTask.Result);
        Assert.IsTrue(getAddressTask.Result.Length == 42);
    }

    [UnityTest]
    public IEnumerator GetAddress_WithLocalWallet_Fail()
    {
        yield return Connect_WithLocalWallet_Success();

        var disconnectTask = ThirdwebManager.Instance.SDK.wallet.Disconnect();
        yield return new WaitUntil(() => disconnectTask.IsCompleted);
        Assert.IsTrue(disconnectTask.IsCompletedSuccessfully);

        var getAddressTask = ThirdwebManager.Instance.SDK.wallet.GetAddress();
        yield return new WaitUntil(() => getAddressTask.IsCompleted);
        Assert.IsTrue(getAddressTask.IsFaulted);
    }

    [UnityTest]
    public IEnumerator IsConnected_WithLocalWallet_Success()
    {
        yield return Connect_WithLocalWallet_Success();

        var isConnectedTask = ThirdwebManager.Instance.SDK.wallet.IsConnected();
        yield return new WaitUntil(() => isConnectedTask.IsCompleted);
        Assert.IsTrue(isConnectedTask.IsCompletedSuccessfully);
        Assert.IsTrue(isConnectedTask.Result);
    }

    [UnityTest]
    public IEnumerator IsConnected_WithLocalWallet_Fail()
    {
        ThirdwebManager.Instance.Initialize(_chain);
        var isConnectedTask = ThirdwebManager.Instance.SDK.wallet.IsConnected();
        yield return new WaitUntil(() => isConnectedTask.IsCompleted);
        Assert.IsTrue(isConnectedTask.IsCompletedSuccessfully);
        Assert.IsFalse(isConnectedTask.Result);
    }

    [UnityTest]
    public IEnumerator GetChainId_WithLocalWallet_Success()
    {
        yield return Connect_WithLocalWallet_Success();

        var getChainIdTask = ThirdwebManager.Instance.SDK.wallet.GetChainId();
        yield return new WaitUntil(() => getChainIdTask.IsCompleted);
        Assert.IsTrue(getChainIdTask.IsCompletedSuccessfully);
        Assert.AreEqual(getChainIdTask.Result, _chainId);
    }

    [UnityTest]
    public IEnumerator Transfer_WithLocalWallet_Fail()
    {
        yield return Connect_WithLocalWallet_Success();

        var randomAddress = "0x0C741CBb712708b866764C82096dDfA7976B8e0c";
        var transferTask = ThirdwebManager.Instance.SDK.wallet.Transfer(to: randomAddress, amount: "0");
        yield return new WaitUntil(() => transferTask.IsCompleted);
        Assert.IsTrue(transferTask.IsFaulted);
    }

    [UnityTest]
    public IEnumerator Sign_WithLocalWallet_Success()
    {
        yield return Connect_WithLocalWallet_Success();

        var message = "Hello World!";
        var signTask = ThirdwebManager.Instance.SDK.wallet.Sign(message);
        yield return new WaitUntil(() => signTask.IsCompleted);
        Assert.IsTrue(signTask.IsCompletedSuccessfully);
        Assert.IsNotNull(signTask.Result);
        Assert.IsTrue(signTask.Result.Length == 132);
    }

    [UnityTest]
    public IEnumerator SendRawTransaction_WithLocalWallet_Fail()
    {
        yield return Connect_WithLocalWallet_Success();

        var randomAddress = "0x0C741CBb712708b866764C82096dDfA7976B8e0c";

        var addressTask = ThirdwebManager.Instance.SDK.wallet.GetAddress();
        yield return new WaitUntil(() => addressTask.IsCompleted);
        Assert.IsTrue(addressTask.IsCompletedSuccessfully);

        var sendRawTransactionTask = ThirdwebManager.Instance.SDK.wallet.SendRawTransaction(
            new TransactionRequest()
            {
                from = addressTask.Result,
                to = randomAddress,
                data = "0x",
                value = "0",
                gasLimit = "0",
            }
        );
        yield return new WaitUntil(() => sendRawTransactionTask.IsCompleted);
        Assert.IsTrue(sendRawTransactionTask.IsFaulted);
    }
}
