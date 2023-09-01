using UnityEditor;

using UnityEngine;

using MetaMask.Transports.Unity.UI;

namespace MetaMask.Unity
{
    public class MetaMaskWindow : EditorWindow
    {
        #region Consts
        /// <summary>The path to the header Image.</summary>
        private const string _headerImagePath = "MetaMask/EditorImages/MetaMask_Header_Logo";
        /// <summary>The path to the logo Image.</summary>
        private const string _metamaskLogoImagePath = "MetaMask/EditorImages/Metamask_Stacked_Logo";
        /// <summary>The path to the background Image.</summary>
        private const string _backgroundImagePath = "MetaMask/EditorImages/MetaMask_EditorWindow_BG";
        /// <summary>The path to the background Image.</summary>
        private const string _buttonImagePath = "MetaMask/EditorImages/MetaMask_Button";

        #endregion

        #region Fields

        /// <summary>The current state of the MetaMask Editor UI Window.</summary>
        enum MetaMaskState
        {
            main,
            install,
            connect
        }

        /// <summary>The current state of the MetaMask client.</summary>       
        private MetaMaskState _state = MetaMaskState.main;
        /// <summary>The style for the header of the main window.</summary>
        private GUIStyle _headerStyle;
        /// <summary>The style for the MetaMask logo.</summary>       
        private GUIStyle _metamaskLogoStyle;
        /// <summary>The style used for text that is displayed in the main window.</summary>
        private GUIStyle _higherTextStyle;
        /// <summary>The style for the H2 text.</summary>
        private GUIStyle _h2TextStyle;
        /// <summary>The style for the body text.</summary>
        private GUIStyle _bodyTextStyle;
        /// <summary>The style for the button style.</summary>
        private GUIStyle _buttonStyle;
        /// <summary>The style for the input field.</summary>
        private GUIStyle _inputFieldStyle;
        /// <summary>The style for the input toggle.</summary>
        private GUIStyle _inputToggleStyle;
        /// <summary>The style for the side-by-side view.</summary>
        private GUIStyle _sidebySideStyle;
        /// <summary>The style for the small Header view.</summary>
        private GUIStyle _smallHeaderStyle;

        /// <summary>The last y-coordinate of the pointer.</summary>
        private float _lastYPosition;
        /// <summary>Gets the name of the application.</summary>
        /// <returns>The name of the application.</returns>
        private string _appNameText = "App Name";
        /// <summary>The text to display in the app URL field.</summary>
        private string _appUrlText = "App Url";
        /// <summary>Gets the user agent string for the current application.</summary>
        /// <returns>The user agent string for the current application.</returns>
        private string _appUserAgentText = "User Agent";
        /// <summary>The text to display in the Encryption Password field.</summary>
        private string _encryptionPasswordText = "Encryption Password";
        /// <summary>The text to display in the Session Identifier field.</summary>
        private string _sessionIdentifierText = "Session Identifier";
        /// <summary>The text to display in the Socket URL field.</summary>
        private string _socketURLText = "Socket Url";
        /// <summary>Gets or sets a value indicating whether the application is in deep linking mode.</summary>
        private bool _deepLinking;
        /// <summary>Gets or sets a value indicating whether the application is in debug mode.</summary>
        private bool _logsEnabled;

        #endregion

        #region Editor Methods

        [MenuItem("Window/MetaMask/Setup Window")]
        /// <summary>Shows the window.</summary>
        public static void ShowWindow()
        {
            var window = GetWindow<MetaMaskWindow>("MetaMask Setup");
            LoadSettings(MetaMaskConfig.DefaultInstance, MetaMaskUnityUITransport.DefaultInstance, window);
        }

        /// <summary>The main GUI function.</summary>
        private void OnGUI()
        {
            DrawBackground();
            MaximumWindow();
            InitStyles();
            if (_state == MetaMaskState.main)
                Installer();
            else if (_state == MetaMaskState.install)
                Credentials();
            else if (_state == MetaMaskState.connect)
                DrawConnect();
        }

        #endregion

        #region Drawer Methods

        private void DrawHeader(string title)
        {
            GUILayout.Box(Resources.Load<Texture>(_headerImagePath), _headerStyle);
            GUILayout.Box(Resources.Load<Texture>(_metamaskLogoImagePath), _metamaskLogoStyle);
            GUILayout.Box(title, _higherTextStyle);
        }

