using System.Text;
using UnityEngine;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Controllers;

namespace WalletConnectUnity.Core
{
    [CreateAssetMenu(fileName = "WalletConnectProjectConfig", menuName = "WalletConnect/Project Configuration")]
    public sealed class ProjectConfiguration : ScriptableObject
    {
        [field: SerializeField, Header("Application")] public string Id { get; private set; }
        
        [field: SerializeField] public Metadata Metadata { get; private set; }

        [field: SerializeField] public string RelayUrl { get; private set; } = Relayer.DEFAULT_RELAY_URL;
        
        [field: SerializeField, Header ("Debug")] public bool LoggingEnabled { get; private set; }
        
        private const string ConfigName = "WalletConnectProjectConfig";

        public static readonly string ConfigPath = $"Assets/WalletConnectUnity/Resources/{ConfigName}.asset";

        public static ProjectConfiguration Load(string path = null)
        {
            return Resources.Load<ProjectConfiguration>(path ?? ConfigName);
        }

#if UNITY_EDITOR
        public static void Create()
        {
            var config = Load();
            if (config != null)
            {
                Debug.LogError("[WalletConnect] Project configuration already exists");
                return;
            }

            EnsureFolderStructureExists(ConfigPath);
            
            config = CreateInstance<ProjectConfiguration>();
            UnityEditor.AssetDatabase.CreateAsset(config, ConfigPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            Debug.Log($"[WalletConnect] Project configuration created at <i>{ConfigPath}</i>");
        }
        
        private static void EnsureFolderStructureExists(string desiredPath)
        {
            if (!desiredPath.StartsWith("Assets/"))
                desiredPath = "Assets/" + desiredPath;

            var pathBuilder = new StringBuilder("Assets");
            var folders = desiredPath.Split('/');

            // Start from 1 to skip "Assets", and -1 to exclude the filename itself
            for (var i = 1; i < folders.Length - 1; i++)
            {
                pathBuilder.Append("/").Append(folders[i]);

                var currentPath = pathBuilder.ToString();
                if (!UnityEditor.AssetDatabase.IsValidFolder(currentPath))
                    UnityEditor.AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(currentPath), folders[i]);
            }
        }
#endif

        public override string ToString()
        {
            return $"[ProjectConfiguration] Name: {Metadata.Name}; Id: {Id}";
        }
    }
}