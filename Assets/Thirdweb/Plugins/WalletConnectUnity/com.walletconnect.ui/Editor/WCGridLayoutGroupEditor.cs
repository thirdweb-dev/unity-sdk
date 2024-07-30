using UnityEditor;
using UnityEditor.UI;

namespace WalletConnectUnity.UI.Editor
{
    [CustomEditor(typeof(WCGridLayoutGroup))]
    public class WCGridLayoutGroupEditor : GridLayoutGroupEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Responsiveness", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_minSpacingX"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}