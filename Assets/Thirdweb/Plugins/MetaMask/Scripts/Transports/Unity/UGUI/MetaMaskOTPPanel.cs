using MetaMask.Unity;
using TMPro;
using UnityEngine;

namespace MetaMask.Transports.Unity.UI
{
    public class MetaMaskOTPPanel : MonoBehaviour
    {
        public TextMeshProUGUI codeText;

        public void OnDisconnect()
        {
            MetaMaskUnity.Instance.EndSession();
            
            gameObject.SetActive(false);
        }

        public void ShowOTP(int code)
        {
            codeText.text = code.ToString();

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
    }
}