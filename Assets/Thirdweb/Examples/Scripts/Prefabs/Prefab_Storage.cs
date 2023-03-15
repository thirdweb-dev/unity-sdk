using System.Collections;
using UnityEngine;
using Thirdweb;
using System.IO;

public class Prefab_Storage : MonoBehaviour
{
    public async void OnUpload()
    {
        try
        {
            string fullPath = Application.temporaryCachePath + "/myImage.png";
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            ScreenCapture.CaptureScreenshot(fullPath);
            Debugger.Instance.Log("Screenshot Saved! Uploading to IPFS...", $"Screenshot Path: {fullPath}");

            // Give some time for the file to be saved if first time, try again if need be
            // Task.Delay may not work in WebGL without customizations, use Coroutines
            await System.Threading.Tasks.Task.Delay(5);

            var response = await ThirdwebManager.Instance.SDK.storage.UploadFromPath(fullPath); // SDK must be initialized with StorageOptions apiToken
            Debugger.Instance.Log("Uploaded to IPFS Successfully!", response.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Storage] Upload Error", $"Error uploading from path: {e.Message}");
        }
    }
}
