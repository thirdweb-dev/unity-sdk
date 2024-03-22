using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class ThirdwebManagerTests : ConfigManager
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
    public IEnumerator SingletonPattern_Enforcement_Success()
    {
        var go2 = new GameObject("ThirdwebManagerSecond");
        go2.AddComponent<ThirdwebManager>();
        yield return null;

        Assert.IsTrue(go2 == null || go2.Equals(null));
        Assert.IsNotNull(ThirdwebManager.Instance);
    }

    [UnityTest]
    public IEnumerator Initialization_HappyPath()
    {
        Assert.IsNotNull(ThirdwebManager.Instance);
        Assert.IsNull(ThirdwebManager.Instance.SDK);

        string chain = "Arbitrum Sepolia";
        BigInteger chainId = 421614;
        string chainIdHex = chainId.BigIntToHex();
        string rpc = null;
        ThirdwebManager.Instance.supportedChains = new List<ChainData> { new(chain, chainId.ToString(), rpc), };
        ThirdwebManager.Instance.Initialize(chain);
        yield return null;

        string expectedRpc = $"https://{chainId}.rpc.thirdweb.com/";
        Assert.IsNotNull(ThirdwebManager.Instance.SDK);
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.Session);
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.Session.Options);
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.Session.ChainId);
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.Session.RPC);
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.Session.CurrentChainData);

        Assert.IsNull(ThirdwebManager.Instance.SDK.Session.ActiveWallet);
        Assert.GreaterOrEqual(ThirdwebSession.Nonce, 0);

        Assert.AreEqual(chainId, ThirdwebManager.Instance.SDK.Session.ChainId);
        Assert.AreEqual(chainIdHex, ThirdwebManager.Instance.SDK.Session.CurrentChainData.chainId);

        Assert.AreEqual(expectedRpc, ThirdwebManager.Instance.SDK.Session.RPC);
        Assert.AreEqual(expectedRpc, ThirdwebManager.Instance.SDK.Session.CurrentChainData.rpcUrls[0]);

        Assert.AreEqual(chain, ThirdwebManager.Instance.SDK.Session.CurrentChainData.chainName);
    }

    [UnityTest]
    public IEnumerator Initialization_WithDefaultChain_Success()
    {
        ThirdwebManager.Instance.Initialize(ThirdwebManager.Instance.activeChain);
        yield return null;

        Assert.IsNotNull(ThirdwebManager.Instance.SDK);
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.Session);
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.Session.Options);
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.Session.ChainId);
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.Session.RPC);
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.Session.CurrentChainData);
        Assert.IsNull(ThirdwebManager.Instance.SDK.Session.ActiveWallet);

        var chainData = ThirdwebManager.Instance.supportedChains.Find(c => c.identifier == ThirdwebManager.Instance.activeChain);
        Assert.AreEqual(chainData.chainId, ThirdwebManager.Instance.SDK.Session.ChainId.ToString());
        Assert.AreEqual(BigInteger.Parse(chainData.chainId).BigIntToHex(), ThirdwebManager.Instance.SDK.Session.CurrentChainData.chainId);
    }

    [UnityTest]
    public IEnumerator Initialization_WithRpcOverride_AppliesCorrectly()
    {
        string chain = "ethereum";
        string customRpc = "https://custom.rpc.url/";
        ThirdwebManager.Instance.supportedChains = new List<ChainData> { new ChainData(chain, "1", customRpc), };
        ThirdwebManager.Instance.Initialize(chain);
        yield return null;

        Assert.AreEqual(customRpc, ThirdwebManager.Instance.SDK.Session.RPC);
        Assert.AreEqual(customRpc, ThirdwebManager.Instance.SDK.Session.CurrentChainData.rpcUrls[0]);
    }

    [UnityTest]
    public IEnumerator Initialization_WithUnsupportedActiveChain_Throws()
    {
        string unsupportedChain = "unsupported-chain";
        Exception caughtException = null;

        try
        {
            ThirdwebManager.Instance.Initialize(unsupportedChain);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        Assert.IsNotNull(caughtException);
        Assert.IsInstanceOf<UnityException>(caughtException);
        Assert.AreEqual(caughtException.Message, "Please add your active chain to the supported chains list! See https://thirdweb.com/dashboard/rpc for a list of supported chains.");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Initialization_WithUnknownChain_ConnectsButFails()
    {
        string chain = "Invalid Chain";
        BigInteger chainId = 1928347172371129831;
        ThirdwebManager.Instance.supportedChains = new List<ChainData> { new(chain, chainId.ToString(), null), };
        ThirdwebManager.Instance.Initialize(chain);
        Assert.IsTrue(ThirdwebManager.Instance.SDK.Session.CurrentChainData.chainName.Contains("Unknown Chain"));

        var connectTask = ThirdwebManager.Instance.SDK.Wallet.Connect(new WalletConnection(provider: WalletProvider.LocalWallet, chainId: chainId));
        yield return new WaitUntil(() => connectTask.IsCompleted);
        Assert.IsTrue(Utils.IsWebGLBuild() ? connectTask.IsFaulted : connectTask.IsCompletedSuccessfully);

        var rpcTask = ThirdwebManager.Instance.SDK.Wallet.GetBalance();
        yield return new WaitUntil(() => rpcTask.IsCompleted);
        Assert.IsTrue(rpcTask.IsFaulted);
    }

    [UnityTest]
    public IEnumerator Initialization_WithClientIdNoBundleIdOverride_AppliesCorrectly()
    {
        ThirdwebManager.Instance.clientId = "testClientId";
        Assert.IsNull(ThirdwebManager.Instance.bundleIdOverride);

        string bundleId = Utils.GetBundleId();
        Assert.IsNotNull(bundleId);

        ThirdwebManager.Instance.supportedChains = new List<ChainData> { new("arbitrum-sepolia", "421614", null), };
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");
        yield return null;

        Assert.AreEqual(ThirdwebManager.Instance.clientId, ThirdwebManager.Instance.SDK.Session.Options.clientId);
        Assert.AreEqual(bundleId, ThirdwebManager.Instance.SDK.Session.Options.bundleId);
        if (Utils.IsWebGLBuild())
        {
            Assert.AreEqual(ThirdwebManager.Instance.SDK.Session.RPC, $"https://421614.rpc.thirdweb.com/{ThirdwebManager.Instance.clientId}");
        }
        else
        {
            Assert.AreEqual(ThirdwebManager.Instance.SDK.Session.RPC, $"https://421614.rpc.thirdweb.com/{ThirdwebManager.Instance.clientId}?bundleId={bundleId}");
        }
    }

    [UnityTest]
    public IEnumerator Initialization_WithClientIdAndBundleIdOverride_AppliesCorrectly()
    {
        ThirdwebManager.Instance.clientId = "testClientId";
        ThirdwebManager.Instance.bundleIdOverride = "com.example.test";

        ThirdwebManager.Instance.supportedChains = new List<ChainData> { new("arbitrum-sepolia", "421614", null), };
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");
        yield return null;

        // Validate that the SDK session has correctly applied clientId and bundleId
        Assert.AreEqual(ThirdwebManager.Instance.clientId, ThirdwebManager.Instance.SDK.Session.Options.clientId);
        Assert.AreEqual("com.example.test", ThirdwebManager.Instance.SDK.Session.Options.bundleId);

        if (Utils.IsWebGLBuild())
        {
            Assert.AreEqual(ThirdwebManager.Instance.SDK.Session.RPC, $"https://421614.rpc.thirdweb.com/{ThirdwebManager.Instance.clientId}");
        }
        else
        {
            Assert.AreEqual(ThirdwebManager.Instance.SDK.Session.RPC, $"https://421614.rpc.thirdweb.com/{ThirdwebManager.Instance.clientId}?bundleId={ThirdwebManager.Instance.bundleIdOverride}");
        }
    }
}
