using UnityEngine;
using UnityEditor;

namespace Thirdweb
{
    public class ThirdwebSDKInstallerPopup : EditorWindow
    {
        #region Private Fields

        private Texture2D bannerImage;
        private GUIStyle headerStyle;
        private GUIStyle descriptionStyle;
        private GUIStyle codeStyle;

        #endregion

        #region Editor Window Setup

        [MenuItem("Tools/Thirdweb/Thirdweb Unity SDK Setup")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<ThirdwebSDKInstallerPopup>("Thirdweb Unity SDK Setup").minSize = new Vector2(450, 600);
        }

        #endregion

        #region GUI Styles & Resources

        void LoadResourcesAndStyles()
        {
            if (bannerImage == null)
            {
                bannerImage = Resources.Load<Texture2D>("EditorBanner");
            }

            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleLeft,
                    margin = new RectOffset(0, 0, 20, 10)
                };
            }

            if (descriptionStyle == null)
            {
                descriptionStyle = new GUIStyle(EditorStyles.wordWrappedLabel) { margin = new RectOffset(5, 5, 10, 5), alignment = TextAnchor.MiddleLeft };
            }

            if (codeStyle == null)
            {
                codeStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold,
                    border = new RectOffset(2, 2, 2, 2),
                    padding = new RectOffset(5, 5, 2, 2),
                    normal = { textColor = Color.cyan }
                };
            }
        }

        #endregion

        #region OnGUI Implementation

        void OnGUI()
        {
            LoadResourcesAndStyles();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            DrawBanner();

            GUILayout.Space(20);

            DrawDocumentationSection();

            GUILayout.Space(20);

            DrawClientIDSection();

            GUILayout.Space(20);

            DrawPrefabSection();

            GUILayout.Space(30);

            DrawFinishSetupButton();

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawBanner()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(bannerImage, GUILayout.Width(80), GUILayout.Height(80));
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Label("Thirdweb Unity SDK", headerStyle);
            GUILayout.Label("Quick Setup", EditorStyles.miniBoldLabel);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawDocumentationSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width(380));
            EditorGUILayout.LabelField("Read the official documentation to get started and understand more about the SDK's capabilities.", descriptionStyle, GUILayout.Width(380));
            if (GUILayout.Button("View Documentation", GUILayout.Height(40)))
            {
                Application.OpenURL("https://portal.thirdweb.com/unity");
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawClientIDSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width(380));
            EditorGUILayout.LabelField(
                "To access thirdweb services such as Storage, RPC, and Account Abstraction, you need a Client ID. For WebGL builds, whitelist your domain. For Native builds, include your Bundle ID.",
                descriptionStyle,
                GUILayout.Width(380)
            );
            if (GUILayout.Button("Get Client ID", GUILayout.Height(40)))
            {
                Application.OpenURL("https://thirdweb.com/create-api-key");
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawPrefabSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width(380));
            EditorGUILayout.LabelField(
                "The ThirdwebManager is a MonoBehaviour that offers a streamlined way to instantiate and control the ThirdwebSDK. Access the SDK universally using: ",
                descriptionStyle,
                GUILayout.Width(380)
            );

            EditorGUILayout.LabelField("ThirdwebManager.Instance.SDK", codeStyle, GUILayout.Width(380));

            if (GUILayout.Button("Add ThirdwebManager to Scene", GUILayout.Height(40)))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Thirdweb/Core/Prefabs/ThirdwebManager.prefab");
                if (prefab)
                {
                    Instantiate(prefab);
                }
                else
                {
                    Debug.LogError("Failed to find ThirdwebManager prefab. Please ensure it's in the correct path.");
                }
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawFinishSetupButton()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Finish Setup", GUILayout.Height(50), GUILayout.Width(250)))
            {
                EditorPrefs.SetBool("ThirdwebSDK_SetupCompleted", true);
                Close();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        #endregion
    }

    [InitializeOnLoad]
    public class ThirdwebSDKInitializer
    {
        static ThirdwebSDKInitializer()
        {
            EditorApplication.update += RunOnce;
        }

        static void RunOnce()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorPrefs.HasKey("ThirdwebSDK_ShownPopup"))
                {
                    ThirdwebSDKInstallerPopup.ShowWindow();
                    EditorPrefs.SetBool("ThirdwebSDK_ShownPopup", true);
                }
                EditorApplication.update -= RunOnce;
            };
        }
    }
}
