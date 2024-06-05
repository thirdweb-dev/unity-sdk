using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class ThirdwebSDKTests : ConfigManager
{
    private ThirdwebSDK _sdk;
    private readonly string _dropErc20Address = "0xEBB8a39D865465F289fa349A67B3391d8f910da9";

    [SetUp]
    public void SetUp()
    {
        var chainId = 421614;
        var clientId = GetClientId();
        var bundleId = Application.identifier.ToLower();
        var options = new ThirdwebSDK.Options()
        {
            clientId = clientId,
            bundleId = bundleId,
            supportedChains = new ThirdwebChainData[] { ThirdwebSession.FetchChainData(chainId) },
            smartWalletConfig = new ThirdwebSDK.SmartWalletConfig()
            {
                factoryAddress = Thirdweb.AccountAbstraction.Constants.DEFAULT_FACTORY_ADDRESS,
                gasless = true,
                bundlerUrl = $"https://{chainId}.bundler.thirdweb.com",
                paymasterUrl = $"https://{chainId}.bundler.thirdweb.com",
                entryPointAddress = Thirdweb.AccountAbstraction.Constants.DEFAULT_ENTRYPOINT_ADDRESS,
            }
        };
        _sdk = new ThirdwebSDK($"https://{chainId}.rpc.thirdweb.com/{clientId}&bundleId={bundleId}", 421614, options);
    }

    [TearDown]
    public void TearDown()
    {
        _sdk = null;
    }

    [UnityTest]
    public IEnumerator Initialization_Success()
    {
        Assert.IsNotNull(_sdk);
        Assert.AreEqual(_sdk.Session.Options.clientId, GetClientId());
        Assert.AreEqual(_sdk.Session.Options.bundleId, Application.identifier.ToLower());
        yield return null;
    }

    [UnityTest]
    public IEnumerator ContractRead_Success()
    {
        var contract = _sdk.GetContract(_dropErc20Address);
        var readTask = contract.ERC20.BalanceOf(_dropErc20Address);
        yield return new WaitUntil(() => readTask.IsCompleted);
        if (readTask.IsFaulted)
            throw readTask.Exception;
        Assert.IsTrue(readTask.IsCompletedSuccessfully);
        Assert.NotNull(readTask.Result);
    }

    [UnityTest]
    public IEnumerator ContractWrite_Success()
    {
        Utils.DeleteLocalAccount();
        var connection = new WalletConnection(provider: WalletProvider.SmartWallet, chainId: 421614, personalWallet: WalletProvider.LocalWallet);
        var connectTask = _sdk.Wallet.Connect(connection);
        yield return new WaitUntil(() => connectTask.IsCompleted);
        if (connectTask.IsFaulted)
            throw connectTask.Exception;
        Assert.IsTrue(connectTask.IsCompletedSuccessfully);

        var contract = _sdk.GetContract(_dropErc20Address);
        var task = contract.ERC20.SetAllowance(_dropErc20Address, "0");
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    }

    [UnityTest]
    public IEnumerator CustomContractRead_Success()
    {
        var contract = _sdk.GetContract(_dropErc20Address);
        var readTask = contract.Read<BigInteger>("balanceOf", _dropErc20Address);
        yield return new WaitUntil(() => readTask.IsCompleted);
        if (readTask.IsFaulted)
            throw readTask.Exception;
        Assert.IsTrue(readTask.IsCompletedSuccessfully);
        Assert.NotNull(readTask.Result);
    }

    [UnityTest]
    public IEnumerator CustomContractWrite_Success()
    {
        Utils.DeleteLocalAccount();
        var connection = new WalletConnection(provider: WalletProvider.SmartWallet, chainId: 421614, personalWallet: WalletProvider.LocalWallet);
        var connectTask = _sdk.Wallet.Connect(connection);
        yield return new WaitUntil(() => connectTask.IsCompleted);
        if (connectTask.IsFaulted)
            throw connectTask.Exception;
        Assert.IsTrue(connectTask.IsCompletedSuccessfully);

        var contract = _sdk.GetContract(_dropErc20Address);
        var task = contract.ERC20.SetAllowance(_dropErc20Address, "0");
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
        Assert.IsTrue(task.Result.receipt.transactionHash.Length == 66);
    }

    [UnityTest]
    public IEnumerator WalletSignMessage_Success()
    {
        Utils.DeleteLocalAccount();
        var connection = new WalletConnection(provider: WalletProvider.LocalWallet, chainId: 421614);
        var connectTask = _sdk.Wallet.Connect(connection);
        yield return new WaitUntil(() => connectTask.IsCompleted);
        if (connectTask.IsFaulted)
            throw connectTask.Exception;
        Assert.IsTrue(connectTask.IsCompletedSuccessfully);

        var message = "Hello, World!";
        var task = _sdk.Wallet.Sign(message);
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsFaulted)
            throw task.Exception;
        Assert.IsTrue(task.IsCompletedSuccessfully);
        Assert.IsNotNull(task.Result);
    }

    [UnityTest]
    public IEnumerator IPFSDownload_Success()
    {
        string url = "ipfs://QmNQ2djT2u4my5xpKPgJMnQEpoNjYZE8ugpLndvgEJBb3X";

        var downloadTask = _sdk.Storage.DownloadText<string>(url);
        yield return new WaitUntil(() => downloadTask.IsCompleted);
        Assert.IsTrue(downloadTask.IsCompletedSuccessfully);
        Assert.IsNotNull(downloadTask.Result);
        Assert.IsTrue(downloadTask.Result.Length > 0);
        Assert.IsTrue(downloadTask.Result.StartsWith("{\"compiler\":{\"version\":\"0.8.23+commit.f704f362\"},\"language\":\"Solidity\""));
    }

    [UnityTest]
    public IEnumerator IPFSUpload_Success()
    {
        string text = "Hello World!";
        var uploadTask = _sdk.Storage.UploadText(text);
        yield return new WaitUntil(() => uploadTask.IsCompleted);
        Assert.IsTrue(uploadTask.IsCompletedSuccessfully);
        Assert.IsNotNull(uploadTask.Result);
        Assert.IsNotNull(uploadTask.Result.IpfsHash);

        string url = "ipfs://" + uploadTask.Result.IpfsHash;
        var downloadTask = _sdk.Storage.DownloadText<string>(url);
        yield return new WaitUntil(() => downloadTask.IsCompleted);
        Assert.IsTrue(downloadTask.IsCompletedSuccessfully);
        Assert.IsNotNull(downloadTask.Result);
        Assert.AreEqual(downloadTask.Result, text);
    }
}
