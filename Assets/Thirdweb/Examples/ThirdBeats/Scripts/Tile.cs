using UnityEngine;

namespace Thirdweb.Unity.Examples
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Tile : MonoBehaviour
    {
        private float fallSpeed;
        private bool isBeingDestroyed;

        public void Initialize(float speed)
        {
            fallSpeed = speed;
            isBeingDestroyed = false;
        }

        private void Update()
        {
            if (isBeingDestroyed)
            {
                return;
            }

            // Normal downward movement
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            if (transform.position.y < -5f)
            {
                MusicGameManager.Instance.Score -= 10;
                CameraShake.Instance.Shake(0.1f, 0.1f);
                Destroy(gameObject); // Destroy tile when it goes off-screen
                isBeingDestroyed = true;
            }
        }
    }
}
