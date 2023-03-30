using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using GameAnalyticsSDK.Setup;
#if UNITY_2017_1_OR_NEWER
using UnityEngine.Networking;
#endif

namespace GameAnalyticsSDK.Editor
{
    public class GA_SignUp : EditorWindow
    {
        private GUIContent _firstNameLabel = new GUIContent("First name", "Your first name.");
        private GUIContent _lastNameLabel = new GUIContent("Last name", "Your last name (surname).");
        private GUIContent _studioNameLabel = new GUIContent("Studio name", "Your studio's name. You can add more studios and games on the GameAnalytics website.");
        private GUIContent _organizationNameLabel = new GUIContent("Organization name", "Your organization's name. You can add more studios and games under your organization on the GameAnalytics website.");
        private GUIContent _organizationIdentifierLabel = new GUIContent("Organization identifier", "Your organization's identifier to be used in url. Must be unique. Can only contain lowercase letters, digits and hyphens");
        private GUIContent _gameNameLabel = new GUIContent("Game name", "Your game's name. You can add more studies and games on the GameAnalytics website.");
        private GUIContent _passwordConfirmLabel = new GUIContent("Confirm password", "Your GameAnalytics user account password.");
        private GUIContent _emailOptInLabel = new GUIContent("Subscribe to release updates, news and tips and tricks.", "If enabled GameAnalytics may send you news about updates, cool tips and tricks, and other news to help you get the most out of our service.");
        private GUIContent _termsLabel = new GUIContent("I have read and agree with your");
        private GUIContent _emailLabel = new GUIContent("Email", "Your GameAnalytics user account email.");
        private GUIContent _passwordLabel = new GUIContent("Password", "Your GameAnalytics user account password. Must be at least 8 characters in length.");
        //private GUIContent _studiosLabel            = new GUIContent("Studio", "Studios tied to your GameAnalytics user account.");
        //private GUIContent _gamesLabel                = new GUIContent("Game", "Games tied to the selected GameAnalytics studio.");

        public int TourStep = 0;
        public bool AcceptedTerms = false;
        public string FirstName = "";
        public string LastName = "";
        public string PasswordConfirm = "";
        public bool EmailOptIn = true;

        private Vector2 _appScrollPos;
        private string _appFigName;

        private const int INPUT_WIDTH = 230;

        private List<AppFiguresGame> _appFiguresGames;
        private AppFiguresGame _appFiguresGame;

        private static GA_SignUp _instance;

        private bool _signUpInProgress = false;
        private bool _createGameInProgress = false;
        private string _googlePlayPublicKey = "";
        private RuntimePlatform _selectedPlatform;
        private int _selectedOrganization;
        private int _selectedStudio;

        private enum StringType
        {
            Label,
            TextBox,
            Link

        }

        private struct StringWithType
        {
            public string Text;
            public StringType Type;
            public string Link;
        }

