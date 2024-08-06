using UnityEngine.Networking;

namespace WalletConnectUnity.Core.Networking
{
    public static class UnityWebRequestExtensions
    {
        public static UnityWebRequest SetWalletConnectRequestHeaders(this UnityWebRequest uwr)
        {
            var projectId = ProjectConfiguration.Load().Id;

            uwr.SetRequestHeader("x-project-id", projectId);
            uwr.SetRequestHeader("x-sdk-type", SdkMetadata.Type);
            uwr.SetRequestHeader("x-sdk-version", SdkMetadata.Version);

            return uwr;
        }
    }
}