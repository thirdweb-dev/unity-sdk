using UnityEngine;

namespace Thirdweb.Unity
{
    public class MainThreadExecutorBehaviour : MonoBehaviour
    {
        void Update()
        {
            MainThreadExecutor.ExecuteAllActions();
        }
    }
}
