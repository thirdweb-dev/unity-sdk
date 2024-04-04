using UnityEngine;
using System.IO;
using Thirdweb.Redcode.Awaiting;

namespace Thirdweb.Examples
{
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

                var response = await ThirdwebManager.Instance.SDK.Storage.UploadFromPath(fullPath); // SDK must be initialized with StorageOptions apiToken
                Debugger.Instance.Log("Uploaded to IPFS Successfully!", response.ToString());
            }
            catch (System.Exception e)
            {
                Debugger.Instance.Log("[Storage] Upload Error", $"Error uploading: {e.Message}");
            }
        }
    }
}
