/// <summary>
/// The inspector for the GA prefab.
/// </summary>

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System;
using GameAnalyticsSDK.Utilities;
using GameAnalyticsSDK.Setup;
using System.Text.RegularExpressions;
#if UNITY_2017_1_OR_NEWER
using UnityEngine.Networking;
#endif

namespace GameAnalyticsSDK.Editor
{
    [CustomEditor(typeof(GameAnalyticsSDK.Setup.Settings))]
    public class GA_SettingsInspector : UnityEditor.Editor
    {
        public const bool IsCustomPackage = false;
        private const string AssetsPrependPath = IsCustomPackage ? "Packages/com.gameanalytics.sdk" : "Assets/GameAnalytics";

        private GUIContent _publicKeyLabel = new GUIContent("Game Key", "Your GameAnalytics Game Key - copy/paste from the GA website.");
        private GUIContent _privateKeyLabel = new GUIContent("Secret Key", "Your GameAnalytics Secret Key - copy/paste from the GA website.");
        private GUIContent _emailLabel = new GUIContent("Email", "Your GameAnalytics user account email.");
        private GUIContent _passwordLabel = new GUIContent("Password", "Your GameAnalytics user account password. Must be at least 8 characters in length.");
        private GUIContent _organizationsLabel = new GUIContent("Org.", "Organizations tied to your GameAnalytics user account.");
        private GUIContent _studiosLabel = new GUIContent("Studio", "Studios tied to your GameAnalytics user account.");
        private GUIContent _gamesLabel = new GUIContent("Game", "Games tied to the selected GameAnalytics studio.");
        private GUIContent _build = new GUIContent("Build", "The current version of the game. Updating the build name for each test version of the game will allow you to filter by build when viewing your data on the GA website.");
        private GUIContent _infoLogEditor = new GUIContent("Info Log Editor", "Show info messages from GA in the unity editor console when submitting data.");
        private GUIContent _infoLogBuild = new GUIContent("Info Log Build", "Show info messages from GA in builds (f.x. Xcode for iOS).");
        private GUIContent _verboseLogBuild = new GUIContent("Verbose Log Build", "Show full info messages from GA in builds (f.x. Xcode for iOS). Noet that this option includes long JSON messages sent to the server.");
        private GUIContent _useManualSessionHandling = new GUIContent("Use manual session handling", "Manually choose when to end and start a new session. Note initializing of the SDK will automatically start the first session.");
#if UNITY_5_6_OR_NEWER
        private GUIContent _usePlayerSettingsBunldeVersionForBuild = new GUIContent("Send Version* (Android, iOS) as build number", "The SDK will automatically fetch the version* number on Android and iOS and send it as the GameAnalytics build number.");
#else
        private GUIContent _usePlayerSettingsBunldeVersionForBuild = new GUIContent("Send Build number (iOS) and Version* (Android) as build number", "The SDK will automatically fetch the build number on iOS and the version* number on Android and send it as the GameAnalytics build number.");
#endif
        //private GUIContent _sendExampleToMyGame        = new GUIContent("Get Example Game Data", "If enabled data collected while playing the example tutorial game will be sent to your game (using your game key and secret key). Otherwise data will be sent to a premade GA test game, to prevent it from polluting your data.");
        private GUIContent _account = new GUIContent("Account", "This tab allows you to easily create a GameAnalytics account. You can also login to automatically retrieve your Game Key and Secret Key.");
        private GUIContent _setup = new GUIContent("Setup", "This tab shows general options which are relevant for a wide variety of messages sent to GameAnalytics.");
        private GUIContent _advanced = new GUIContent("Advanced", "This tab shows advanced and misc. options for the GameAnalytics SDK.");
        private GUIContent _customDimensions01 = new GUIContent("Custom Dimensions 01", "List of custom dimensions 01.");
        private GUIContent _customDimensions02 = new GUIContent("Custom Dimensions 02", "List of custom dimensions 02.");
        private GUIContent _customDimensions03 = new GUIContent("Custom Dimensions 03", "List of custom dimensions 03.");
        private GUIContent _resourceItemTypes = new GUIContent("Resource Item Types", "List of Resource Item Types.");
        private GUIContent _resourceCurrrencies = new GUIContent("Resource Currencies", "List of Resource Currencies.");
        private GUIContent _gaFpsAverage = new GUIContent("Submit Average FPS", "Submit the average frames per second.");
        private GUIContent _gaFpsCritical = new GUIContent("Submit Critical FPS", "Submit a message whenever the frames per second falls below a certain threshold. The location of the Track Target will be used for critical FPS events.");
        private GUIContent _gaFpsCriticalThreshold = new GUIContent("FPS <", "Frames per second threshold.");
        private GUIContent _gaSubmitErrors = new GUIContent("Submit Errors", "Submit error and exception messages to the GameAnalytics server. Useful for getting relevant data when the game crashes, etc.");
        private GUIContent _gaNativeErrorReporting = new GUIContent("Native error reporting (Android, iOS)", "Submit error and exception messages from native errors and exceptions to the GameAnalytics server. Useful for getting relevant data when the game crashes, etc. from native code.");

        private GUIContent _gameSetupIcon;
        private bool _gameSetupIconOpen = false;
        private GUIContent _gameSetupIconMsg = new GUIContent("Your game and secret key will authenticate the game. Please set the build version too. All fields are required.");
        private GUIContent _customDimensionsIcon;
        private bool _customDimensionsIconOpen = false;
        private GUIContent _customDimensionsIconMsg = new GUIContent("Define your custom dimension values below. Values that are not defined will be ignored.");
        private GUIContent _resourceTypesIcon;
        private bool _resourceTypesIconOpen = false;
        private GUIContent _resourceTypesIconMsg = new GUIContent("Define all your resource currencies and resource item types. Values that are not defined will be ignored.");
        private GUIContent _advancedSettingsIcon;
        private bool _advancedSettingsIconOpen = false;
        private GUIContent _advancedSettingsIconMsg = new GUIContent("Advanced settings allows you to enable tracking of Unity errors and exceptions, and frames per second (for performance).");
        private GUIContent _debugSettingsIcon;
        private bool _debugSettingsIconOpen = false;
        private GUIContent _debugSettingsIconMsg = new GUIContent("Debug settings allows you to enable info log for the editor or for builds (Xcode, etc.). Enabling verbose logging will show additional JSON messages in builds.");

        private GUIContent _deleteIcon;
        private GUIContent _homeIcon;
        private GUIContent _infoIcon;
        private GUIContent _instrumentIcon;
        private GUIContent _questionIcon;

        private GUIStyle _orangeUpdateLabelStyle;
        private GUIStyle _orangeUpdateIconStyle;

        //private static readonly Texture2D _triggerAdNotEnabledTexture = new Texture2D(1, 1);
        private static bool _checkedProjectNames = false;

        private const string _unityToken = "KKy7MQNc2TEUOeK0EMtR";

        private const string _gaUrl = "https://userapi.gameanalytics.com/ext/v1/";

        private const int MaxNumberOfDimensions = 20;

        private int selectedPlatformIndex = 0;
        private string[] availablePlatforms;

        void OnEnable()
        {
            GameAnalyticsSDK.Setup.Settings ga = target as GameAnalyticsSDK.Setup.Settings;

            if (ga.UpdateIcon == null)
            {
                ga.UpdateIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetsPrependPath + "/Gizmos/GameAnalytics/Images/update_orange.png", typeof(Texture2D));
            }

            if (ga.DeleteIcon == null)
            {
                ga.DeleteIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetsPrependPath + "/Gizmos/GameAnalytics/Images/delete.png", typeof(Texture2D));
            }

