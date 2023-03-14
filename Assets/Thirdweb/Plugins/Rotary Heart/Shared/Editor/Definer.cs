using System.Collections.Generic;
using UnityEditor;

namespace RotaryHeart.Lib
{
    public class Definer
    {
        /// <summary>
        /// Applies the defines to the script symbols
        /// </summary>
        /// <param name="defines">List of defines to add</param>
        public static void ApplyDefines(List<string> defines)
        {
            if (defines == null || defines.Count == 0)
            {
                return;
            }

            string availableDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> definesSplit = new List<string>(availableDefines.Split(';'));

            foreach (string define in defines)
            {
                if (!definesSplit.Contains(define))
                {
                    definesSplit.Add(define);
                }
            }

            _ApplyDefine(string.Join(";", definesSplit.ToArray()));
        }

        /// <summary>
        /// Removes the defines from the script symbols
        /// </summary>
        /// <param name="defines">List of defines to remove</param>
        public static void RemoveDefines(List<string> defines)
        {
            if (defines == null || defines.Count == 0)
                return;

            string availableDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> definesSplit = new List<string>(availableDefines.Split(';'));

            foreach (string define in defines)
            {
                definesSplit.Remove(define);
            }

            _ApplyDefine(string.Join(";", definesSplit.ToArray()));
        }

        /// <summary>
        /// Returns true if a define is already defined
        /// </summary>
        /// <param name="define">Define to check</param>
        public static bool ContainsDefine(string define)
        {
            if (string.IsNullOrEmpty(define))
            {
                return false;
            }
            
            string availableDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> definesSplit = new List<string>(availableDefines.Split(';'));

            return definesSplit.Contains(define);
        }

        /// <summary>
        /// Actual logic that applies the defines symbols
        /// </summary>
        /// <param name="define">List of defines to save, this includes the already defined ones</param>
        static void _ApplyDefine(string define)
        {
            if (string.IsNullOrEmpty(define))
            {
                return;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, define);
        }
    }
    
}
