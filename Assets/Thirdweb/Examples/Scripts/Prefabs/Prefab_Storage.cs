using System.Collections;
using UnityEngine;
using Thirdweb;

public class Prefab_Storage : MonoBehaviour
{
    public async void OnUpload()
    {
        string fullPath = Application.persistentDataPath + "/myImage " + System.DateTime.Now.ToString("yy-MM-dd") + ".png";
        ScreenCapture.CaptureScreenshot(fullPath);

        // Give some time for the file to be saved if first time, try again if need be

        var response = await ThirdwebManager.Instance.SDK.storage.UploadDataFromStringHttpClient(fullPath); // SDK must be initialized with StorageOptions apiToken
        Debugger.Instance.Log("Uploaded to IPFS Successfully!", response.ToString());
    }
}
