using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Thirdweb.Examples
{
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldEnter : MonoBehaviour
    {
        public Button SubmitButton;

        private TMP_InputField _inputField;

        private void Awake()
        {
            _inputField = GetComponent<TMP_InputField>();
            _inputField.onSubmit.AddListener((s) => SubmitButton.onClick.Invoke());
        }
    }
}
