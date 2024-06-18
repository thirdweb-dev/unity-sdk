using TMPro.EditorUtilities;
using UnityEditor;

namespace WalletConnectUnity.UI.Editor
{
    [CustomEditor(typeof(WCInputField))]
    public class WCInputFieldEditor : TMP_InputFieldEditor
    {
        private bool _showProperties = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            _showProperties = EditorGUILayout.Foldout(_showProperties, "WalletConnect InputField Properties");

            if (_showProperties)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_backgroundImage"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_borderImage"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_ringImage"));

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("States", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_defaultState"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_selectedState"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_highlightedState"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}