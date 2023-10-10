using UnityEngine;

namespace Thirdweb
{
    [CreateAssetMenu(fileName = "ThirdwebConfig", menuName = "Thirdweb/Configuration", order = 1)]
    public class ThirdwebConfig : ScriptableObject
    {
        [Header("When using OAuth2 (e.g. Google) to login on mobile, you can provide a redirect URL such as 'myapp://'.\nMake sure you do a Clean Build every time you update it.")]
        public string customScheme;
    }
}
