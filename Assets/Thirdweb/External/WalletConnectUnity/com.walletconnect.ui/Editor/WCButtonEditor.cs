using UnityEditor;
using UnityEditor.UI;

namespace WalletConnectUnity.UI.Editor
{
    [CustomEditor(typeof(WCButton))]
    public class WCButtonEditor : ButtonEditor
    {
        private SerializedProperty _backgroundProperty;
        private SerializedProperty _borderProperty;
        private SerializedProperty _ringProperty;

        private SerializedProperty _normalConfigProperty;
        private SerializedProperty _highlightedConfigProperty;
        private SerializedProperty _selectedConfigProperty;
        private SerializedProperty _pressedConfigProperty;

        private SerializedProperty _onClickProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            _backgroundProperty = serializedObject.FindProperty("_background");
            _borderProperty = serializedObject.FindProperty("_border");
            _ringProperty = serializedObject.FindProperty("_ring");

            _normalConfigProperty = serializedObject.FindProperty("_normalConfig");
            _highlightedConfigProperty = serializedObject.FindProperty("_highlightedConfig");
            _selectedConfigProperty = serializedObject.FindProperty("_selectedConfig");
            _pressedConfigProperty = serializedObject.FindProperty("_pressedConfig");

            _onClickProperty = serializedObject.FindProperty("m_OnClick");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Image References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_backgroundProperty);
            EditorGUILayout.PropertyField(_borderProperty);
            EditorGUILayout.PropertyField(_ringProperty);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("States", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_normalConfigProperty);
            EditorGUILayout.PropertyField(_highlightedConfigProperty);
            EditorGUILayout.PropertyField(_selectedConfigProperty);
            EditorGUILayout.PropertyField(_pressedConfigProperty);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_onClickProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}