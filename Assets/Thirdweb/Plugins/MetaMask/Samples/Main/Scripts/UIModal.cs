using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MetaMask.Unity.Samples
{

    public class UIModal : MonoBehaviour
    {

        #region Events

        /// <summary>Occurs when the window is closed.</summary>
        public event EventHandler Closed;

        #endregion

        #region Fields

        [Header("References")]
        /// <summary>The text component that displays the title.</summary>
        [SerializeField]
        protected TextMeshProUGUI titleText;
        /// <summary>The text field that displays the content of the modal.</summary>
        [SerializeField]
        protected TextMeshProUGUI contentText;
        /// <summary>The close button.</summary>       
        [SerializeField]
        protected Button closeButton;

        [Header("Parameters")]
        /// <summary>The time in seconds that the object has been alive.</summary>
        public float AliveTime;
        /// <summary>The delay after which the popup closes.</summary>
        public float CloseDelay = 0.0f;

        #endregion

        #region Protected Methods

        /// <summary>Starts the timer for the object appearance.</summary>
        private IEnumerator AppearTimer()
        {
            yield return new WaitForSeconds(this.AliveTime);
            OnCloseButton();
        }

        #endregion

        #region Protected Methods

        /// <summary>Triggered when the Modal is opened.</summary>
        protected void OnOpen(string headerText, string bodyText)
        {
            this.titleText.text = headerText;
            this.contentText.text = bodyText;
            GetComponentInChildren<TweenIn>().Open();
            StartCoroutine(AppearTimer());
        }

        #endregion

        #region Public Methods

        /// <summary>Opens the modal.</summary>
        public void Open(ModalData data)
        {
            OnOpen(data.headerText, data.bodyText);
            data.headerText = data.headerText ?? string.Empty;
            data.bodyText = data.bodyText ?? string.Empty;
        }

        /// <summary>Closes the modal.</summary>
        public void Close()
        {
            UIModalManager.Instance.CloseModal(this);
        }

        /// <summary>Closes the modal.</summary>
        public async void OnCloseButton()
        {
            StopAllCoroutines();
            GetComponentInChildren<TweenIn>().Close();
            await Task.Delay((int)this.CloseDelay);
            Closed?.Invoke(this, EventArgs.Empty);
            Close();
        }

        #endregion

    }

}