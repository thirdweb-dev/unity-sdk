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

            await new WaitForSeconds(3f);

            Debugger.Instance.Log("Screenshot Saved! Uploading to IPFS...", $"Screenshot Path: {fullPath}");

            var response = await ThirdwebManager.Instance.SDK.storage.UploadFromPath(fullPath); // SDK must be initialized with StorageOptions apiToken
            Debugger.Instance.Log("Uploaded to IPFS Successfully!", response.ToString());
        }
        catch (System.Exception e)
        {
            Debugger.Instance.Log("[Storage] Upload Error", $"Error uploading, make sure a Client ID is set: {e.Message}");
        }
    }
}