            if (ga.GameIcon == null)
            {
                ga.GameIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetsPrependPath + "/Gizmos/GameAnalytics/Images/game.png", typeof(Texture2D));
            }

            if (ga.HomeIcon == null)
            {
                ga.HomeIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetsPrependPath + "/Gizmos/GameAnalytics/Images/home.png", typeof(Texture2D));
            }

            if (ga.InfoIcon == null)
            {
                ga.InfoIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetsPrependPath + "/Gizmos/GameAnalytics/Images/info.png", typeof(Texture2D));
            }

            if (ga.InstrumentIcon == null)
            {
                ga.InstrumentIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetsPrependPath + "/Gizmos/GameAnalytics/Images/instrument.png", typeof(Texture2D));
            }

            if (ga.QuestionIcon == null)
            {
                ga.QuestionIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetsPrependPath + "/Gizmos/GameAnalytics/Images/question.png", typeof(Texture2D));
            }

            if (ga.UserIcon == null)
            {
                ga.UserIcon = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetsPrependPath + "/Gizmos/GameAnalytics/Images/user.png", typeof(Texture2D));
            }

            if (_gameSetupIcon == null)
            {
                _gameSetupIcon = new GUIContent(ga.InfoIcon, "Game Setup.");
            }

            if (_customDimensionsIcon == null)
            {
                _customDimensionsIcon = new GUIContent(ga.InfoIcon, "Custom Dimensions.");
            }

            if (_resourceTypesIcon == null)
            {
                _resourceTypesIcon = new GUIContent(ga.InfoIcon, "Resource Types.");
            }

            if (_advancedSettingsIcon == null)
            {
                _advancedSettingsIcon = new GUIContent(ga.InfoIcon, "Advanced Settings.");
            }

            if (_debugSettingsIcon == null)
            {
                _debugSettingsIcon = new GUIContent(ga.InfoIcon, "Debug Settings.");
            }

            if (_deleteIcon == null)
            {
                _deleteIcon = new GUIContent(ga.DeleteIcon, "Delete.");
            }

            if (_homeIcon == null)
            {
                _homeIcon = new GUIContent(ga.HomeIcon, "Your GameAnalytics webpage tool.");
            }

            if (_instrumentIcon == null)
            {
                _instrumentIcon = new GUIContent(ga.InstrumentIcon, "GameAnalytics setup guide.");
            }

            if (_questionIcon == null)
            {
                _questionIcon = new GUIContent(ga.QuestionIcon, "GameAnalytics support.");
            }

            if (ga.Logo == null)
            {
                ga.Logo = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetsPrependPath + "/Gizmos/GameAnalytics/gaLogo.png", typeof(Texture2D));
            }
        }

        public override void OnInspectorGUI()
        {
            GameAnalyticsSDK.Setup.Settings ga = target as GameAnalyticsSDK.Setup.Settings;

            EditorGUI.indentLevel = 1;
            EditorGUILayout.Space();

            if (ga.SignupButton == null)
            {
                GUIStyle signupButton = new GUIStyle(GUI.skin.button);
                signupButton.normal.background = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetsPrependPath + "/Gizmos/GameAnalytics/Images/default.png", typeof(Texture2D));
                signupButton.active.background = (Texture2D)AssetDatabase.LoadAssetAtPath(AssetsPrependPath + "/Gizmos/GameAnalytics/Images/active.png", typeof(Texture2D));
                signupButton.normal.textColor = Color.white;
                signupButton.active.textColor = Color.white;
                signupButton.fontSize = 14;
                signupButton.fontStyle = FontStyle.Bold;
                ga.SignupButton = signupButton;
            }

            #region Header section

            GUILayout.BeginHorizontal();

            GUILayout.Label(ga.Logo, new GUILayoutOption[] {
                GUILayout.Width(32),
                GUILayout.Height(32)
            });

            GUILayout.BeginVertical();

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();

            GUILayout.Label("Unity SDK v." + GameAnalyticsSDK.Setup.Settings.VERSION);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            DrawLinkButton(_homeIcon, GUI.skin.label, "https://go.gameanalytics.com/login", GUILayout.Width(24), GUILayout.Height(24));

            DrawLinkButton(_questionIcon, GUI.skin.label, "http://support.gameanalytics.com/", GUILayout.Width(24), GUILayout.Height(24));

            DrawButton(_instrumentIcon, GUI.skin.label, OpenSignUpSwitchToGuideStep, GUILayout.Width(24), GUILayout.Height(24));

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            string updateStatus = GA_UpdateWindow.UpdateStatus(GameAnalyticsSDK.Setup.Settings.VERSION);

            if (!updateStatus.Equals(string.Empty))
            {
                GUILayout.BeginHorizontal();

                GUILayout.Space(10);

                _orangeUpdateLabelStyle = new GUIStyle(EditorStyles.label);
                _orangeUpdateLabelStyle.normal.textColor = new Color(0.875f, 0.309f, 0.094f);

                _orangeUpdateIconStyle = new GUIStyle(EditorStyles.label);

                if (GUILayout.Button(ga.UpdateIcon, _orangeUpdateIconStyle, GUILayout.MaxWidth(17)))
                {
                    OpenUpdateWindow();
                }

                GUILayout.Label(updateStatus, _orangeUpdateLabelStyle);

                if (ga.Organizations == null)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
            }
            else
            {
                if (ga.Organizations != null)
                {
                    GUILayout.BeginHorizontal();
                }
                else
                {
                    GUILayout.Space(22);
                }
            }

            if (ga.Organizations != null)
            {
                GUILayout.FlexibleSpace();

                float minW = 0;
                float maxW = 0;
                GUIContent email = new GUIContent(ga.EmailGA);
                EditorStyles.miniLabel.CalcMinMaxWidth(email, out minW, out maxW);
                GUILayout.Label(email, EditorStyles.miniLabel, GUILayout.MaxWidth(maxW));

                GUILayout.BeginVertical();
                //GUILayout.Space(-1);

                if (GUILayout.Button("Log out", GUILayout.MaxWidth(67)))
                {
                    ga.Organizations = null;
                    SetLoginStatus("Not logged in.", ga);
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            #endregion // Header section

            #region IntroScreen
            if (ga.IntroScreen)
            {
                bool finishIntro = false;
                for (int i = 0; i < GameAnalytics.SettingsGA.Platforms.Count; ++i)
                {
                    if (GameAnalytics.SettingsGA.GetGameKey(i).Length > 0 || GameAnalytics.SettingsGA.GetSecretKey(i).Length > 0)
                    {
                        finishIntro = true;
                        break;
                    }
                }

                if (finishIntro)
                {
                    GameAnalytics.SettingsGA.IntroScreen = false;
                }
                else
                {
                    if (!_checkedProjectNames && !EditorPrefs.GetBool("GA_Installed" + "-" + Application.dataPath, false))
                    {
                        _checkedProjectNames = true;

                        if (!PlayerSettings.companyName.Equals("DefaultCompany"))
                        {
                            GameAnalytics.SettingsGA.StudioName = PlayerSettings.companyName;
                        }
                        if (!PlayerSettings.productName.StartsWith("New Unity Project"))
                        {
                            GameAnalytics.SettingsGA.GameName = PlayerSettings.productName;
                        }
                        EditorPrefs.SetBool("GA_Installed" + "-" + Application.dataPath, true);
                        Selection.activeObject = GameAnalytics.SettingsGA;
                    }

                    GUILayout.Space(5);

                    Splitter(new Color(0.35f, 0.35f, 0.35f));

                    GUILayout.Space(10);

                    GUIStyle largeWhiteStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
                    if (!Application.HasProLicense())
                    {
                        largeWhiteStyle = new GUIStyle(EditorStyles.largeLabel);
                    }
                    largeWhiteStyle.fontSize = 16;
                    //largeWhiteStyle.fontStyle = FontStyle.Bold;

                    DrawLabelWithFlexibleSpace("Thank you for downloading!", largeWhiteStyle, 30);

                    GUILayout.Space(20);

                    GUIStyle greyStyle = new GUIStyle(EditorStyles.label);
                    greyStyle.fontSize = 12;

                    DrawLabelWithFlexibleSpace("Get started tracking your game by signing up to", greyStyle, 20);

                    GUILayout.Space(-5);

                    DrawLabelWithFlexibleSpace("GameAnalytics for FREE.", greyStyle, 20);

                    GUILayout.Space(20);

                    DrawButtonWithFlexibleSpace("Sign up", ga.SignupButton, OpenSignUp, GUILayout.Width(175), GUILayout.Height(40));

                    GUILayout.Space(15);

                    Splitter(new Color(0.35f, 0.35f, 0.35f));

                    GUILayout.Space(15);

                    DrawLabelWithFlexibleSpace("Already have an account? Please login", greyStyle, 20);

                    GUILayout.Space(15);

                    GUILayout.BeginHorizontal();
                    //GUILayout.Label("", GUILayout.Width(3));
                    GUILayout.Label(_emailLabel, GUILayout.Width(75));
                    ga.EmailGA = EditorGUILayout.TextField("", ga.EmailGA);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    //GUILayout.Label("", GUILayout.Width(3));
                    GUILayout.Label(_passwordLabel, GUILayout.Width(75));
                    ga.PasswordGA = EditorGUILayout.PasswordField("", ga.PasswordGA);
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(90));
                    if (GUILayout.Button("Login", new GUILayoutOption[] {
                        GUILayout.Width(130),
                        GUILayout.MaxHeight(30)
                    }))
                    {
                        ga.IntroScreen = false;
                        ga.SignUpOpen = false;
                        ga.CurrentInspectorState = GameAnalyticsSDK.Setup.Settings.InspectorStates.Account;

                        ga.Organizations = null;
                        SetLoginStatus("Contacting Server..", ga);
                        LoginUser(ga);
                    }
                    GUILayout.Label("", GUILayout.Width(10));
                    GUILayout.BeginVertical();
                    GUILayout.Space(8);

                    DrawLinkButton("Forgot password?", EditorStyles.label, "https://go.gameanalytics.com/login?showreset&email=" + ga.EmailGA, GUILayout.Width(105));
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(15);

                    Splitter(new Color(0.35f, 0.35f, 0.35f));

                    GUILayout.Space(15);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("I want to fill in my game keys manually", EditorStyles.label, GUILayout.Width(207)))
                    {
                        ga.IntroScreen = false;
                        ga.CurrentInspectorState = GameAnalyticsSDK.Setup.Settings.InspectorStates.Basic;
                    }
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            #endregion // IntroScreen
            else
            {
                //Tabs
                GUILayout.BeginHorizontal();

                GUIStyle activeTabStyle = new GUIStyle(EditorStyles.miniButtonMid);
                GUIStyle activeTabStyleLeft = new GUIStyle(EditorStyles.miniButtonLeft);
                GUIStyle activeTabStyleRight = new GUIStyle(EditorStyles.miniButtonRight);

                activeTabStyle.normal = EditorStyles.miniButtonMid.active;
                activeTabStyleLeft.normal = EditorStyles.miniButtonLeft.active;
                activeTabStyleRight.normal = EditorStyles.miniButtonRight.active;

                GUIStyle inactiveTabStyle = new GUIStyle(EditorStyles.miniButtonMid);
                GUIStyle inactiveTabStyleLeft = new GUIStyle(EditorStyles.miniButtonLeft);
                GUIStyle inactiveTabStyleRight = new GUIStyle(EditorStyles.miniButtonRight);

                GUIStyle basicTabStyle = ga.CurrentInspectorState == GameAnalyticsSDK.Setup.Settings.InspectorStates.Basic ? activeTabStyleLeft : inactiveTabStyleLeft;

                if (ga.Organizations == null)
                {
                    if (GUILayout.Button(_account, ga.CurrentInspectorState == GameAnalyticsSDK.Setup.Settings.InspectorStates.Account ? activeTabStyleLeft : inactiveTabStyleLeft))
                    {
                        ga.CurrentInspectorState = GameAnalyticsSDK.Setup.Settings.InspectorStates.Account;
                    }

                    basicTabStyle = ga.CurrentInspectorState == GameAnalyticsSDK.Setup.Settings.InspectorStates.Basic ? activeTabStyle : inactiveTabStyle;
                }

                if (GUILayout.Button(_setup, basicTabStyle))
                {
                    ga.CurrentInspectorState = GameAnalyticsSDK.Setup.Settings.InspectorStates.Basic;
                }

                if (GUILayout.Button(_advanced, ga.CurrentInspectorState == GameAnalyticsSDK.Setup.Settings.InspectorStates.Pref ? activeTabStyleRight : inactiveTabStyleRight))
                {
                    ga.CurrentInspectorState = GameAnalyticsSDK.Setup.Settings.InspectorStates.Pref;
                }

                GUILayout.EndHorizontal();

                #region Settings.InspectorStates.Account
                if (ga.CurrentInspectorState == GameAnalyticsSDK.Setup.Settings.InspectorStates.Account)
                {
                    EditorGUILayout.Space();

                    GUILayout.Label("Already have an account with GameAnalytics?", EditorStyles.largeLabel);

                    EditorGUILayout.Space();

                    if (!string.IsNullOrEmpty(ga.LoginStatus) && !ga.LoginStatus.Equals("Not logged in."))
                    {
                        EditorGUILayout.Space();
                        if (ga.JustSignedUp && !ga.HideSignupWarning)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(-18));
                            EditorGUILayout.HelpBox("Please be aware that our service might take a few minutes to get ready to receive events. Click here to open Integration Status to follow the progress as you start sending events.", MessageType.Warning);
                            Rect r = GUILayoutUtility.GetLastRect();
                            if (GUI.Button(r, "", EditorStyles.label))
                            {
                                //Application.OpenURL("https://go.gameanalytics.com/login?token=" + ga.TokenGA + "&exp=" + ga.ExpireTime + "&goto=/game/" + ga.Studios[ga.SelectedStudio - 1].Games[ga.SelectedGame - 1].ID + "/initialize");
                            }
                            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
                            if (GUILayout.Button("X"))
                            {
                                ga.HideSignupWarning = true;
                            }
                            GUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                        }
                        GUILayout.BeginHorizontal();
                        //GUILayout.Label("", GUILayout.Width(7));
                        GUILayout.Label("Status", GUILayout.Width(88));
                        GUILayout.Label(ga.LoginStatus);
                        GUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();

                    if (ga.Organizations == null)
                    {
                        GUILayout.Label(_emailLabel, GUILayout.Width(75));
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(-17));
                        ga.EmailGA = EditorGUILayout.TextField("", ga.EmailGA, GUILayout.MaxWidth(270));
                        GUILayout.EndHorizontal();

                        GUILayout.Space(12);

                        GUILayout.Label(_passwordLabel, GUILayout.Width(75));
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(-17));
                        ga.PasswordGA = EditorGUILayout.PasswordField("", ga.PasswordGA, GUILayout.MaxWidth(270));
                        GUILayout.EndHorizontal();

                        GUILayout.Space(12);

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(2);
                        if (GUILayout.Button("Login", new GUILayoutOption[] {
                            GUILayout.Width(130),
                            GUILayout.MaxHeight(40)
                        }))
                        {
                            ga.Organizations = null;
                            SetLoginStatus("Contacting Server..", ga);
                            LoginUser(ga);
                        }
                        GUILayout.Label("", GUILayout.Width(10));
                        GUILayout.BeginVertical();
                        GUILayout.Space(14);
                        if (GUILayout.Button("Forgot password?", EditorStyles.label, GUILayout.Width(105)))
                        {
                            Application.OpenURL("https://go.gameanalytics.com/login?showreset&email=" + ga.EmailGA);
                        }
                        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(20);

                        Splitter(new Color(0.35f, 0.35f, 0.35f));

                        GUILayout.Space(16);

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Sign up", new GUILayoutOption[] {
                            GUILayout.Width(130),
                            GUILayout.Height(40)
                        }))
                        {
                            GA_SignUp signup = ScriptableObject.CreateInstance<GA_SignUp>();
                            signup.maxSize = new Vector2(640, 600);
                            signup.minSize = new Vector2(640, 600);
                            signup.titleContent = new GUIContent("GameAnalytics - Sign up for FREE");
                            signup.ShowUtility();
                            signup.Opened();
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(16);

                        Splitter(new Color(0.35f, 0.35f, 0.35f));

                        GUILayout.Space(16);

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("I want to fill in my game keys manually", EditorStyles.label, GUILayout.Width(207)))
                        {
                            ga.CurrentInspectorState = GameAnalyticsSDK.Setup.Settings.InspectorStates.Basic;
                        }
                        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
                #endregion // Settings.InspectorStates.Account
                #region Settings.InspectorStates.Basic
                else if (ga.CurrentInspectorState == GameAnalyticsSDK.Setup.Settings.InspectorStates.Basic)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    GUILayout.Space(-4);
                    GUILayout.Label("Game Setup", EditorStyles.largeLabel);
                    GUILayout.EndVertical();

                    #region Setup help
                    if (!_gameSetupIconOpen)
                    {
                        GUI.color = new Color(0.54f, 0.54f, 0.54f);
                    }
                    if (GUILayout.Button(_gameSetupIcon, GUIStyle.none, new GUILayoutOption[] {
                        GUILayout.Width(12),
                        GUILayout.Height(12)
                    }))
                    {
                        _gameSetupIconOpen = !_gameSetupIconOpen;
                    }
                    GUI.color = Color.white;
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();

                    if (_gameSetupIconOpen)
                    {
                        GUILayout.BeginHorizontal();
                        TextAnchor tmpAnchor = GUI.skin.box.alignment;
                        GUI.skin.box.alignment = TextAnchor.UpperLeft;
                        Color tmpColor = GUI.skin.box.normal.textColor;
                        GUI.skin.box.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                        RectOffset tmpOffset = GUI.skin.box.padding;
                        GUI.skin.box.padding = new RectOffset(6, 6, 5, 32);
                        GUILayout.Box(_gameSetupIconMsg);
                        GUI.skin.box.alignment = tmpAnchor;
                        GUI.skin.box.normal.textColor = tmpColor;
                        GUI.skin.box.padding = tmpOffset;
                        //GUILayout.Label("Advanced settings are pretty awesome! They allow you to do all kinds of things, such as tracking Unity errors and exceptions, and frames per second (for performance). See http://www.support.gameanalytics.com", EditorStyles.wordWrappedMiniLabel);
                        GUILayout.EndHorizontal();

                        Rect tmpRect = GUILayoutUtility.GetLastRect();
                        if (GUI.Button(new Rect(tmpRect.x + 5, tmpRect.y + tmpRect.height - 25, 80, 20), "Learn more"))
                        {
                            Application.OpenURL("https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Settings#setup");
                        }
                    }
                    #endregion // Setup help

                    EditorGUILayout.Space();

                    if (!string.IsNullOrEmpty(ga.LoginStatus) && !ga.LoginStatus.Equals("Not logged in."))
                    {
                        if (ga.JustSignedUp && !ga.HideSignupWarning)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(-18));
                            EditorGUILayout.HelpBox("Please be aware that our service might take a few minutes to get ready to receive events. Click here to open Integration Status to follow the progress as you start sending events.", MessageType.Warning);
                            Rect r = GUILayoutUtility.GetLastRect();
                            if (GUI.Button(r, "", EditorStyles.label))
                            {
                                //Application.OpenURL("https://go.gameanalytics.com/login?token=" + ga.TokenGA + "&exp=" + ga.ExpireTime + "&goto=/game/" + ga.Studios[ga.SelectedStudio - 1].Games[ga.SelectedGame - 1].ID + "/initialize");
                            }
                            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
                            if (GUILayout.Button("X"))
                            {
                                ga.HideSignupWarning = true;
                            }
                            GUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                        }

                        if (GUILayout.Button("Add game"))
                        {
                            GA_SignUp signup = ScriptableObject.CreateInstance<GA_SignUp>();
                            signup.maxSize = new Vector2(640, 600);
                            signup.minSize = new Vector2(640, 600);
                            signup.TourStep = 1;
                            signup.titleContent = new GUIContent("GameAnalytics - Sign up for FREE");
                            signup.ShowUtility();
                            signup.Opened();
                        }

                        GUILayout.BeginHorizontal();
                        //GUILayout.Label("", GUILayout.Width(7));
                        GUILayout.Label("Status", GUILayout.Width(63));
                        GUILayout.Label(ga.LoginStatus);
                        GUILayout.EndHorizontal();
                    }

                    Splitter(new Color(0.35f, 0.35f, 0.35f));

                    // sanity check
                    if(ga.SelectedPlatformOrganization.Count != GameAnalytics.SettingsGA.Platforms.Count)
                    {
                        int diff = ga.SelectedPlatformOrganization.Count - GameAnalytics.SettingsGA.Platforms.Count;

                        if(diff < 0)
                        {
                            int absDiff = Mathf.Abs(diff);

                            for(int i = 0; i < absDiff; ++i)
                            {
                                ga.SelectedPlatformOrganization.Add("");
                            }
                        }
                        else
                        {
                            for (int i = 0; i < diff; ++i)
                            {
                                ga.SelectedPlatformOrganization.RemoveAt(ga.SelectedPlatformOrganization.Count - 1);
                            }
                        }
                    }

                    for (int i = 0; i < GameAnalytics.SettingsGA.Platforms.Count; ++i)
                    {
                        ga.PlatformFoldOut[i] = EditorGUILayout.Foldout(ga.PlatformFoldOut[i], PlatformToString(GameAnalytics.SettingsGA.Platforms[i]));

                        if (ga.PlatformFoldOut[i])
                        {
                            if (ga.Organizations != null && ga.Organizations.Count > 0 && i < ga.SelectedOrganization.Count)
                            {
                                EditorGUILayout.Space();
                                //Splitter(new Color(0.35f, 0.35f, 0.35f));

                                GUILayout.BeginHorizontal();
                                //GUILayout.Label("", GUILayout.Width(7));
                                GUILayout.Label(_organizationsLabel, GUILayout.Width(50));
                                string[] organizationNames = Organization.GetOrganizationNames(ga.Organizations);
                                if (ga.SelectedOrganization[i] >= organizationNames.Length)
                                {
                                    ga.SelectedOrganization[i] = 0;
                                }
                                int tmpSelectedOrganization = ga.SelectedOrganization[i];
                                ga.SelectedOrganization[i] = EditorGUILayout.Popup("", ga.SelectedOrganization[i], organizationNames);
                                if (tmpSelectedOrganization != ga.SelectedOrganization[i])
                                {
                                    ga.SelectedStudio[i] = 0;
                                    ga.SelectedGame[i] = 0;
                                }
                                GUILayout.EndHorizontal();

                                if (ga.SelectedOrganization[i] > 0)
                                {
                                    if (tmpSelectedOrganization != ga.SelectedOrganization[i])
                                    {
                                        SelectOrganization(ga.SelectedOrganization[i], ga, i);
                                    }

                                    GUILayout.BeginHorizontal();
                                    //GUILayout.Label("", GUILayout.Width(7));
                                    GUILayout.Label(_studiosLabel, GUILayout.Width(50));
                                    string[] studioNames = Studio.GetStudioNames(ga.Organizations[ga.SelectedOrganization[i] - 1].Studios);
                                    if (ga.SelectedStudio[i] >= studioNames.Length)
                                    {
                                        ga.SelectedStudio[i] = 0;
                                    }
                                    int tmpSelectedStudio = ga.SelectedStudio[i];
                                    ga.SelectedStudio[i] = EditorGUILayout.Popup("", ga.SelectedStudio[i], studioNames);
                                    GUILayout.EndHorizontal();

                                    if (ga.SelectedStudio[i] > 0)
                                    {
                                        if (tmpSelectedStudio != ga.SelectedStudio[i])
                                        {
                                            SelectStudio(ga.SelectedStudio[i], ga, i);
                                        }

                                        GUILayout.BeginHorizontal();
                                        //GUILayout.Label("", GUILayout.Width(7));
                                        GUILayout.Label(_gamesLabel, GUILayout.Width(50));
                                        string[] gameNames = Studio.GetGameNames(ga.SelectedStudio[i] - 1, ga.Organizations[ga.SelectedOrganization[i] - 1].Studios);
                                        if (ga.SelectedGame[i] >= gameNames.Length)
                                        {
                                            ga.SelectedGame[i] = 0;
                                        }

                                        int tmpSelectedGame = ga.SelectedGame[i];
                                        ga.SelectedGame[i] = EditorGUILayout.Popup("", ga.SelectedGame[i], gameNames);
                                        GUILayout.EndHorizontal();

                                        if (ga.SelectedStudio[i] > 0 && tmpSelectedGame != ga.SelectedGame[i])
                                        {
                                            SelectGame(ga.SelectedGame[i], ga, i);
                                        }
                                    }
                                    else if (tmpSelectedStudio != ga.SelectedStudio[i])
                                    {
                                        SetLoginStatus("Please select studio..", ga);
                                    }
                                }
                                else if (tmpSelectedOrganization != ga.SelectedOrganization[i])
                                {
                                    SetLoginStatus("Please select organization..", ga);
                                }
                            }
                            else
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(_organizationsLabel, GUILayout.Width(85));
                                GUILayout.Space(-10);
                                GUILayout.Label(!string.IsNullOrEmpty(ga.SelectedPlatformOrganization[i]) ? ga.SelectedPlatformOrganization[i] : "N/A");
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                GUILayout.Label(_studiosLabel, GUILayout.Width(85));
                                GUILayout.Space(-10);
                                GUILayout.Label(!string.IsNullOrEmpty(ga.SelectedPlatformStudio[i]) ? ga.SelectedPlatformStudio[i] : "N/A");
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                GUILayout.Label(_gamesLabel, GUILayout.Width(85));
                                GUILayout.Space(-10);
                                GUILayout.Label(!string.IsNullOrEmpty(ga.SelectedPlatformGame[i]) ? ga.SelectedPlatformGame[i] : "N/A");
                                GUILayout.EndHorizontal();
                            }

                            GUILayout.BeginHorizontal();
                            GUILayout.Label(_publicKeyLabel, GUILayout.Width(70));
                            GUILayout.Space(-10);
                            string beforeGameKey = ga.GetGameKey(i);
                            string tmpGameKey = EditorGUILayout.TextField("", ga.GetGameKey(i));

                            if (!tmpGameKey.Equals(beforeGameKey))
                            {
                                ga.SelectedPlatformOrganization[i] = "";
                                ga.SelectedPlatformStudio[i] = "";
                                ga.SelectedPlatformGame[i] = "";
                            }

                            ga.UpdateGameKey(i, tmpGameKey);

                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            GUILayout.Label(_privateKeyLabel, GUILayout.Width(70));
                            GUILayout.Space(-10);
                            string beforeSecretKey = ga.GetSecretKey(i);
                            string tmpSecretKey = EditorGUILayout.TextField("", ga.GetSecretKey(i));

                            if (!tmpSecretKey.Equals(beforeSecretKey))
                            {
                                ga.SelectedPlatformOrganization[i] = "";
                                ga.SelectedPlatformStudio[i] = "";
                                ga.SelectedPlatformGame[i] = "";
                            }

                            ga.UpdateSecretKey(i, tmpSecretKey);

                            GUILayout.EndHorizontal();

                            EditorGUILayout.Space();

                            switch (GameAnalytics.SettingsGA.UsePlayerSettingsBuildNumber)
                            {
                                case true:
                                    if (GameAnalytics.SettingsGA.Platforms[i] != RuntimePlatform.Android && GameAnalytics.SettingsGA.Platforms[i] != RuntimePlatform.IPhonePlayer)
                                    {
                                        GUILayout.BeginHorizontal();
                                        //GUILayout.Label("", GUILayout.Width(7));
                                        GUILayout.Label(_build, GUILayout.Width(60));
                                        ga.Build[i] = EditorGUILayout.TextField("", ga.Build[i]);
                                        GUILayout.EndHorizontal();

                                        EditorGUILayout.Space();
                                    }
                                    else
                                    {
                                        if (GameAnalytics.SettingsGA.Platforms[i] == RuntimePlatform.Android)
                                        {
                                            ga.Build[i] = PlayerSettings.bundleVersion;
                                            EditorGUILayout.HelpBox("Using Android Player Settings Version* number as build number in events. \nBuild number is currently set to \"" + ga.Build[i] + "\".", MessageType.Info);
                                        }
                                        if (GameAnalytics.SettingsGA.Platforms[i] == RuntimePlatform.IPhonePlayer)
                                        {
#if UNITY_5_6_OR_NEWER
                                            ga.Build[i] = PlayerSettings.bundleVersion;
                                            EditorGUILayout.HelpBox("Using iOS Player Settings Version* number as build number in events. \nBuild number is currently set to \"" + ga.Build[i] + "\".", MessageType.Info);
#else
									    ga.Build[i] = PlayerSettings.iOS.buildNumber;
										EditorGUILayout.HelpBox("Using iOS Player Settings Build number as build number in events. \nBuild number is currently set to \"" + ga.Build[i] + "\".", MessageType.Info);
#endif
                                        }
                                    }
                                    break;
                                case false:
                                    GUILayout.BeginHorizontal();
                                    //GUILayout.Label("", GUILayout.Width(7));
                                    GUILayout.Label(_build, GUILayout.Width(60));
                                    ga.Build[i] = EditorGUILayout.TextField("", ga.Build[i]);
                                    GUILayout.EndHorizontal();

                                    EditorGUILayout.Space();
                                    break;
                            }

                            if (ga.SelectedPlatformGameID[i] >= 0)
                            {
                                EditorGUILayout.Space();
                                GUILayout.BeginHorizontal();
                                //GUILayout.Label("View", GUILayout.Width(65));
                                if (GUILayout.Button("Integration Status"))
                                {
                                    if (string.IsNullOrEmpty(ga.TokenGA))
                                    {
                                        Application.OpenURL("https://go.gameanalytics.com/game/" + ga.SelectedPlatformGameID[i] + "/initialize");
                                    }
                                    else
                                    {
                                        Application.OpenURL("https://go.gameanalytics.com/login?token=" + ga.TokenGA + "&exp=" + ga.ExpireTime + "&goto=/game/" + ga.SelectedPlatformGameID[i] + "/initialize");
                                    }
                                }
                                if (GUILayout.Button("Game Settings"))
                                {
                                    if (string.IsNullOrEmpty(ga.TokenGA))
                                    {
                                        Application.OpenURL("https://go.gameanalytics.com/game/" + ga.SelectedPlatformGameID[i] + "/settings");
                                    }
                                    else
                                    {
                                        Application.OpenURL("https://go.gameanalytics.com/login?token=" + ga.TokenGA + "&exp=" + ga.ExpireTime + "&goto=/game/" + ga.SelectedPlatformGameID[i] + "/settings");
                                    }
                                }
                                GUILayout.EndHorizontal();
                            }
                        }

                        if (GUILayout.Button("Remove platform"))
                        {
                            GameAnalytics.SettingsGA.RemovePlatformAtIndex(i);
                            this.availablePlatforms = GameAnalytics.SettingsGA.GetAvailablePlatforms();
                            this.selectedPlatformIndex = 0;
                        }

                        Splitter(new Color(0.35f, 0.35f, 0.35f));
                    }

                    if (this.availablePlatforms == null)
                    {
                        this.availablePlatforms = GameAnalytics.SettingsGA.GetAvailablePlatforms();
                    }

                    this.selectedPlatformIndex = EditorGUILayout.Popup("Platform to add", this.selectedPlatformIndex, this.availablePlatforms);
                    if (GUILayout.Button("Add platform"))
                    {
                        if (this.availablePlatforms[this.selectedPlatformIndex].Equals("WSA"))
                        {
                            GameAnalytics.SettingsGA.AddPlatform(RuntimePlatform.WSAPlayerARM);
                        }
                        else
                        {
                            GameAnalytics.SettingsGA.AddPlatform((RuntimePlatform)System.Enum.Parse(typeof(RuntimePlatform), this.availablePlatforms[this.selectedPlatformIndex]));
                        }
                        this.availablePlatforms = GameAnalytics.SettingsGA.GetAvailablePlatforms();
                        this.selectedPlatformIndex = 0;
                    }

