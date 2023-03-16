using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

public enum Platform
{
    WebGL,
    Other,
}

[System.Serializable]
public class ObjectList
{
    public List<GameObject> gameObjects;
}

[System.Serializable]
public class PlatformSpecificObjects : SerializableDictionaryBase<Platform, ObjectList> { }

public class PlatformSpecific : MonoBehaviour
{
    [SerializeField]
    public PlatformSpecificObjects platformSpecificObjects;

    Platform currentPlatform;

    private void Awake()
    {
        currentPlatform = GetCurrentPlatform();

        foreach (var obj in platformSpecificObjects)
        {
            foreach (var subObj in obj.Value.gameObjects)
            {
                subObj.SetActive(currentPlatform == obj.Key);
            }
        }
    }

    Platform GetCurrentPlatform()
    {
        if (Thirdweb.Utils.IsWebGLBuild())
        {
            return Platform.WebGL;
        }
        else
        {
            return Platform.Other;
        }
    }
}
