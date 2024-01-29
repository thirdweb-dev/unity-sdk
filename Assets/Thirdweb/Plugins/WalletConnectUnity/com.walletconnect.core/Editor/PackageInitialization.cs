using UnityEditor;
using UnityEngine;

namespace WalletConnectUnity.Core.Editor
{
    internal sealed class PackageInitialization : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets, 
            string[] deletedAssets, 
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var projectConfig = ProjectConfiguration.Load();
            
            if (projectConfig == null)
            {
                Debug.Log("[WalletConnectUnity] Project configuration not found. Creating...");
                ProjectConfiguration.Create();
            }
        }
    }
}