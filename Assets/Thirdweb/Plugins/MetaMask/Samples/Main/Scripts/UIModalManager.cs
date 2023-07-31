using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetaMask.Unity.Samples
{

    public class UIModalManager : MonoBehaviour
    {

        #region Constants

        /// <summary>The path to the resources folder.</summary>       
        protected const string ResourcesPath = "MetaMask/Modals";
        /// <summary>The path to the modal alert folder.</summary>     
        protected const string AlertIdentifier = "MetaMask/Alert";
        /// <summary>The path to the modal prompt folder.</summary>     
        protected const string PromptIdentifier = "MetaMask/Prompt";

        #endregion

        #region Event

        /// <summary>Occurs when the modal dialog is opening.</summary>
        public event EventHandler ModalOpening;
        /// <summary>Occurs when the modal dialog is opened.</summary>
        public event EventHandler ModalOpened;
        /// <summary>Raised when the modal dialog is closed.</summary>
        public event EventHandler ModalClosed;

        #endregion

        #region Properties

        /// <summary>Gets the singleton instance of the <see cref="UIModalManager"/> class.</summary>
        /// <returns>The singleton instance of the <see cref="UIModalManager"/> class.</returns>
        public static UIModalManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObject.FindObjectOfType<UIModalManager>();
                }
                return instance;
            }
        }

        #endregion

        #region Fields

        /// <summary>Gets the singleton instance of the <see cref="UIModalManager"/>.</summary>
        protected static UIModalManager instance;
        /// <summary>The prefab to use for modal dialogs.</summary>
        public GameObject ModalPrefab;
        /// <summary>The texture of the button.</summary>
        public Texture btnTexture;
        /// <summary>The modal prefab holder.</summary>
        [SerializeField] protected ModalPrefabs modalPrefabHolder;
        /// <summary>A list of the modals that are currently active.</summary>
        protected List<UIModal> activeModals = new List<UIModal>();
        /// <summary>Prefab cache to be used at runtime.</summary>
        protected Dictionary<ModalData.ModalType, GameObject> prefabCache = new Dictionary<ModalData.ModalType, GameObject>();

        #endregion

        #region Unity Messages

        /// <summary>Awake is called when the script instance is being loaded.</summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                FillPrefabCache();
                DontDestroyOnLoad(transform.root);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>Fills the prefab cache.</summary>
        private void FillPrefabCache()
        {
            if (this.prefabCache.Count > 0)
            {
                this.prefabCache.Clear();
            }
            foreach (var modal in this.modalPrefabHolder.modalPrefabs)
            {
                this.prefabCache.Add(modal.prefabIdentifier, modal.prefab.gameObject);
            }
        }

        /// <summary>Opens a modal window.</summary>
        /// <param name="parameters">The parameters to pass to the modal window.</param>
        /// <exception cref="InvalidOperationException">Thrown when the application isn't in foreground.</exception>
        public void OpenModal(ModalData parameters = null)
        {
            ModalOpening?.Invoke(this, EventArgs.Empty);

            var prefab = GetModalPrefab(parameters.type);
            var modalWindowObj = Instantiate(prefab, transform.position, Quaternion.identity);
            var modalWindow = modalWindowObj.GetComponentInChildren<UIModal>();

            this.activeModals.Add(modalWindow);
            modalWindow.Open(parameters);
            ModalOpened?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Closes a modal window.</summary>
        /// <param name="modalWindow">The modal window to close.</param>
        public void CloseModal(UIModal modalWindow)
        {
            ModalClosed?.Invoke(this, EventArgs.Empty);
            this.activeModals.Remove(modalWindow);
            if (modalWindow != null)
            {
                Destroy(modalWindow.transform.root.gameObject);
            }
        }

        /// <summary>Gets the modal prefab for the given identifier.</summary>
        /// <param name="identifier">The identifier of the modal prefab to get.</param>
        /// <returns>The modal prefab for the given identifier.</returns>
        public GameObject GetModalPrefab(ModalData.ModalType identifier)
        {
            GameObject prefab;
            if (this.prefabCache.TryGetValue(identifier, out prefab))
            {
                return prefab;
            }
            if (prefab != null)
            {
                this.prefabCache[identifier] = prefab;
            }
            return prefab;
        }

        #endregion


    }

}