using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Thirdweb.Examples
{
    [RequireComponent(typeof(Image))]
    public class SpriteSwap : MonoBehaviour
    {
        public Sprite sprite1;
        public Sprite sprite2;

        private Image image;
        private bool swapped;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        public void Swap()
        {
            if (IsInvoking("Swap"))
                return;

            if (!swapped)
                Invoke("Swap", 3f);

            image.sprite = swapped ? sprite1 : sprite2;

            swapped = !swapped;
        }
    }
}