#if UNITY_IOS || UNITY_TVOS

                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(-18));
                    EditorGUILayout.HelpBox("PLEASE NOTICE: Xcode needs to be configured to work with GameAnalytics. Click here to learn more about the build process for iOS.", MessageType.Info);

                    if(GUI.Button(GUILayoutUtility.GetLastRect(), "", GUIStyle.none))
                    {
                        Application.OpenURL("https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Configure%20XCode");
                    }
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

                    GUILayout.EndHorizontal();

#elif UNITY_ANDROID || UNITY_STANDALONE || UNITY_WEBGL || UNITY_WSA || UNITY_WP_8_1 || UNITY_TIZEN || UNITY_SAMSUNGTV

                    // Do nothing

#else

                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(-18));
                    EditorGUILayout.HelpBox("PLEASE NOTICE: Currently the GameAnalytics Unity SDK does not support your selected build Platform. Please refer to the GameAnalytics documentation for additional information.", MessageType.Warning);

                    if (GUI.Button(GUILayoutUtility.GetLastRect(), "", GUIStyle.none))
                    {
                        Application.OpenURL("http://www.gameanalytics.com/docs");
                    }
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

                    GUILayout.EndHorizontal();

#endif

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical();
                    GUILayout.Space(-4);
                    GUILayout.Label("Custom Dimensions", EditorStyles.largeLabel);
                    GUILayout.EndVertical();

                    if (!_customDimensionsIconOpen)
                    {
                        GUI.color = new Color(0.54f, 0.54f, 0.54f);
                    }
                    if (GUILayout.Button(_customDimensionsIcon, GUIStyle.none, GUILayout.Width(12), GUILayout.Height(12)))
                    {
                        _customDimensionsIconOpen = !_customDimensionsIconOpen;
                    }
                    GUI.color = Color.white;
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();

                    if (_customDimensionsIconOpen)
                    {
                        GUILayout.BeginHorizontal();
                        TextAnchor tmpAnchor = GUI.skin.box.alignment;
                        GUI.skin.box.alignment = TextAnchor.UpperLeft;
                        Color tmpColor = GUI.skin.box.normal.textColor;
                        GUI.skin.box.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                        RectOffset tmpOffset = GUI.skin.box.padding;
                        GUI.skin.box.padding = new RectOffset(6, 6, 5, 32);
                        GUILayout.Box(_customDimensionsIconMsg);
                        GUI.skin.box.alignment = tmpAnchor;
                        GUI.skin.box.normal.textColor = tmpColor;
                        GUI.skin.box.padding = tmpOffset;
                        //GUILayout.Label("Advanced settings are pretty awesome! They allow you to do all kinds of things, such as tracking Unity errors and exceptions, and frames per second (for performance). See http://www.support.gameanalytics.com", EditorStyles.wordWrappedMiniLabel);
                        GUILayout.EndHorizontal();

                        Rect tmpRect = GUILayoutUtility.GetLastRect();
                        if (GUI.Button(new Rect(tmpRect.x + 5, tmpRect.y + tmpRect.height - 25, 80, 20), "Learn more"))
                        {
                            Application.OpenURL("https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Settings#custom-dimensions");
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    // Custom dimensions 1
                    ga.CustomDimensions01FoldOut = EditorGUILayout.Foldout(ga.CustomDimensions01FoldOut, new GUIContent("   " + _customDimensions01.text + " (" + ga.CustomDimensions01.Count + " / " + MaxNumberOfDimensions + " values)", _customDimensions01.tooltip));

                    if (ga.CustomDimensions01FoldOut)
                    {
                        List<int> c1ToRemove = new List<int>();

                        for (int i = 0; i < ga.CustomDimensions01.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(21));
                            GUILayout.Label("-", GUILayout.Width(10));

                            ga.CustomDimensions01[i] = ValidateCustomDimensionEditor(EditorGUILayout.TextField(ga.CustomDimensions01[i]));

                            if (GUILayout.Button(_deleteIcon, GUI.skin.label, new GUILayoutOption[] {
                                GUILayout.Width(16),
                                GUILayout.Height(16)
                            }))
                            {
                                c1ToRemove.Add(i);
                            }
                            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                            GUILayout.EndHorizontal();
                            GUILayout.Space(2);
                        }

                        foreach (int i in c1ToRemove)
                        {
                            ga.CustomDimensions01.RemoveAt(i);
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(21));
                        if (GUILayout.Button("Add", GUILayout.Width(63)))
                        {
                            if (ga.CustomDimensions01.Count < MaxNumberOfDimensions)
                            {
                                ga.CustomDimensions01.Add("New (" + (ga.CustomDimensions01.Count + 1) + ")");
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();

                    // Custom dimensions 2
                    ga.CustomDimensions02FoldOut = EditorGUILayout.Foldout(ga.CustomDimensions02FoldOut, new GUIContent("   " + _customDimensions02.text + " (" + ga.CustomDimensions02.Count + " / " + MaxNumberOfDimensions + " values)", _customDimensions02.tooltip));

                    if (ga.CustomDimensions02FoldOut)
                    {
                        List<int> c2ToRemove = new List<int>();

                        for (int i = 0; i < ga.CustomDimensions02.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(21));
                            GUILayout.Label("-", GUILayout.Width(10));

                            ga.CustomDimensions02[i] = ValidateCustomDimensionEditor(EditorGUILayout.TextField(ga.CustomDimensions02[i]));

                            if (GUILayout.Button(_deleteIcon, GUI.skin.label, new GUILayoutOption[] {
                                GUILayout.Width(16),
                                GUILayout.Height(16)
                            }))
                            {
                                c2ToRemove.Add(i);
                            }
                            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                            GUILayout.EndHorizontal();
                            GUILayout.Space(2);
                        }

                        foreach (int i in c2ToRemove)
                        {
                            ga.CustomDimensions02.RemoveAt(i);
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(21));
                        if (GUILayout.Button("Add", GUILayout.Width(63)))
                        {
                            if (ga.CustomDimensions02.Count < MaxNumberOfDimensions)
                            {
                                ga.CustomDimensions02.Add("New (" + (ga.CustomDimensions02.Count + 1) + ")");
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();

                    // Custom dimensions 3
                    ga.CustomDimensions03FoldOut = EditorGUILayout.Foldout(ga.CustomDimensions03FoldOut, new GUIContent("   " + _customDimensions03.text + " (" + ga.CustomDimensions03.Count + " / " + MaxNumberOfDimensions + " values)", _customDimensions03.tooltip));

                    if (ga.CustomDimensions03FoldOut)
                    {
                        List<int> c3ToRemove = new List<int>();

                        for (int i = 0; i < ga.CustomDimensions03.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(21));
                            GUILayout.Label("-", GUILayout.Width(10));

                            ga.CustomDimensions03[i] = ValidateCustomDimensionEditor(EditorGUILayout.TextField(ga.CustomDimensions03[i]));

                            if (GUILayout.Button(_deleteIcon, GUI.skin.label, new GUILayoutOption[] {
                                GUILayout.Width(16),
                                GUILayout.Height(16)
                            }))
                            {
                                c3ToRemove.Add(i);
                            }
                            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                            GUILayout.EndHorizontal();
                            GUILayout.Space(2);
                        }

                        foreach (int i in c3ToRemove)
                        {
                            ga.CustomDimensions03.RemoveAt(i);
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(21));
                        if (GUILayout.Button("Add", GUILayout.Width(63)))
                        {
                            if (ga.CustomDimensions03.Count < MaxNumberOfDimensions)
                            {
                                ga.CustomDimensions03.Add("New (" + (ga.CustomDimensions03.Count + 1) + ")");
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical();
                    GUILayout.Space(-4);
                    GUILayout.Label("Resource Types", EditorStyles.largeLabel);
                    GUILayout.EndVertical();

                    if (!_resourceTypesIconOpen)
                    {
                        GUI.color = new Color(0.54f, 0.54f, 0.54f);
                    }
                    if (GUILayout.Button(_resourceTypesIcon, GUIStyle.none, new GUILayoutOption[] {
                        GUILayout.Width(12),
                        GUILayout.Height(12)
                    }))
                    {
                        _resourceTypesIconOpen = !_resourceTypesIconOpen;
                    }
                    GUI.color = Color.white;
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();

                    if (_resourceTypesIconOpen)
                    {
                        GUILayout.BeginHorizontal();
                        TextAnchor tmpAnchor = GUI.skin.box.alignment;
                        GUI.skin.box.alignment = TextAnchor.UpperLeft;
                        Color tmpColor = GUI.skin.box.normal.textColor;
                        GUI.skin.box.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                        RectOffset tmpOffset = GUI.skin.box.padding;
                        GUI.skin.box.padding = new RectOffset(6, 6, 5, 32);
                        GUILayout.Box(_resourceTypesIconMsg);
                        GUI.skin.box.alignment = tmpAnchor;
                        GUI.skin.box.normal.textColor = tmpColor;
                        GUI.skin.box.padding = tmpOffset;
                        GUILayout.EndHorizontal();

                        Rect tmpRect = GUILayoutUtility.GetLastRect();
                        if (GUI.Button(new Rect(tmpRect.x + 5, tmpRect.y + tmpRect.height - 25, 80, 20), "Learn more"))
                        {
                            Application.OpenURL("https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Settings#resource-types");
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    // Resource types

                    ga.ResourceCurrenciesFoldOut = EditorGUILayout.Foldout(ga.ResourceCurrenciesFoldOut, new GUIContent("   " + _resourceCurrrencies.text + " (" + ga.ResourceCurrencies.Count + " / " + MaxNumberOfDimensions + " values)", _resourceCurrrencies.tooltip));

                    if (ga.ResourceCurrenciesFoldOut)
                    {
                        List<int> rcToRemove = new List<int>();

                        for (int i = 0; i < ga.ResourceCurrencies.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(21));
                            GUILayout.Label("-", GUILayout.Width(10));
                            ga.ResourceCurrencies[i] = ValidateResourceCurrencyEditor(EditorGUILayout.TextField(ga.ResourceCurrencies[i]));

                            if (GUILayout.Button(_deleteIcon, GUI.skin.label, new GUILayoutOption[] {
                                GUILayout.Width(16),
                                GUILayout.Height(16)
                            }))
                            {
                                rcToRemove.Add(i);
                            }
                            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                            GUILayout.EndHorizontal();
                            GUILayout.Space(2);
                        }

                        foreach (int i in rcToRemove)
                        {
                            ga.ResourceCurrencies.RemoveAt(i);
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(21));
                        if (GUILayout.Button("Add", GUILayout.Width(63)))
                        {
                            if (ga.ResourceCurrencies.Count < MaxNumberOfDimensions)
                            {
                                ga.ResourceCurrencies.Add("NewCurrency"); // + (ga.ResourceCurrencies.Count + 1));
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();

                    ga.ResourceItemTypesFoldOut = EditorGUILayout.Foldout(ga.ResourceItemTypesFoldOut, new GUIContent("   " + _resourceItemTypes.text + " (" + ga.ResourceItemTypes.Count + " / " + MaxNumberOfDimensions + " values)", _resourceItemTypes.tooltip));

                    if (ga.ResourceItemTypesFoldOut)
                    {
                        List<int> ritToRemove = new List<int>();

                        for (int i = 0; i < ga.ResourceItemTypes.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(21));
                            GUILayout.Label("-", GUILayout.Width(10));
                            //string tmp = ga.ResourceTypes[i];
                            ga.ResourceItemTypes[i] = ValidateResourceItemTypeEditor(EditorGUILayout.TextField(ga.ResourceItemTypes[i]));

                            if (GUILayout.Button(_deleteIcon, GUI.skin.label, new GUILayoutOption[] {
                                GUILayout.Width(16),
                                GUILayout.Height(16)
                            }))
                            {
                                ritToRemove.Add(i);
                            }
                            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                            GUILayout.EndHorizontal();
                            GUILayout.Space(2);
                        }

                        foreach (int i in ritToRemove)
                        {
                            ga.ResourceItemTypes.RemoveAt(i);
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(21));
                        if (GUILayout.Button("Add", GUILayout.Width(63)))
                        {
                            if (ga.ResourceItemTypes.Count < MaxNumberOfDimensions)
                            {
                                ga.ResourceItemTypes.Add("New (" + (ga.ResourceItemTypes.Count + 1) + ")");
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();
                }
                #endregion // Settings.InspectorStates.Basic
                #region Settings.InspectorStates.Pref
                else if (ga.CurrentInspectorState == GameAnalyticsSDK.Setup.Settings.InspectorStates.Pref)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical();
                    GUILayout.Space(-4);
                    GUILayout.Label("Advanced Settings", EditorStyles.largeLabel);
                    GUILayout.EndVertical();

                    if (!_advancedSettingsIconOpen)
                    {
                        GUI.color = new Color(0.54f, 0.54f, 0.54f);
                    }
                    if (GUILayout.Button(_advancedSettingsIcon, GUIStyle.none, new GUILayoutOption[] {
                        GUILayout.Width(12),
                        GUILayout.Height(12)
                    }))
                    {
                        _advancedSettingsIconOpen = !_advancedSettingsIconOpen;
                    }
                    GUI.color = Color.white;
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();

                    if (_advancedSettingsIconOpen)
                    {
                        GUILayout.BeginHorizontal();
                        TextAnchor tmpAnchor = GUI.skin.box.alignment;
                        GUI.skin.box.alignment = TextAnchor.UpperLeft;
                        Color tmpColor = GUI.skin.box.normal.textColor;
                        GUI.skin.box.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                        RectOffset tmpOffset = GUI.skin.box.padding;
                        GUI.skin.box.padding = new RectOffset(6, 6, 5, 32);
                        GUILayout.Box(_advancedSettingsIconMsg);
                        GUI.skin.box.alignment = tmpAnchor;
                        GUI.skin.box.normal.textColor = tmpColor;
                        GUI.skin.box.padding = tmpOffset;
                        GUILayout.EndHorizontal();

                        Rect tmpRect = GUILayoutUtility.GetLastRect();
                        if (GUI.Button(new Rect(tmpRect.x + 5, tmpRect.y + tmpRect.height - 25, 80, 20), "Learn more"))
                        {
                            Application.OpenURL("https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Settings#advanced");
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(-18));
                    ga.UseManualSessionHandling = EditorGUILayout.Toggle("", ga.UseManualSessionHandling, GUILayout.Width(35));
                    GUILayout.Label(_useManualSessionHandling);
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(-18));
                    ga.UsePlayerSettingsBuildNumber = EditorGUILayout.Toggle("", ga.UsePlayerSettingsBuildNumber, GUILayout.Width(35));
                    GUILayout.Label(_usePlayerSettingsBunldeVersionForBuild);
                    GUILayout.EndHorizontal();

                    if (ga.UsePlayerSettingsBuildNumber)
                    {
#if UNITY_5_6_OR_NEWER
                        EditorGUILayout.HelpBox("PLEASE NOTICE: The SDK will use the Version* number (Android, iOS) from Player Settings as the build number in events.", MessageType.Info);
#else
                        EditorGUILayout.HelpBox("PLEASE NOTICE: The SDK will use the Build number (iOS) and the Version* number (Android) from Player Settings as the build number in events.", MessageType.Info);
#endif
                    }

                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(-18));
                    ga.SubmitErrors = EditorGUILayout.Toggle("", ga.SubmitErrors, GUILayout.Width(35));
                    GUILayout.Label(_gaSubmitErrors);
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(-18));
                    ga.NativeErrorReporting = EditorGUILayout.Toggle("", ga.NativeErrorReporting, GUILayout.Width(35));
                    GUILayout.Label(_gaNativeErrorReporting);
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(-18));
                    ga.SubmitFpsAverage = EditorGUILayout.Toggle("", ga.SubmitFpsAverage, GUILayout.Width(35));
                    GUILayout.Label(_gaFpsAverage);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(-18));
                    ga.SubmitFpsCritical = EditorGUILayout.Toggle("", ga.SubmitFpsCritical, GUILayout.Width(35));
                    GUILayout.Label(_gaFpsCritical, GUILayout.Width(135));
                    GUI.enabled = ga.SubmitFpsCritical;
                    GUILayout.Label(_gaFpsCriticalThreshold, GUILayout.Width(40));
                    GUILayout.Label("", GUILayout.Width(-26));

                    int tmpFpsCriticalThreshold = 0;
                    if (int.TryParse(EditorGUILayout.TextField(ga.FpsCriticalThreshold.ToString(), GUILayout.Width(45)), out tmpFpsCriticalThreshold))
                    {
                        ga.FpsCriticalThreshold = Mathf.Max(Mathf.Min(tmpFpsCriticalThreshold, 99), 5);
                    }
                    GUI.enabled = true;

                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical();
                    GUILayout.Space(-4);
                    GUILayout.Label("Debug Settings", EditorStyles.largeLabel);
                    GUILayout.EndVertical();

                    if (!_debugSettingsIconOpen)
                    {
                        GUI.color = new Color(0.54f, 0.54f, 0.54f);
                    }
                    if (GUILayout.Button(_debugSettingsIcon, GUIStyle.none, new GUILayoutOption[] {
                        GUILayout.Width(12),
                        GUILayout.Height(12)
                    }))
                    {
                        _debugSettingsIconOpen = !_debugSettingsIconOpen;
                    }
                    GUI.color = Color.white;
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    GUILayout.FlexibleSpace();

                    GUILayout.EndHorizontal();

                    if (_debugSettingsIconOpen)
                    {
                        GUILayout.BeginHorizontal();
                        TextAnchor tmpAnchor = GUI.skin.box.alignment;
                        GUI.skin.box.alignment = TextAnchor.UpperLeft;
                        Color tmpColor = GUI.skin.box.normal.textColor;
                        GUI.skin.box.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                        RectOffset tmpOffset = GUI.skin.box.padding;
                        GUI.skin.box.padding = new RectOffset(6, 6, 5, 32);
                        GUILayout.Box(_debugSettingsIconMsg);
                        GUI.skin.box.alignment = tmpAnchor;
                        GUI.skin.box.normal.textColor = tmpColor;
                        GUI.skin.box.padding = tmpOffset;
                        GUILayout.EndHorizontal();

                        Rect tmpRect = GUILayoutUtility.GetLastRect();
                        if (GUI.Button(new Rect(tmpRect.x + 5, tmpRect.y + tmpRect.height - 25, 80, 20), "Learn more"))
                        {
                            Application.OpenURL("https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Settings#debug-settings");
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(-18));
                    ga.InfoLogEditor = EditorGUILayout.Toggle("", ga.InfoLogEditor, GUILayout.Width(35));
                    GUILayout.Label(_infoLogEditor);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(-18));
                    ga.InfoLogBuild = EditorGUILayout.Toggle("", ga.InfoLogBuild, GUILayout.Width(35));
                    GUILayout.Label(_infoLogBuild);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(-18));
                    ga.VerboseLogBuild = EditorGUILayout.Toggle("", ga.VerboseLogBuild, GUILayout.Width(35));
                    GUILayout.Label(_verboseLogBuild);
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }
                #endregion // Settings.InspectorStates.Pref
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(ga);
            }
        }

        private MessageType ConvertMessageType(GameAnalyticsSDK.Setup.Settings.MessageTypes msgType)
        {
            switch (msgType)
            {
                case GameAnalyticsSDK.Setup.Settings.MessageTypes.Error:
                    return MessageType.Error;
                case GameAnalyticsSDK.Setup.Settings.MessageTypes.Info:
                    return MessageType.Info;
                case GameAnalyticsSDK.Setup.Settings.MessageTypes.Warning:
                    return MessageType.Warning;
                default:
                    return MessageType.None;
            }
        }

        public static void SignupUser(GameAnalyticsSDK.Setup.Settings ga, GA_SignUp signup)
        {
            Hashtable jsonTable = new Hashtable();
            jsonTable["email"] = ga.EmailGA;
            jsonTable["password"] = ga.PasswordGA;
            jsonTable["password_confirm"] = signup.PasswordConfirm;
            jsonTable["first_name"] = signup.FirstName;
            jsonTable["last_name"] = signup.LastName;
            jsonTable["studio_name"] = ga.StudioName;
            jsonTable["org_name"] = ga.OrganizationName;
            jsonTable["org_identifier"] = ga.OrganizationIdentifier;
            jsonTable["email_opt_out"] = signup.EmailOptIn;
            jsonTable["accept_terms"] = signup.AcceptedTerms;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(GA_MiniJSON.Serialize(jsonTable));

#if UNITY_2017_1_OR_NEWER
            UnityWebRequest www = new UnityWebRequest(_gaUrl + "user", UnityWebRequest.kHttpVerbPOST);
            UploadHandlerRaw uH = new UploadHandlerRaw(data)
            {
                contentType = "application/json"
            };
            www.uploadHandler = uH;
            www.downloadHandler = new DownloadHandlerBuffer();
            Dictionary<string, string> headers = GA_EditorUtilities.WWWHeaders();
            foreach (KeyValuePair<string, string> entry in headers)
            {
                www.SetRequestHeader(entry.Key, entry.Value);
            }
#else
            WWW www = new WWW(_gaUrl + "user", data, GA_EditorUtilities.WWWHeaders());
#endif

            GA_ContinuationManager.StartCoroutine(SignupUserFrontend(www, ga, signup), () => www.isDone);
        }

#if UNITY_2017_1_OR_NEWER
        private static IEnumerator SignupUserFrontend(UnityWebRequest www, GameAnalyticsSDK.Setup.Settings ga, GA_SignUp signup)
#else
        private static IEnumerator<WWW> SignupUserFrontend(WWW www, Settings ga, GA_SignUp signup)
#endif
        {
#if UNITY_2017_1_OR_NEWER

#if UNITY_2017_2_OR_NEWER
            yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif
            while (!www.isDone)
                yield return null;
#else
            yield return www;
#endif

            try
            {
                IDictionary<string, object> returnParam = null;
                string error = "";
#if UNITY_2017_1_OR_NEWER
                string text = www.downloadHandler.text;
#else
                string text = www.text;
#endif
                if (!string.IsNullOrEmpty(text))
                {
                    returnParam = GA_MiniJSON.Deserialize(text) as IDictionary<string, object>;
                    if (returnParam.ContainsKey("errors"))
                    {
                        IList<object> errorList = returnParam["errors"] as IList<object>;
                        if (errorList != null && errorList.Count > 0)
                        {
                            IDictionary<string, object> errors = errorList[0] as IDictionary<string, object>;
                            if (errors.ContainsKey("msg"))
                            {
                                error = errors["msg"].ToString();
                            }
                        }
                    }
                }

#if UNITY_2020_1_OR_NEWER
                if (!(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError))
#elif UNITY_2017_1_OR_NEWER
                if (!(www.isNetworkError || www.isHttpError))
#else
                if (string.IsNullOrEmpty(www.error))
#endif
                {
                    if (!String.IsNullOrEmpty(error))
                    {
                        Debug.LogError(error);
                        SetLoginStatus("Failed to sign up.", ga);
                        signup.SignUpFailed();
                    }
                    else if (returnParam != null)
                    {
                        IList<object> resultList = returnParam["results"] as IList<object>;
                        IDictionary<string, object> results = resultList[0] as IDictionary<string, object>;
                        ga.TokenGA = results["token"].ToString();
                        ga.ExpireTime = results["exp"].ToString();

                        ga.JustSignedUp = true;

                        //ga.SignUpOpen = false;

                        ga.Organizations = null;
                        SetLoginStatus("Signed up. Getting data.", ga);

                        GetUserData(ga);
                        signup.SignUpComplete();
                    }
                }
#if UNITY_5_4_OR_NEWER
                else if(www.responseCode == 301 || www.responseCode == 404 || www.responseCode == 410)
                {
                    Debug.LogError("Failed to sign up. GameAnalytics request not successful. API was changed. Please update your SDK to the latest version: " + www.error + " " + error);
                    SetLoginStatus("Failed to sign up. GameAnalytics request not successful. API was changed. Please update your SDK to the latest version.", ga);
                    signup.SignUpFailed();
                }
#endif
                else
                {
                    Debug.LogError("Failed to sign up: " + www.error + " " + error);
                    SetLoginStatus("Failed to sign up.", ga);
                    signup.SignUpFailed();
                }
            }
            catch
            {
                Debug.LogError("Failed to sign up");
                SetLoginStatus("Failed to sign up.", ga);
                signup.SignUpFailed();
            }
        }

        private static void LoginUser(GameAnalyticsSDK.Setup.Settings ga)
        {
            Hashtable jsonTable = new Hashtable();
            jsonTable["email"] = ga.EmailGA;
            jsonTable["password"] = ga.PasswordGA;

            byte[] data = System.Text.Encoding.UTF8.GetBytes(GA_MiniJSON.Serialize(jsonTable));

#if UNITY_2017_1_OR_NEWER
            UnityWebRequest www = new UnityWebRequest(_gaUrl + "token", UnityWebRequest.kHttpVerbPOST);
            UploadHandlerRaw uH = new UploadHandlerRaw(data)
            {
                contentType = "application/json"
            };
            www.uploadHandler = uH;
            www.downloadHandler = new DownloadHandlerBuffer();

            Dictionary<string, string> headers = GA_EditorUtilities.WWWHeaders();
            foreach (KeyValuePair<string, string> entry in headers)
            {
                www.SetRequestHeader(entry.Key, entry.Value);
            }
#else
            WWW www = new WWW(_gaUrl + "token", data, GA_EditorUtilities.WWWHeaders());
#endif
            GA_ContinuationManager.StartCoroutine(LoginUserFrontend(www, ga), () => www.isDone);
        }

#if UNITY_2017_1_OR_NEWER
        private static IEnumerator LoginUserFrontend(UnityWebRequest www, GameAnalyticsSDK.Setup.Settings ga)
#else
        private static IEnumerator<WWW> LoginUserFrontend(WWW www, Settings ga)
#endif
        {
#if UNITY_2017_1_OR_NEWER
#if UNITY_2017_2_OR_NEWER
            yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif
            while (!www.isDone)
                yield return null;
#else
            yield return www;
#endif

            try
            {
                string error = "";
                IDictionary<string, object> returnParam = null;
#if UNITY_2017_1_OR_NEWER
                string text = www.downloadHandler.text;
#else
                string text = www.text;
#endif
                if (!string.IsNullOrEmpty(text))
                {
                    returnParam = GA_MiniJSON.Deserialize(text) as IDictionary<string, object>;

                    if (returnParam.ContainsKey("errors"))
                    {
                        IList<object> errorList = returnParam["errors"] as IList<object>;
                        if (errorList != null && errorList.Count > 0)
                        {
                            IDictionary<string, object> errors = errorList[0] as IDictionary<string, object>;
                            if (errors.ContainsKey("msg"))
                            {
                                error = errors["msg"].ToString();
                            }
                        }
                    }
                }

#if UNITY_2020_1_OR_NEWER
                if (!(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError))
#elif UNITY_2017_1_OR_NEWER
                if (!(www.isNetworkError || www.isHttpError))
#else
                if (string.IsNullOrEmpty(www.error))
#endif
                {
                    if (!String.IsNullOrEmpty(error))
                    {
                        Debug.LogError(error);
                        SetLoginStatus("Failed to login.", ga);
                    }
                    else if (returnParam != null)
                    {
                        IList<object> resultList = returnParam["results"] as IList<object>;
                        IDictionary<string, object> results = resultList[0] as IDictionary<string, object>;
                        ga.TokenGA = results["token"].ToString();
                        ga.ExpireTime = results["exp"].ToString();

                        SetLoginStatus("Logged in. Getting data.", ga);

                        GetUserData(ga);
                    }
                }
#if UNITY_5_4_OR_NEWER
                else if (www.responseCode == 301 || www.responseCode == 404 || www.responseCode == 410)
                {
                    Debug.LogError("Failed to login. GameAnalytics request not successful. API was changed. Please update your SDK to the latest version: " + www.error + " " + error);
                    SetLoginStatus("Failed to login. GameAnalytics request not successful. API was changed. Please update your SDK to the latest version.", ga);
                }
#endif
                else
                {
                    Debug.LogError("Failed to login: " + www.error + " " + error);
                    SetLoginStatus("Failed to login.", ga);
                }
            }
            catch
            {
                Debug.LogError("Failed to login");
                SetLoginStatus("Failed to login.", ga);
            }
        }

        private static void GetUserData(GameAnalyticsSDK.Setup.Settings ga)
        {
#if UNITY_2017_1_OR_NEWER
            UnityWebRequest www = UnityWebRequest.Get(_gaUrl + "user");
            Dictionary<string, string> headers = GA_EditorUtilities.WWWHeadersWithAuthorization(ga.TokenGA);
            foreach (KeyValuePair<string, string> entry in headers)
            {
                www.SetRequestHeader(entry.Key, entry.Value);
            }
#else
            WWW www = new WWW(_gaUrl + "user", null, GA_EditorUtilities.WWWHeadersWithAuthorization(ga.TokenGA));
#endif
            GA_ContinuationManager.StartCoroutine(GetUserDataFrontend(www, ga), () => www.isDone);
        }

#if UNITY_2017_1_OR_NEWER
        private static IEnumerator GetUserDataFrontend(UnityWebRequest www, GameAnalyticsSDK.Setup.Settings ga)
#else
        private static IEnumerator<WWW> GetUserDataFrontend(WWW www, Settings ga)
#endif
        {
#if UNITY_2017_1_OR_NEWER
#if UNITY_2017_2_OR_NEWER
            yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif
            while (!www.isDone)
                yield return null;
#else
            yield return www;
#endif

            try
            {
                IDictionary<string, object> returnParam = null;
                string error = "";
#if UNITY_2017_1_OR_NEWER
                string text = www.downloadHandler.text;
#else
                string text = www.text;
#endif
                if (!string.IsNullOrEmpty(text))
                {
                    returnParam = GA_MiniJSON.Deserialize(text) as IDictionary<string, object>;
                    if (returnParam.ContainsKey("errors"))
                    {
                        IList<object> errorList = returnParam["errors"] as IList<object>;
                        if (errorList != null && errorList.Count > 0)
                        {
                            IDictionary<string, object> errors = errorList[0] as IDictionary<string, object>;
                            if (errors.ContainsKey("msg"))
                            {
                                error = errors["msg"].ToString();
                            }
                        }
                    }
                }

#if UNITY_2020_1_OR_NEWER
                if (!(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError))
#elif UNITY_2017_1_OR_NEWER
                if (!(www.isNetworkError || www.isHttpError))
#else
                if (string.IsNullOrEmpty(www.error))
#endif
                {
                    if (!String.IsNullOrEmpty(error))
                    {
                        Debug.LogError(error);
                        SetLoginStatus("Failed to get data.", ga);
                    }
                    else if (returnParam != null)
                    {
                        IList<object> resultList = returnParam["results"] as IList<object>;
                        IDictionary<string, object> results = resultList[0] as IDictionary<string, object>;
                        IDictionary<string, object> orgs = results["organizations"] as IDictionary<string, object>;
                        IList<object> studioList = results["studios"] as IList<object>;

                        Dictionary<string, GameAnalyticsSDK.Setup.Organization> organizationMap = new Dictionary<string, GameAnalyticsSDK.Setup.Organization>();
                        List<GameAnalyticsSDK.Setup.Organization> returnOrganizations = new List<GameAnalyticsSDK.Setup.Organization>();
                        foreach(KeyValuePair<string, object> pair in orgs)
                        {
                            IDictionary<string, object> organization = pair.Value as IDictionary<string, object>;
                            GameAnalyticsSDK.Setup.Organization o = new GameAnalyticsSDK.Setup.Organization(organization["name"].ToString(), organization["id"].ToString());
                            returnOrganizations.Add(o);
                            organizationMap.Add(o.ID, o);
                        }

                        for (int s = 0; s < studioList.Count; s++)
                        {
                            IDictionary<string, object> studio = studioList[s] as IDictionary<string, object>;

                            if ((!studio.ContainsKey("demo") || !((bool)studio["demo"])) && (!studio.ContainsKey("archived") || !((bool)studio["archived"])))
                            {
                                List<GameAnalyticsSDK.Setup.Game> returnGames = new List<GameAnalyticsSDK.Setup.Game>();

                                List<object> gamesList = (List<object>)studio["games"];
                                for (int g = 0; g < gamesList.Count; g++)
                                {
                                    IDictionary<string, object> game = gamesList[g] as IDictionary<string, object>;

                                    if ((!game.ContainsKey("archived") || !((bool)game["archived"])) && (!game.ContainsKey("disabled") || !((bool)game["disabled"])))
                                    {
                                        returnGames.Add(new GameAnalyticsSDK.Setup.Game(game["name"].ToString(), int.Parse(game["id"].ToString()), game["key"].ToString(), game["secret"].ToString()));
                                    }
                                }

                                GameAnalyticsSDK.Setup.Studio st = new GameAnalyticsSDK.Setup.Studio(studio["name"].ToString(), studio["id"].ToString(), studio["org_id"].ToString(), returnGames);
                                organizationMap[st.OrganizationID].Studios.Add(st);
                            }
                        }
                        ga.Organizations = returnOrganizations;

                        if (ga.Organizations.Count == 1 && ga.Organizations[0].Studios.Count == 1)
                        {
                            bool autoSelectedPlatform = false;
                            for (int i = 0; i < GameAnalytics.SettingsGA.Platforms.Count; ++i)
                            {
                                RuntimePlatform platform = GameAnalytics.SettingsGA.Platforms[i];

                                if (platform == ga.LastCreatedGamePlatform)
                                {
                                    SelectOrganization(1, ga, i);
                                    autoSelectedPlatform = true;
                                }
                            }
                            ga.LastCreatedGamePlatform = (RuntimePlatform)(-1);
                            SetLoginStatus(autoSelectedPlatform ? "Received data. Autoselected platform.." : "Received data. Add a platform..", ga);
                        }
                        else
                        {
                            SetLoginStatus("Received data. Add a platform..", ga);
                        }

                        ga.CurrentInspectorState = GameAnalyticsSDK.Setup.Settings.InspectorStates.Basic;
                    }
                }
#if UNITY_5_4_OR_NEWER
                else if (www.responseCode == 301 || www.responseCode == 404 || www.responseCode == 410)
                {
                    Debug.LogError("Failed to get data. GameAnalytics request not successful. API was changed. Please update your SDK to the latest version: " + www.error + " " + error);
                    SetLoginStatus("Failed to get data. GameAnalytics request not successful. API was changed. Please update your SDK to the latest version.", ga);
                }
#endif
                else
                {
                    Debug.LogError("Failed to get user data: " + www.error + " " + error);
                    SetLoginStatus("Failed to get data.", ga);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to get user data: " + e.ToString() + ", " + e.StackTrace);
                SetLoginStatus("Failed to get data.", ga);
            }
        }

        public static void CreateGame(GameAnalyticsSDK.Setup.Settings ga, GA_SignUp signup, int organizationIndex, int studioIndex, string gameTitle, string googlePlayPublicKey, RuntimePlatform platform, AppFiguresGame appFiguresGame)
        {
            Hashtable jsonTable = new Hashtable();

            if (appFiguresGame != null)
            {
                jsonTable["title"] = gameTitle;
                jsonTable["store_id"] = appFiguresGame.AppID;
                jsonTable["store"] = appFiguresGame.Store;
                jsonTable["googleplay_key"] = string.IsNullOrEmpty(googlePlayPublicKey) ? null : googlePlayPublicKey;
            }
            else
            {
                jsonTable["title"] = gameTitle;
                jsonTable["store_id"] = null;
                jsonTable["store"] = null;
                jsonTable["googleplay_key"] = string.IsNullOrEmpty(googlePlayPublicKey) ? null : googlePlayPublicKey;
            }

            byte[] data = System.Text.Encoding.UTF8.GetBytes(GA_MiniJSON.Serialize(jsonTable));

            string url = _gaUrl + "studios/" + ga.Organizations[organizationIndex].Studios[studioIndex].ID + "/games";
#if UNITY_2017_1_OR_NEWER
            UnityWebRequest www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            UploadHandlerRaw uH = new UploadHandlerRaw(data)
            {
                contentType = "application/json"
            };
            www.uploadHandler = uH;
            www.downloadHandler = new DownloadHandlerBuffer();
            Dictionary<string, string> headers = GA_EditorUtilities.WWWHeadersWithAuthorization(ga.TokenGA);
            foreach (KeyValuePair<string, string> entry in headers)
            {
                www.SetRequestHeader(entry.Key, entry.Value);
            }
#else
            WWW www = new WWW(url, data, GA_EditorUtilities.WWWHeadersWithAuthorization(ga.TokenGA));
#endif
            GA_ContinuationManager.StartCoroutine(CreateGameFrontend(www, ga, signup, platform, appFiguresGame), () => www.isDone);
        }

#if UNITY_2017_1_OR_NEWER
        private static IEnumerator CreateGameFrontend(UnityWebRequest www, GameAnalyticsSDK.Setup.Settings ga, GA_SignUp signup, RuntimePlatform platform, AppFiguresGame appFiguresGame)
#else
        private static IEnumerator<WWW> CreateGameFrontend(WWW www, Settings ga, GA_SignUp signup, RuntimePlatform platform, AppFiguresGame appFiguresGame)
#endif
        {
#if UNITY_2017_1_OR_NEWER
#if UNITY_2017_2_OR_NEWER
            yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif
            while (!www.isDone)
                yield return null;
#else
            yield return www;
#endif

            try
            {
                IDictionary<string, object> returnParam = null;
                string error = "";
#if UNITY_2017_1_OR_NEWER
                string text = www.downloadHandler.text;
#else
                string text = www.text;
#endif
                if (!string.IsNullOrEmpty(text))
                {
                    returnParam = GA_MiniJSON.Deserialize(text) as IDictionary<string, object>;
                    if (returnParam.ContainsKey("errors"))
                    {
                        IList<object> errorList = returnParam["errors"] as IList<object>;
                        if (errorList != null && errorList.Count > 0)
                        {
                            IDictionary<string, object> errors = errorList[0] as IDictionary<string, object>;
                            if (errors.ContainsKey("msg"))
                            {
                                error = errors["msg"].ToString();
                            }
                        }
                    }
                }

#if UNITY_2020_1_OR_NEWER
                if (!(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError))
#elif UNITY_2017_1_OR_NEWER
                if (!(www.isNetworkError || www.isHttpError))
#else
                if (string.IsNullOrEmpty(www.error))
#endif
                {
                    if (!String.IsNullOrEmpty(error))
                    {
                        Debug.LogError(error);
                        SetLoginStatus("Failed to create game.", ga);
                        signup.CreateGameFailed();
                    }
                    else
                    {
                        ga.LastCreatedGamePlatform = platform;
                        GetUserData(ga);
                        signup.CreateGameComplete();
                    }
                }
#if UNITY_5_4_OR_NEWER
                else if (www.responseCode == 301 || www.responseCode == 404 || www.responseCode == 410)
                {
                    Debug.LogError("Failed to create game. GameAnalytics request not successful. API was changed. Please update your SDK to the latest version: " + www.error + " " + error);
                    SetLoginStatus("Failed to create game. GameAnalytics request not successful. API was changed. Please update your SDK to the latest version.", ga);
                }
#endif
                else
                {
                    Debug.LogError("Failed to create game: " + www.error + " " + error);
                    SetLoginStatus("Failed to create game.", ga);
                    signup.CreateGameFailed();
                }
            }
            catch
            {
                Debug.LogError("Failed to create game");
                SetLoginStatus("Failed to create game.", ga);
                signup.CreateGameFailed();
            }
        }

        public static void GetAppFigures(GameAnalyticsSDK.Setup.Settings ga, GA_SignUp signup)
        {
#if UNITY_2017_1_OR_NEWER
            UnityWebRequest www = UnityWebRequest.Get(_gaUrl + "apps/search?query=" + UnityWebRequest.EscapeURL(ga.GameName));
            Dictionary<string, string> headers = GA_EditorUtilities.WWWHeadersWithAuthorization(ga.TokenGA);
            foreach (KeyValuePair<string, string> pair in headers)
            {
                www.SetRequestHeader(pair.Key, pair.Value);
            }
            GA_ContinuationManager.StartCoroutine(GetAppFiguresFrontend(www, ga, signup, ga.GameName), () => www.isDone);
#else
            WWW www = new WWW(_gaUrl + "apps/search?query=" + WWW.EscapeURL(ga.GameName), null, GA_EditorUtilities.WWWHeadersWithAuthorization(ga.TokenGA));
            GA_ContinuationManager.StartCoroutine(GetAppFiguresFrontend(www, ga, signup, ga.GameName), () => www.isDone);
#endif

            if (ga.AmazonIcon == null)
            {
#if UNITY_2017_1_OR_NEWER
                UnityWebRequest wwwAmazon = UnityWebRequestTexture.GetTexture("http://public.gameanalytics.com/resources/images/sdk_doc/appstore_icons/amazon.png");
                GA_ContinuationManager.StartCoroutine(signup.GetAppStoreIconTexture(wwwAmazon, "amazon_appstore", signup), () => wwwAmazon.isDone);
#else
                WWW wwwAmazon = new WWW("http://public.gameanalytics.com/resources/images/sdk_doc/appstore_icons/amazon.png");
                GA_ContinuationManager.StartCoroutine(signup.GetAppStoreIconTexture(wwwAmazon, "amazon_appstore", signup), () => wwwAmazon.isDone);
#endif
            }

            if (ga.GooglePlayIcon == null)
            {
#if UNITY_2017_1_OR_NEWER
                UnityWebRequest wwwGoogle = UnityWebRequestTexture.GetTexture("http://public.gameanalytics.com/resources/images/sdk_doc/appstore_icons/google_play.png");
                GA_ContinuationManager.StartCoroutine(signup.GetAppStoreIconTexture(wwwGoogle, "google_play", signup), () => wwwGoogle.isDone);
#else
                WWW wwwGoogle = new WWW("http://public.gameanalytics.com/resources/images/sdk_doc/appstore_icons/google_play.png");
                GA_ContinuationManager.StartCoroutine(signup.GetAppStoreIconTexture(wwwGoogle, "google_play", signup), () => wwwGoogle.isDone);
#endif
            }

            if (ga.iosIcon == null)
            {
#if UNITY_2017_1_OR_NEWER
                UnityWebRequest wwwIos = UnityWebRequestTexture.GetTexture("http://public.gameanalytics.com/resources/images/sdk_doc/appstore_icons/ios.png");
                GA_ContinuationManager.StartCoroutine(signup.GetAppStoreIconTexture(wwwIos, "apple:ios", signup), () => wwwIos.isDone);
#else
                WWW wwwIos = new WWW("http://public.gameanalytics.com/resources/images/sdk_doc/appstore_icons/ios.png");
                GA_ContinuationManager.StartCoroutine(signup.GetAppStoreIconTexture(wwwIos, "apple:ios", signup), () => wwwIos.isDone);
#endif
            }

            if (ga.macIcon == null)
            {
#if UNITY_2017_1_OR_NEWER
                UnityWebRequest wwwMac = UnityWebRequestTexture.GetTexture("http://public.gameanalytics.com/resources/images/sdk_doc/appstore_icons/mac.png");
                GA_ContinuationManager.StartCoroutine(signup.GetAppStoreIconTexture(wwwMac, "apple:mac", signup), () => wwwMac.isDone);
#else
                WWW wwwMac = new WWW("http://public.gameanalytics.com/resources/images/sdk_doc/appstore_icons/mac.png");
                GA_ContinuationManager.StartCoroutine(signup.GetAppStoreIconTexture(wwwMac, "apple:mac", signup), () => wwwMac.isDone);
#endif
            }

            if (ga.windowsPhoneIcon == null)
            {
#if UNITY_2017_1_OR_NEWER
                UnityWebRequest wwwWindowsPhone = UnityWebRequestTexture.GetTexture("http://public.gameanalytics.com/resources/images/sdk_doc/appstore_icons/windows_phone.png");
                GA_ContinuationManager.StartCoroutine(signup.GetAppStoreIconTexture(wwwWindowsPhone, "windows_phone", signup), () => wwwWindowsPhone.isDone);
#else
                WWW wwwWindowsPhone = new WWW("http://public.gameanalytics.com/resources/images/sdk_doc/appstore_icons/windows_phone.png");
                GA_ContinuationManager.StartCoroutine(signup.GetAppStoreIconTexture(wwwWindowsPhone, "windows_phone", signup), () => wwwWindowsPhone.isDone);
#endif
            }
        }

#if UNITY_2017_1_OR_NEWER
        private static IEnumerator GetAppFiguresFrontend(UnityWebRequest www, GameAnalyticsSDK.Setup.Settings ga, GA_SignUp signup, string gameName)
#else
        private static IEnumerator<WWW> GetAppFiguresFrontend(WWW www, Settings ga, GA_SignUp signup, string gameName)
#endif
        {
#if UNITY_2017_1_OR_NEWER
#if UNITY_2017_2_OR_NEWER
            yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif
            while (!www.isDone)
                yield return null;
#else
            yield return www;
#endif

            try
            {
                IDictionary<string, object> returnParam = null;
                string error = "";
                string text;
#if UNITY_2017_1_OR_NEWER
                text = www.downloadHandler.text;
#else
                text = www.text;
#endif
                if (!string.IsNullOrEmpty(text))
                {
                    returnParam = GA_MiniJSON.Deserialize(text) as IDictionary<string, object>;
                    if (returnParam.ContainsKey("errors"))
                    {
                        IList<object> errorList = returnParam["errors"] as IList<object>;
                        if (errorList != null && errorList.Count > 0)
                        {
                            IDictionary<string, object> errors = errorList[0] as IDictionary<string, object>;
                            if (errors.ContainsKey("msg"))
                            {
                                error = errors["msg"].ToString();
                            }
                        }
                    }
                }

#if UNITY_2020_1_OR_NEWER
                if (!(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError))
#elif UNITY_2017_1_OR_NEWER
                if (!(www.isNetworkError || www.isHttpError))
#else
                if (string.IsNullOrEmpty(www.error))
#endif
                {
                    if (!String.IsNullOrEmpty(error))
                    {
                        Debug.LogError(error);
                        SetLoginStatus("Failed to get app.", ga);
                    }
                    else if (returnParam != null)
                    {
                        IList<object> resultList = returnParam["results"] as IList<object>;

                        List<AppFiguresGame> appFiguresGames = new List<AppFiguresGame>();
                        for (int s = 0; s < resultList.Count; s++)
                        {
                            IDictionary<string, object> result = resultList[s] as IDictionary<string, object>;

                            string name = result["title"].ToString();
                            string appID = result["store_id"].ToString();
                            string store = result["store"].ToString();
                            string developer = result["developer"].ToString();
                            string iconUrl = result["image"].ToString();

                            if (store.Equals("apple") || store.Equals("google_play") || store.Equals("amazon_appstore"))
                            {
                                appFiguresGames.Add(new AppFiguresGame(name, appID, store, developer, iconUrl, signup));
                            }
                        }

                        signup.AppFigComplete(gameName, appFiguresGames);
                    }
                }
                else
                {
                    // expired tokens / not signed in
#if UNITY_2017_1_OR_NEWER
                    if (www.responseCode == 401)
#else
                    if (www.responseHeaders["status"] != null && www.responseHeaders["status"].Contains("401"))
#endif
                    {
                        Selection.objects = new UnityEngine.Object[] { AssetDatabase.LoadAssetAtPath("Assets/Resources/GameAnalytics/Settings.asset", typeof(GameAnalyticsSDK.Setup.Settings)) };
                        ga.CurrentInspectorState = GameAnalyticsSDK.Setup.Settings.InspectorStates.Account;
                        string message = "Please sign-in and try again to search for your game in the stores.";
                        SetLoginStatus(message, ga);
                        Debug.LogError(message);
                    }
#if UNITY_5_4_OR_NEWER
                    else if (www.responseCode == 301 || www.responseCode == 404 || www.responseCode == 410)
                    {
                        Debug.LogError("Failed to find app. GameAnalytics request not successful. API was changed. Please update your SDK to the latest version: " + www.error + " " + error);
                        SetLoginStatus("Failed to find app. GameAnalytics request not successful. API was changed. Please update your SDK to the latest version.", ga);
                    }
#endif
                    else
                    {
                        Debug.LogError("Failed to find app: " + www.error + " " + text);
                        SetLoginStatus("Failed to find app.", ga);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to find app: " + e.ToString() + ", " + e.StackTrace);
                SetLoginStatus("Failed to find app.", ga);
            }
        }

        private static void SelectOrganization(int index, GameAnalyticsSDK.Setup.Settings ga, int platform)
        {
            ga.SelectedOrganization[platform] = index;
            if (ga.Organizations[index - 1].Studios.Count == 1)
            {
                SelectStudio(1, ga, platform);
            }
            else
            {
                SetLoginStatus("Please select studio..", ga);
            }
        }

        private static void SelectStudio(int index, GameAnalyticsSDK.Setup.Settings ga, int platform)
        {
            ga.SelectedStudio[platform] = index;
            if (ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[index - 1].Games.Count == 1)
            {
                if (ga.IsGameKeyValid(platform, ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[ga.SelectedStudio[platform] - 1].Games[0].GameKey) &&
                   ga.IsSecretKeyValid(platform, ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[ga.SelectedStudio[platform] - 1].Games[0].SecretKey))
                {
                    SelectGame(1, ga, platform);
                }
            }
            else
            {
                SetLoginStatus("Please select game..", ga);
            }
        }

        private static void SelectGame(int index, GameAnalyticsSDK.Setup.Settings ga, int platform)
        {
            ga.SelectedGame[platform] = index;

            if (index == 0)
            {
                ga.UpdateGameKey(platform, "");
                ga.UpdateSecretKey(platform, "");
            }
            else if (ga.IsGameKeyValid(platform, ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[ga.SelectedStudio[platform] - 1].Games[index - 1].GameKey) &&
               ga.IsSecretKeyValid(platform, ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[ga.SelectedStudio[platform] - 1].Games[index - 1].SecretKey))
            {
                ga.SelectedPlatformOrganization[platform] = ga.Organizations[ga.SelectedOrganization[platform] - 1].Name;
                ga.SelectedPlatformStudio[platform] = ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[ga.SelectedStudio[platform] - 1].Name;
                ga.SelectedPlatformGame[platform] = ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[ga.SelectedStudio[platform] - 1].Games[index - 1].Name;
                ga.SelectedPlatformGameID[platform] = ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[ga.SelectedStudio[platform] - 1].Games[index - 1].ID;
                ga.UpdateGameKey(platform, ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[ga.SelectedStudio[platform] - 1].Games[index - 1].GameKey);
                ga.UpdateSecretKey(platform, ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[ga.SelectedStudio[platform] - 1].Games[index - 1].SecretKey);
                SetLoginStatus("Received keys. Ready to go!", ga);
            }
            else
            {
                if (!ga.IsGameKeyValid(platform, ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[ga.SelectedStudio[platform] - 1].Games[index - 1].GameKey))
                {
                    Debug.LogError("[GameAnalytics] Game key already exists for another platform. Platforms can't use the same key.");
                    ga.SelectedGame[platform] = 0;
                }
                else if (!ga.IsSecretKeyValid(platform, ga.Organizations[ga.SelectedOrganization[platform] - 1].Studios[ga.SelectedStudio[platform] - 1].Games[index - 1].SecretKey))
                {
                    Debug.LogError("[GameAnalytics] Secret key already exists for another platform. Platforms can't use the same key.");
                    ga.SelectedGame[platform] = 0;
                }
            }
        }

        private static void SetLoginStatus(string status, GameAnalyticsSDK.Setup.Settings ga)
        {
            ga.LoginStatus = status;
            EditorUtility.SetDirty(ga);
        }

        public static void CheckForUpdates()
        {
            if (GameAnalyticsSDK.Setup.Settings.CheckingForUpdates)
            {
                return;
            }

            GameAnalyticsSDK.Setup.Settings.CheckingForUpdates = true;
#if UNITY_2017_1_OR_NEWER
            UnityWebRequest www = UnityWebRequest.Get("https://s3.amazonaws.com/public.gameanalytics.com/sdk_status/current.json");
#else
            WWW www = new WWW("https://s3.amazonaws.com/public.gameanalytics.com/sdk_status/current.json");
#endif
            GA_ContinuationManager.StartCoroutine(CheckForUpdatesCoroutine(www), () => www.isDone);
        }

        private static void GetChangeLogsAndShowUpdateWindow(string newVersion)
        {
#if UNITY_2017_1_OR_NEWER
            UnityWebRequest www = UnityWebRequest.Get("https://s3.amazonaws.com/public.gameanalytics.com/sdk_status/change_logs.json");
#else
            WWW www = new WWW("https://s3.amazonaws.com/public.gameanalytics.com/sdk_status/change_logs.json");
#endif
            GA_ContinuationManager.StartCoroutine(GetChangeLogsAndShowUpdateWindowCoroutine(www, newVersion), () => www.isDone);
        }

#if UNITY_2017_1_OR_NEWER
        private static IEnumerator CheckForUpdatesCoroutine(UnityWebRequest www)
#else
        private static IEnumerator CheckForUpdatesCoroutine(WWW www)
#endif
        {
#if UNITY_2017_1_OR_NEWER
#if UNITY_2017_2_OR_NEWER
            yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif
            while (!www.isDone)
                yield return null;
#else
            yield return www;
#endif

            try
            {
#if UNITY_2020_1_OR_NEWER
                if (!(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError))
#elif UNITY_2017_1_OR_NEWER
                if (!(www.isNetworkError || www.isHttpError))
#else
                if (string.IsNullOrEmpty(www.error))
#endif
                {
                    string text;
#if UNITY_2017_1_OR_NEWER
                    text = www.downloadHandler.text;
#else
                    text = www.text;
#endif
                    IDictionary<string, object> returnParam = GA_MiniJSON.Deserialize(text) as IDictionary<string, object>;
                    if (returnParam.ContainsKey("unity"))
                    {
                        IDictionary<string, object> unityParam = returnParam["unity"] as IDictionary<string, object>;
                        if (unityParam.ContainsKey("version"))
                        {
                            string newVersion = (returnParam["unity"] as IDictionary<string, object>)["version"].ToString();

                            if (IsNewVersion(newVersion, GameAnalyticsSDK.Setup.Settings.VERSION))
                            {
                                GetChangeLogsAndShowUpdateWindow(newVersion);
                            }
                        }
                    }
                }
            }
            catch
            {
                GameAnalyticsSDK.Setup.Settings.CheckingForUpdates = false;
            }
        }

#if UNITY_2017_1_OR_NEWER
        private static IEnumerator GetChangeLogsAndShowUpdateWindowCoroutine(UnityWebRequest www, string newVersion)
#else
        private static IEnumerator<WWW> GetChangeLogsAndShowUpdateWindowCoroutine(WWW www, string newVersion)
#endif
        {
#if UNITY_2017_1_OR_NEWER
#if UNITY_2017_2_OR_NEWER
            yield return www.SendWebRequest();
#else
            yield return www.Send();
#endif
            while (!www.isDone)
                yield return null;
#else
            yield return www;
#endif

            try
            {
#if UNITY_2020_1_OR_NEWER
                if (!(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError))
#elif UNITY_2017_1_OR_NEWER
                if (!(www.isNetworkError || www.isHttpError))
#else
                if (string.IsNullOrEmpty(www.error))
#endif
                {
                    string text;
#if UNITY_2017_1_OR_NEWER
                    text = www.downloadHandler.text;
#else
                    text = www.text;
#endif
                    IDictionary<string, object> returnParam = GA_MiniJSON.Deserialize(text) as IDictionary<string, object>;

                    IList<object> unity = (returnParam["unity"] as IList<object>);
                    string newChanges = "";
                    for (int i = 0; i < unity.Count; i++)
                    {
                        IDictionary<string, object> unityHash = unity[i] as IDictionary<string, object>;
                        IList<object> changes = (unityHash["changes"] as IList<object>);

                        if (unityHash["version"].ToString() == GameAnalyticsSDK.Setup.Settings.VERSION)
                        {
                            break;
                        }

                        if (string.IsNullOrEmpty(newChanges))
                        {
                            newChanges = unityHash["version"].ToString();
                        }
                        else
                        {
                            newChanges += "\n\n" + unityHash["version"].ToString();
                        }

                        for (int u = 0; u < changes.Count; u++)
                        {
                            if (string.IsNullOrEmpty(newChanges))
                            {
                                newChanges = "- " + changes[u].ToString();
                            }
                            else
                            {
                                newChanges += "\n- " + changes[u].ToString();
                            }
                        }

                        if (unityHash["version"].ToString() == newVersion)
                        {
                            GA_UpdateWindow.SetNewVersion(newVersion);
                        }
                    }

                    string skippedVersion = EditorPrefs.GetString("ga_skip_version" + "-" + Application.dataPath, "");

                    GA_UpdateWindow.SetChanges(newChanges);
                    if (!skippedVersion.Equals(newVersion))
                    {
                        OpenUpdateWindow();
                    }

                    GameAnalyticsSDK.Setup.Settings.CheckingForUpdates = false;
                }
            }
            catch
            {
                GameAnalyticsSDK.Setup.Settings.CheckingForUpdates = false;
            }
        }

        private static void OpenUpdateWindow()
        {
#if UNITY_2018_2_OR_NEWER
            if(!Application.isBatchMode)
#else
            string commandLineOptions = System.Environment.CommandLine;
            if (!commandLineOptions.Contains("-batchmode"))
#endif
            {
                // TODO: possible to close existing window if already there?
                //GA_UpdateWindow updateWindow = ScriptableObject.CreateInstance<GA_UpdateWindow> ();
                GA_UpdateWindow updateWindow = (GA_UpdateWindow)EditorWindow.GetWindow(typeof(GA_UpdateWindow), utility: true);
                updateWindow.position = new Rect(150, 150, 415, 340);
                updateWindow.titleContent = new GUIContent("An update for GameAnalytics is available!");
                updateWindow.Show();
            }
        }

        public static void Splitter(Color rgb, float thickness = 1, int margin = 0)
        {
            GUIStyle splitter = new GUIStyle();
            splitter.normal.background = EditorGUIUtility.whiteTexture;
            splitter.stretchWidth = true;
            splitter.margin = new RectOffset(margin, margin, 7, 7);

            Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitter, GUILayout.Height(thickness));

            if(Event.current.type == EventType.Repaint)
            {
                Color restoreColor = GUI.color;
                GUI.color = rgb;
                splitter.Draw(position, false, false, false, false);
                GUI.color = restoreColor;
            }
        }

        private static string PlatformToString(RuntimePlatform platform)
        {
            string result = platform.ToString();

            if (platform == RuntimePlatform.IPhonePlayer)
            {
                result = "iOS";
            }
            if (platform == RuntimePlatform.tvOS) {
                result = "tvOS";
            }
            else if (platform == RuntimePlatform.WSAPlayerARM ||
                platform == RuntimePlatform.WSAPlayerX64 ||
                platform == RuntimePlatform.WSAPlayerX86)
            {
                result = "WSA";
            }

            return result;
        }

        // versionstring is:
        // [majorVersion].[minorVersion].[patchnumber]
        static bool IsNewVersion(string newVersion, string currentVersion)
        {
            int[] newVersionInts = GetVersionIntegersFromString(newVersion);
            int[] currentVersionInts = GetVersionIntegersFromString(currentVersion);

            if(newVersionInts == null || currentVersionInts == null)
            {
                return false;
            }

            // compare majorVersion
            if(newVersionInts[0] > currentVersionInts[0])
            {
                return true;
            }
            else if(newVersionInts[0] < currentVersionInts[0])
            {
                return false;
            }

            // compare minorVersion (majorVersion is unchanged)
            if(newVersionInts[1] > currentVersionInts[1])
            {
                return true;
            }
            else if(newVersionInts[1] < currentVersionInts[1])
            {
                return false;
            }

            // compare patchnumber (majorVersion, minorVersion is unchanged)
            if(newVersionInts[2] > currentVersionInts[2])
            {
                return true;
            }

            // not valid new version
            return false;
        }

        // version string need to be: x.y.z
        // return validated ints in array or null
        static int[] GetVersionIntegersFromString(string versionString)
        {
            string[] versionNumbers = versionString.Split('.');
            if(versionNumbers.Length != 3)
            {
                return null;
            }

            // container for validated version integers
            int[] validatedVersionNumbers = new int[3];

            // verify int parsing
            bool isIntMajorVersion = int.TryParse(versionNumbers[0], out validatedVersionNumbers[0]);
            bool isIntMinorVersion = int.TryParse(versionNumbers[1], out validatedVersionNumbers[1]);
            bool isIntPatchnumber = int.TryParse(versionNumbers[2], out validatedVersionNumbers[2]);

            if(isIntMajorVersion && isIntMinorVersion && isIntPatchnumber)
            {
                return validatedVersionNumbers;
            }
            else
            {
                return null;
            }
        }

#region Button actions

        private void OpenSignUp()
        {
            GameAnalyticsSDK.Setup.Settings ga = target as GameAnalyticsSDK.Setup.Settings;
            ga.IntroScreen = false;
            ga.CurrentInspectorState = GameAnalyticsSDK.Setup.Settings.InspectorStates.Account;
            ga.SignUpOpen = true;

            GA_SignUp signup = ScriptableObject.CreateInstance<GA_SignUp>();
            signup.maxSize = new Vector2(640, 600);
            signup.minSize = new Vector2(640, 600);
            signup.titleContent = new GUIContent("GameAnalytics - Sign up for FREE");
            signup.ShowUtility();
            signup.Opened();
        }

#endregion // Button actions

#region Helper functions

        private static void OpenSignUpSwitchToGuideStep()
        {
            GA_SignUp signup = ScriptableObject.CreateInstance<GA_SignUp>();
            signup.maxSize = new Vector2(640, 600);
            signup.minSize = new Vector2(640, 600);
            signup.titleContent = new GUIContent("GameAnalytics - Sign up for FREE");
            signup.ShowUtility();
            signup.Opened();

            signup.SwitchToGuideStep();
        }

        private static void DrawLinkButton(string text, GUIStyle style, string url, params GUILayoutOption[] options)
        {
            DrawLinkButton(new GUIContent(text), style, url, options);
        }

        private static void DrawLinkButton(GUIContent content, GUIStyle style, string url, params GUILayoutOption[] options)
        {
            Action action = () => Application.OpenURL(url);
            DrawButton(content, style, action, options);
        }

        private static void DrawButton(string text, GUIStyle style, Action action, params GUILayoutOption[] options)
        {
            DrawButton(new GUIContent(text), style, action, options);
        }

        private static void DrawButton(GUIContent content, GUIStyle style, Action action, params GUILayoutOption[] options)
        {
            if(GUILayout.Button(content, style, options))
            {
                action();
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

        private static void DrawButtonWithFlexibleSpace(string text, GUIStyle style, Action action, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawButton(text, style, action, options);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static void DrawLabelWithFlexibleSpace(string text, GUIStyle style, int height)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(text, style, new GUILayoutOption[] { GUILayout.Height(height) });
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

#endregion // Helper functions

#region UIvalidation

		/// <summary>
		/// Check if a string matches a defined pattern
		/// </summary>
		/// <returns><c>true</c>, if match <c>false</c> otherwise.</returns>
		/// <param name="s">Given string</param>
		/// <param name="pattern">Pattern.</param>
		public static bool StringMatch(string s, string pattern)
		{
			if(s == null || pattern == null)
			{
				return false;
			}

			return Regex.IsMatch(s, pattern);
		}

		private string ValidateResourceCurrencyEditor(string currency)
		{
			if (!StringMatch (currency, "^[A-Za-z]+$")) {
				if (currency != null) {
					Debug.LogError ("Validation fail - resource currency: Cannot contain other characters than 'A-Za-z'. String:'" + currency + "'");
				}
				return "Empty";
			}
			if (ConsistsOfWhiteSpace(currency)) {
				return "Empty";
			}
			return currency;
		}

		private string ValidateResourceItemTypeEditor (string itemType)
		{
			if (itemType.Length > 64) {
				Debug.LogError ("Validation fail - resource itemType cannot be longer than 64 chars.");
				return "Empty";
			}
			if (!StringMatch (itemType, "^[A-Za-z0-9\\s\\-_\\.\\(\\)\\!\\?]{1,64}$")) {
				if (itemType != null) {
					Debug.LogError ("Validation fail - resource itemType: Cannot contain other characters than A-z, 0-9, -_., ()!?. String: '" + itemType + "'");
				}
				return "Empty";
			}
			if (ConsistsOfWhiteSpace(itemType)) {
				return "Empty";
			}
			return itemType;
		}

		private string ValidateCustomDimensionEditor(string customDimension)
		{
			if (customDimension.Length > 32) {
				Debug.LogError ("Validation fail - custom dimension cannot be longer than 32 chars.");
				return "Empty";
			}
			if (!StringMatch (customDimension, "^[A-Za-z0-9\\s\\-_\\.\\(\\)\\!\\?]{1,32}$")) {
				if (customDimension != null) {
					Debug.LogError ("Validation fail - custom dimension: Cannot contain other characters than A-z, 0-9, -_., ()!?. String: '" + customDimension + "'");
				}
				return "Empty";
			}
			if (ConsistsOfWhiteSpace(customDimension)) {
				return "Empty";
			}
			return customDimension;
		}

		private bool ConsistsOfWhiteSpace(string s)
		{
			foreach (char c in s) {
				if (c != ' ')
					return false;
			}
			return true;
		}

#endregion
    }
}