        /// <summary>Draws the connect screen.</summary>
        private void DrawConnect()
        {
            GUILayout.Box(Resources.Load<Texture>(_headerImagePath), _headerStyle);
            GUILayout.Box(Resources.Load<Texture>(_metamaskLogoImagePath), _metamaskLogoStyle);
            GUILayout.Box("SDK Configured!", _higherTextStyle);
            GUILayout.Box(
                "Thank you for configuring the MetaMask SDK. You can now use the MetaMask SDK to connect to the MetaMask Wallet.",
                _bodyTextStyle);
            GUILayout.BeginArea(new Rect((EditorGUIUtility.currentViewWidth / 2) - 85, _lastYPosition + 120, 165, 100));
            if (GUILayout.Button("Thank You!", _buttonStyle))
            {
                _state = MetaMaskState.main;
                Repaint();
            }

            GUILayout.EndArea();
            StoreYPosition();
        }

        /// <summary>The installer window.</summary>
        private void Installer()
        {
            GUILayout.Box(Resources.Load<Texture>(_headerImagePath), _headerStyle);
            GUILayout.Box(Resources.Load<Texture>(_metamaskLogoImagePath), _metamaskLogoStyle);
            GUILayout.Box("Welcome Back!", _higherTextStyle);
            GUILayout.Box(
                "Welcome to the MetaMask SDK Installer Window, Below you will find our documentation as well as a section to modify the SDK configuration!",
                _bodyTextStyle);
            GUILayout.BeginArea(new Rect((EditorGUIUtility.currentViewWidth / 2) - 85, _lastYPosition + 100, 165, 100));
            if (GUILayout.Button("Documentation", _buttonStyle))
            {
                Application.OpenURL("https://docs.metamask.io/guide/");
            }

            GUILayout.EndArea();
            StoreYPosition();
            GUILayout.BeginArea(new Rect((EditorGUIUtility.currentViewWidth / 2) - 85, _lastYPosition + 170, 165, 100));
            if (GUILayout.Button("Credentials", _buttonStyle))
            {
                _state = MetaMaskState.install;
            }

            GUILayout.EndArea();
            StoreYPosition();
        }

        /// <summary>Displays the credentials screen which allows the configuration of the MetaMask SDK.</summary>
        private void Credentials()
        {
            GUILayout.Box(Resources.Load<Texture>(_headerImagePath), _headerStyle);
            GUILayout.Box("App Configuration", _higherTextStyle);
            GUILayout.Box("Please enter your application configuration data below!", _bodyTextStyle);
            EditorGUILayout.LabelField("App Name", this._smallHeaderStyle);
            _appNameText = GUILayout.TextField(_appNameText, 25, _inputFieldStyle);
            EditorGUILayout.LabelField("App Url", this._smallHeaderStyle);
            _appUrlText = GUILayout.TextField(_appUrlText, 25, _inputFieldStyle);
            EditorGUILayout.LabelField("User Agent", this._smallHeaderStyle);
            _appUserAgentText = GUILayout.TextField(_appUserAgentText, 25, _inputFieldStyle);
            _deepLinking = GUILayout.Toggle(_deepLinking, "Deep Linking", _inputToggleStyle);
            _logsEnabled = GUILayout.Toggle(_logsEnabled, "Logs Enabled", _inputToggleStyle);
            GUILayout.Box("Persistent Data", _h2TextStyle);
            EditorGUILayout.LabelField("Encryption Password", this._smallHeaderStyle);
            _encryptionPasswordText = GUILayout.TextField(_encryptionPasswordText, 25, _inputFieldStyle);
            EditorGUILayout.LabelField("Session Identifier", this._smallHeaderStyle);
            _sessionIdentifierText = GUILayout.TextField(_sessionIdentifierText, 25, _inputFieldStyle);
            GUILayout.Space(2);
            GUILayout.Box("Advanced", _h2TextStyle);
            EditorGUILayout.LabelField("Socket Url", this._smallHeaderStyle);
            _socketURLText = GUILayout.TextField(_socketURLText, 80, _inputFieldStyle);
            GUILayout.BeginHorizontal(_sidebySideStyle);
            if (GUILayout.Button("Back", _buttonStyle))
            {
                _state = MetaMaskState.main;
                Repaint();
            }

            if (GUILayout.Button("Apply Settings", _buttonStyle))
            {
                _state = MetaMaskState.connect;
                Repaint();
                ApplySettings();
            }

            GUILayout.EndHorizontal();
            StoreYPosition();
        }

        #endregion

        #region Private Methods

