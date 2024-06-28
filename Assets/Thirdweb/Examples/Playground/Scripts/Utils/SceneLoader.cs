using UnityEngine;

namespace Thirdweb.Unity.Examples
{
    public class SceneLoader : MonoBehaviour
    {
        public void LoadThirdBeats()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Scene_ThirdBeats");
        }
    }
}
