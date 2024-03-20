using System.IO;
using UnityEngine;

public class ConfigManager
{
    [System.Serializable]
    private class ConfigData
    {
        public string clientId;
    }

    private ConfigData _config;

    internal string GetClientId()
    {
        if (_config != null)
            return _config.clientId;

        string path = Path.Combine(Application.dataPath, "Tests/config.json");
        if (File.Exists(path))
        {
            string jsonContents = File.ReadAllText(path);
            _config = JsonUtility.FromJson<ConfigData>(jsonContents);
        }
        else
        {
            throw new System.Exception($"Config file not found at path: {path}");
        }

        return _config?.clientId;
    }
}
