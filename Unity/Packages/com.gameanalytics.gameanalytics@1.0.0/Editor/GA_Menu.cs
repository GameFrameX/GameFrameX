using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace GameAnalyticsSDK.Editor
{
    public static class GA_Menu
    {
        [MenuItem("Window/GameAnalytics/Select Settings", false, 0)]
        static void SelectGASettings()
        {
            Selection.activeObject = GameAnalytics.SettingsGA;
        }

        [MenuItem("Window/GameAnalytics/Setup Guide", false, 100)]
        static void SetupAndTour()
        {
            GA_SignUp signup = ScriptableObject.CreateInstance<GA_SignUp>();
            signup.maxSize = new Vector2(640, 600);
            signup.minSize = new Vector2(640, 600);

            signup.titleContent = new GUIContent("GameAnalytics - Sign up for FREE");
            signup.ShowUtility();
            signup.Opened();

            signup.SwitchToGuideStep();
        }

        [MenuItem("Window/GameAnalytics/Create GameAnalytics Object", false, 200)]
        static void AddGASystemTracker()
        {
            if (Object.FindObjectOfType(typeof(GameAnalytics)) == null)
            {
                GameObject go = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(GameAnalytics.WhereIs("GameAnalytics.prefab", "Prefab"), typeof(GameObject))) as GameObject;
                go.name = "GameAnalytics";
                Selection.activeObject = go;
                Undo.RegisterCreatedObjectUndo(go, "Created GameAnalytics Object");
            }
            else
            {
                Debug.LogWarning("A GameAnalytics object already exists in this scene - you should never have more than one per scene!");
            }
        }
        //
        // [MenuItem ("Window/GameAnalytics/PlayMaker/Toggle Scripts", false, 400)]
        // static void TogglePlayMaker ()
        // {
        //     bool enabled = false;
        //     bool fail = false;
        //
        //     string searchText = "#if false";
        //     string replaceText = "#if true";
        //
        //     string[] _files = new string[] {
        //         "GAInitialize.cs",
        //         "GetABTestingId.cs",
        //         "GetABTestingVariantId.cs",
        //         "GetRemoteConfigsValueAsString.cs",
        //         "IsRemoteConfigsReady.cs",
        //         "SendAdEvent.cs",
        //         "SendBusinessEvent.cs",
        //         "SendDesignEvent.cs",
        //         "SendErrorEvent.cs",
        //         "SendProgressionEvent.cs",
        //         "SendResourceEvent.cs",
        //         "SetCustomDimension.cs",
        //         "SendProgressionEventActionEditor.cs",
        //         "SendResourceEventActionEditor.cs"
        //     };
        //
        //     foreach(string _file in _files)
        //     {
        //         try {
        //             enabled = ReplaceInFile (GameAnalytics.WhereIs(_file, "Script"), searchText, replaceText);
        //         } catch {
        //             Debug.Log("Failed to toggle "+_file);
        //             fail = true;
        //         }
        //     }
        //
        //     AssetDatabase.Refresh();
        //
        //     if (fail)
        //     {
        //         PlayMakerPresenceCheck.ResetPrefs();
        //         Debug.Log("Failed to toggle PlayMaker Scripts.");
        //     }else if (enabled)
        //     {
        //         Debug.Log("Enabled PlayMaker Scripts.");
        //     }else
        //     {
        //         PlayMakerPresenceCheck.ResetPrefs();
        //         Debug.Log("Disabled PlayMaker Scripts.");
        //     }
        // }

        public static bool ReplaceInFile(string filePath, string searchText, string replaceText)
        {
            bool enabled = false;

            StreamReader reader = new StreamReader(filePath);
            string content = reader.ReadToEnd();
            reader.Close();

            if (content.StartsWith(searchText))
            {
                enabled = true;
                content = Regex.Replace(content, searchText, replaceText);
            }
            else
            {
                enabled = false;
                content = Regex.Replace(content, replaceText, searchText);
            }

            StreamWriter writer = new StreamWriter(filePath);
            writer.Write(content);
            writer.Close();

            return enabled;
        }
    }
}