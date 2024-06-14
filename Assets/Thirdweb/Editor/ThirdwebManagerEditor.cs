using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Thirdweb
{
    [CustomEditor(typeof(ThirdwebManager))]
    public class ThirdwebManagerEditor : Editor
    {
        private SerializedProperty activeChainProperty;
        private SerializedProperty supportedChainsProperty;
        private SerializedProperty clientIdProperty;
        private SerializedProperty bundleIdOverrideProperty;
        private SerializedProperty initializeOnAwakeProperty;
        private SerializedProperty showDebugLogsProperty;
        private SerializedProperty thirdwebConfigProperty;
        private SerializedProperty appNameProperty;
        private SerializedProperty appDescriptionProperty;
        private SerializedProperty appIconsProperty;
        private SerializedProperty appUrlProperty;
        private SerializedProperty storageIpfsGatewayUrlProperty;
        private SerializedProperty relayerUrlProperty;
        private SerializedProperty forwarderAddressProperty;
        private SerializedProperty forwarderDomainOverrideProperty;
        private SerializedProperty forwaderVersionOverrideProperty;
        private SerializedProperty walletConnectProjectIdProperty;
        private SerializedProperty walletConnectEnableExplorerProperty;
        private SerializedProperty walletConnectExplorerRecommendedWalletIdsProperty;
        private SerializedProperty walletConnectWalletImagesProperty;
        private SerializedProperty walletConnectDesktopWalletsProperty;
        private SerializedProperty walletConnectMobileWalletsProperty;
        private SerializedProperty walletConnectThemeModeProperty;
        private SerializedProperty factoryAddressProperty;
        private SerializedProperty gaslessProperty;
        private SerializedProperty erc20PaymasterAddressProperty;
        private SerializedProperty erc20TokenAddressProperty;
        private SerializedProperty bundlerUrlProperty;
        private SerializedProperty paymasterUrlProperty;
        private SerializedProperty entryPointAddressProperty;
        private SerializedProperty WalletConnectPrefabProperty;
        private SerializedProperty MetamaskPrefabProperty;
        private SerializedProperty InAppWalletPrefabProperty;

        private ReorderableList supportedChainsList;
        private bool[] sectionExpanded;
        private bool showDangerZone = false;
        private bool showGaslessOptionalFields = false;
        private bool showSmartWalletOptionalFields = false;
        private GUIStyle dangerZoneStyle;
        private GUIContent warningIcon;
        private Texture2D bannerImage;

        private static readonly string ExpandedStateKey = "ThirdwebManagerEditor_ExpandedState_4.16.4";
        private static readonly string OptionalStateKey = "ThirdwebManagerEditor_OptionalState_4.16.4";

        private void OnEnable()
        {
            activeChainProperty = serializedObject.FindProperty("activeChain");
            supportedChainsProperty = serializedObject.FindProperty("supportedChains");
            clientIdProperty = serializedObject.FindProperty("clientId");
            bundleIdOverrideProperty = serializedObject.FindProperty("bundleIdOverride");
            initializeOnAwakeProperty = serializedObject.FindProperty("initializeOnAwake");
            showDebugLogsProperty = serializedObject.FindProperty("showDebugLogs");
            thirdwebConfigProperty = serializedObject.FindProperty("thirdwebConfig");
            appNameProperty = serializedObject.FindProperty("appName");
            appDescriptionProperty = serializedObject.FindProperty("appDescription");
            appIconsProperty = serializedObject.FindProperty("appIcons");
            appUrlProperty = serializedObject.FindProperty("appUrl");
            storageIpfsGatewayUrlProperty = serializedObject.FindProperty("storageIpfsGatewayUrl");
            relayerUrlProperty = serializedObject.FindProperty("relayerUrl");
            forwarderAddressProperty = serializedObject.FindProperty("forwarderAddress");
            forwarderDomainOverrideProperty = serializedObject.FindProperty("forwarderDomainOverride");
            forwaderVersionOverrideProperty = serializedObject.FindProperty("forwaderVersionOverride");
            walletConnectProjectIdProperty = serializedObject.FindProperty("walletConnectProjectId");
            walletConnectEnableExplorerProperty = serializedObject.FindProperty("walletConnectEnableExplorer");
            walletConnectExplorerRecommendedWalletIdsProperty = serializedObject.FindProperty("walletConnectExplorerRecommendedWalletIds");
            walletConnectWalletImagesProperty = serializedObject.FindProperty("walletConnectWalletImages");
            walletConnectDesktopWalletsProperty = serializedObject.FindProperty("walletConnectDesktopWallets");
            walletConnectMobileWalletsProperty = serializedObject.FindProperty("walletConnectMobileWallets");
            walletConnectThemeModeProperty = serializedObject.FindProperty("walletConnectThemeMode");
            factoryAddressProperty = serializedObject.FindProperty("factoryAddress");
            gaslessProperty = serializedObject.FindProperty("gasless");
            erc20PaymasterAddressProperty = serializedObject.FindProperty("erc20PaymasterAddress");
            erc20TokenAddressProperty = serializedObject.FindProperty("erc20TokenAddress");
            bundlerUrlProperty = serializedObject.FindProperty("bundlerUrl");
            paymasterUrlProperty = serializedObject.FindProperty("paymasterUrl");
            entryPointAddressProperty = serializedObject.FindProperty("entryPointAddress");
            WalletConnectPrefabProperty = serializedObject.FindProperty("WalletConnectPrefab");
            MetamaskPrefabProperty = serializedObject.FindProperty("MetamaskPrefab");
            InAppWalletPrefabProperty = serializedObject.FindProperty("InAppWalletPrefab");

            supportedChainsList = new ReorderableList(serializedObject, supportedChainsProperty, true, true, true, true);
            supportedChainsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.1f, rect.y, rect.width * 0.3f, rect.height), "Identifier", EditorStyles.miniBoldLabel);
                EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.45f, rect.y, rect.width * 0.3f, rect.height), "Chain ID", EditorStyles.miniBoldLabel);
                EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.775f, rect.y, rect.width * 0.3f, rect.height), "RPC Override", EditorStyles.miniBoldLabel);
            };

            supportedChainsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = supportedChainsProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                Rect identifierRect = new Rect(rect.x, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
                Rect chainIdRect = new Rect(rect.x + rect.width * 0.35f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
                Rect rpcOverrideRect = new Rect(rect.x + rect.width * 0.7f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);

                EditorGUI.BeginProperty(rect, GUIContent.none, element);

                EditorGUI.LabelField(identifierRect, "Identifier");
                EditorGUI.PropertyField(identifierRect, element.FindPropertyRelative("identifier"), GUIContent.none);

                EditorGUI.LabelField(chainIdRect, "Chain ID");
                EditorGUI.PropertyField(chainIdRect, element.FindPropertyRelative("chainId"), GUIContent.none);

                var rpcOverrideProperty = element.FindPropertyRelative("rpcOverride");
                EditorGUI.LabelField(rpcOverrideRect, "RPC Override (Optional)");
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rpcOverrideRect, rpcOverrideProperty, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    rpcOverrideProperty.stringValue = rpcOverrideProperty.stringValue.Trim();
                }

                if (string.IsNullOrEmpty(rpcOverrideProperty.stringValue))
                {
                    GUIStyle italicStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter };
                    EditorGUI.LabelField(rpcOverrideRect, "(Optional)", italicStyle);
                }

                EditorGUI.EndProperty();
            };

            warningIcon = EditorGUIUtility.IconContent("console.warnicon.sml");
            bannerImage = Resources.Load<Texture2D>("EditorBanner");

            sectionExpanded = GetExpandedState();

            showGaslessOptionalFields = EditorPrefs.GetBool($"{OptionalStateKey}_showGaslessOptionalFields", false);
            showSmartWalletOptionalFields = EditorPrefs.GetBool($"{OptionalStateKey}_showSmartWalletOptionalFields", false);
        }

        private void OnDisable()
        {
            SetExpandedState(sectionExpanded);
            EditorPrefs.SetBool($"{OptionalStateKey}_showGaslessOptionalFields", showGaslessOptionalFields);
            EditorPrefs.SetBool($"{OptionalStateKey}_showSmartWalletOptionalFields", showSmartWalletOptionalFields);
        }

        public override void OnInspectorGUI()
        {
            dangerZoneStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.yellow },
                fontSize = 14,
                margin = new RectOffset(0, 0, 8, 4),
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter
            };

            serializedObject.Update();

            EditorGUILayout.Space();

            // Banner
            GUILayout.BeginHorizontal();
            GUILayout.Label(bannerImage, GUILayout.Width(80), GUILayout.Height(80));
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.Label("Thirdweb Manager", EditorStyles.boldLabel);
            GUILayout.Label("Customize your Thirdweb SDK settings here.", EditorStyles.miniBoldLabel);
            GUILayout.Label("Accessible from anywhere - ThirdwebManager.Instance.SDK", EditorStyles.miniLabel);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Required Settings
            sectionExpanded[0] = DrawSectionWithExpand(
                "General Settings",
                sectionExpanded[0],
                () =>
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(activeChainProperty);
                    EditorGUILayout.Space(10);
                    supportedChainsList.DoLayoutList();
                    EditorGUILayout.PropertyField(clientIdProperty);
                    EditorGUILayout.PropertyField(bundleIdOverrideProperty);
                    EditorGUILayout.PropertyField(initializeOnAwakeProperty);
                    EditorGUILayout.PropertyField(showDebugLogsProperty);

                    // Draw the ThirdwebConfig property as a read-only field
                    ThirdwebConfig thirdwebConfig = Resources.Load<ThirdwebConfig>("ThirdwebConfig");
                    if (thirdwebConfig != null)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField("Thirdweb Config", thirdwebConfig, typeof(ThirdwebConfig), false);
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("No ThirdwebConfig asset found in Resources. Please create a ThirdwebConfig asset.", MessageType.Warning);

                        if (GUILayout.Button("Create ThirdwebConfig in Resources"))
                        {
                            ThirdwebConfig newConfig = ScriptableObject.CreateInstance<ThirdwebConfig>();

                            string directoryPath = "Assets/Thirdweb/Resources";
                            if (!AssetDatabase.IsValidFolder(directoryPath))
                            {
                                if (!AssetDatabase.IsValidFolder("Assets/Thirdweb"))
                                {
                                    AssetDatabase.CreateFolder("Assets", "Thirdweb");
                                }
                                AssetDatabase.CreateFolder("Assets/Thirdweb", "Resources");
                            }

                            string assetPath = directoryPath + "/ThirdwebConfig.asset";
                            AssetDatabase.CreateAsset(newConfig, assetPath);
                            AssetDatabase.SaveAssets();
                        }
                    }

                    EditorGUILayout.EndVertical();
                }
            );

            EditorGUILayout.Space();

            // App Metadata
            sectionExpanded[1] = DrawSectionWithExpand(
                "App Metadata",
                sectionExpanded[1],
                () =>
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(appNameProperty);
                    EditorGUILayout.PropertyField(appDescriptionProperty);
                    EditorGUILayout.PropertyField(appIconsProperty, true);
                    EditorGUILayout.PropertyField(appUrlProperty);
                    EditorGUILayout.EndVertical();
                }
            );

            EditorGUILayout.Space();

            // Storage Options
            sectionExpanded[2] = DrawSectionWithExpand(
                "Storage Options",
                sectionExpanded[2],
                () =>
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(storageIpfsGatewayUrlProperty);
                    EditorGUILayout.EndVertical();
                }
            );

            EditorGUILayout.Space();

            // OZ Defender Options
            sectionExpanded[3] = DrawSectionWithExpand(
                "Gasless Relayer Options",
                sectionExpanded[3],
                () =>
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(relayerUrlProperty);

                    EditorGUI.BeginChangeCheck();
                    showGaslessOptionalFields = EditorGUILayout.ToggleLeft("Show Optional Fields", showGaslessOptionalFields);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(target);
                    }

                    if (showGaslessOptionalFields)
                    {
                        EditorGUILayout.PropertyField(forwarderAddressProperty);
                        EditorGUILayout.PropertyField(forwarderDomainOverrideProperty);
                        EditorGUILayout.PropertyField(forwaderVersionOverrideProperty);
                    }

                    EditorGUILayout.EndVertical();
                }
            );

            EditorGUILayout.Space();

            // Wallet Connect Options
            sectionExpanded[4] = DrawSectionWithExpand(
                "Wallet Connect Options",
                sectionExpanded[4],
                () =>
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(walletConnectProjectIdProperty);
                    EditorGUILayout.PropertyField(walletConnectEnableExplorerProperty);
                    EditorGUILayout.PropertyField(walletConnectExplorerRecommendedWalletIdsProperty);
                    EditorGUILayout.PropertyField(walletConnectWalletImagesProperty, true);
                    EditorGUILayout.PropertyField(walletConnectDesktopWalletsProperty, true);
                    EditorGUILayout.PropertyField(walletConnectMobileWalletsProperty, true);
                    EditorGUILayout.PropertyField(walletConnectThemeModeProperty);
                    EditorGUILayout.EndVertical();
                }
            );

            EditorGUILayout.Space();

            // Smart Wallet Options
            sectionExpanded[5] = DrawSectionWithExpand(
                "Smart Wallet Options",
                sectionExpanded[5],
                () =>
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(gaslessProperty);

                    EditorGUI.BeginChangeCheck();
                    showSmartWalletOptionalFields = EditorGUILayout.ToggleLeft("Show Optional Fields", showSmartWalletOptionalFields);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(target);
                    }

                    if (showSmartWalletOptionalFields)
                    {
                        EditorGUILayout.PropertyField(factoryAddressProperty);
                        EditorGUILayout.PropertyField(erc20PaymasterAddressProperty);
                        EditorGUILayout.PropertyField(erc20TokenAddressProperty);
                        EditorGUILayout.PropertyField(bundlerUrlProperty);
                        EditorGUILayout.PropertyField(paymasterUrlProperty);
                        EditorGUILayout.PropertyField(entryPointAddressProperty);
                    }

                    EditorGUILayout.EndVertical();
                }
            );

            EditorGUILayout.Space();

            // Native Prefabs (Danger Zone)
            DrawSectionWithoutExpand(
                "",
                () =>
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(warningIcon, GUILayout.Width(20), GUILayout.Height(20));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Label("DANGER ZONE", dangerZoneStyle);

                    GUILayout.Space(10);

                    Color originalColor = GUI.backgroundColor; // Store the original button color

                    GUI.backgroundColor = new Color(0.9f, 0.6f, 0.6f);
                    if (GUILayout.Button(showDangerZone ? "Hide" : "Reveal", GUILayout.Height(30)))
                    {
                        showDangerZone = !showDangerZone;
                    }

                    GUI.backgroundColor = originalColor; // Restore the original button color

                    if (showDangerZone)
                    {
                        EditorGUILayout.PropertyField(WalletConnectPrefabProperty);
                        EditorGUILayout.PropertyField(MetamaskPrefabProperty);
                        EditorGUILayout.PropertyField(InAppWalletPrefabProperty);
                    }

                    EditorGUILayout.EndVertical();
                }
            );

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSectionWithoutExpand(string title, System.Action drawContent)
        {
            GUIStyle sectionTitleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(0, 0, 10, 4),
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.LowerLeft,
            };

            EditorGUILayout.LabelField(title, sectionTitleStyle);
            EditorGUILayout.Space(3);

            drawContent?.Invoke();

            EditorGUILayout.Space();
        }

        private bool DrawSectionWithExpand(string title, bool expanded, System.Action drawContent)
        {
            GUIStyle sectionTitleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(0, 0, 10, 4),
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.LowerLeft,
            };

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;
            expanded = EditorGUILayout.ToggleLeft(title, expanded, sectionTitleStyle);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndHorizontal();

            if (expanded)
            {
                EditorGUILayout.Space(3);
                drawContent?.Invoke();
                EditorGUILayout.Space(3);
            }

            EditorGUILayout.EndVertical();

            return expanded;
        }

        private bool[] GetExpandedState()
        {
            string expandedState = EditorPrefs.GetString(ExpandedStateKey, string.Empty);
            if (!string.IsNullOrEmpty(expandedState))
            {
                string[] stateArray = expandedState.Split(',');
                bool[] expanded = new bool[stateArray.Length];
                for (int i = 0; i < stateArray.Length; i++)
                {
                    bool.TryParse(stateArray[i], out expanded[i]);
                }
                return expanded;
            }
            else
            {
                var states = new bool[6];
                states[0] = true;
                return states;
            }
        }

        private void SetExpandedState(bool[] expandedState)
        {
            string stateString = string.Join(",", expandedState);
            EditorPrefs.SetString(ExpandedStateKey, stateString);
        }
    }
}
