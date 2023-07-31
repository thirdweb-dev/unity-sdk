using UnityEngine;

namespace MetaMask.Unity.Samples
{
    public class LookAtCamera : MonoBehaviour
    {
        #region Fields

        /// <summary>The target transform.</summary>       
        [SerializeField]
        private Transform target;
        /// <summary>The camera used to render the scene.</summary>       
        [SerializeField]
        private Camera _camera;
        /// <summary>The speed at which the object moves.</summary>       
        [SerializeField]
        private float speed;

        #endregion

        #region Unity Methods

        /// <summary>Updates the target's position.</summary>
        private void Update()
        {
            Vector3 targetPos = this.target.position;
            float camZ = this._camera.transform.position.z;
            targetPos = this._camera.ScreenToWorldPoint(Input.mousePosition + (Vector3.back * camZ)) * this.speed;
            this.target.LookAt(targetPos, Vector3.up);
        }

        #endregion
    }
}