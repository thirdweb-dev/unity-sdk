using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Thirdweb.Unity.Examples
{
    public class PlaygroundAnimation : MonoBehaviour
    {
        private Coroutine _coroutine;
        private Dictionary<RectTransform, Vector3> originalPositions = new Dictionary<RectTransform, Vector3>();

        public float animationDuration = 1f; // Duration of the animation

        private void OnEnable()
        {
            // Start the animation coroutine
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            _coroutine = StartCoroutine(Animate());
        }

        private void OnDisable()
        {
            // Stop the animation coroutine and reset positions
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                ResetPositions();
            }
        }

        private IEnumerator Animate()
        {
            // Get the current object's transform
            var rectTransform = GetComponent<RectTransform>();
            var children = rectTransform.GetComponentsInChildren<RectTransform>();

            // Store the original positions and set initial setup
            foreach (var child in children)
            {
                if (child.parent == transform)
                {
                    if (!originalPositions.ContainsKey(child))
                    {
                        originalPositions[child] = child.anchoredPosition;
                    }

                    if (child.TryGetComponent(out CanvasRenderer canvasRenderer))
                    {
                        canvasRenderer.SetAlpha(0);
                        child.anchoredPosition = new Vector2(originalPositions[child].x, originalPositions[child].y - 50);
                    }
                }
                else if (IsChildOfLayoutGroup(child))
                {
                    if (child.TryGetComponent(out CanvasRenderer canvasRenderer))
                    {
                        canvasRenderer.SetAlpha(0);
                    }
                }
            }

            // Animate each top-level child element
            foreach (var child in children)
            {
                if (child.parent == transform && child.TryGetComponent(out CanvasRenderer canvasRenderer))
                {
                    StartCoroutine(FadeInAndMoveUp(child, canvasRenderer));
                }
                else if (IsChildOfLayoutGroup(child) && child.TryGetComponent(out CanvasRenderer childCanvasRenderer))
                {
                    StartCoroutine(FadeInOnly(child, childCanvasRenderer));
                }
            }

            yield return null;
        }

        private IEnumerator FadeInAndMoveUp(RectTransform child, CanvasRenderer canvasRenderer)
        {
            float elapsedTime = 0f;
            Vector3 targetPosition = originalPositions[child];
            Vector3 startPosition = new Vector3(targetPosition.x, targetPosition.y - 50, targetPosition.z);

            while (elapsedTime < animationDuration)
            {
                float alpha = elapsedTime / animationDuration;
                canvasRenderer.SetAlpha(alpha);
                child.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, alpha);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure final state
            canvasRenderer.SetAlpha(1);
            child.anchoredPosition = targetPosition; // Set to original position
        }

        private IEnumerator FadeInOnly(RectTransform child, CanvasRenderer canvasRenderer)
        {
            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                float alpha = elapsedTime / animationDuration;
                canvasRenderer.SetAlpha(alpha);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure final state
            canvasRenderer.SetAlpha(1);
        }

        private void ResetPositions()
        {
            var rectTransform = GetComponent<RectTransform>();
            var children = rectTransform.GetComponentsInChildren<RectTransform>();

            foreach (var child in children)
            {
                if (originalPositions.ContainsKey(child))
                {
                    child.anchoredPosition = originalPositions[child];
                }
            }
        }

        private bool IsChildOfLayoutGroup(Transform child)
        {
            Transform parent = child.parent;
            while (parent != null)
            {
                if (parent.GetComponent<LayoutGroup>() != null)
                {
                    return true;
                }
                parent = parent.parent;
            }
            return false;
        }
    }
}
