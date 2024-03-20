using System.Collections;
using NUnit.Framework;
using Thirdweb;
using UnityEngine;
using UnityEngine.TestTools;

public class StorageTests : ConfigManager
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
    public IEnumerator Gateway_WithoutClientId_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.storage);
        Assert.AreEqual(ThirdwebManager.Instance.SDK.storage.IPFSGateway, "https://cloudflare-ipfs.com/ipfs/");

        string testIpfsRawUrl = "ipfs://Qblabla";
        Assert.AreEqual(Utils.ReplaceIPFS(testIpfsRawUrl), "https://cloudflare-ipfs.com/ipfs/Qblabla");

        yield return null;
    }

    [UnityTest]
    public IEnumerator Gateway_WithClientId_Success()
    {
        string clientId = "hello";
        ThirdwebManager.Instance.clientId = clientId;
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.storage);
        Assert.AreEqual(ThirdwebManager.Instance.SDK.storage.IPFSGateway, $"https://{clientId}.ipfscdn.io/ipfs/");

        string testIpfsRawUrl = "ipfs://Qblabla";
        Assert.AreEqual(Utils.ReplaceIPFS(testIpfsRawUrl), $"https://{clientId}.ipfscdn.io/ipfs/Qblabla");

        yield return null;
    }

    [UnityTest]
    public IEnumerator Gateway_WithOverride_Success()
    {
        string ipfsGatewayUrl = "https://ipfs.io/ipfs/";
        ThirdwebManager.Instance.storageIpfsGatewayUrl = ipfsGatewayUrl;
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");
        Assert.IsNotNull(ThirdwebManager.Instance.SDK.storage);
        Assert.AreEqual(ThirdwebManager.Instance.SDK.storage.IPFSGateway, ipfsGatewayUrl);

        string testIpfsRawUrl = "ipfs://Qblabla";
        Assert.AreEqual(Utils.ReplaceIPFS(testIpfsRawUrl), "https://ipfs.io/ipfs/Qblabla");

        yield return null;
    }

    [UnityTest]
    public IEnumerator DownloadText_WithoutIPFS_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        string url = "https://www.gutenberg.org/files/11/11-0.txt";

        var downloadTask = ThirdwebManager.Instance.SDK.storage.DownloadText<string>(url);
        yield return new WaitUntil(() => downloadTask.IsCompleted);
        Assert.IsTrue(downloadTask.IsCompletedSuccessfully);
        Assert.IsNotNull(downloadTask.Result);
        Assert.IsTrue(downloadTask.Result.Length > 0);
        Assert.IsTrue(downloadTask.Result.StartsWith("*** START OF THE PROJECT GUTENBERG EBOOK"));
    }

    [UnityTest]
    public IEnumerator DownloadImage_WithoutIPFS_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        string url = "https://upload.wikimedia.org/wikipedia/commons/4/47/PNG_transparency_demonstration_1.png";

        var downloadTask = ThirdwebManager.Instance.SDK.storage.DownloadImage(url);
        yield return new WaitUntil(() => downloadTask.IsCompleted);
        Assert.IsTrue(downloadTask.IsCompletedSuccessfully);
        Assert.IsNotNull(downloadTask.Result);
        Assert.IsTrue(downloadTask.Result.texture.width > 0);
        Assert.IsTrue(downloadTask.Result.texture.height > 0);
    }

    [UnityTest]
    public IEnumerator DownloadText_WithIPFS_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        string url = "ipfs://QmNQ2djT2u4my5xpKPgJMnQEpoNjYZE8ugpLndvgEJBb3X";

        var downloadTask = ThirdwebManager.Instance.SDK.storage.DownloadText<string>(url);
        yield return new WaitUntil(() => downloadTask.IsCompleted);
        Assert.IsTrue(downloadTask.IsCompletedSuccessfully);
        Assert.IsNotNull(downloadTask.Result);
        Assert.IsTrue(downloadTask.Result.Length > 0);
        Assert.IsTrue(downloadTask.Result.StartsWith("{\"compiler\":{\"version\":\"0.8.23+commit.f704f362\"},\"language\":\"Solidity\""));
    }

    [UnityTest]
    public IEnumerator DownloadImage_WithIPFS_Success()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        string url = "ipfs://QmfNyxShuV6Nrt3CLLdgaBPXfVr5MAbeWTHFCt8TZFsxgW/6.png";

        var downloadTask = ThirdwebManager.Instance.SDK.storage.DownloadImage(url);
        yield return new WaitUntil(() => downloadTask.IsCompleted);
        Assert.IsTrue(downloadTask.IsCompletedSuccessfully);
        Assert.IsNotNull(downloadTask.Result);
        Assert.IsTrue(downloadTask.Result.texture.width > 0);
        Assert.IsTrue(downloadTask.Result.texture.height > 0);
    }

    [UnityTest]
    public IEnumerator UploadText_WithoutClientId_Fail()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        string text = "Hello World!";
        var uploadTask = ThirdwebManager.Instance.SDK.storage.UploadText(text);
        yield return new WaitUntil(() => uploadTask.IsCompleted);
        Assert.IsTrue(uploadTask.IsFaulted);
    }

    [UnityTest]
    public IEnumerator UploadText_WithClientId_Success()
    {
        ThirdwebManager.Instance.clientId = GetClientId();
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        string text = "Hello World!";
        var uploadTask = ThirdwebManager.Instance.SDK.storage.UploadText(text);
        yield return new WaitUntil(() => uploadTask.IsCompleted);
        Assert.IsTrue(uploadTask.IsCompletedSuccessfully);
        Assert.IsNotNull(uploadTask.Result);
        Assert.IsNotNull(uploadTask.Result.IpfsHash);

        string url = "ipfs://" + uploadTask.Result.IpfsHash;
        var downloadTask = ThirdwebManager.Instance.SDK.storage.DownloadText<string>(url);
        yield return new WaitUntil(() => downloadTask.IsCompleted);
        Assert.IsTrue(downloadTask.IsCompletedSuccessfully);
        Assert.IsNotNull(downloadTask.Result);
        Assert.AreEqual(downloadTask.Result, text);
    }

    [UnityTest]
    public IEnumerator UploadFromPath_WithoutClientId_Fail()
    {
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        string path = Application.persistentDataPath + "/myObject.json";
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
        System.IO.File.WriteAllText(path, "{\"name\":\"John Doe\",\"age\":30,\"city\":\"New York\"}");

        yield return new WaitForSeconds(3f);

        var uploadTask = ThirdwebManager.Instance.SDK.storage.UploadFromPath(path);
        yield return new WaitUntil(() => uploadTask.IsCompleted);
        Assert.IsTrue(uploadTask.IsFaulted);
    }

    [UnityTest]
    public IEnumerator UploadFromPath_WithClientId_Success()
    {
        ThirdwebManager.Instance.clientId = GetClientId();
        ThirdwebManager.Instance.Initialize("arbitrum-sepolia");

        string path = Application.persistentDataPath + "/myObject.json";
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
        System.IO.File.WriteAllText(path, "{\"name\":\"John Doe\",\"age\":30,\"city\":\"New York\"}");

        var uploadTask = ThirdwebManager.Instance.SDK.storage.UploadFromPath(path);
        yield return new WaitUntil(() => uploadTask.IsCompleted);
        Assert.IsTrue(uploadTask.IsCompletedSuccessfully);
        Assert.IsNotNull(uploadTask.Result);
        Assert.IsNotNull(uploadTask.Result.IpfsHash);
    }
}
