using System.Collections;
using System.Numerics;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class MarketplaceReadTests : ConfigManager
{
    private GameObject _go;
    private string _marketplaceAddress = "0xc9671F631E8313D53ec0b5358e1a499c574fCe6A";

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

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        Assert.IsNotNull(contract);
        Assert.AreEqual(_marketplaceAddress, contract.address);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DirectListings_GetAll_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.directListings.GetAll();
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        Assert.Greater(result.Result.Count, 0);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DirectListings_GetAllValid_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.directListings.GetAllValid();
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        Assert.GreaterOrEqual(result.Result.Count, 0);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DirectListings_GetListing_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.directListings.GetListing("1");
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DirectListings_GetTotalCount_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.directListings.GetTotalCount();
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        Assert.Greater(int.Parse(result.Result), 0);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DirectListings_IsBuyerApprovedForListing_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.directListings.IsBuyerApprovedForListing("1", _marketplaceAddress);
        yield return new WaitUntil(() => result.IsCompleted);
        if (Utils.IsWebGLBuild())
        {
            Assert.IsTrue(result.IsFaulted);
        }
        else
        {
            Assert.IsTrue(result.IsCompletedSuccessfully);
            Assert.IsNotNull(result.Result);
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator DirectListings_IsCurrencyApprovedForListing_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.directListings.IsCurrencyApprovedForListing("1", _marketplaceAddress);
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator EnglishAuctions_GetAll_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.englishAuctions.GetAll();
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        Assert.Greater(result.Result.Count, 0);
        yield return null;
    }

    [UnityTest]
    public IEnumerator EnglishAuctions_GetAllValid_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.englishAuctions.GetAllValid();
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        Assert.GreaterOrEqual(result.Result.Count, 0);
        yield return null;
    }

    [UnityTest]
    public IEnumerator EnglishAuctions_GetAuction_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.englishAuctions.GetAuction("0");
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator EnglishAuctions_GetBidBufferBps_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.englishAuctions.GetBidBufferBps("0");
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator EnglishAuctions_GetMinimumNextBid_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.englishAuctions.GetMinimumNextBid("0");
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator EnglishAuctions_GetTotalCount_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.englishAuctions.GetTotalCount();
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        Assert.Greater(int.Parse(result.Result), 0);
        yield return null;
    }

    [UnityTest]
    public IEnumerator EnglishAuctions_GetWinner_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.englishAuctions.GetWinner("0");
        yield return new WaitUntil(() => result.IsCompleted);
        if (Utils.IsWebGLBuild())
        {
            Assert.IsTrue(result.IsFaulted);
        }
        else
        {
            Assert.IsTrue(result.IsCompletedSuccessfully);
            Assert.IsNotNull(result.Result);
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator EnglishAuctions_GetWinningBid_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.englishAuctions.GetWinningBid("0");
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator EnglishAuctions_IsWinningBid_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.englishAuctions.IsWinningBid("0", "1");
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Offers_GetAll_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.offers.GetAll();
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        Assert.Greater(result.Result.Count, 0);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Offers_GetAllValid_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.offers.GetAllValid();
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        Assert.GreaterOrEqual(result.Result.Count, 0);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Offers_GetOffer_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.offers.GetOffer("0");
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Offers_GetTotalCount_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        var contract = ThirdwebManager.Instance.SDK.GetContract(_marketplaceAddress);
        var result = contract.marketplace.offers.GetTotalCount();
        yield return new WaitUntil(() => result.IsCompleted);
        Assert.IsTrue(result.IsCompletedSuccessfully);
        Assert.IsNotNull(result.Result);
        Assert.Greater(int.Parse(result.Result), 0);
        yield return null;
    }
}