        /// <summary>Applies the current settings to the SDK.</summary>
        private void ApplySettings()
        {
            var metaMaskConfig = MetaMaskConfig.DefaultInstance;
            var metaMaskConfigUI = MetaMaskUnityUITransport.DefaultInstance;
            SerializedObject soMetaMaskConfig = new SerializedObject(metaMaskConfig);
            SerializedObject spMetaMaskUIConfig = new SerializedObject(metaMaskConfigUI);
            SerializedProperty spDeeplink = spMetaMaskUIConfig.FindProperty("useDeeplink");
            SerializedProperty spLoggingEnabled = soMetaMaskConfig.FindProperty("log");
            SerializedProperty spUserAgent = spMetaMaskUIConfig.FindProperty("userAgent");
            SerializedProperty spAppName = soMetaMaskConfig.FindProperty("appName");
            SerializedProperty spAppUrl = soMetaMaskConfig.FindProperty("appUrl");
            SerializedProperty spEncryptionPassword = soMetaMaskConfig.FindProperty("encryptionPassword");
            SerializedProperty spSessionIdentifier = soMetaMaskConfig.FindProperty("sessionIdentifier");
            SerializedProperty spSocketUrl = soMetaMaskConfig.FindProperty("socketUrl");
            spAppName.stringValue = this._appNameText;
            spAppUrl.stringValue = this._appUrlText;
            spEncryptionPassword.stringValue = this._appUserAgentText;
            spSessionIdentifier.stringValue = this._sessionIdentifierText;
            spSocketUrl.stringValue = this._socketURLText;
            spDeeplink.boolValue = this._deepLinking;
            spLoggingEnabled.boolValue = this._logsEnabled;
            spUserAgent.stringValue = this._appUserAgentText;
            soMetaMaskConfig.ApplyModifiedProperties();
            spMetaMaskUIConfig.ApplyModifiedProperties();
        }

        /// <summary>Loads the settings from the SDK's settings .</summary>
        /// <param name="metaMaskConfig">The configuration to load the settings into.</param>
        /// <param name="metaMaskUIConfig">The configuration to load the settings into.</param>
        /// <param name="window">The window to load the settings into.</param>
        private static void LoadSettings(MetaMaskConfig metaMaskConfig, MetaMaskUnityUITransport metaMaskUIConfig,
            MetaMaskWindow window)
        {
            SerializedObject soMetaMaskConfig = new SerializedObject(metaMaskConfig);
            SerializedObject soMetaMaskUIConfig = new SerializedObject(metaMaskUIConfig);
            SerializedProperty spDeepLinking = soMetaMaskUIConfig.FindProperty("useDeeplink");
            SerializedProperty spLoggingEnabled = soMetaMaskConfig.FindProperty("log");
            SerializedProperty spUserAgent = soMetaMaskUIConfig.FindProperty("userAgent");
            SerializedProperty spAppName = soMetaMaskConfig.FindProperty("appName");
            SerializedProperty spAppUrl = soMetaMaskConfig.FindProperty("appUrl");
            SerializedProperty spEncryptionPassword = soMetaMaskConfig.FindProperty("encryptionPassword");
            SerializedProperty spSessionIdentifier = soMetaMaskConfig.FindProperty("sessionIdentifier");
            SerializedProperty spSocketUrl = soMetaMaskConfig.FindProperty("socketUrl");
            window._appNameText = spAppName.stringValue;
            window._appUrlText = spAppUrl.stringValue;
            window._appUserAgentText = spUserAgent.stringValue;
            window._deepLinking = spDeepLinking.boolValue;
            window._encryptionPasswordText = spEncryptionPassword.stringValue;
            window._sessionIdentifierText = spSessionIdentifier.stringValue;
            window._socketURLText = spSocketUrl.stringValue;
            window._logsEnabled = spLoggingEnabled.boolValue;
        }

        /// <summary>Stores the y-position of the last drawn GUI element.</summary>
        private void StoreYPosition()
        {
            if (Event.current.type == EventType.Repaint)
            {
                _lastYPosition = GUILayoutUtility.GetLastRect().y;
            }
        }

        /// <summary>Initializes the styles.</summary>
        private void InitStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle();
                RemovePadding(_headerStyle);
                _headerStyle.fixedHeight = 90;
                _headerStyle.stretchWidth = true;
            }

            if (_metamaskLogoStyle == null)
            {
                _metamaskLogoStyle = new GUIStyle();
                RemovePadding(_metamaskLogoStyle);
                _metamaskLogoStyle.fixedHeight = 220;
                _metamaskLogoStyle.alignment = TextAnchor.MiddleCenter;
                _metamaskLogoStyle.fixedWidth = 240;
                _metamaskLogoStyle.margin = new RectOffset(59, 0, 0, 0);
            }

            if (_higherTextStyle == null)
            {
                _higherTextStyle = new GUIStyle();
                _higherTextStyle.wordWrap = true;
                _higherTextStyle.alignment = TextAnchor.MiddleCenter;
                _higherTextStyle.fontSize = 28;
                _higherTextStyle.fontStyle = FontStyle.Bold;
                _higherTextStyle.normal.textColor = Color.black;
                _higherTextStyle.margin = new RectOffset(0, 0, 0, 20);
            }

