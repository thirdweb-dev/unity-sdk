using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetaMask.Unity.Samples
{
    [Serializable]
    [CreateAssetMenu(fileName = "Modal Prefab", menuName = "MetaMask/Samples/Modal Prefab")]
    public class ModalPrefabs : ScriptableObject
    {
        #region Fields

         /// <summary>The list of prefab to use for the modals.</summary>
        [SerializeField]
        public List<ModalPrefabData> modalPrefabs;

        #endregion
    }
}
