using UnityEngine;

namespace Thirdweb.Unity.Examples
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Tile : MonoBehaviour
    {
        private float fallSpeed;

        public void Initialize(float speed)
        {
            fallSpeed = speed;
        }

        private void Update()
        {
            // Normal downward movement
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            if (transform.position.y < -5f)
            {
                MusicGameManager.Instance.Score -= 50;
                CameraShake.Instance.Shake(0.1f, 0.1f);
                Destroy(gameObject); // Destroy tile when it goes off-screen
            }
        }
    }
}
