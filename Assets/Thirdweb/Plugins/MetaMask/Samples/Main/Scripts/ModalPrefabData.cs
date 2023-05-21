using System;

namespace MetaMask.Unity.Samples
{
    [Serializable]
    public partial class ModalPrefabData
    {
        #region Fields

        /// <summary>The prefab identifier.</summary>       
        public ModalData.ModalType prefabIdentifier;
        /// <summary>The prefab to use for the modal UI.</summary>
        public UIModal prefab;

        #endregion
    }
}