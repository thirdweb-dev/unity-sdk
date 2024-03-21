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

        try
        {
            string jsonContents = Resources.Load<TextAsset>("config").text;
            _config = JsonUtility.FromJson<ConfigData>(jsonContents);
            return _config?.clientId;
        }
        catch (System.Exception e)
        {
            throw new System.Exception("Failed to load config file from Resources: " + e.Message);
        }
    }
}
