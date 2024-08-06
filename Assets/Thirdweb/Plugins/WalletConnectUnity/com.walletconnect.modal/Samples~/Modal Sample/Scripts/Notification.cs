using TMPro;
using UnityEngine;

namespace WalletConnectUnity.Modal.Sample
{
    public class Notification : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _messageText;

        public static Notification Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Notification>(true);
                }

                return _instance;
            }
        }

        private static Notification _instance;

        public static void ShowMessage(string message)
        {
            Instance.Show(message);
        }

        public void Show(string message)
        {
            Debug.Log(message);

            _messageText.text = message;
            _root.SetActive(true);
        }

        public void OnButtonHide()
        {
            _root.SetActive(false);
        }
    }
}