            if (_h2TextStyle == null)
            {
                _h2TextStyle = new GUIStyle();
                _h2TextStyle.wordWrap = true;
                _h2TextStyle.alignment = TextAnchor.LowerLeft;
                _h2TextStyle.fontSize = 20;
                _h2TextStyle.fontStyle = FontStyle.Bold;
                _h2TextStyle.normal.textColor = Color.black;
                _h2TextStyle.margin = new RectOffset(20, 0, 5, 10);
            }

            if (_bodyTextStyle == null)
            {
                _bodyTextStyle = new GUIStyle();
                _bodyTextStyle.wordWrap = true;
                _bodyTextStyle.alignment = TextAnchor.MiddleCenter;
                _bodyTextStyle.fontSize = 18;
                _bodyTextStyle.fontStyle = FontStyle.Bold;
                _bodyTextStyle.normal.textColor = Color.grey;
                _bodyTextStyle.margin = new RectOffset(20, 20, 0, 20);
            }

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle();
                _buttonStyle.normal.background = Resources.Load<Texture2D>(_buttonImagePath);
                _buttonStyle.fontSize = 16;
                _buttonStyle.fontStyle = FontStyle.Bold;
                _buttonStyle.border = new RectOffset(0, 0, 0, 0);
                _buttonStyle.normal.textColor = Color.white;
                _buttonStyle.alignment = TextAnchor.MiddleCenter;
                _buttonStyle.fixedWidth = 165;
                _buttonStyle.fixedHeight = 65;
            }

            if (_inputFieldStyle == null)
            {
                _inputFieldStyle = new GUIStyle();
                _inputFieldStyle.normal.background = MakeTexture(2, 2, Color.black);
                _inputFieldStyle.fontSize = 16;
                _inputFieldStyle.normal.textColor = Color.white;
                _inputFieldStyle.fontStyle = FontStyle.Bold;
                _inputFieldStyle.margin = new RectOffset(20, 20, 0, 0);
                _inputFieldStyle.padding = new RectOffset(5, 5, 10, 10);
            }

            if (_inputToggleStyle == null)
            {
                _inputToggleStyle = new GUIStyle("Toggle");
                _inputToggleStyle.fontSize = 20;
                _inputToggleStyle.normal.textColor = Color.black;
                _inputToggleStyle.fontStyle = FontStyle.Bold;
                _inputToggleStyle.margin = new RectOffset(20, 20, 0, 0);
                _inputToggleStyle.padding = new RectOffset(35, 5, 10, 10);
            }

            if (_smallHeaderStyle == null)
            {
                _smallHeaderStyle = new GUIStyle("miniLabel");
                _smallHeaderStyle.margin = new RectOffset(20, 0, 0, 0);
                _smallHeaderStyle.padding = new RectOffset(20, 0, 0, 0);
            }

            if (_sidebySideStyle == null)
            {
                _sidebySideStyle = new GUIStyle();
                _sidebySideStyle.margin = new RectOffset(20, 0, 0, 0);
                _sidebySideStyle.padding = new RectOffset(0, 0, 5, 0);
            }
        }

        /// <summary>Removes the padding and border from a GUIStyle.</summary>
        /// <param name="style">The style to remove the padding and border from.</param>
        /// <returns>The style with the padding and border removed.</returns>
        private GUIStyle RemovePadding(GUIStyle style)
        {
            style.padding = new RectOffset(0, 0, 0, 0);
            style.border = new RectOffset(0, 0, 0, 0);
            return style;
        }

        /// <summary>Sets the window to its maximum size.</summary>
        private void MaximumWindow()
        {
            if (_state == MetaMaskState.install)
            {
                this.maxSize = new Vector2(365, 825);
                this.minSize = new Vector2(365, 825);
            }
            else
            {
                this.maxSize = new Vector2(365, 615);
                this.minSize = new Vector2(365, 615);
            }
        }

        /// <summary>Draws the background image.</summary>
        private void DrawBackground()
        {
            Texture texture = Resources.Load<Texture>(_backgroundImagePath);
            GUI.DrawTexture(new Rect(0, 0, this.position.width, this.position.height), texture);
        }

        /// <summary>Creates a texture with the specified color.</summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="color">The color of the texture.</param>
        /// <returns>The texture.</returns>
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D backgroundTexture = new Texture2D(width, height);
            backgroundTexture.SetPixels(pixels);
            backgroundTexture.Apply();
            return backgroundTexture;
        }

        #endregion
    }
}