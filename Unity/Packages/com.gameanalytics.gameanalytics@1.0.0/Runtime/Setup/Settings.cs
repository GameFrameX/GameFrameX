using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameAnalyticsSDK.Setup
{
    /// <summary>
    /// The Settings object contains an array of options which allows you to customize your use of GameAnalytics. Most importantly you will need to fill in your Game Key and Secret Key on the Settings object to use the service.
    /// </summary>
    ///
    public class Settings : ScriptableObject
    {
        /// <summary>
        /// Types of help given in the help box of the GA inspector
        /// </summary>
        public enum HelpTypes
        {
            None,
            IncludeSystemSpecsHelp,
            ProvideCustomUserID
        }

        ;

        public enum MessageTypes
        {
            None,
            Error,
            Info,
            Warning
        }

        ;

        /// <summary>
        /// A message and message type for the help box displayed on the GUI inspector
        /// </summary>
        public struct HelpInfo
        {
            public string Message;
            public MessageTypes MsgType;
            public HelpTypes HelpType;
        }

        #region public static values

        /// <summary>
        /// The version of the GA Unity Wrapper plugin
        /// </summary>
        [HideInInspector]
        public static string VERSION = "7.4.0";

        [HideInInspector]
        public static bool CheckingForUpdates = false;

        #endregion

        #region public values

        public int TotalMessagesSubmitted;
        public int TotalMessagesFailed;

        public int DesignMessagesSubmitted;
        public int DesignMessagesFailed;
        public int QualityMessagesSubmitted;
        public int QualityMessagesFailed;
        public int ErrorMessagesSubmitted;
        public int ErrorMessagesFailed;
        public int BusinessMessagesSubmitted;
        public int BusinessMessagesFailed;
        public int UserMessagesSubmitted;
        public int UserMessagesFailed;

        public string CustomArea = string.Empty;

        [SerializeField]
        private List<string> gameKey = new List<string>();
        [SerializeField]
        private List<string> secretKey = new List<string>();
        [SerializeField]
        public List<string> Build = new List<string>();
        [SerializeField]
        public List<string> SelectedPlatformOrganization = new List<string>();
        [SerializeField]
        public List<string> SelectedPlatformStudio = new List<string>();
        [SerializeField]
        public List<string> SelectedPlatformGame = new List<string>();
        [SerializeField]
        public List<int> SelectedPlatformGameID = new List<int>();
        [SerializeField]
        public List<int> SelectedOrganization = new List<int>();
        [SerializeField]
        public List<int> SelectedStudio = new List<int>();
        [SerializeField]
        public List<int> SelectedGame = new List<int>();

        public string NewVersion = "";
        public string Changes = "";

        public bool SignUpOpen = true;
        public string StudioName = "";
        public string GameName = "";
        public string OrganizationName = "";
        public string OrganizationIdentifier = "";
        public string EmailGA = "";

        [System.NonSerialized]
        public string PasswordGA = "";
        [System.NonSerialized]
        public string TokenGA = "";
        [System.NonSerialized]
        public string ExpireTime = "";
        [System.NonSerialized]
        public string LoginStatus = "Not logged in.";
        [System.NonSerialized]
        public bool JustSignedUp = false;
        [System.NonSerialized]
        public bool HideSignupWarning = false;

        public bool IntroScreen = true;

        [System.NonSerialized]
        public List<GameAnalyticsSDK.Setup.Organization> Organizations;

        public bool InfoLogEditor = true;
        public bool InfoLogBuild = true;
        public bool VerboseLogBuild = false;
        public bool UseManualSessionHandling = false;
        public bool SendExampleGameDataToMyGame = false;
        //public bool UseBundleVersion = false;

        public bool InternetConnectivity;

        public List<string> CustomDimensions01 = new List<string>();
        public List<string> CustomDimensions02 = new List<string>();
        public List<string> CustomDimensions03 = new List<string>();

        public List<string> ResourceItemTypes = new List<string>();
        public List<string> ResourceCurrencies = new List<string>();

        public RuntimePlatform LastCreatedGamePlatform;

        public List<RuntimePlatform> Platforms = new List<RuntimePlatform>();

        //These values are used for the GA_Inspector only
        public enum InspectorStates
        {
            Account,
            Basic,
            Debugging,
            Pref

        }

        public InspectorStates CurrentInspectorState;
        public List<HelpTypes> ClosedHints = new List<HelpTypes>();
        public bool DisplayHints;
        public Vector2 DisplayHintsScrollState;
        public Texture2D Logo;
        public Texture2D UpdateIcon;
        public Texture2D InfoIcon;
        public Texture2D DeleteIcon;
        public Texture2D GameIcon;
        public Texture2D HomeIcon;
        public Texture2D InstrumentIcon;
        public Texture2D QuestionIcon;
        public Texture2D UserIcon;

        public Texture2D AmazonIcon;
        public Texture2D GooglePlayIcon;
        public Texture2D iosIcon;
        public Texture2D macIcon;
        public Texture2D windowsPhoneIcon;

        [System.NonSerialized]
        public GUIStyle SignupButton;

        public bool UsePlayerSettingsBuildNumber = false;
        public bool SubmitErrors = true;
        public bool NativeErrorReporting = false;
        public int MaxErrorCount = 10;
        public bool SubmitFpsAverage = false;
        public bool SubmitFpsCritical = false;
        public bool IncludeGooglePlay = true;
        public int FpsCriticalThreshold = 20;
        public int FpsCirticalSubmitInterval = 1;

        public List<bool> PlatformFoldOut = new List<bool>();

        public bool CustomDimensions01FoldOut = false;
        public bool CustomDimensions02FoldOut = false;
        public bool CustomDimensions03FoldOut = false;

        public bool ResourceItemTypesFoldOut = false;
        public bool ResourceCurrenciesFoldOut = false;

        #endregion

        #region public methods

        /// <summary>
        /// Sets a custom user ID.
        /// Make sure each user has a unique user ID. This is useful if you have your own log-in system with unique user IDs.
        /// NOTE: Only use this method if you have enabled "Custom User ID" on the GA inspector!
        /// </summary>
        /// <param name="customID">
        /// The custom user ID - this should be unique for each user
        /// </param>
        public void SetCustomUserID(string customID)
        {
            if (customID != string.Empty)
            {
                // Set custom ID native
            }
        }

        public void RemovePlatformAtIndex(int index)
        {
            if (index >= 0 && index < this.Platforms.Count)
            {
                this.gameKey.RemoveAt(index);
                this.secretKey.RemoveAt(index);
                this.Build.RemoveAt(index);
                this.SelectedPlatformOrganization.RemoveAt(index);
                this.SelectedPlatformStudio.RemoveAt(index);
                this.SelectedPlatformGame.RemoveAt(index);
                this.SelectedPlatformGameID.RemoveAt(index);
                this.SelectedOrganization.RemoveAt(index);
                this.SelectedStudio.RemoveAt(index);
                this.SelectedGame.RemoveAt(index);
                this.PlatformFoldOut.RemoveAt(index);
                this.Platforms.RemoveAt(index);
            }
        }

        public void AddPlatform(RuntimePlatform platform)
        {
            this.gameKey.Add("");
            this.secretKey.Add("");
            this.Build.Add("0.1");
            this.SelectedPlatformOrganization.Add("");
            this.SelectedPlatformStudio.Add("");
            this.SelectedPlatformGame.Add("");
            this.SelectedPlatformGameID.Add(-1);
            this.SelectedOrganization.Add(0);
            this.SelectedStudio.Add(0);
            this.SelectedGame.Add(0);
            this.PlatformFoldOut.Add(true);
            this.Platforms.Add(platform);
        }

        public static readonly RuntimePlatform[] AvailablePlatforms = new RuntimePlatform[]
        {
            RuntimePlatform.Android,
            RuntimePlatform.IPhonePlayer,
            RuntimePlatform.LinuxPlayer,
            RuntimePlatform.OSXPlayer,
            RuntimePlatform.tvOS,
            RuntimePlatform.WebGLPlayer,
            RuntimePlatform.WindowsPlayer,
            RuntimePlatform.WSAPlayerARM
#if UNITY_2018_1_OR_NEWER

#else
            ,RuntimePlatform.TizenPlayer
#endif
        };

        public string[] GetAvailablePlatforms()
        {
            List<string> result = new List<string>();

            for(int i = 0; i < AvailablePlatforms.Length; ++i)
            {
                RuntimePlatform value = AvailablePlatforms[i];

                if(value == RuntimePlatform.IPhonePlayer)
                {
                    if(!this.Platforms.Contains(RuntimePlatform.tvOS) && !this.Platforms.Contains(value))
                    {
                        result.Add(value.ToString());
                    }
                    else
                    {
                        if(!this.Platforms.Contains(value))
                        {
                            result.Add(value.ToString());
                        }
                    }
                }
                else if(value == RuntimePlatform.tvOS)
                {
                    if(!this.Platforms.Contains(RuntimePlatform.IPhonePlayer) && !this.Platforms.Contains(value))
                    {
                        result.Add(value.ToString());
                    }
                    else
                    {
                        if(!this.Platforms.Contains(value))
                        {
                            result.Add(value.ToString());
                        }
                    }
                }
                else if(value == RuntimePlatform.WSAPlayerARM)
                {
                    if(!this.Platforms.Contains(value))
                    {
                        result.Add("WSA");
                    }
                }
                else
                {
                    if(!this.Platforms.Contains(value))
                    {
                        result.Add(value.ToString());
                    }
                }
            }

            return result.ToArray();
        }

        public bool IsGameKeyValid(int index, string value)
        {
            bool valid = true;

            for(int i = 0; i < this.Platforms.Count; ++i)
            {
                if(index != i)
                {
                    if(value.Equals(this.gameKey[i]))
                    {
                        valid = false;
                        break;
                    }
                }
            }

            return valid;
        }

        public bool IsSecretKeyValid(int index, string value)
        {
            bool valid = true;

            for(int i = 0; i < this.Platforms.Count; ++i)
            {
                if(index != i)
                {
                    if(value.Equals(this.secretKey[i]))
                    {
                        valid = false;
                        break;
                    }
                }
            }

            return valid;
        }

        public static void UpdateKeys(int index, string gameKey, string secretKey)
        {
            GameAnalytics.SettingsGA.gameKey[index] = gameKey;
            GameAnalytics.SettingsGA.secretKey[index] = secretKey;
        }

        public void UpdateGameKey(int index, string value)
        {
            if(!string.IsNullOrEmpty(value))
            {
                bool valid = this.IsGameKeyValid(index, value);

                if(valid)
                {
                    this.gameKey[index] = value;
                }
                else if(this.gameKey[index].Equals(value))
                {
                    this.gameKey[index] = "";
                }
            }
            else
            {
                this.gameKey[index] = value;
            }
        }

        public void UpdateSecretKey(int index, string value)
        {
            if(!string.IsNullOrEmpty(value))
            {
                bool valid = this.IsSecretKeyValid(index, value);

                if(valid)
                {
                    this.secretKey[index] = value;
                }
                else if(this.secretKey[index].Equals(value))
                {
                    this.secretKey[index] = "";
                }
            }
            else
            {
                this.secretKey[index] = value;
            }
        }

        public string GetGameKey(int index)
        {
            return this.gameKey[index];
        }

        public string GetSecretKey(int index)
        {
            return this.secretKey[index];
        }

        /// <summary>
        /// Sets a custom area string. An area is often just a level, but you can set it to whatever makes sense for your game. F.x. in a big open world game you will probably need custom areas to identify regions etc.
        /// By default, if no custom area is set, the Application.loadedLevelName string is used.
        /// </summary>
        /// <param name="customID">
        /// The custom area.
        /// </param>
        public void SetCustomArea(string customArea)
        {
            // Set custom area native
        }

        public void SetKeys(string gamekey, string secretkey)
        {
            // set keys native
        }

#endregion
    }

    public class Organization
    {
        public string Name { get; private set; }
        public string ID { get; private set; }
        public List<GameAnalyticsSDK.Setup.Studio> Studios { get; private set; }

        public Organization(string name, string id)
        {
            Name = name;
            ID = id;
            Studios = new List<GameAnalyticsSDK.Setup.Studio>();
        }

        public static string[] GetOrganizationNames(List<GameAnalyticsSDK.Setup.Organization> organizations, bool addFirstEmpty = true)
        {
            if (organizations == null)
            {
                return new string[] { "-" };
            }

            if (addFirstEmpty)
            {
                string[] names = new string[organizations.Count + 1];
                names[0] = "-";

                string spaceAdd = "";
                for (int i = 0; i < organizations.Count; i++)
                {
                    names[i + 1] = organizations[i].Name + spaceAdd;
                    spaceAdd += " ";
                }

                return names;
            }
            else
            {
                string[] names = new string[organizations.Count];

                string spaceAdd = "";
                for (int i = 0; i < organizations.Count; i++)
                {
                    names[i] = organizations[i].Name + spaceAdd;
                    spaceAdd += " ";
                }

                return names;
            }
        }
    }

    //[System.Serializable]
    public class Studio
    {
        public string Name { get; private set; }

        public string ID { get; private set; }

        public string OrganizationID { get; private set; }

        //[SerializeField]
        public List<GameAnalyticsSDK.Setup.Game> Games { get; private set; }

        public Studio(string name, string id, string orgId, List<GameAnalyticsSDK.Setup.Game> games)
        {
            Name = name;
            ID = id;
            OrganizationID = orgId;
            Games = games;
        }

        public static string[] GetStudioNames(List<GameAnalyticsSDK.Setup.Studio> studios, bool addFirstEmpty = true)
        {
            if(studios == null)
            {
                return new string[] { "-" };
            }

            if(addFirstEmpty)
            {
                string[] names = new string[studios.Count + 1];
                names[0] = "-";

                for(int i = 0; i < studios.Count; i++)
                {
                    int j = i + 1;
                    names[j] = j + ". " + studios[i].Name;
                }

                return names;
            }
            else
            {
                string[] names = new string[studios.Count];

                for(int i = 0; i < studios.Count; i++)
                {
                    int j = i + 1;
                    names[i] = j + ". " + studios[i].Name;
                }

                return names;
            }
        }

        public static string[] GetGameNames(int index, List<GameAnalyticsSDK.Setup.Studio> studios)
        {
            if(studios == null || studios[index].Games == null)
            {
                return new string[] { "-" };
            }

            string[] names = new string[studios[index].Games.Count + 1];
            names[0] = "-";

            for(int i = 0; i < studios[index].Games.Count; i++)
            {
                int j = i + 1;
                names[j] = j + ". " + studios[index].Games[i].Name;
            }

            return names;
        }
    }

    public class Game
    {
        public string Name { get; private set; }

        public int ID { get; private set; }

        public string GameKey { get; private set; }

        public string SecretKey { get; private set; }

        public Game(string name, int id, string gameKey, string secretKey)
        {
            Name = name;
            ID = id;
            GameKey = gameKey;
            SecretKey = secretKey;
        }
    }
}
