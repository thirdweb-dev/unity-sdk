using UnityEditor;

namespace WalletConnectUnity.UI.Editor
{
    [CustomEditor(typeof(WCListSelect))]
    public class WCListSelectEditor : WCButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("List Select", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_title"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_icon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_iconBorder"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_installedLabelObject"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_defaultBorderColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_tagText"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}