        public void Opened()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Close();
            }
        }

        void OnDisable()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        void OnGUI()
        {
            switch (TourStep)
            {
                #region sign up
                case 0: // sign up

                    GUILayout.Space(20);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(GameAnalytics.SettingsGA.UserIcon, new GUILayoutOption[] {
                        GUILayout.Width(40),
                        GUILayout.MaxHeight(40)
                    });
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Create your account", EditorStyles.whiteLargeLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // first name

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_firstNameLabel, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    FirstName = EditorGUILayout.TextField("", FirstName, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // last name

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_lastNameLabel, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    LastName = EditorGUILayout.TextField("", LastName, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // e-mail

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_emailLabel, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GameAnalytics.SettingsGA.EmailGA = EditorGUILayout.TextField("", GameAnalytics.SettingsGA.EmailGA, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // password

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_passwordLabel, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GameAnalytics.SettingsGA.PasswordGA = EditorGUILayout.PasswordField("", GameAnalytics.SettingsGA.PasswordGA, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // confirm password

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_passwordConfirmLabel, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    PasswordConfirm = EditorGUILayout.PasswordField("", PasswordConfirm, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // studio name

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_studioNameLabel, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GameAnalytics.SettingsGA.StudioName = EditorGUILayout.TextField("", GameAnalytics.SettingsGA.StudioName, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // organization name

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_organizationNameLabel, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GameAnalytics.SettingsGA.OrganizationName = EditorGUILayout.TextField("", GameAnalytics.SettingsGA.OrganizationName, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // organization identifier

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_organizationIdentifierLabel, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GameAnalytics.SettingsGA.OrganizationIdentifier = EditorGUILayout.TextField("", GameAnalytics.SettingsGA.OrganizationIdentifier, GUILayout.Width(INPUT_WIDTH));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // email opt in

                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EmailOptIn = EditorGUILayout.Toggle("", EmailOptIn, GUILayout.Width(15));
                    GUILayout.Label(_emailOptInLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // terms of service

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    AcceptedTerms = EditorGUILayout.Toggle("", AcceptedTerms, GUILayout.Width(15));
                    GUILayout.Label(_termsLabel, GUILayout.Width(171));
                    GUILayout.Space(-5);
                    GUILayout.BeginVertical();
                    GUILayout.Space(2);
                    if (GUILayout.Button("Terms of Service", EditorStyles.boldLabel, GUILayout.Width(105)))
                    {
                        Application.OpenURL("http://www.gameanalytics.com/terms");
                    }
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // create account button

                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.enabled = !_signUpInProgress;
                    if (AcceptedTerms)
                    {
                        if (GUILayout.Button("Create account", new GUILayoutOption[] {
                            GUILayout.Width(200),
                            GUILayout.MaxHeight(30)
                        }))
                        {
                            _signUpInProgress = true;
                            GA_SettingsInspector.SignupUser(GameAnalytics.SettingsGA, this);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Please read and agree with our terms before you can create an account.", MessageType.Warning);
                    }

                    GUI.enabled = true;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    break;
                #endregion // sign up

                #region add your game
                case 1: // add your game

                    GUILayout.Space(20);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(GameAnalytics.SettingsGA.GameIcon, new GUILayoutOption[] {
                        GUILayout.Width(40),
                        GUILayout.MaxHeight(40)
                    });
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Add your game", EditorStyles.whiteLargeLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("You can quickly add your game by searching the app store.");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    // game name

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_gameNameLabel, GUILayout.Width(500));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    int tmpFontSize = GUI.skin.textField.fontSize;
                    Vector2 tmpOffset = GUI.skin.textField.contentOffset;
                    GUI.skin.textField.fontSize = 12;
                    GUI.skin.textField.contentOffset = new Vector2(6, 6);
                    GameAnalytics.SettingsGA.GameName = EditorGUILayout.TextField("", GameAnalytics.SettingsGA.GameName, new GUILayoutOption[] {
                        GUILayout.Width(300),
                        GUILayout.Height(30)
                    });
                    GUI.skin.textField.fontSize = tmpFontSize;
                    GUI.skin.textField.contentOffset = tmpOffset;
                    if (GUILayout.Button("Find your game", new GUILayoutOption[] {
                        GUILayout.Width(200),
                        GUILayout.MaxHeight(30)
                    }))
                    {
                        GA_SettingsInspector.GetAppFigures(GameAnalytics.SettingsGA, this);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GA_SettingsInspector.Splitter(new Color(0.35f, 0.35f, 0.35f), 1, 30);

                    if (_appFiguresGames != null && _appFiguresGames.Count > 0)
                    {
                        EditorGUILayout.Space();

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("'" + _appFigName + "' matched " + _appFiguresGames.Count + " titles.", EditorStyles.boldLabel);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        EditorGUILayout.Space();

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        _appScrollPos = GUILayout.BeginScrollView(_appScrollPos, GUI.skin.box, GUILayout.Width(571));

                        for (int i = 0; i < _appFiguresGames.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            if (_appFiguresGames[i].Icon != null)
                            {
                                GUILayout.Label(_appFiguresGames[i].Icon, new GUILayoutOption[] {
                                    GUILayout.Width(32),
                                    GUILayout.Height(32)
                                });
                            }
                            else
                            {
                                GUILayout.Label("", new GUILayoutOption[] {
                                    GUILayout.Width(32),
                                    GUILayout.Height(32)
                                });
                            }
                            Rect lastRect = GUILayoutUtility.GetLastRect();
                            Vector2 tmpOffsetLabel = GUI.skin.label.contentOffset;
                            GUI.skin.label.contentOffset = new Vector2(0, 9);
                            GUILayout.Label(_appFiguresGames[i].Name, GUILayout.Width(200));
                            GUILayout.Label(_appFiguresGames[i].Developer, GUILayout.Width(200));

                            PaintAppStoreIcon(_appFiguresGames[i].Store);

                            GUI.skin.label.contentOffset = tmpOffsetLabel;
                            GUILayout.EndHorizontal();
                            GA_SettingsInspector.Splitter(new Color(0.35f, 0.35f, 0.35f), 1, 10);

                            Rect appFigRect = new Rect(lastRect.x - 5, lastRect.y - 5, lastRect.width + 520, lastRect.height + 10);
                            if (GUI.Button(appFigRect, "", GUIStyle.none))
                            {
                                _appFiguresGame = _appFiguresGames[i];
                                TourStep = 3;
                            }
                            EditorGUIUtility.AddCursorRect(appFigRect, MouseCursor.Link);
                        }

                        GUILayout.EndScrollView();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("If your game is still in development or not in the app store, please add it manually.");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    // create new game button

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Create new game", new GUILayoutOption[] {
                        GUILayout.Width(200),
                        GUILayout.Height(30)
                    }))
                    {
                        TourStep = 2;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    break;
                #endregion // add your game

                #region create new game
                case 2: // create new game

                    GUILayout.Space(20);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(GameAnalytics.SettingsGA.GameIcon, new GUILayoutOption[] {
                        GUILayout.Width(40),
                        GUILayout.MaxHeight(40)
                    });
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Create new game", EditorStyles.whiteLargeLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    // game name

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_gameNameLabel, GUILayout.Width(300));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    int tmpFontSize2 = GUI.skin.textField.fontSize;
                    Vector2 tmpOffset2 = GUI.skin.textField.contentOffset;
                    GUI.skin.textField.fontSize = 12;
                    GUI.skin.textField.contentOffset = new Vector2(5, 5);
                    GameAnalytics.SettingsGA.GameName = EditorGUILayout.TextField("", GameAnalytics.SettingsGA.GameName, new GUILayoutOption[] {
                        GUILayout.Width(300),
                        GUILayout.Height(30)
                    });
                    GUI.skin.textField.fontSize = tmpFontSize2;
                    GUI.skin.textField.contentOffset = tmpOffset2;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    this._selectedOrganization = EditorGUILayout.Popup("", this._selectedOrganization, Organization.GetOrganizationNames(GameAnalytics.SettingsGA.Organizations, false), GUILayout.Width(200));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    this._selectedStudio = EditorGUILayout.Popup("", this._selectedStudio, this._selectedOrganization > 0 ? Studio.GetStudioNames(GameAnalytics.SettingsGA.Organizations[this._selectedOrganization - 1].Studios, false) : new string[0], GUILayout.Width(200));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GameAnalyticsSDK.Setup.Settings settings = CreateInstance<GameAnalyticsSDK.Setup.Settings>();
                    this._selectedPlatform = (RuntimePlatform)EditorGUILayout.Popup("", (int)this._selectedPlatform, settings.GetAvailablePlatforms(), GUILayout.Width(200));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    if (this._selectedPlatform == RuntimePlatform.Android)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("PLEASE NOTICE: If you want to validate your Android in-app purchase please enter your Google Play License key (public key). Click here to learn more about the Google Play License key.", MessageType.Info);

                        if (GUI.Button(GUILayoutUtility.GetLastRect(), "", GUIStyle.none))
                        {
                            //Application.OpenURL("https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Configure%20XCode");
                        }
                        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

                        GUILayout.EndHorizontal();

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Google Play License key", GUILayout.Width(150));
                        this._googlePlayPublicKey = GUILayout.TextField(this._googlePlayPublicKey);
                        GUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    // create game button

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.enabled = !_createGameInProgress;
                    if (GUILayout.Button("Create game", new GUILayoutOption[] {
                        GUILayout.Width(200),
                        GUILayout.MaxHeight(30)
                    }))
                    {
                        _createGameInProgress = true;
                        GA_SettingsInspector.CreateGame(GameAnalytics.SettingsGA, this, this._selectedOrganization, this._selectedStudio, GameAnalytics.SettingsGA.GameName, this._googlePlayPublicKey, this._selectedPlatform, null);
                    }
                    GUI.enabled = true;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GA_SettingsInspector.Splitter(new Color(0.35f, 0.35f, 0.35f), 1, 30);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Go back to ");
                    Rect r1 = GUILayoutUtility.GetLastRect();
                    GUILayout.Space(-5);
                    GUILayout.BeginVertical();
                    GUILayout.Space(2);
                    GUILayout.Label("Add your game", EditorStyles.boldLabel);
                    GUILayout.EndVertical();
                    Rect r2 = GUILayoutUtility.GetLastRect();
                    Rect r3 = new Rect(r1.x, r1.y, r1.width + r2.width, r2.height);
                    if (GUI.Button(r3, "", GUIStyle.none))
                    {
                        TourStep = 1;
                    }
                    EditorGUIUtility.AddCursorRect(r3, MouseCursor.Link);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    break;
                #endregion // create new game

                #region  app figures add game
                case 3: // app figures add game

                    GUILayout.Space(20);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(GameAnalytics.SettingsGA.GameIcon, new GUILayoutOption[] {
                        GUILayout.Width(40),
                        GUILayout.MaxHeight(40)
                    });
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Is this your game?", EditorStyles.whiteLargeLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Please confirm that this is the game you want to add.");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    // game name

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (_appFiguresGame.Icon != null)
                    {
                        GUILayout.Label(_appFiguresGame.Icon, new GUILayoutOption[] {
                            GUILayout.Width(100),
                            GUILayout.Height(100)
                        });
                    }
                    else
                    {
                        GUILayout.Label("", new GUILayoutOption[] {
                            GUILayout.Width(100),
                            GUILayout.Height(100)
                        });
                    }
                    GUILayout.Label("", GUILayout.Width(25));
                    GUILayout.BeginVertical();
                    GUILayout.Label(_appFiguresGame.Name, EditorStyles.whiteLargeLabel, GUILayout.Width(200));
                    GUILayout.Label(_appFiguresGame.Developer, GUILayout.Width(200));
                    GUILayout.Label("", GUILayout.Height(20));
                    PaintAppStoreIcon(_appFiguresGame.Store);
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    if (_appFiguresGame.Store.Equals("google_play"))
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("PLEASE NOTICE: If you want to validate your Android in-app purchase please enter your Google Play License key (public key). Click here to learn more about the Google Play License key.", MessageType.Info);

                        if (GUI.Button(GUILayoutUtility.GetLastRect(), "", GUIStyle.none))
                        {
                            //Application.OpenURL("https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Configure%20XCode");
                        }
                        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

                        GUILayout.EndHorizontal();

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Google Play License key", GUILayout.Width(150));
                        this._googlePlayPublicKey = GUILayout.TextField(this._googlePlayPublicKey);
                        GUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    this._selectedOrganization = EditorGUILayout.Popup("", this._selectedOrganization, Organization.GetOrganizationNames(GameAnalytics.SettingsGA.Organizations, false), GUILayout.Width(200));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    this._selectedStudio = EditorGUILayout.Popup("", this._selectedStudio, this._selectedOrganization > 0 ? Studio.GetStudioNames(GameAnalytics.SettingsGA.Organizations[this._selectedOrganization - 1].Studios, false) : new string[0], GUILayout.Width(200));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    // create game button

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUI.enabled = !_createGameInProgress;
                    if (GUILayout.Button("Add game", new GUILayoutOption[] {
                        GUILayout.Width(200),
                        GUILayout.MaxHeight(30)
                    }))
                    {
                        _createGameInProgress = true;
                        this._selectedPlatform = _appFiguresGame.Store.Equals("google_play") || _appFiguresGame.Store.Equals("amazon_appstore") ? RuntimePlatform.Android : RuntimePlatform.IPhonePlayer;
                        GA_SettingsInspector.CreateGame(GameAnalytics.SettingsGA, this, this._selectedOrganization, this._selectedStudio, _appFiguresGame.Name, this._googlePlayPublicKey, this._selectedPlatform, _appFiguresGame);
                    }
                    GUI.enabled = true;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Go back to ");
                    Rect r21 = GUILayoutUtility.GetLastRect();
                    GUILayout.Space(-5);
                    GUILayout.BeginVertical();
                    GUILayout.Space(2);
                    GUILayout.Label("results", EditorStyles.boldLabel);
                    GUILayout.EndVertical();
                    Rect r22 = GUILayoutUtility.GetLastRect();
                    Rect r23 = new Rect(r21.x, r21.y, r21.width + r22.width, r22.height);
                    if (GUI.Button(r23, "", GUIStyle.none))
                    {
                        TourStep = 1;
                    }
                    EditorGUIUtility.AddCursorRect(r23, MouseCursor.Link);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GA_SettingsInspector.Splitter(new Color(0.35f, 0.35f, 0.35f), 1, 30);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("If your game is still in development or not in the app store, please add it manually.");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    // create new game button

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Create new game", new GUILayoutOption[] {
                        GUILayout.Width(200),
                        GUILayout.MaxHeight(30)
                    }))
                    {
                        TourStep = 2;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    break;
                #endregion // app figures add game

                #region game created
                case 4:

                    GUILayout.Space(20);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(GameAnalytics.SettingsGA.Logo, new GUILayoutOption[] {
                        GUILayout.Width(40),
                        GUILayout.Height(40)
                    });
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Congratulations!", EditorStyles.whiteLargeLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Your game has been created successfully.");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GA_SettingsInspector.Splitter(new Color(0.35f, 0.35f, 0.35f), 1, 30);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    TextAnchor tmpLA = GUI.skin.label.alignment;
                    GUI.skin.label.alignment = TextAnchor.UpperCenter;
                    GUILayout.Label("We've put together a simple guide to help you instrument GameAnalytics in your game.", GUILayout.MaxWidth(540));
                    GUI.skin.label.alignment = tmpLA;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    // create game button

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Start guide", new GUILayoutOption[] {
                        GUILayout.Width(200),
                        GUILayout.MaxHeight(30)
                    }))
                    {
                        TourStep = 5;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    break;
                #endregion // game created

                #region guide
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:


                    int guideStep = TourStep - 4;

                    GUILayout.Space(20);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(GameAnalytics.SettingsGA.InstrumentIcon, new GUILayoutOption[] {
                        GUILayout.Width(40),
                        GUILayout.Height(40)
                    });
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Start instrumenting", EditorStyles.whiteLargeLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Let us guide you through getting properly setup with GameAnalytics.");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GA_SettingsInspector.Splitter(new Color(0.35f, 0.35f, 0.35f), 1, 30);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (guideStep == 10)
                        GUILayout.Label(GetGuideStepTitle(guideStep), EditorStyles.whiteLargeLabel, GUILayout.Width(464));
                    else
                        GUILayout.Label(GetGuideStepTitle(guideStep), EditorStyles.whiteLargeLabel, GUILayout.Width(470));
                    GUILayout.BeginVertical();
                    GUILayout.Space(7);
                    if (guideStep == 10)
                        GUILayout.Label("STEP " + (guideStep) + " OF 10", GUILayout.Width(87));
                    else
                        GUILayout.Label("STEP " + (guideStep) + " OF 10", GUILayout.Width(80));
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginVertical();
                    StringWithType[] guideStepTexts = GetGuideStepText(guideStep);
                    foreach (StringWithType s in guideStepTexts)
                    {
                        if (s.Type == StringType.Label)
                        {
                            GUILayout.Label(s.Text, EditorStyles.wordWrappedLabel, GUILayout.MaxWidth(550));
                        }
                        else if (s.Type == StringType.TextBox)
                        {
                            TextAnchor tmpA = GUI.skin.textField.alignment;
                            int tmpFS = GUI.skin.textField.fontSize;
                            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
                            GUI.skin.textField.fontSize = 12;
                            GUI.skin.textField.padding = new RectOffset(10, 1, 10, 10);
                            GUILayout.TextField(s.Text, new GUILayoutOption[] {
                                GUILayout.MaxWidth(550),
                                GUILayout.Height(34)
                            });
                            GUI.skin.textField.alignment = tmpA;
                            GUI.skin.textField.fontSize = tmpFS;
                            GUI.skin.textField.padding = new RectOffset(3, 3, 1, 2);
                        }
                        else if (s.Type == StringType.Link)
                        {
                            GUI.skin.label.fontStyle = FontStyle.Bold;
                            float sl = GUI.skin.button.CalcSize(new GUIContent(s.Text)).x;
                            if (GUILayout.Button(s.Text, EditorStyles.whiteLabel, GUILayout.MaxWidth(sl)))
                            {
                                Application.OpenURL(s.Link);
                            }
                            GUI.skin.label.fontStyle = FontStyle.Normal;

                            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GA_SettingsInspector.Splitter(new Color(0.35f, 0.35f, 0.35f), 1, 30);

                    // create game button

                    string buttonText = "Next step";
                    if (TourStep == 14)
                    {
                        buttonText = "Done";
                    }

                    GUI.BeginGroup(new Rect(0, 420, 640, 50));
                    //GUILayout.BeginHorizontal();
                    //GUILayout.FlexibleSpace();
                    //GUILayout.BeginVertical();
                    //GUILayout.Space(7);
                    GUI.Label(new Rect(43, 7, 500, 50), GetGuideStepNext(guideStep));
                    //GUILayout.EndVertical();
                    if (guideStep > 1 && GUI.Button(new Rect(454, 0, 30, 30), "<"))
                    {
                        TourStep--;
                    }
                    if (GUI.Button(new Rect(489, 0, 100, 30), buttonText))
                    {
                        if (TourStep < 14)
                            TourStep++;
                        else
                            Close();
                    }
                    //GUILayout.FlexibleSpace();
                    //GUILayout.EndHorizontal();
                    GUI.EndGroup();

                    break;
                    #endregion // guide
            }
        }

        public void AppFigComplete(string gameName, List<AppFiguresGame> appFiguresGames)
        {
            _appFigName = gameName;
            _appFiguresGames = appFiguresGames;
            Repaint();
        }

        public void SignUpComplete()
        {
            _instance.TourStep = 1;
            _signUpInProgress = false;
            Repaint();
        }

        public void SignUpFailed()
        {
            _signUpInProgress = false;
            Repaint();
        }

        public void CreateGameComplete()
        {
            TourStep = 4;
            _createGameInProgress = false;
            Repaint();
        }

        public void CreateGameFailed()
        {
            _createGameInProgress = false;
            Repaint();
        }

        public void SwitchToGuideStep()
        {
            TourStep = 5;
        }

        private string GetGuideStepTitle(int step)
        {
            switch (step)
            {
                case 1:
                    return "1. SETUP GAME KEYS";
                case 2:
                    return "2. ADD GAMEANALYTICS OBJECT";
                case 3:
                    return "3. START TRACKING EVENTS";
                case 4:
                    return "4. TRACK REAL MONEY TRANSACTIONS";
                case 5:
                    return "5. BALANCE VIRTUAL ECONOMY";
                case 6:
                    return "6. TRACK PLAYER PROGRESSION";
                case 7:
                    return "7. USE CUSTOM DESIGN EVENTS";
                case 8:
                    return "8. LOG ERROR EVENTS";
                case 9:
                    return "9. USE CUSTOM DIMENSIONS";
                case 10:
                    return "10. BUILD AND COMPLETE INTEGRATION";
                default:
                    return "-";
            }
        }

        private StringWithType[] GetGuideStepText(int step)
        {
            switch (step)
            {
                case 1:
                    return new StringWithType[] {
                        new StringWithType { Text = "The unique game and secret key are used to authenticate your game. If you're logged into GameAnalytics, in Settings under the Account tab, choose your studio and game to sync your keys with your Unity project. If you don't have an account, choose Sign up to create your account and game." },
                        new StringWithType { Text = "You can also input your keys manually in Settings under the Setup tab. The keys can always be found under Game Settings in the webtool." },
                        new StringWithType { Text = "" },
                        new StringWithType {
                            Text = "Click here to learn more about the Game and Secret keys.",
                            Type = StringType.Link,
                            Link = "https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Settings#setup"
                        },
                    };
                case 2:
                    return new StringWithType[] {
                        new StringWithType { Text = "To use GameAnalytics you need to add the GameAnalytics object to your starting scene. To add this object go to Window/GameAnalytics/Create GameAnalytics Object." },
                        new StringWithType { Text = "Now you're set up to start tracking data - it's that easy! Out of the box GameAnalytics will give you access to lots of core metrics, such as daily active users (DAU), without implementing any custom events." },
                        new StringWithType { Text = "" },
                        new StringWithType {
                            Text = "Click here to learn more about the full list of core metrics and dimensions.",
                            Type = StringType.Link,
                            Link = "http://www.gameanalytics.com/docs/metric-and-dimensions-reference/"
                        }
                    };
                case 3:
                    return new StringWithType[] {
                        new StringWithType { Text = "GameAnalytics supports 5 different types of events: Business, Resource, Progression, Error and Design." },
                        new StringWithType { Text = "To send an event, remember to include the namespace GameAnalyticsSDK:" },
                        new StringWithType {
                            Text = "using GameAnalyticsSDK;",
                            Type = StringType.TextBox
                        },
                        new StringWithType { Text = "" },
                        new StringWithType { Text = "The next steps will guide you through the instrumentation of each of the different event types." },
                    };
                case 4:
                    return new StringWithType[] {
                        new StringWithType { Text = "With the Business event, you can include information on the specific type of in-app item purchased, and where in the game the purchase was made. Additionally, the GameAnalytics SDK captures the app store receipt to validate the purchases." },
                        new StringWithType { Text = "To add a business event call the following function:" },
                        new StringWithType {
                            Text = "GameAnalytics.NewBusinessEvent (string currency, int amount, string itemType, string itemId, string cartType, string receipt, bool autoFetchReceipt);",
                            Type = StringType.TextBox
                        },
                        new StringWithType { Text = "" },
                        new StringWithType {
                            Text = "Click here to learn more about the Business event and purchase validation.",
                            Type = StringType.Link,
                            Link = "https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Business%20Event"
                        }
                    };
                case 5:
                    return new StringWithType[] {
                        new StringWithType { Text = "Resources events are used to track your in-game economy. From setting up the event you will be able to see three types of events in the tool. Flow, the total balance from currency spent and rewarded. Sink is all currency spent on items, and lastly source, being all currency rewarded in game." },
                        new StringWithType { Text = "To add a resource event call the following function:" },
                        new StringWithType {
                            Text = "GameAnalytics.NewResourceEvent (GA_Resource.GAResourceFlowType flowType, string resourceType, float amount, string itemType, string itemId);",
                            Type = StringType.TextBox
                        },
                        new StringWithType { Text = "Please note that any Resource Currencies and Resource Item Types you want to use must first be defined in Settings, under the Setup tab. Any value which is not defined will be ignored." },
                        new StringWithType { Text = "" },
                        new StringWithType {
                            Text = "Click here to learn more about the Resource event.",
                            Type = StringType.Link,
                            Link = "https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Resource%20Event"
                        }
                    };
                case 6:
                    return new StringWithType[] {
                        new StringWithType { Text = "Use this event to track when players start and finish levels in your game. This event follows a 3 tier hierarchy structure (World, Level and Phase) to indicate a player's path or place in the game." },
                        new StringWithType { Text = "To add a progression event call the following function:" },
                        new StringWithType {
                            Text = "GameAnalytics.NewProgressionEvent (GA_Progression.GAProgressionStatus progressionStatus, string progression01, string progression02);",
                            Type = StringType.TextBox
                        },
                        new StringWithType { Text = "" },
                        new StringWithType {
                            Text = "Click here to learn more about the Progression event.",
                            Type = StringType.Link,
                            Link = "https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Progression%20Event"
                        }
                    };
                case 7:
                    return new StringWithType[] {
                        new StringWithType { Text = "Track any other concept in your game using this event type. For example, you could use this event to track GUI elements or tutorial steps. Custom dimensions are not supported on this event type." },
                        new StringWithType { Text = "To add a design event call the following function:" },
                        new StringWithType {
                            Text = "GameAnalytics.NewDesignEvent (string eventName, float eventValue);",
                            Type = StringType.TextBox
                        },
                        new StringWithType { Text = "" },
                        new StringWithType {
                            Text = "Click here to learn more about the Design event.",
                            Type = StringType.Link,
                            Link = "https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Design%20Event"
                        }
                    };
                case 8:
                    return new StringWithType[] {
                        new StringWithType { Text = "You can use the Error event to log errors or warnings that players generate in your game. You can group the events by severity level and attach a message, such as the stack trace." },
                        new StringWithType { Text = "To add a custom error event call the following function:" },
                        new StringWithType {
                            Text = "GameAnalytics.NewErrorEvent (GA_Error.GAErrorSeverity severity, string message);",
                            Type = StringType.TextBox
                        },
                        new StringWithType { Text = "" },
                        new StringWithType {
                            Text = "Click here to learn more about the Error event.",
                            Type = StringType.Link,
                            Link = "https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Error%20Event"
                        }
                    };
                case 9:
                    return new StringWithType[] {
                        new StringWithType { Text = "Custom Dimensions can be used to filter your data in the GameAnalytics webtool. To add custom dimensions to your events you will first have to create a list of all the allowed values. You can do this in Settings under the Setup tab. Any value which is not defined will be ignored." },
                        new StringWithType { Text = "For example, to set Custom Dimension 01, call the following function:" },
                        new StringWithType {
                            Text = "GameAnalytics.SetCustomDimension01(string customDimension);",
                            Type = StringType.TextBox
                        },
                        new StringWithType { Text = "" },
                        new StringWithType {
                            Text = "Click here to learn more about Custom Dimensions.",
                            Type = StringType.Link,
                            Link = "https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Set%20Custom%20Dimension"
                        }
                    };
                case 10:
                    return new StringWithType[] {
                        new StringWithType { Text = "You're almost there! To complete the integration and start sending data to GameAnalytics, all you need to do is build and run your game." },
                    #if UNITY_IOS || UNITY_TVOS || UNITY_ANDROID
                        new StringWithType {Text = "The link below describes the important last steps you need to complete to build for the build platform you selected in the editor."},
                    #endif
                        new StringWithType { Text = "" },

                    #if UNITY_IOS || UNITY_TVOS

                    new StringWithType {
                        Text = "iOS/tvOS",
                        Type = StringType.Link,
                        Link = "https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/iOS%20Build"
                    }

                    #elif UNITY_ANDROID

                    new StringWithType {
                        Text = "Android",
                        Type = StringType.Link,
                        Link = "https://github.com/GameAnalytics/GA-SDK-UNITY/wiki/Android%20Build"
                    }

                    #elif UNITY_STANDALONE || UNITY_TIZEN || UNITY_WEBGL || UNITY_WINRT

                    new StringWithType {
                    Text = "Click here to check online documentation!",
                    Type = StringType.Link,
                    Link = "https://github.com/GameAnalytics/GA-SDK-UNITY/wiki"
                    }

                    #else

                    new StringWithType { Text = "Your selected build platform is not currently supported by GameAnalytics." },
                    new StringWithType { Text = "The Unity SDK includes support for Windows, Mac, Linux, WebGL, iOS, tvOS, UWP, Tizen, Universal Windows 8 and Android.", Type = StringType.Link, Link = "https://github.com/GameAnalytics/GA-SDK-UNITY" },

                    #endif
                    };
                default:
                    return new StringWithType[] {
                        new StringWithType { Text = "-" }
                    };
            }
        }

        private string GetGuideStepNext(int step)
        {
            switch (step)
            {
                case 1:
                    return "In the next step we look at how to add the GameAnalytics object.";
                case 2:
                    return "In the next step we look at how to start tracking events.";
                case 3:
                    return "In the next step we look at how to track real money transactions.";
                case 4:
                    return "In the next step we look at how to balance your virtual economy.";
                case 5:
                    return "In the next step we look at how to track player progression.";
                case 6:
                    return "In the next step we look at how to use custom design events.";
                case 7:
                    return "In the next step we look at how to log error events.";
                case 8:
                    return "In the next step we look at how to use custom dimensions.";
                case 9:
                    return "In the last step we look at completing the integration.";
                case 10:
                    return "Thank you for choosing GameAnalytics!";
                default:
                    return "-";
            }
        }

        private void PaintAppStoreIcon(string storeName)
        {
            switch (storeName)
            {
                case "amazon_appstore":
                    if (GameAnalytics.SettingsGA.AmazonIcon != null)
                    {
                        //GUILayout.Label("", GUILayout.Height(-20));
                        GUILayout.Label(GameAnalytics.SettingsGA.AmazonIcon, new GUILayoutOption[] {
                            GUILayout.Width(20),
                            GUILayout.MaxHeight(20)
                        });
                    }

                    GUILayout.Label("Amazon", GUILayout.Width(80));
                    break;
                case "google_play":
                    if (GameAnalytics.SettingsGA.GooglePlayIcon != null)
                    {
                        //GUILayout.Label("", GUILayout.Height(-20));
                        GUILayout.Label(GameAnalytics.SettingsGA.GooglePlayIcon, new GUILayoutOption[] {
                            GUILayout.Width(20),
                            GUILayout.MaxHeight(20)
                        });
                    }

                    GUILayout.Label("Google Play", GUILayout.Width(80));
                    break;
                case "apple":
                    if (GameAnalytics.SettingsGA.iosIcon != null)
                    {
                        //GUILayout.Label("", GUILayout.Height(-20));
                        GUILayout.Label(GameAnalytics.SettingsGA.iosIcon, new GUILayoutOption[] {
                            GUILayout.Width(20),
                            GUILayout.MaxHeight(20)
                        });
                    }

                    GUILayout.Label("iOS", GUILayout.Width(80));
                    break;
                case "apple:mac":
                    if (GameAnalytics.SettingsGA.macIcon != null)
                    {
                        //GUILayout.Label("", GUILayout.Height(-20));
                        GUILayout.Label(GameAnalytics.SettingsGA.macIcon, new GUILayoutOption[] {
                            GUILayout.Width(20),
                            GUILayout.MaxHeight(20)
                        });
                    }

                    GUILayout.Label("Mac", GUILayout.Width(80));
                    break;
                case "windows_phone":
                    if (GameAnalytics.SettingsGA.windowsPhoneIcon != null)
                    {
                        //GUILayout.Label("", GUILayout.Height(-20));
                        GUILayout.Label(GameAnalytics.SettingsGA.windowsPhoneIcon, new GUILayoutOption[] {
                            GUILayout.Width(20),
                            GUILayout.MaxHeight(20)
                        });
                    }

                    GUILayout.Label("Win. Phone", GUILayout.Width(80));
                    break;
                default:
                    GUILayout.Label(storeName, GUILayout.Width(100));
                    break;
            }
        }

#if UNITY_2017_1_OR_NEWER
        public IEnumerator GetAppStoreIconTexture(UnityWebRequest www, string storeName, GA_SignUp signup)
#else
        public IEnumerator<WWW> GetAppStoreIconTexture(WWW www, string storeName, GA_SignUp signup)
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
                    switch (storeName)
                    {
                        case "amazon_appstore":
#if UNITY_2017_1_OR_NEWER
                            GameAnalytics.SettingsGA.AmazonIcon = ((DownloadHandlerTexture)www.downloadHandler).texture;
#else
                            GameAnalytics.SettingsGA.AmazonIcon = www.texture;
#endif
                            break;
                        case "google_play":
#if UNITY_2017_1_OR_NEWER
                            GameAnalytics.SettingsGA.GooglePlayIcon = ((DownloadHandlerTexture)www.downloadHandler).texture;
#else
                            GameAnalytics.SettingsGA.GooglePlayIcon = www.texture;
#endif
                            break;
                        case "apple:ios":
#if UNITY_2017_1_OR_NEWER
                            GameAnalytics.SettingsGA.iosIcon = ((DownloadHandlerTexture)www.downloadHandler).texture;
#else
                            GameAnalytics.SettingsGA.iosIcon = www.texture;
#endif
                            break;
                        case "apple:mac":
#if UNITY_2017_1_OR_NEWER
                            GameAnalytics.SettingsGA.macIcon = ((DownloadHandlerTexture)www.downloadHandler).texture;
#else
                            GameAnalytics.SettingsGA.macIcon = www.texture;
#endif
                            break;
                        case "windows_phone":
#if UNITY_2017_1_OR_NEWER
                            GameAnalytics.SettingsGA.windowsPhoneIcon = ((DownloadHandlerTexture)www.downloadHandler).texture;
#else
                            GameAnalytics.SettingsGA.windowsPhoneIcon = www.texture;
#endif
                            break;
                    }
                    signup.Repaint();
                }
            }
            catch
            {
            }
        }
    }

    public class AppFiguresGame
    {
        public string Name { get; private set; }

        public string AppID { get; private set; }

        public string Store { get; private set; }

        public string Developer { get; private set; }

        public string IconUrl { get; private set; }

        public Texture2D Icon { get; private set; }

        public AppFiguresGame(string name, string appID, string store, string developer, string iconUrl, GA_SignUp signup)
        {
            Name = name;
            AppID = appID;
            Store = store;
            Developer = developer;
            IconUrl = iconUrl;

#if UNITY_2017_1_OR_NEWER
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(iconUrl);
            GA_ContinuationManager.StartCoroutine(GetIconTexture(www, signup), () => www.isDone);
#else
            WWW www = new WWW(iconUrl);
            GA_ContinuationManager.StartCoroutine(GetIconTexture(www, signup), () => www.isDone);
#endif
        }

#if UNITY_2017_1_OR_NEWER
        private IEnumerator GetIconTexture(UnityWebRequest www, GA_SignUp signup)
#else
        private IEnumerator<WWW> GetIconTexture(WWW www, GA_SignUp signup)
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
#if UNITY_2017_1_OR_NEWER
                    Icon = ((DownloadHandlerTexture)www.downloadHandler).texture;
#else
                    Icon = www.texture;
#endif
                    signup.Repaint();
                }
                else
                {
                    Debug.LogError("Failed to get icon: " + www.error);
                }
            }
            catch
            {
                Debug.LogError("Failed to get icon");
            }
        }
    }
}
