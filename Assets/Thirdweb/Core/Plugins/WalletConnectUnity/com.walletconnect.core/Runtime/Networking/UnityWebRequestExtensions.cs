using UnityEngine.Networking;

namespace WalletConnectUnity.Core.Networking
{
    public static class UnityWebRequestExtensions
    {
        public static string sdkType = "wcm";
        public static string sdkVersion = "unity-sdk-v1.0.0";

        public static UnityWebRequest SetWalletConnectRequestHeaders(this UnityWebRequest uwr)
        {
            var projectId = ProjectConfiguration.Load().Id;

            uwr.SetRequestHeader("x-project-id", projectId);
            uwr.SetRequestHeader("x-sdk-type", sdkType);
            uwr.SetRequestHeader("x-sdk-version", sdkVersion);

            return uwr;
        }
    }
}