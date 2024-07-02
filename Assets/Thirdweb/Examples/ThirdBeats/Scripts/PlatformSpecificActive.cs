using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

namespace Thirdweb.Unity.Examples
{
    public class PlatformSpecificActive : MonoBehaviour
    {
        [SerializeField]
        public SerializableDictionaryBase<RuntimePlatform, bool> platformActive = new();

        private void Awake()
        {
            gameObject.SetActive(platformActive[Application.platform]);
        }
    }
}
