using System.Collections.Generic;
using UnityEditor;

namespace RotaryHeart.Lib.SerializableDictionary
{
    [InitializeOnLoad]
    public class SerializableDictionaryDefiner : Definer
    {
        static SerializableDictionaryDefiner()
        {
            List<string> defines = new List<string>(1)
            {
                "RH_SerializedDictionary"
            };
            
            if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath("ccdbbd5c7cb114949a686ae4fe06d208")))
            {
                RemoveDefines(new List<string>(1)
                {
                    "RH_SerializedDictionaryPro"
                });
            }

            ApplyDefines(defines);
        }
    }
}