using System.Collections;
using UnityEngine;
using Thirdweb;

public class Prefab_Storage : MonoBehaviour
{
    [Header("NFT.STORAGE API KEY (REQUIRED)")]
    public string apiToken = null;

    public async void OnUpload()
    {
        string fullPath = Application.persistentDataPath + "/myImage " + System.DateTime.Now.ToString("yy-MM-dd") + ".png";
        ScreenCapture.CaptureScreenshot(fullPath);

        // Give some time for the file to be saved if first time, try again if need be

        var response = await Storage.UploadDataFromStringHttpClient(apiToken, fullPath);
        Debugger.Instance.Log("Uploaded to IPFS Successfully!", response.ToString());
    }
}
