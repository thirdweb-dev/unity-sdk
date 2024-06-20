using System.Collections;
using UnityEngine;

namespace Thirdweb.Unity.Examples
{
    public class CameraShake : MonoBehaviour
    {
        private Coroutine _shakeCoroutine;

        public static CameraShake Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void Shake(float duration, float magnitude)
        {
            if (_shakeCoroutine != null)
            {
                return;
            }

            _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(x, y, originalPosition.z);

                elapsed += Time.deltaTime;

                yield return null;
            }

            transform.localPosition = originalPosition;

            _shakeCoroutine = null;
        }
    }
}
